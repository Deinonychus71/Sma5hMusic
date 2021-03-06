using paracobNET;
using System;

namespace Sma5h.ResourceProviders.Prc.Helpers
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrcHexMapping : Attribute
    {
        public ulong Value { get; private set; }
        public bool IsHash40 { get; private set; }

        public PrcHexMapping(ulong hex, bool isHash40 = false)
        {
            Value = hex;
            IsHash40 = isHash40;
        }

        public PrcHexMapping(string strValue, bool isHash40 = false)
        {
            Value = Hash40Util.StringToHash40(strValue);
            IsHash40 = isHash40;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrcFilterMatch : Attribute
    {
        public string Regex { get; private set; }

        public PrcFilterMatch(string regex)
        {
            Regex = regex;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class PrcIgnore : Attribute { }

    public class PrcDictionary : Attribute
    {
        public string Key { get; private set; }

        public PrcDictionary(string key)
        {
            Key = key;
        }
    }
}
