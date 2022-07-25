using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer.Backend.DataStructures
{
    public enum EvolutionType
    {
            None,                       // 0x0000 - No Evolution in this slot
            Friendship,                 // 0x0001 - Evolves by friendship without regards to time of day
            FriendshipDay,              // 0x0002 - Evolves by friendship during the day
            FriendshipNight,            // 0x0003 - Evolves by friendship at night
            LevelUp,                    // 0x0004 - Evolves by level up
            Trade,                      // 0x0005 - Evolves by trade without items
            TradeWithItem,              // 0x0006 - Evolves by trading the Pokémon while holding an item
            UseItem,                    // 0x0007 - Evolves by using an item on the Pokémon
            LevelUpWithDefLessThanAtk,  // 0x0008 - Evolves by level up, but only when Attack is greater than Defense
            LevelUpWithAtkEqualToDef,   // 0x0009 - Evolves by level up, but only when Attack is equal to Defense
            LevelUpWithAtkLessThanDef,  // 0x000A - Evolves by level up, but only when Attack is lower than Defense
            LevelUpWithPersonality1,    // 0x000B - Evolves by level up, but only when the personality value permits (Wurmple → Silcoon evolution)
            LevelUpWithPersonality2,    // 0x000C - Evolves by level up, but only when the personality value permits (Wurmple → Cascoon evolution)
            LevelUpButMaySpawn,         // 0x000D - Evolves by level up, but may spawn another Pokémon if permitted (Nincada → Ninjask evolution)
            LevelUpConditionalSpawn,    // 0x000E - Evolves by level up, but is only spawned if the conditions permit (Nincada → Shedinja evolution)
            Beauty,                     // 0x000F - Evolves by beauty
            UseItemMale,                // 0x0010 - Evolves by using an item on the Pokémon, but only if it is Male
            UseItemFemale,              // 0x0011 - Evolves by using an item on the Pokémon, but only if it is Female
            LevelUpWithItemDay,         // 0x0012 - Evolves by level up, but only while holding a specific item during the day
            LevelUpWithItemNight,       // 0x0013 - Evolves by level up, but only while holding a specific item during the night
            LevelUpWithMove,            // 0x0014 - Evolves by level up, but only when the pokemon knows a certain move
            LevelUpWithPokemonInParty,  // 0x0015 - Evolves by level up, but only when a specific pokmon is in the party (mantyke -> mantine w/ remoraid)
            LevelUpMale,                // 0x0016 - Evolves by level up, but only if it is Male
            LevelUpFemale,              // 0x0017 - Evolves by level up, but only if it is Female
            LevelUpAtMagneticField,     // 0x0018 - Evolves by level up, but only at a special magnetic field (Mt Coronet)
            LevelUpAtMossyRock,         // 0x0019 - Evolves by level up, but only at a Mossy Rock (Eterna Forest)
            LevelUpAtIcyRock,           // 0x001A - Evolves by level up, but only at an Icy Rock (Route 217)
    }
    // Represents a single evolution definition
    // Each pokemon has five of these (most of which are usually empty)
    // https://bulbapedia.bulbagarden.net/wiki/Pok%C3%A9mon_evolution_data_structure_in_Generation_III
    public class Evolution
    {
        public bool IsRealEvolution => Type != EvolutionType.None;
        public EvolutionType Type { get; set; }
        public Item ItemParamater { get; set; }
        public Pokemon PokemonParameter { get; set; }
        public Move MoveParameter { get; set; }
        public int IntParameter { get; set; }
        public Pokemon Pokemon { get; set; }
        public bool EvolvesByLevel => Type is EvolutionType.LevelUp or EvolutionType.LevelUpButMaySpawn or EvolutionType.LevelUpConditionalSpawn or EvolutionType.LevelUpFemale or EvolutionType.LevelUpMale or EvolutionType.LevelUpWithAtkEqualToDef or EvolutionType.LevelUpWithAtkLessThanDef or EvolutionType.LevelUpWithDefLessThanAtk or EvolutionType.LevelUpWithPersonality1 or EvolutionType.LevelUpWithPersonality2;
        public bool EvolvesWithItem => Type is EvolutionType.UseItem or EvolutionType.TradeWithItem or EvolutionType.LevelUpWithItemDay or EvolutionType.LevelUpWithItemNight or EvolutionType.UseItemMale or EvolutionType.UseItemFemale;
        public bool EvolvesByTrade => Type is EvolutionType.Trade or EvolutionType.TradeWithItem;
        public bool EvolvesWithMove => Type is EvolutionType.LevelUpWithMove;
        public bool EvolvesWithPokemon => Type is EvolutionType.LevelUpWithPokemonInParty;
        public bool EvolvesByFriendship => Type is EvolutionType.Friendship or EvolutionType.FriendshipDay or EvolutionType.FriendshipNight;
        public Evolution(EvolutionType type, Pokemon pokemon)
        {
            Type = type;
            Pokemon = pokemon;
        }

        public Evolution(Evolution toCopy) : this(toCopy.Type, toCopy.Pokemon)
        {
            ItemParamater = toCopy.ItemParamater;
            MoveParameter = toCopy.MoveParameter;
            PokemonParameter = toCopy.PokemonParameter;
            IntParameter = toCopy.IntParameter;
        }

        public override string ToString()
        {
            string ret = $"{Pokemon.ToDisplayString()}: {Type.ToDisplayString()}";
            if (EvolvesByLevel)
            {
                return ret + $" (Level {IntParameter})";
            }
            else if(EvolvesWithItem)
            {
                return ret + $" ({ItemParamater.ToDisplayString()})";
            }
            else if(Type is EvolutionType.LevelUpWithMove)
            {
                return ret + $" ({MoveParameter.ToDisplayString()})";
            }
            else if(Type is EvolutionType.LevelUpWithPokemonInParty)
            {
                return ret + $" ({PokemonParameter.ToDisplayString()})";
            }
            else if(Type == EvolutionType.Beauty)
            {
                return ret + $" ({IntParameter})";
            }
            return ret;
        }
    }
}
