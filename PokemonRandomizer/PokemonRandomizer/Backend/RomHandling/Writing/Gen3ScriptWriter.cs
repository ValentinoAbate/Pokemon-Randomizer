using System;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    using DataStructures;
    using DataStructures.Scripts;
    using EnumTypes;
    using Scripting.GenIII;
    public class Gen3ScriptWriter
    {
        private readonly Func<Item, Item> remapItem;
        public Gen3ScriptWriter(Func<Item, Item> remapItem)
        {
            this.remapItem = remapItem;
        }
        public void Write(Script script, Rom rom, int offset, RomMetadata metadata)
        {
            if (script == null)
            {
                return;
            }
            rom.SaveOffset();
            rom.Seek(offset);
            foreach (var command in script)
            {
                switch (command)
                {
                    case GotoIfCommand gotoIf:
                        rom.WriteByte(Gen3Command.gotoif);
                        rom.WriteByte(gotoIf.condition);
                        rom.WritePointer(gotoIf.offset);
                        Write(gotoIf.script, rom, gotoIf.offset, metadata);
                        break;
                    case GotoCommand @goto:
                        rom.WriteByte(Gen3Command.@goto);
                        rom.WritePointer(@goto.offset);
                        Write(@goto.script, rom, @goto.offset, metadata);
                        break;
                    case GivePokedexCommand givePokedex:
                        WriteGivePokedexCommand(rom, givePokedex, metadata);
                        break;
                    case GiveItemCommand giveItem:
                        rom.WriteByte(Gen3Command.copyvarifnotzero);
                        rom.WriteUInt16(Gen3Command.itemTypeVar);
                        rom.WriteUInt16((int)remapItem(giveItem.Item));
                        rom.WriteByte(Gen3Command.copyvarifnotzero);
                        rom.WriteUInt16(Gen3Command.itemQuantityVar);
                        rom.WriteUInt16(giveItem.amount);
                        rom.WriteByte(Gen3Command.callstd);
                        rom.WriteByte((byte)giveItem.messageType);
                        break;
                    case CheckItemRoomCommand checkItemRoom:
                        rom.WriteByte(Gen3Command.checkitemroom);
                        rom.WriteUInt16((int)remapItem(checkItemRoom.Item));
                        rom.WriteUInt16(checkItemRoom.quantity);
                        break;
                    case TrainerBattleCommand trainerBattle:
                        WriteTrainerBattleCommand(rom, trainerBattle, metadata);
                        break;
                    case GivePokemonCommand givePokemon:
                        rom.WriteByte(Gen3Command.givePokemon);
                        rom.WriteUInt16((int)givePokemon.pokemon);
                        rom.WriteByte(givePokemon.level);
                        rom.WriteUInt16((int)remapItem(givePokemon.heldItem));
                        rom.SetBlock(9, 0x00);
                        break;
                    case GiveEggCommand giveEggCommand:
                        rom.WriteByte(Gen3Command.giveEgg);
                        rom.WriteUInt16((int)giveEggCommand.pokemon);
                        break;
                    case ShopCommand shopCommand:
                        WriteShopCommand(rom, shopCommand);
                        break;
                    case SetBerryTreeCommand setBerryTreeCommand:
                        rom.WriteByte(Gen3Command.setberrytree);
                        rom.WriteByte(setBerryTreeCommand.treeId);
                        rom.WriteByte((byte)(setBerryTreeCommand.berry + 1 - Item.Cheri_Berry));
                        rom.WriteByte(setBerryTreeCommand.growthStage);
                        break;
                    case SetWeatherCommand setWeatherCommand:
                        rom.WriteByte(Gen3Command.setweather);
                        rom.WriteUInt16((int)setWeatherCommand.weather);
                        break;
                    case Gen3Command gen3Command:
                        rom.WriteByte(gen3Command.code);
                        foreach (var arg in gen3Command.args)
                        {
                            switch (arg.type)
                            {
                                case Gen3Command.Arg.Byte:
                                    rom.WriteByte((byte)arg.data);
                                    break;
                                case Gen3Command.Arg.Word:
                                    rom.WriteUInt16(arg.data);
                                    break;
                                case Gen3Command.Arg.Long:
                                    rom.WriteUInt32(arg.data);
                                    break;
                                case Gen3Command.Arg.Pointer:
                                    rom.WritePointer(arg.data);
                                    break;
                            }
                        }
                        break;
                }
            }
            rom.LoadOffset();
        }

        private void WriteTrainerBattleCommand(Rom rom, TrainerBattleCommand battle, RomMetadata metadata)
        {
            rom.WriteByte(Gen3Command.trainerbattle);
            rom.WriteByte((byte)battle.trainerType);
            rom.WriteUInt16(battle.trainerIndex);
            rom.WriteUInt16(battle.unknown); // Blank word
            if (battle.trainerType == TrainerBattleCommand.Type.ContinueScriptAfterBattle)
            {
                rom.WritePointer(battle.defeatedTextOffset);
                return;
            }
            if (battle.trainerType == TrainerBattleCommand.Type.ProfessorOakTutorial)
            {
                rom.WritePointer(battle.defeatedTextOffset);
                rom.WritePointer(battle.winTextOffset);
                return;
            }
            // Standard Args 3 and 4
            rom.WritePointer(battle.encounterTextOffset);
            rom.WritePointer(battle.defeatedTextOffset);
            // Return if there are no extra arguments
            if (battle.trainerType == TrainerBattleCommand.Type.Standard || battle.trainerType == TrainerBattleCommand.Type.Rematch)
            {
                return;
            }
            // Post-battle script argument
            if (battle.trainerType == TrainerBattleCommand.Type.GymLeader || battle.trainerType == TrainerBattleCommand.Type.MatchCallRegister)
            {
                rom.WritePointer(battle.postBattleScriptOffset);
                Write(battle.postBattleScript, rom, battle.postBattleScriptOffset, metadata);
                return;
            }
            // Double battles
            rom.WritePointer(battle.onlyOnePokemonTextOffset);
            // Double battle with Post-Battle script argument
            if (battle.trainerType == TrainerBattleCommand.Type.DoubleBattleWithExtraScript || battle.trainerType == TrainerBattleCommand.Type.DoubleBattleGymLeader)
            {
                rom.WritePointer(battle.postBattleScriptOffset);
                Write(battle.postBattleScript, rom, battle.postBattleScriptOffset, metadata);
            }
        }

        private void WriteShopCommand(Rom rom, ShopCommand command)
        {
            rom.WriteByte(command.code);
            if (command.shop.items.Count <= command.shop.OriginalSize)
            {
                rom.WritePointer(command.shopOffset);
                // Write the shop contents
                rom.SaveAndSeekOffset(command.shopOffset);
                foreach (var item in command.shop.items)
                {
                    rom.WriteUInt16((int)remapItem(item));
                }
                rom.WriteUInt16((int)Item.None); // Add the terminator
                rom.LoadOffset();
            }
            else // shop has been expanded, repoint necessary
            {
                // Create shop data block
                var dataBlock = new Rom((command.shop.items.Count + 1) * 2, rom.FreeSpaceByte);
                foreach (var item in command.shop.items)
                {
                    dataBlock.WriteUInt16((int)remapItem(item));
                }
                dataBlock.WriteUInt16((int)Item.None);
                // Attempt to write shop data block in free space
                var newOffset = rom.WriteInFreeSpace(dataBlock.File);
                // If successful, write the new offset, else write the old one
                rom.WritePointer(newOffset ?? command.shopOffset);
            }
        }

        private void WriteGivePokedexCommand(Rom rom, GivePokedexCommand command, RomMetadata metadata)
        {
            rom.WriteByte(Gen3Command.special);
            if (metadata.IsFireRedOrLeafGreen)
            {
                rom.WriteUInt16(command.Type == GivePokedexCommand.PokedexType.National ? Gen3Command.specialGiveNationalDexFrlg : Gen3Command.specialGiveRegionalDexFrlg);
            }
            else if (metadata.IsEmerald)
            {
                // TODO: properly write emerald regional dex
                rom.WriteUInt16(command.Type == GivePokedexCommand.PokedexType.National ? Gen3Command.specialGiveNationalDexEmerald : Gen3Command.specialGiveRegionalDexFrlg);
            }

        }
    }
}
