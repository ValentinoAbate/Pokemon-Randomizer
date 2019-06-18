using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public class HexUtils
    {
        /// <summary> Converts a hex string to an int. Assumes the string is prefixed with "0x"</summary>
        public static int HexToInt(string hex) => Convert.ToInt32(hex, 16);
        /// <summary> Converts a hex string to an array of bytes. Assumes the string is prefixed with "0x"</summary>
        public static byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                throw new Exception("Invalid hex string. Please use a string prefixed with 0x");
            if (hex.Length % 2 != 0)
                throw new Exception("The hex string cannot be an odd number of digits");
            var bytes = new byte[(hex.Length / 2) - 1];
            int j = 2;
            for(int i = 0; i < bytes.Length; ++i, j += 2)
                bytes[i] = (byte)((HexToByte(hex[j]) << 4) + HexToByte(hex[j + 1]));
            return bytes;
        }
        /// <summary> Converts a single hex character to am int </summary> 
        public static int HexToByte(char hex)
        {
            return (hex - (hex < 58 ? 48 : (hex < 97 ? 55 : 87)));
        }
    }
}
