﻿using PokemonRandomizer.Backend.EnumTypes;
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
            LevelUpWithPersonality1,    // 0x000B - Evolves by level up, but only when the personality value permits(Wurmple → Silcoon evolution)
            LevelUpWithPersonality2,    // 0x000C - Evolves by level up, but only when the personality value permits(Wurmple → Cascoon evolution)
            LevelUpButMaySpawn,         // 0x000D - Evolves by level up, but may spawn another Pokémon if permitted(Nincada → Ninjask evolution)
            LevelUpConditionalSpawn,    // 0x000E - Evolves by level up, but is only spawned if the conditions permit(Nincada → Shedinja evolution)
            Beauty,                     // 0x000F - Evolves by beauty
        }
    // Represents a single evolution definition
    // Each pokemon has five of these (most of which are usually empty)
    // https://bulbapedia.bulbagarden.net/wiki/Pok%C3%A9mon_evolution_data_structure_in_Generation_III
    public class Evolution
    {
        public bool IsRealEvolution => Type != EvolutionType.None;
        public EvolutionType Type { get; set; }
        // The parameter to the evolution function (dependend on evolution type)
        // For LevelUp evoltion types, the parameter is the evolution level
        // For Item-related evolution types, the parameter is the Item needed
        // For the Beauty type, the parameter is the beauty threshold (0-255)
        // On any other types the parameter does not apply
        public int parameter;
        public Pokemon Pokemon { get; set; }
        public bool EvolvesByLevel { get => Type.ToString().Contains("Level"); }
        public bool EvolvesByTrade { get => Type == EvolutionType.Trade || Type == EvolutionType.TradeWithItem; }
        public bool EvolvesByFriendship { get => Type == EvolutionType.Friendship || Type == EvolutionType.FriendshipDay || Type == EvolutionType.FriendshipNight; }
        public Evolution(EvolutionType type, int parameter, Pokemon pokemon)
        {
            Type = type;
            Pokemon = pokemon;
            this.parameter = parameter;
        }
        public Evolution(Evolution toCopy)
        {
            Type = toCopy.Type;
            Pokemon = toCopy.Pokemon;
            parameter = toCopy.parameter;
        }
        public override string ToString()
        {
            string ret = $"{Pokemon.ToDisplayString()}: {Type.ToDisplayString()}";
            if (EvolvesByLevel)
            {
                return ret + $" (Level {parameter})";
            }
            else if(Type == EvolutionType.UseItem || Type == EvolutionType.TradeWithItem)
            {
                return ret + $" ({((Item)parameter).ToDisplayString()})";
            }
            else if(Type == EvolutionType.Beauty)
            {
                return ret + $" ({parameter})";
            }
            return ret;
        }
    }
}
