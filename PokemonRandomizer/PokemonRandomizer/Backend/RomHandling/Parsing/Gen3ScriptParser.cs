using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.RomHandling.Parsing
{
    using DataStructures;
    using DataStructures.Scripts;
    using EnumTypes;
    using Utilities.Debug;
    using Scripting.GenIII;

    public class Gen3ScriptParser
    {
        public Dictionary<int, Script> scriptDatabase = new Dictionary<int, Script>();
        private readonly HashSet<int> visitedOffsets = new HashSet<int>(255);
        public Script Parse(Rom rom, int offset, RomMetadata metadata)
        {
            visitedOffsets.Clear();
            return Parse(rom, offset, offset, metadata);
        }

        private Script Parse(Rom rom, int offset, int originalOffset, RomMetadata metadata)
        {
            // If we have already been to this offset or we have already parsed this script
            if (visitedOffsets.Contains(offset) || scriptDatabase.ContainsKey(offset))
            {
                return null;
            }
            var script = new Script();
            scriptDatabase.Add(offset, script);
            rom.SaveOffset();
            rom.Seek(offset);
            while (true)
            {
                visitedOffsets.Add(rom.InternalOffset);
                var command = ReadCommand(rom, metadata, out bool knownCommand);
                if (!knownCommand)
                {
                    Logger.main.Info($"Unknown script command was detected in snippet at {offset:x2}, from script originating at {originalOffset:x2}");
                    foreach(var c in script)
                    {
                        Logger.main.Info(c.ToString());
                    }
                    break;
                }
                if (command.code is Gen3Command.end or Gen3Command.@return or Gen3Command.gotostd or Gen3Command.killscript or Gen3Command.jumpram)
                {
                    script.Add(command);
                    break;
                }
                else if (command.code == Gen3Command.callstd && command.ArgData(0) == CallStd.unknown28)
                {
                    script.Add(command);
                    break;
                }
                else if (command.code == Gen3Command.@goto)
                {
                    var gotoCommand = new GotoCommand()
                    {
                        offset = command.ArgData(0),
                    };
                    script.Add(gotoCommand);
                    gotoCommand.script = Parse(rom, gotoCommand.offset, originalOffset, metadata);
                    break;
                }
                else if (command.code == Gen3Command.gotoif)
                {
                    var gotoCommand = new GotoIfCommand()
                    {
                        condition = (byte)command.ArgData(0),
                        offset = command.ArgData(1),
                    };
                    script.Add(gotoCommand);
                    gotoCommand.script = Parse(rom, gotoCommand.offset, originalOffset, metadata);
                }
                else if (command.code == Gen3Command.call)
                {
                    var callCommand = new CallCommand()
                    {
                        offset = command.ArgData(0),
                    };
                    script.Add(callCommand);
                    callCommand.script = Parse(rom, callCommand.offset, originalOffset, metadata);
                }
                else if (command.code == Gen3Command.callif)
                {
                    var callCommand = new CallIfCommand()
                    {
                        condition = (byte)command.ArgData(0),
                        offset = command.ArgData(1),
                    };
                    script.Add(callCommand);
                    callCommand.script = Parse(rom, callCommand.offset, originalOffset, metadata);
                }
                else if (command.code == Gen3Command.special)
                {
                    script.Add(ParseSpecialCommand(command, metadata, script));
                }
                else if (command.code == Gen3Command.setvar)
                {
                    // Check for set wild event pokemon multi-command
                    if(TryParseSetEventWildBattleMultiCommand(rom, metadata, command, out SetWildBattleCommand setWildBattleCommand))
                    {
                        script.Add(setWildBattleCommand);
                    }
                    else
                    {
                        script.Add(command);
                    }
                }
                else if (command.code == Gen3Command.loadpointer)
                {
                    // Check for message box multi-command
                    if (TryParseMessageBoxMultiCommand(rom, metadata, command, out MessageBoxCommand messageBoxCommand))
                    {
                        script.Add(messageBoxCommand);
                    }
                    else
                    {
                        script.Add(command);
                    }
                }
                else if (command.code == Gen3Command.copyvarifnotzero)
                {
                    // Check for give item multi-command
                    if (TryParseGiveItemMultiCommand(rom, metadata, command, out GiveItemCommand giveItemCommand))
                    {
                        script.Add(giveItemCommand);
                    }
                    else
                    {
                        script.Add(command);
                    }
                }
                else if (command.code == Gen3Command.checkitemroom)
                {
                    var checkItemRoomCommand = new CheckItemRoomCommand()
                    {
                        Item = (Item)command.ArgData(0),
                        quantity = command.ArgData(1),
                    };
                    MarkCommandInputType(checkItemRoomCommand, command.ArgData(0));
                    script.Add(checkItemRoomCommand);
                }
                else if (command.code == Gen3Command.trainerbattle)
                {
                    script.Add(ParseTrainerBattleCommand(rom, command, originalOffset, metadata));
                }
                else if (command.code == Gen3Command.setwildbattle)
                {
                    var setWildBattleCommand = new SetWildBattleCommand
                    {
                        Pokemon = (Pokemon)command.ArgData(0),
                        Level = (byte)command.ArgData(1),
                        HeldItem = (Item)command.ArgData(2),
                    };
                    MarkCommandInputType(setWildBattleCommand, command.ArgData(0));
                    script.Add(setWildBattleCommand);
                }
                else if (command.code == Gen3Command.cry)
                {
                    var cryCommand = new CryCommand()
                    {
                        Pokemon = (Pokemon)command.ArgData(0),
                        effect = command.ArgData(1),
                    };
                    MarkCommandInputType(cryCommand, command.ArgData(0));
                    script.Add(cryCommand);
                }
                else if (command.code == Gen3Command.givePokemon)
                {
                    var givePokemonCommand = new GivePokemonCommand()
                    {
                        pokemon = (Pokemon)command.ArgData(0),
                        level = (byte)command.ArgData(1),
                        heldItem = (Item)command.ArgData(2)
                    };
                    MarkCommandInputType(givePokemonCommand, command.ArgData(0));
                    // Add new give pokemon command
                    script.Add(givePokemonCommand);
                }
                else if (command.code == Gen3Command.giveEgg)
                {
                    // Add new give egg event
                    script.Add(new GiveEggCommand() { pokemon = (Pokemon)command.ArgData(0) });
                }
                else if (command.code is Gen3Command.pokemart or Gen3Command.pokemart2 or Gen3Command.pokemart3)
                {
                    // Add new poke mart event
                    script.Add(ParseShopCommand(rom, command));
                }
                else if (command.code == Gen3Command.setberrytreerse)
                {
                    script.Add(ParseBerryTreeCommand(rom, command));
                }
                else if (command.code == Gen3Command.setweather)
                {
                    script.Add(new SetWeatherCommand() { Weather = (Map.Weather)command.ArgData(0) });
                }
                else if(command.code == Gen3Command.paymoney)
                {
                    script.Add(new PayMoneyCommand() { Amount = command.ArgData(0), Disable = command.ArgData(1) });
                }
                else if (command.code == Gen3Command.givemoney)
                {
                    script.Add(new GiveMoneyCommand() { Amount = command.ArgData(0), Disable = command.ArgData(1) });
                }
                else if (command.code == Gen3Command.checkmoney)
                {
                    script.Add(new CheckMoneyCommand() { Amount = command.ArgData(0), Disable = command.ArgData(1) });
                }
                else // Not a special code, just push the command
                {
                    script.Add(command);
                }
            }
            rom.LoadOffset();
            PostProcessScript(script);
            return script;
        }

        private void PostProcessScript(Script script)
        {
            for (int i = 0; i < script.Count; i++)
            {
                Command command = script[i];
                // Check for move tutor pattern
                if (i + 1 >= script.Count || command is not Gen3Command gen3Command)
                {
                    continue;
                }
                if (gen3Command.code == Gen3Command.setvar && gen3Command.ArgData(0) == Gen3Command.moveTutorIndexVar)
                {
                    var nextCommand = script[i + 1];
                    if(nextCommand is not CallCommand callCommand)
                    {
                        continue;
                    }
                    if(scriptDatabase.TryGetValue(callCommand.offset, out var calledScript) && calledScript.Metadata.HasFlag(Script.ScriptMetadata.IsMoveTutorChooseScript))
                    {
                        script.Metadata |= Script.ScriptMetadata.IsMoveTutorScript;
                        script.IntParam = gen3Command.ArgData(1);
                    }
                }
            }
        }

        private bool TryParseGiveItemMultiCommand(Rom rom, RomMetadata metadata, Gen3Command command1, out GiveItemCommand giveItemMultiCommand)
        {
            giveItemMultiCommand = null;
            // If the first command isn't setting variable 0x8000 or the next command is end return false
            if (command1.ArgData(0) != Gen3Command.itemTypeVar || rom.Peek() == Gen3Command.end)
                return false;
            rom.SaveOffset();
            var command2 = ReadCommand(rom, metadata, out _);
            // If the second command isn't copyvarifnotzero with a target of var 0x8001 return false
            if (command2.code != Gen3Command.copyvarifnotzero || command2.ArgData(0) != Gen3Command.itemQuantityVar)
            {
                rom.LoadOffset();
                return false;
            }
            var command3 = ReadCommand(rom, metadata, out _);
            // If the third command isn't callstd with an valid give item arg return false
            if (command3.code != Gen3Command.callstd || command3.ArgData(0) != CallStd.giveItemObtain && command3.ArgData(0) != CallStd.giveItemFind)
            {
                rom.LoadOffset();
                return false;
            }
            // Valid giveitem multicommand found
            giveItemMultiCommand = new GiveItemCommand()
            {
                Item = (Item)command1.ArgData(1),
                amount = command2.ArgData(1),
                messageType = (GiveItemCommand.MessageType)command3.ArgData(0),
            };
            MarkCommandInputType(giveItemMultiCommand, (int)giveItemMultiCommand.Item);
            rom.DumpOffset();
            return true;
        }

        private bool TryParseSetEventWildBattleMultiCommand(Rom rom, RomMetadata metadata, Gen3Command command1, out SetWildBattleCommand setWildEventBattleCommand)
        {
            setWildEventBattleCommand = null;
            // Event multicommand only in Emerald and FRLG
            if (!(metadata.IsEmerald || metadata.IsFireRedOrLeafGreen))
            {
                return false;
            }
            // If the first command isn't setting variable 0x8004 or the next command is end return false
            if (command1.ArgData(0) != Gen3Command.eventPokemonSpeciesVar || rom.Peek() == Gen3Command.end)
            {
                return false;
            }
            rom.SaveOffset();
            var command2 = ReadCommand(rom, metadata, out _);
            // If the second command isn't setvar with a target of var 0x8005 return false
            if (command2.code != Gen3Command.setvar || command2.ArgData(0) != Gen3Command.eventPokemonLevelVar)
            {
                rom.LoadOffset();
                return false;
            }
            var command3 = ReadCommand(rom, metadata, out _);
            // If the second command isn't setvar with a target of var 0x8006 return false
            if (command3.code != Gen3Command.setvar || command3.ArgData(0) != Gen3Command.eventPokemonItemVar)
            {
                rom.LoadOffset();
                return false;
            }
            var command4 = ReadCommand(rom, metadata, out _);
            var specialCode = metadata.IsEmerald ? Gen3Command.specialSetWildEventPokemonEmerald : Gen3Command.specialSetWildEventPokemonFrlg;
            if (command4.code != Gen3Command.special || command4.ArgData(0) != specialCode)
            {
                rom.LoadOffset();
                return false;
            }
            setWildEventBattleCommand = new SetWildBattleCommand
            {
                Pokemon = (Pokemon)command1.ArgData(1),
                Level = (byte)command2.ArgData(1),
                HeldItem = (Item)command3.ArgData(1),
                IsEventPokemon = true,
            };
            MarkCommandInputType(setWildEventBattleCommand, (int)setWildEventBattleCommand.Pokemon);
            rom.DumpOffset();
            return true;
        }

        private bool TryParseMessageBoxMultiCommand(Rom rom, RomMetadata metadata, Gen3Command command1, out MessageBoxCommand messageBoxCommand)
        {
            messageBoxCommand = null;
            if(command1.ArgData(0) != 0)
            {
                return false;
            }
            rom.SaveOffset();
            var command2 = ReadCommand(rom, metadata, out _);
            if (command2.code != Gen3Command.callstd || !CallStd.IsMsgBox((byte)command2.ArgData(0), metadata))
            {
                rom.LoadOffset();
                return false;
            }
            messageBoxCommand = new MessageBoxCommand
            {
                value = command1.ArgData(1),
                specialCode = command2.ArgData(0),
            };
            MarkCommandInputType(messageBoxCommand, messageBoxCommand.value);
            if(messageBoxCommand.InputType == CommandInputType.Pointer)
            {
                messageBoxCommand.Text = rom.ReadVariableLengthString(messageBoxCommand.value);
            }
            messageBoxCommand.SetOriginalValues();
            rom.DumpOffset();
            return true;
        }

        private void MarkCommandInputType(IHasCommandInputType command, int input)
        {
            // Mark command type
            if (input > 10000)
            {
                if(input > 32768)
                {
                    command.InputType = input > 50000 ? CommandInputType.Pointer : CommandInputType.Variable;
                }
                else
                {
                    command.InputType = CommandInputType.Unknown;
                }
                
            }
            else
            {
                command.InputType = CommandInputType.Normal;
            }
        }

        private TrainerBattleCommand ParseTrainerBattleCommand(Rom rom, Gen3Command command, int originalOffset, RomMetadata metadata)
        {
            var battle = new TrainerBattleCommand()
            {
                trainerType = (TrainerBattleCommand.Type)command.ArgData(0),
                trainerIndex = command.ArgData(1),
                unknown = command.ArgData(2),
            };
            if (battle.trainerType == TrainerBattleCommand.Type.ContinueScriptAfterBattle)
            {
                battle.defeatedTextOffset = command.ArgData(3);
                return battle;
            }
            if (battle.trainerType == TrainerBattleCommand.Type.ProfessorOakTutorial)
            {
                battle.defeatedTextOffset = command.ArgData(3);
                battle.winTextOffset = command.ArgData(4);
                return battle;
            }
            // Standard Args 3 and 4
            battle.encounterTextOffset = command.ArgData(3);
            battle.defeatedTextOffset = command.ArgData(4);
            // Return if there are no extra arguments
            if (battle.trainerType == TrainerBattleCommand.Type.Standard || battle.trainerType == TrainerBattleCommand.Type.Rematch)
            {
                return battle;
            }
            // Post-battle script argument
            if (battle.trainerType == TrainerBattleCommand.Type.GymLeader || battle.trainerType == TrainerBattleCommand.Type.MatchCallRegister)
            {
                battle.postBattleScriptOffset = command.ArgData(5);
                battle.postBattleScript = Parse(rom, battle.postBattleScriptOffset, originalOffset, metadata);
                return battle;
            }
            // Double battles
            battle.onlyOnePokemonTextOffset = command.ArgData(5);
            // Double battle with Post-Battle script argument
            if (battle.trainerType == TrainerBattleCommand.Type.DoubleBattleWithExtraScript || battle.trainerType == TrainerBattleCommand.Type.DoubleBattleGymLeader)
            {
                battle.postBattleScriptOffset = command.ArgData(6);
                battle.postBattleScript = Parse(rom, battle.postBattleScriptOffset, originalOffset, metadata);
            }
            return battle;
        }

        private SetBerryTreeCommand ParseBerryTreeCommand(Rom rom, Gen3Command command)
        {
            return new SetBerryTreeCommand()
            {
                treeId = (byte)command.ArgData(0),
                berry = (Item)(command.ArgData(1) + (int)Item.Cheri_Berry - 1),
                growthStage = (byte)command.ArgData(2)
            };
        }

        private ShopCommand ParseShopCommand(Rom rom, Gen3Command command)
        {
            var shopCommand = new ShopCommand()
            {
                code = command.code,
                shopOffset = command.ArgData(0)
            };
            rom.SaveAndSeekOffset(shopCommand.shopOffset);
            // Read items until we hit 0x00 0x00 (Item.None)
            var item = (Item)rom.ReadUInt16();
            while (item != Item.None)
            {
                shopCommand.shop.items.Add(item);
                item = (Item)rom.ReadUInt16();
            }
            // Mark the original size of the shop so it can be repointed if necessary
            shopCommand.shop.SetOriginalSize();
            rom.LoadOffset();
            return shopCommand;
        }

        private Command ParseSpecialCommand(Gen3Command command, RomMetadata metadata, Script script)
        {
            int specialCode = command.ArgData(0);
            if (metadata.IsFireRedOrLeafGreen)
            {
                if(specialCode == Gen3Command.specialSelectMonForMoveTutorFrlg)
                {
                    script.Metadata |= Script.ScriptMetadata.IsMoveTutorChooseScript;
                }
                return specialCode switch
                {
                    Gen3Command.specialGiveNationalDexFrlg => new GivePokedexCommand { Type = GivePokedexCommand.PokedexType.National },
                    Gen3Command.specialGiveRegionalDexFrlg => new GivePokedexCommand { Type = GivePokedexCommand.PokedexType.Regional },
                    _ => command,
                };
            }
            else if (metadata.IsEmerald)
            {
                if (specialCode == Gen3Command.specialSelectMonForMoveTutorEmerald)
                {
                    script.Metadata |= Script.ScriptMetadata.IsMoveTutorChooseScript;
                }
                return specialCode switch
                {
                    Gen3Command.specialGiveNationalDexEmerald => new GivePokedexCommand { Type = GivePokedexCommand.PokedexType.National },
                    //Gen3Command.specialGiveRegionalDexFrlg => new GivePokedexCommand { Type = GivePokedexCommand.PokedexType.Regional },
                    _ => command,
                };
            }
            return command;
        }

        /// <summary>
        /// Read a raw command from the ROM
        /// </summary>
        private Gen3Command ReadCommand(Rom rom, RomMetadata metadata, out bool success)
        {
            var command = new Gen3Command() { code = rom.ReadByte() };
            Gen3Command.Arg[] args = null;
            if (Gen3Command.commandMap.ContainsKey(command.code))
            {
                args = Gen3Command.commandMap[command.code];

            }
            else if(metadata.IsFireRedOrLeafGreen)
            {
                if (Gen3Command.frlgCommandMap.ContainsKey(command.code))
                {
                    args = Gen3Command.frlgCommandMap[command.code];
                }
            }
            else if (metadata.IsRubySapphireOrEmerald)
            {
                if (Gen3Command.rseCommandMap.ContainsKey(command.code))
                {
                    args = Gen3Command.rseCommandMap[command.code];
                }
            }
            if(args == null)
            {
                Logger.main.Error($"Unrecognized script command code: {command.code:x2}");
                success = false;
                return command;
            }
            ReadArgs(ref command, rom, args);
            success = true;
            return command;
        }

        /// <summary>
        /// Read the command args depending on the command code
        /// </summary>
        private void ReadArgs(ref Gen3Command command, Rom rom, Gen3Command.Arg[] typeArr)
        {
            foreach (var type in typeArr)
            {
                // Trainer battle commands have different argument layouts based on the first byte
                if (type == Gen3Command.Arg.TrainerBattle)
                {
                    var trainerType = new Gen3Command.Argument(rom.ReadByte(), Gen3Command.Arg.Byte);
                    command.args.Add(trainerType);
                    ReadArgs(ref command, rom, Gen3Command.GetTrainerArgs(trainerType.data));
                }
                else // Normal arg, parse normally
                {
                    command.args.Add(type switch
                    {
                        Gen3Command.Arg.Byte => new Gen3Command.Argument(rom.ReadByte(), type),
                        Gen3Command.Arg.Word => new Gen3Command.Argument(rom.ReadUInt16(), type),
                        Gen3Command.Arg.Long => new Gen3Command.Argument(rom.ReadUInt32(), type),
                        Gen3Command.Arg.Pointer => new Gen3Command.Argument(rom.ReadPointer(), type),
                        _ => throw new NotImplementedException(),
                    });
                }
            }
        }

        public void Clear()
        {
            scriptDatabase.Clear();
        }
    }
}