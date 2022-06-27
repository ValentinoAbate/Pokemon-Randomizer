using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.GenIII.Constants.AttributeNames;
using PokemonRandomizer.Backend.GenIII.Constants.ElementNames;
using PokemonRandomizer.Backend.Utilities;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Reading
{
    public abstract class RomParser
    {
        public abstract RomData Parse(Rom rom, RomMetadata metadata, XmlManager info);

        // Read the attacks starting at offset (returns the index after completion)
        protected int ReadLearnSet(Rom rom, int offset, out LearnSet moves)
        {
            moves = new LearnSet
            {
                OriginalOffset = offset
            };
            rom.Seek(offset);
            byte curr = rom.ReadByte();
            byte next = rom.ReadByte();
            while (curr != 0xFF || next != 0xFF)
            {
                // lvl is in the lvl byte but divided by 2 (lose the last bit)
                int lvl = next >> 1;
                // if the move number is over 255, the last bit of the learn level byte is set to 1
                Move move = InternalIndexToMove((next % 2) * 256 + curr);
                moves.Add(move, lvl);
                curr = rom.ReadByte();
                next = rom.ReadByte();
            }
            moves.SetOriginalCount();
            offset = rom.InternalOffset;
            rom.LoadOffset();
            return offset;
        }

        protected void ReadEggMoves(Rom rom, int offset, XmlManager info, List<PokemonBaseStats> pokemon)
        {
            rom.SaveAndSeekOffset(offset);
            int pkmnSigniture = info.HexAttr(ElementNames.eggMoves, AttributeNames.eggMovePokemonSigniture);
            var moves = new Dictionary<Pokemon, List<Move>>(pokemon.Count);
            var pkmn = Pokemon.None;
            int counter = 0;
            // Limit on loop just in case we are at the wrong place
            while (++counter < 3000)
            {
                int number = rom.ReadUInt16();
                if (number > pkmnSigniture + 1000 || number < 0)
                    break;
                if (number >= pkmnSigniture)
                {
                    pkmn = InternalIndexToPokemon(number - pkmnSigniture);
                    if (pkmn > Pokemon.None)
                        moves.Add(pkmn, new List<Move>());
                }
                else if (pkmn != Pokemon.None)
                {
                    moves[pkmn].Add(InternalIndexToMove(number));
                }
            }
            rom.LoadOffset();
            // Link Egg Moves
            foreach (var p in pokemon)
            {
                p.eggMoves = moves.ContainsKey(p.species) ? moves[p.species] : new List<Move>(0);
            }
        }

        // Read evolutions
        protected void ReadEvolutions(Rom rom, int offset, int numEvolutions, int paddingSize, out Evolution[] evolutions)
        {
            evolutions = new Evolution[numEvolutions];
            rom.SaveAndSeekOffset(offset);
            for (int i = 0; i < evolutions.Length; ++i)
            {
                var evoType = (EvolutionType)rom.ReadUInt16();
                int param = rom.ReadUInt16();
                var evo = evolutions[i] = new Evolution(evoType, InternalIndexToPokemon(rom.ReadUInt16()));
                if (evo.EvolvesWithItem)
                {
                    evo.ItemParamater = InternalIndexToItem(param);
                }
                else if (evo.EvolvesWithMove)
                {
                    evo.MoveParameter = InternalIndexToMove(param);
                }
                else if (evo.EvolvesWithPokemon)
                {
                    evo.PokemonParameter = InternalIndexToPokemon(param);
                }
                else
                {
                    evo.IntParameter = param;
                }
                rom.Skip(paddingSize);
            }
            rom.LoadOffset();
        }

        protected abstract Pokemon InternalIndexToPokemon(int internalIndex);
        protected abstract Item InternalIndexToItem(int internalIndex);
        protected static Move InternalIndexToMove(int internalIndex) => (Move)internalIndex;
    }
}
