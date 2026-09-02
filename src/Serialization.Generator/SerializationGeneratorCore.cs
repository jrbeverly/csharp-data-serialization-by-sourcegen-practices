using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Serialization.Generator;

[Generator]
public sealed class SerializationGeneratorCore : IIncrementalGenerator
{
    private static readonly IReadOnlyList<ISerializationBackend> Backends =
        new ISerializationBackend[] { new JsonSerializationBackend(), new CsvSerializationBackend() };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var results = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "Serialization.Abstractions.GenerateSerializersAttribute",
                predicate: static (node, _) => node is TypeDeclarationSyntax,
                transform: static (ctx, ct) => Analyze(ctx, ct));

        context.RegisterSourceOutput(results, static (spc, result) =>
        {
            foreach (var d in result.Diagnostics)
                spc.ReportDiagnostic(d);

            if (result.Descriptor is null)
                return;

            foreach (var backend in Backends)
            {
                if ((result.Descriptor.Formats & backend.Format) != 0)
                    backend.Generate(result.Descriptor, spc);
            }
        });
    }

    private static AnalysisResult Analyze(GeneratorAttributeSyntaxContext ctx, CancellationToken ct)
    {
        var typeSymbol = (INamedTypeSymbol)ctx.TargetSymbol;
        var attr = ctx.Attributes[0];

        var formats = (SerializationFormat)(int)attr.ConstructorArguments[0].Value!;

        var diagnostics = new List<Diagnostic>();
        var members = new List<MemberDescriptor>();

        foreach (var member in typeSymbol.GetMembers())
        {
            ct.ThrowIfCancellationRequested();

            if (member.Kind != SymbolKind.Property || member.IsStatic || member.IsImplicitlyDeclared)
                continue;

            var prop = (IPropertySymbol)member;
            if (prop.IsIndexer)
                continue;

            AttributeData? serAttr = null;
            foreach (var a in prop.GetAttributes())
            {
                if (a.AttributeClass?.ToDisplayString() == "Serialization.Abstractions.SerializeMemberAttribute")
                {
                    serAttr = a;
                    break;
                }
            }

            if (serAttr is null)
            {
                var propLoc = prop.Locations.Length > 0 ? prop.Locations[0] : Location.None;
                diagnostics.Add(Diagnostic.Create(
                    Diagnostics.MissingSerializeMemberAttribute, propLoc, prop.Name, typeSymbol.Name));
                continue;
            }

            var wireName = (string)serAttr.ConstructorArguments[0].Value!;
            var order    = (int)serAttr.ConstructorArguments[1].Value!;
            var scalarType = ClassifyScalar(prop.Type);
            var kind     = scalarType is null ? MemberKind.Nested : MemberKind.Scalar;
            var nullable = IsNullable(prop.Type);

            var attrLoc = serAttr.ApplicationSyntaxReference is { } r
                ? Location.Create(r.SyntaxTree, r.Span)
                : (prop.Locations.Length > 0 ? prop.Locations[0] : Location.None);

            var modelType = prop.Type is INamedTypeSymbol { IsGenericType: true } namedType &&
                namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T
                    ? namedType.TypeArguments[0]
                    : prop.Type;
            var typeName = modelType.WithNullableAnnotation(NullableAnnotation.NotAnnotated)
                .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

            members.Add(new MemberDescriptor(wireName, prop.Name, kind, scalarType, typeName, nullable, order, attrLoc));
        }

        CheckDuplicateOrdinals(members, diagnostics);
        members.Sort(static (a, b) => a.Order.CompareTo(b.Order));
        CheckCapability(formats, members, diagnostics);

        var ns = typeSymbol.ContainingNamespace is { IsGlobalNamespace: false } nsym
            ? nsym.ToDisplayString()
            : string.Empty;

        TypeDescriptor? descriptor = diagnostics.Count == 0
            ? new TypeDescriptor(ns, typeSymbol.Name, formats, members)
            : null;

        return new AnalysisResult(descriptor, diagnostics);
    }

    private static void CheckDuplicateOrdinals(List<MemberDescriptor> members, List<Diagnostic> diagnostics)
    {
        var seen = new Dictionary<int, string>();
        foreach (var m in members)
        {
            if (seen.TryGetValue(m.Order, out var first))
                diagnostics.Add(Diagnostic.Create(Diagnostics.DuplicateOrdinal, m.Location, first, m.Name, m.Order));
            else
                seen[m.Order] = m.Name;
        }
    }

    private static void CheckCapability(SerializationFormat formats, List<MemberDescriptor> members, List<Diagnostic> diagnostics)
    {
        if ((formats & SerializationFormat.Csv) == 0)
            return;

        foreach (var m in members)
        {
            if (m.Kind == MemberKind.Nested)
                diagnostics.Add(Diagnostic.Create(Diagnostics.NestedMemberNotRepresentableInCsv, m.Location, m.Name));
        }
    }

    private static ScalarType? ClassifyScalar(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { IsGenericType: true } named &&
            named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            type = named.TypeArguments[0];

        return type.SpecialType switch
        {
            SpecialType.System_Boolean => ScalarType.Bool,
            SpecialType.System_Int32   => ScalarType.Int,
            SpecialType.System_Int64   => ScalarType.Long,
            SpecialType.System_Double  => ScalarType.Double,
            SpecialType.System_Decimal => ScalarType.Decimal,
            SpecialType.System_String  => ScalarType.String,
            _                          => null,
        };
    }

    private static bool IsNullable(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol { IsGenericType: true } named &&
            named.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
            return true;

        return type.NullableAnnotation == NullableAnnotation.Annotated;
    }
}

internal sealed class AnalysisResult
{
    internal AnalysisResult(TypeDescriptor? descriptor, IReadOnlyList<Diagnostic> diagnostics)
    {
        Descriptor = descriptor;
        Diagnostics = diagnostics;
    }

    internal TypeDescriptor? Descriptor { get; }
    internal IReadOnlyList<Diagnostic> Diagnostics { get; }
}
