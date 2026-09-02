using System;

namespace Serialization.Generator;

[Flags]
internal enum SerializationFormat
{
    Json = 1,
    Csv  = 2,
}
