using paracobNET;
using System;

namespace Sm5sh.PrcParser
{
    [AttributeUsage(AttributeTargets.Property)]
    public class PrcHexMapping : Attribute
    {
        public ulong Value { get; private set; }

        public PrcHexMapping(ulong hex)
        {
            Value = hex;
        }

        public PrcHexMapping(string strValue)
        {
            Value = Hash40Util.StringToHash40(strValue);
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
}
