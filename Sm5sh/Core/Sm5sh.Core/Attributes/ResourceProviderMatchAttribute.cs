using System;

namespace Sm5sh.Attributes
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
