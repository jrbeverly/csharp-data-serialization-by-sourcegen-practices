using System.Collections.Generic;

namespace Serialization.Generator;

internal sealed class TypeDescriptor
{
    internal TypeDescriptor(
        string namespaceName,
        string typeName,
        SerializationFormat formats,
        IReadOnlyList<MemberDescriptor> members)
    {
        Namespace = namespaceName;
        TypeName = typeName;
        Formats = formats;
        Members = members;
    }

    internal string Namespace { get; }
    internal string TypeName { get; }
    internal SerializationFormat Formats { get; }
    internal IReadOnlyList<MemberDescriptor> Members { get; }
}
