using PokemonRandomizer.Backend.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonRandomizer.Backend.Utilities
{
    public static class RomDiff
    {
        public static DiffData Diff(Rom original, Rom modified)
        {
            return Diff(original.File, modified.File);
        }
        public static DiffData Diff(byte[] original, byte[] modified)
        {
            var ret = new DiffData()
            {
                OriginalLength = original.Length,
                ModifiedLength = modified.Length
            };
            int minLength = Math.Min(original.Length, modified.Length);
            int lastDiff = int.MinValue;
            for (int i = 0; i < minLength; ++i)
            {
                byte originalValue = original[i];
                byte modifiedValue = modified[i];
                if (originalValue != modifiedValue)
                {
                    // New block diff
                    if (i != lastDiff + 1)
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
            public int OriginalLength { get; set; }
            public int ModifiedLength { get; set; }
            public IEnumerable<string> Readout()
            {
                // Readout blocks
                var blockDiffText = BlockDiffs.SelectMany(d => d.Readout());
                // Add diff data
                if(OriginalLength != ModifiedLength)
                {
                    yield return $"Size Diff: {OriginalLength} -> {ModifiedLength} ({Math.Abs(OriginalLength - ModifiedLength)})";
                }
                if(BlockDiffs.Count == 0)
                {
                    yield return "No Data Diffs";
                }
                else
                {
                    yield return $"Block Diffs ({BlockDiffs.Count}):";
                    foreach(var blockDiff in BlockDiffs)
                    {
                        foreach(var line in blockDiff.Readout())
                        {
                            yield return line;
                        }
                    }
                }
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

                public IEnumerable<string> Readout()
                {
                    yield return $"// Block: {offset:x2} - {offset + values.Count - 1:x2} ({values.Count} bytes)";
                    foreach(var (item1, item2) in values)
                    {
                        yield return $"{item1:x2} -> {item2:x2}";
                    }
                    yield return string.Empty;
                }
            }
        }
    }
}
