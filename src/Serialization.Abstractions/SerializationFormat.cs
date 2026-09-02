using System;

namespace Serialization.Abstractions
{
    [Flags]
    public enum SerializationFormat
    {
        Json = 1,
        Csv  = 2,
    }
}
