using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonEmeraldRandomizer.Backend
{
    public enum TrainerClass
    {
        pkmn_trainer1,
        pkmn_trainer2,
        hiker,
        team_aqua,
        breeder,
        cooltrainer_male,
        bird_keeper,
        collector,
        swimmer_male,
        team_magma,
        expert,
        aqua_admin,
        black_belt,
        aqua_leader,
        hex_maniac,
        aroma_lady,
        ruin_maniac,
        reporter,
        tuber_female,
        tuber_male,
        lady,
        beauty,
        rich_boy,
        pokemaniac,
        guitarist,
        kindler,
        camper,
        picknicker,
        bug_maniac,
        psychic,
        gentleman,
        elite_four,
        leader,
        school_kid,
        sr_and_jr,
        winstrate,
        pokefan,
        youngster,
        fisherman,
        triathlete,
        dragon_tamer,
        ninja_boy,
        battle_girl,
        parasol_lady,
        swimmer_female,
        twins,
        sailor,
        cooltrainer_female,
        magma_admin,
        pkmn_trainer3,
        bug_catcher,
        pkmn_ranger,
        magma_leader,
        lass,
        young_couple,
        old_couple,
        sis_and_bro,
        salon_maiden,
        dome_ace,
        palace_maven,
        arena_tycoon,
        factory_head,
        pike_queen,
        pyramid_ace,
        pkmn_trainer4,
    }

    public class Trainer
    {
        public int offset;
        public TrainerPokemon.DataType dataType;
        public TrainerPokemon[] pokemon;
        public TrainerClass trainerClass;
        public string name;
        public Item[] useItems = new Item[4];

        public Trainer(byte[] rom, int offset)
        {
            dataType = (TrainerPokemon.DataType)rom[offset];
            trainerClass = (TrainerClass)rom[offset + 1];
            // What is in bytes 2-3?
            // TODO: Read name (I think bytes 4 - 15?)
            name = rom.readString(offset + 4);
            // Read items (bytes 16-24)
            for (int i = 0; i < 4; ++i)
                useItems[i] = (Item)rom.ReadWord(offset + 16 + (i * 2));
            // What is in bytes 25-31?
            int numPokemon = rom[offset + 32];
            // What is in bytes 33-35?
            int pokemonPtr = rom.ReadPointer(offset + 36);

            #region Read pokemon from pokemonPtr
            pokemon = new TrainerPokemon[numPokemon];
            // The pokemon data structures will be either 8 or 16 bits depending on the dataType of the trainer
            int pkmnDataBytes = (dataType == TrainerPokemon.DataType.Basic || dataType == TrainerPokemon.DataType.HeldItem) ? 8 : 16;
            for (int i = 0; i < numPokemon; ++i)
            {
                TrainerPokemon p = new TrainerPokemon();
                int ptr = pokemonPtr + (i * pkmnDataBytes);
                p.dataType = dataType;
                //TODO: find out what these AI flags mean
                //Reference: https://www.pokecommunity.com/showthread.php?t=333767
                p.AIFlags = new BitArray(new int[]{ rom.ReadWord(ptr) });
                p.level = rom.ReadWord(ptr + 2);
                p.species = (PokemonSpecies)rom.ReadWord(ptr + 4);
                if (dataType == TrainerPokemon.DataType.HeldItem || dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                    p.heldItem = (Item)rom.ReadWord(ptr + 6);
                if(dataType == TrainerPokemon.DataType.SpecialMoves || dataType == TrainerPokemon.DataType.SpecialMovesAndHeldItem)
                {
                    int moveStartAddy = dataType == TrainerPokemon.DataType.SpecialMoves ? 6 : 8;
                    for (int j = 0; j < 4; ++j)
                        p.moves[j] = (Move)rom.ReadWord(ptr + moveStartAddy + (j * 2));
                }
                pokemon[i] = p;
            }
            #endregion
        }

        //Commented code below from Dabomstew's Universal Randomizer source. Thanks Dabomstew!

        //TrainerTable=310030
        //NumberOfTrainers=854
        //TrainerClasses=30FCD4
        //NumberOfTrainerClasses = 66
        //TrainerImageTable=305654
        //NumberOfTrainerImages=92
        //TrainerPaletteTable=30593C
        //TrainerMoneyTable = 31AEB8

        //public List<String> getTrainerClassNames()
        //{
        //    int baseOffset = romEntry.getValue("TrainerClassNames");
        //    int amount = romEntry.getValue("TrainerClassCount");
        //    int length = romEntry.getValue("TrainerClassNameLength");
        //    List<String> trainerClasses = new ArrayList<String>();
        //    for (int i = 0; i < amount; i++)
        //    {
        //        trainerClasses.add(readVariableLengthString(baseOffset + i * length));
        //    }
        //    return trainerClasses;
        //}

        //        TrainerData=0x310030
        //TrainerEntrySize=40
        //TrainerCount=0x357
        //TrainerClassNames=0x30FCD4
        //TrainerClassCount=66
        //TrainerClassNameLength=13
        //TrainerNameLength=12
        //DoublesTrainerClasses=[34, 46, 55, 56, 57]


        // Gym Trainers
        //tag(trs, "GYM1", 0x140, 0x141, 0x23B);
        //tag(trs, "GYM2", 0x1AA, 0x1A9, 0xB3, 0x23C, 0x23D, 0x23E);
        //tag(trs, "GYM3", 0xBF, 0x143, 0xC2, 0x289, 0x322);
        //tag(trs, "GYM4", 0x288, 0xC9, 0xCB, 0x28A, 0xCA, 0xCC, 0x1F5, 0xCD);
        //tag(trs, "GYM5", 0x47, 0x59, 0x49, 0x5A, 0x48, 0x5B, 0x4A);
        //tag(trs, "GYM6", 0x192, 0x28F, 0x191, 0x28E, 0x194, 0x323);
        //tag(trs, "GYM7", 0xE9, 0xEA, 0xEB, 0xF4, 0xF5, 0xF6, 0x24F, 0x248, 0x247, 0x249, 0x246, 0x23F);
        //tag(trs, "GYM8", 0x265, 0x80, 0x1F6, 0x73, 0x81, 0x76, 0x82, 0x12D, 0x83, 0x266);

        //// Gym Leaders + Emerald Rematches!
        //tag(trs, "GYM1", 0x109, 0x302, 0x303, 0x304, 0x305);
        //tag(trs, "GYM2", 0x10A, 0x306, 0x307, 0x308, 0x309);
        //tag(trs, "GYM3", 0x10B, 0x30A, 0x30B, 0x30C, 0x30D);
        //tag(trs, "GYM4", 0x10C, 0x30E, 0x30F, 0x310, 0x311);
        //tag(trs, "GYM5", 0x10D, 0x312, 0x313, 0x314, 0x315);
        //tag(trs, "GYM6", 0x10E, 0x316, 0x317, 0x318, 0x319);
        //tag(trs, "GYM7", 0x10F, 0x31A, 0x31B, 0x31C, 0x31D);
        //tag(trs, "GYM8", 0x110, 0x31E, 0x31F, 0x320, 0x321);

        //// Elite 4
        //tag(trs, 0x105, "ELITE1");
        //tag(trs, 0x106, "ELITE2");
        //tag(trs, 0x107, "ELITE3");
        //tag(trs, 0x108, "ELITE4");
        //tag(trs, 0x14F, "CHAMPION");

        //// Brendan
        //tag(trs, 0x208, "RIVAL1-2");
        //tag(trs, 0x20B, "RIVAL1-0");
        //tag(trs, 0x20E, "RIVAL1-1");

        //tag(trs, 0x251, "RIVAL2-2");
        //tag(trs, 0x250, "RIVAL2-0");
        //tag(trs, 0x257, "RIVAL2-1");

        //tag(trs, 0x209, "RIVAL3-2");
        //tag(trs, 0x20C, "RIVAL3-0");
        //tag(trs, 0x20F, "RIVAL3-1");

        //tag(trs, 0x20A, "RIVAL4-2");
        //tag(trs, 0x20D, "RIVAL4-0");
        //tag(trs, 0x210, "RIVAL4-1");

        //tag(trs, 0x295, "RIVAL5-2");
        //tag(trs, 0x296, "RIVAL5-0");
        //tag(trs, 0x297, "RIVAL5-1");

        //// May
        //tag(trs, 0x211, "RIVAL1-2");
        //tag(trs, 0x214, "RIVAL1-0");
        //tag(trs, 0x217, "RIVAL1-1");

        //tag(trs, 0x258, "RIVAL2-2");
        //tag(trs, 0x300, "RIVAL2-0");
        //tag(trs, 0x301, "RIVAL2-1");

        //tag(trs, 0x212, "RIVAL3-2");
        //tag(trs, 0x215, "RIVAL3-0");
        //tag(trs, 0x218, "RIVAL3-1");

        //tag(trs, 0x213, "RIVAL4-2");
        //tag(trs, 0x216, "RIVAL4-0");
        //tag(trs, 0x219, "RIVAL4-1");

        //tag(trs, 0x298, "RIVAL5-2");
        //tag(trs, 0x299, "RIVAL5-0");
        //tag(trs, 0x29A, "RIVAL5-1");

        //// Themed
        //tag(trs, "THEMED:MAXIE", 0x259, 0x25A, 0x2DE);
        //tag(trs, "THEMED:TABITHA", 0x202, 0x255, 0x2DC);
        //tag(trs, "THEMED:ARCHIE", 0x22);
        //tag(trs, "THEMED:MATT", 0x1E);
        //tag(trs, "THEMED:SHELLY", 0x20, 0x21);

        //// Steven
        //tag(trs, 0x324, "UBER");
    }
}
