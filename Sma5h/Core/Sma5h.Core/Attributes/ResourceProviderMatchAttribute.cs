using System;

namespace Sma5h.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ResourceProviderMatchAttribute : Attribute
    {
        public string ExtensionOrPath { get; private set; }

        public ResourceProviderMatchAttribute(string extensionOrPath)
        {
            ExtensionOrPath = extensionOrPath;
        }
    }
}
