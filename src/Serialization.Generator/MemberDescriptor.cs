using Microsoft.CodeAnalysis;

namespace Serialization.Generator;

internal enum MemberKind { Scalar, Nested }

internal enum ScalarType { Bool, Int, Long, Double, Decimal, String }

internal sealed class MemberDescriptor
{
    internal MemberDescriptor(
        string name,
        string memberName,
        MemberKind kind,
        ScalarType? scalarType,
        string typeName,
        bool isNullable,
        int order,
        Location location)
    {
        Name = name;
        MemberName = memberName;
        Kind = kind;
        ScalarType = scalarType;
        TypeName = typeName;
        IsNullable = isNullable;
        Order = order;
        Location = location;
    }

    internal string Name { get; }
    internal string MemberName { get; }
    internal MemberKind Kind { get; }
    internal ScalarType? ScalarType { get; }
    internal string TypeName { get; }
    internal bool IsNullable { get; }
    internal int Order { get; }
    internal Location Location { get; }
}
