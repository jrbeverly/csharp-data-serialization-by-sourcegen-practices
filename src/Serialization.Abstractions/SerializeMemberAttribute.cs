using System;

namespace Serialization.Abstractions
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SerializeMemberAttribute : Attribute
    {
        public string Name { get; }
        public int Order { get; }

        public SerializeMemberAttribute(string name, int order)
        {
            Name  = name;
            Order = order;
        }
    }
}
