using Microsoft.CodeAnalysis;

namespace Serialization.Generator;

internal interface ISerializationBackend
{
    SerializationFormat Format { get; }
    void Generate(TypeDescriptor descriptor, SourceProductionContext context);
}
