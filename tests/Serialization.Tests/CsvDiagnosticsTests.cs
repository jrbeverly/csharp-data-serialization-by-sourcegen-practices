using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Serialization.Generator;

namespace Serialization.Tests;

/// <summary>
/// Drives SerializationGeneratorCore over in-memory compilations to prove the
/// CSV capability restriction (SER001) without polluting the Sample.Domain
/// project with deliberately broken declarations.
/// </summary>
public class CsvDiagnosticsTests
{
    private const string AbstractionsSource = """
        using System;

        namespace Serialization.Abstractions
        {
            [Flags]
            public enum SerializationFormat
            {
                Json = 1,
                Csv = 2,
            }

            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
            public sealed class GenerateSerializersAttribute : Attribute
            {
                public GenerateSerializersAttribute(SerializationFormat formats)
                {
                }
            }

            [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
            public sealed class SerializeMemberAttribute : Attribute
            {
                public SerializeMemberAttribute(string name, int order)
                {
                }
            }
        }
        """;

    [Fact]
    public void Flat_type_targeting_csv_emits_the_serializer()
    {
        var source = """
            using Serialization.Abstractions;

            namespace TestLib
            {
                [GenerateSerializers(SerializationFormat.Csv)]
                public sealed class FlatRow
                {
                    [SerializeMember("name", 0)] public string Name { get; set; } = "";
                    [SerializeMember("count", 1)] public int Count { get; set; }
                }
            }
            """;

        var result = RunGenerator(AbstractionsSource, source);

        Assert.DoesNotContain(result.Diagnostics, d => d.Id == "SER001");
        var generated = Assert.Single(
            result.GeneratedTrees,
            t => t.FilePath.Contains("FlatRow.CsvSerializer.g.cs"));
        Assert.Contains("public static class FlatRowCsvSerializer", generated.ToString());
        Assert.Contains("\"name,count\"", generated.ToString());
        Assert.Contains("public static FlatRow Deserialize(string csv)", generated.ToString());
    }

    [Fact]
    public void Nested_member_targeting_csv_reports_SER001_and_emits_nothing()
    {
        var source = """
            using Serialization.Abstractions;

            namespace TestLib
            {
                public sealed class Inner
                {
                }

                [GenerateSerializers(SerializationFormat.Csv)]
                public sealed class BadRow
                {
                    [SerializeMember("name", 0)] public string Name { get; set; } = "";
                    [SerializeMember("location", 1)] public Inner Location { get; set; } = new Inner();
                }
            }
            """;

        var result = RunGenerator(AbstractionsSource, source);

        var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == "SER001");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(
            "SER001: Member 'location' is a nested record; CSV does not support nested members",
            $"{diagnostic.Id}: {diagnostic.GetMessage()}");
        Assert.DoesNotContain(result.GeneratedTrees, t => t.FilePath.Contains("BadRow"));
    }

    private static GeneratorDriverRunResult RunGenerator(string abstractionsSource, string testSource)
    {
        var syntaxTrees = new[]
        {
            CSharpSyntaxTree.ParseText(abstractionsSource),
            CSharpSyntaxTree.ParseText(testSource)
        };

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(
                Path.Combine(
                    Path.GetDirectoryName(typeof(object).Assembly.Location)!,
                    "System.Runtime.dll"))
        };

        var compilation = CSharpCompilation.Create(
            "TestAssembly",
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        GeneratorDriver driver = CSharpGeneratorDriver.Create(
            generators: [new SerializationGeneratorCore().AsSourceGenerator()]);

        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }
}
