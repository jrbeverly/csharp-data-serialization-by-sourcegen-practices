using System;

namespace Serialization.Abstractions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class GenerateSerializersAttribute : Attribute
    {
        public SerializationFormat Formats { get; }

        public GenerateSerializersAttribute(SerializationFormat formats)
        {
            Formats = formats;
        }
    }
}
