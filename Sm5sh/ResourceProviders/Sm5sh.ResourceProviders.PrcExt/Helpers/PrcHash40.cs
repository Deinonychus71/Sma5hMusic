using System.Collections.Generic;

namespace Sm5sh.ResourceProviders.Prc.Helpers
{
    public class PrcHash40
    {
        public string StringValue { get; private set; }

        public ulong HexValue { get; set; }

        public override string ToString()
        {
            return $"{StringValue} - 0x{HexValue:X}";
        }

        public PrcHash40(ulong hexValue, Dictionary<ulong, string> paramHashes = null)
        {
            HexValue = hexValue;
            if(paramHashes != null)
                StringValue = paramHashes.ContainsKey(hexValue) ? paramHashes[hexValue] : $"0x{hexValue:x}";
        }

        public PrcHash40(string stringValue)
        {
            HexValue = paracobNET.Hash40Util.StringToHash40(stringValue);
            StringValue = stringValue;
        }

        public static PrcHash40 EmptyValue = new PrcHash40(0);
    }
}
