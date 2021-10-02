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
            int lastDiff = int.MinValue;
            for(int i = 0; i < minLength; ++i)
            {
                byte originalValue = original.ReadByte(i);
                byte modifiedValue = modified.ReadByte(i);
                if(originalValue != modifiedValue)
                {
                    // New block diff
                    if(i != lastDiff + 1)
                    {
                        ret.BlockDiffs.Add(new DiffData.BlockDiff(i, originalValue, modifiedValue));
                    }
                    else
                    {
                        ret.CurrBlock.AddValue(originalValue, modifiedValue);
                    }
                    lastDiff = i;
                    ret.ByteDiffs.Add(new DiffData.ByteDiff(i, originalValue, modifiedValue));
                }
            }
            return ret;
        }
        public class DiffData
        {
            public List<string> Readout()
            {
                // Readout blocks
                var blockDiffText = BlockDiffs.SelectMany(d => d.Readout());
                // Initialize list with proper capacity
                var ret = new List<string>(blockDiffText.Count() + 1);
                // Add diff data
                ret.Add("Block Diffs:");
                ret.AddRange(blockDiffText);
                return ret;
            }
            public List<ByteDiff> ByteDiffs { get; } = new List<ByteDiff>();
            public BlockDiff CurrBlock => BlockDiffs.Count > 0 ? BlockDiffs[BlockDiffs.Count - 1] : null;
            public List<BlockDiff> BlockDiffs { get; } = new List<BlockDiff>();
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
                    return $"{offset:x2}: {originalValue:x2} -> {changedValue:x2}";
                }
            }

            public class BlockDiff
            {
                public int offset;
                public List<(byte, byte)> values = new List<(byte, byte)>();

                public BlockDiff(int offset, byte originalFirstValue, byte changedFirstValue)
                {
                    this.offset = offset;
                    AddValue(originalFirstValue, changedFirstValue);
                }

                public void AddValue(byte original, byte changed)
                {
                    values.Add((original, changed));
                }

                public List<string> Readout()
                {
                    var ret = new List<string>(values.Count + 2);
                    ret.Add($"// Block: {offset:x2} - {offset + values.Count - 1:x2} ({values.Count} bytes)");
                    ret.AddRange(values.Select(v => $"{v.Item1:x2} -> {v.Item2:x2}"));
                    ret.Add(" ");
                    return ret;
                }
            }
        }
    }
}
