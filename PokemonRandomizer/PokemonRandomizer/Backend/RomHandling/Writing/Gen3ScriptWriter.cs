using System;

namespace PokemonRandomizer.Backend.RomHandling.Writing
{
    using DataStructures;
    using DataStructures.Scripts;
    using EnumTypes;
    using Scripting.GenIII;
    public class Gen3ScriptWriter
    {
        public Settings Settings { get; set; }
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
            rom.SaveAndSeekOffset(offset);
            for (int i = 0; i < script.Count; ++i)
            {
                var command = script[i];
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
                    case CallIfCommand callIf:
                        rom.WriteByte(Gen3Command.callif);
                        rom.WriteByte(callIf.condition);
                        rom.WritePointer(callIf.offset);
                        Write(callIf.script, rom, callIf.offset, metadata);
                        break;
                    case CallCommand call:
                        rom.WriteByte(Gen3Command.call);
                        rom.WritePointer(call.offset);
                        Write(call.script, rom, call.offset, metadata);
                        break;
                    case GivePokedexCommand givePokedex:
                        WriteGivePokedexCommand(rom, givePokedex, metadata);
                        break;
                    case MessageBoxCommand messageBoxCommand:
                        WriteMessageBoxCommand(rom, messageBoxCommand);
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
                    case SetWildBattleCommand setWildBattle:
                        WriteSetWildBattleCommand(rom, setWildBattle, metadata);
                        break;
                    case CryCommand cryCommand:
                        rom.WriteByte(Gen3Command.cry);
                        rom.WriteUInt16((int)cryCommand.Pokemon);
                        rom.WriteUInt16(cryCommand.effect);
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
                        rom.WriteByte(Gen3Command.setberrytreerse);
                        rom.WriteByte(setBerryTreeCommand.treeId);
                        rom.WriteByte((byte)(setBerryTreeCommand.berry + 1 - Item.Cheri_Berry));
                        rom.WriteByte(setBerryTreeCommand.growthStage);
                        break;
                    case SetWeatherCommand setWeatherCommand:
                        rom.WriteByte(Gen3Command.setweather);
                        rom.WriteUInt16((int)setWeatherCommand.Weather);
                        break;
                    case GiveMoneyCommand giveMoneyCommand:
                        WriteMoneyCommand(rom, Gen3Command.givemoney, giveMoneyCommand);
                        break;
                    case PayMoneyCommand payMoneyCommand:
                        WriteMoneyCommand(rom, Gen3Command.paymoney, payMoneyCommand);
                        break;
                    case CheckMoneyCommand checkMoneyCommand:
                        WriteMoneyCommand(rom, Gen3Command.checkmoney, checkMoneyCommand);
                        break;
                    case Gen3Command gen3Command:
                        if((i + 1) < script.Count && TryApplyEventPokemonFix(rom, gen3Command,script[i + 1], metadata))
                        {
                            ++i; // Skip next command
                            break;
                        }
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

        private bool TryApplyEventPokemonFix(Rom rom, Gen3Command command1, Command command2, RomMetadata metadata)
        {
            // Should we attempt to apply the hack?
            if(Settings == null || !Settings.EnableMysteyGiftEvents)
            {
                return false;
            }
            // Is the first command checkflag on an event flag
            if(command1.code != Gen3Command.checkflag || !IsSpecialEventEnableFlag(command1.ArgData(0), metadata))
            {
                return false;
            }
            // Is the second command gotoif 0x00?
            if(command2 is not GotoIfCommand gotoIfCommand || gotoIfCommand.condition != 0x00)
            {
                return false;
            }
            // Do hack
            // Set flag instead of checking it
            rom.WriteByte(Gen3Command.setflag);
            rom.WriteUInt16(command1.ArgData(0));
            // Fill over goto w/ nops
            rom.WriteRepeating(0x00, 6);
            return true;
        }

        private bool IsSpecialEventEnableFlag(int flag, RomMetadata metadata)
        {
            if (metadata.IsEmerald)
            {
                return flag is Flags.Emerald.enableShipSouthernIsland or Flags.Emerald.enableShipBirthIsland 
                    or Flags.Emerald.enableShipFarawayIsland or Flags.Emerald.enableShipNavelRock;
            }
            else if (metadata.IsFireRedOrLeafGreen)
            {
                return flag is Flags.FRLG.enableShipBirthIsland or Flags.FRLG.enableShipNavelRock;
            }
            else // Ruby / Sapphire
            {
                return flag == Flags.RubySapphire.hasEonTicket;
            }
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

        private void WriteSetWildBattleCommand(Rom rom, SetWildBattleCommand command, RomMetadata metadata)
        {
            if (command.IsEventPokemon)
            {
                // Write give event pokemon multicommand
                rom.WriteByte(Gen3Command.setvar);
                rom.WriteUInt16(Gen3Command.eventPokemonSpeciesVar);
                rom.WriteUInt16((int)command.Pokemon);
                rom.WriteByte(Gen3Command.setvar);
                rom.WriteUInt16(Gen3Command.eventPokemonLevelVar);
                rom.WriteUInt16(command.Level);
                rom.WriteByte(Gen3Command.setvar);
                rom.WriteUInt16(Gen3Command.eventPokemonItemVar);
                rom.WriteUInt16((int)remapItem(command.HeldItem));
                rom.WriteByte(Gen3Command.special);
                if (metadata.IsEmerald)
                {
                    rom.WriteUInt16(Gen3Command.specialSetWildEventPokemonEmerald);
                }
                else
                {
                    rom.WriteUInt16(Gen3Command.specialSetWildEventPokemonFrlg);
                }
            }
            else
            {
                rom.WriteByte(Gen3Command.setwildbattle);
                rom.WriteUInt16((int)command.Pokemon);
                rom.WriteByte(command.Level);
                rom.WriteUInt16((int)remapItem(command.HeldItem));
            }
        }

        private void WriteMessageBoxCommand(Rom rom, MessageBoxCommand command)
        {
            // Write first command - loadpointer 0x00 <textOffset>
            rom.WriteByte(Gen3Command.loadpointer);
            rom.WriteByte(0x00);
            if(command.InputType == CommandInputType.Pointer)
            {
                if (command.Text.Length <= command.OriginalLength)
                {
                    rom.WritePointer(command.value);
                    //rom.WriteVariableLengthString(command.value, command.Text);
                }
                else // Text was expanded
                {
                    rom.WritePointer(rom.WriteInFreeSpace(rom.TranslateString(command.Text, true)) ?? command.value);
                }
            }
            else // just write the value
            {
                rom.WritePointer(command.value);
            }
            // Write second command
            rom.WriteByte(Gen3Command.callstd);
            rom.WriteByte((byte)command.specialCode);
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

        private void WriteMoneyCommand(Rom rom, byte commandCode, MoneyCommand command)
        {
            rom.WriteByte(commandCode);
            rom.WriteUInt32(command.Amount);
            rom.WriteByte((byte)command.Disable);
        }
    }
}
