using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Utilities
{
    using DataStructures;
    public static class RomDiff
    {
        public static DiffData Diff(Rom original, Rom modified)
        {
            var ret = new DiffData();
            int minLength = Math.Min(original.Length, modified.Length);
            for(int i = 0; i < minLength; ++i)
            {
                byte originalValue = original.ReadByte(i);
                byte modifiedValue = modified.ReadByte(i);
                if(originalValue != modifiedValue)
                {
                    ret.ByteDiffs.Add(new DiffData.ByteDiff(i, originalValue, modifiedValue));
                }
            }
            return ret;
        }
        public class DiffData
        {
            public List<string> Readout()
            {
                var ret = new List<string>(ByteDiffs.Count + 1);
                ret.Add("Byte Diffs:");
                ret.AddRange(ByteDiffs.Select(d => d.ToString()));
                return ret;
            }
            public List<ByteDiff> ByteDiffs { get; } = new List<ByteDiff>();
            public struct ByteDiff
            {
                public int offset;
                public byte originalValue;
                public byte changedValue;

                public ByteDiff(int offset, byte originalValue, byte changedValue)
                {
                    this.offset = offset;
                    this.originalValue = originalValue;
                    this.changedValue = changedValue;
                }

                public override string ToString()
                {
                    return offset.ToString("x2") + ": " + originalValue.ToString("x2") + " -> " + changedValue.ToString("x2");
                }
            }
        }
    }
}
