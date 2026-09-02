using Microsoft.CodeAnalysis;

namespace Serialization.Generator;

internal static class Diagnostics
{
    private const string Category = "Serialization";

    internal static readonly DiagnosticDescriptor NestedMemberNotRepresentableInCsv = new(
        id: "SER001",
        title: "Nested member incompatible with CSV",
        messageFormat: "Member '{0}' is a nested record; CSV does not support nested members",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor DuplicateOrdinal = new(
        id: "SER002",
        title: "Duplicate ordinal",
        messageFormat: "Members '{0}' and '{1}' share ordinal {2}; ordinals must be unique within a type",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    internal static readonly DiagnosticDescriptor MissingSerializeMemberAttribute = new(
        id: "SER003",
        title: "Missing [SerializeMember] attribute",
        messageFormat: "Property '{0}' on '{1}' is opted into serialization but has no [SerializeMember]",
        category: Category,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
