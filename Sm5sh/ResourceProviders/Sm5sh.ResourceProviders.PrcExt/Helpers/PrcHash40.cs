using System;
using System.Collections.Generic;

namespace Sm5sh.ResourceProviders.Prc.Helpers
{
    public class PrcHash40
    {
        public string StringValue { get; private set; }

        public ulong HexValue { get; set; }

        public override string ToString()
        {
            return $"{StringValue} - 0x{HexValue:x}";
        }

        public PrcHash40(ulong hexValue, Dictionary<ulong, string> paramHashes = null)
        {
            HexValue = hexValue;
            if(paramHashes != null)
                StringValue = paramHashes.ContainsKey(hexValue) ? paramHashes[hexValue] : $"0x{hexValue:x}";
        }

        public PrcHash40(string stringValue)
        {
            if (stringValue.StartsWith("0x", System.StringComparison.OrdinalIgnoreCase))
            {
                HexValue = Convert.ToUInt64(stringValue, 16);
            }
            else
            {
                HexValue = paracobNET.Hash40Util.StringToHash40(stringValue);
            }
            StringValue = stringValue;
        }

        public static PrcHash40 EmptyValue = new PrcHash40(0);

        public override bool Equals(object obj)
        {
            if (!(obj is PrcHash40))
                return false;

            return ((PrcHash40)obj).HexValue == this.HexValue;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
