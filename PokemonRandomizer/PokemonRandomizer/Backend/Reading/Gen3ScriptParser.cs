using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Reading
{
    using DataStructures;
    using DataStructures.Scripts;
    using EnumTypes;
    using Utilities.Debug;
    using Scripting.GenIII;

    public class Gen3ScriptParser
    {
        public Script Parse(Rom rom, int offset, RomMetadata metadata)
        {
            var visited = new HashSet<int>();
            var script = Parse(rom, offset, metadata, ref visited);
            return script;
        }

        private Script Parse(Rom rom, int offset, RomMetadata metadata, ref HashSet<int> visited)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            var script = new Script();
            while (true)
            {
                visited.Add(rom.InternalOffset);
                var command = ReadCommand(rom);
                if(command.code == Gen3Command.end || command.code == Gen3Command.@return)
                {
                    script.Add(command);
                    break;
                }
                else if(command.code == Gen3Command.@goto)
                {
                    var gotoCommand = new GotoCommand()
                    {
                        offset = command.ArgData(0),
                    };
                    script.Add(gotoCommand);
                    // If we haven't encountered this offset, then parse the script at the goto location
                    if (!visited.Contains(gotoCommand.offset))
                    {
                        gotoCommand.script = Parse(rom, gotoCommand.offset, metadata, ref visited);
                    }
                    break;
                }
                else if(command.code == Gen3Command.gotoif)
                {
                    var gotoCommand = new GotoIfCommand()
                    {
                        condition = (byte)command.ArgData(0),
                        offset = command.ArgData(1),
                    };
                    script.Add(gotoCommand);
                    // If we haven't encountered this offset, then parse the branch
                    if (!visited.Contains(gotoCommand.offset))
                    {
                        gotoCommand.script = Parse(rom, gotoCommand.offset, metadata, ref visited);
                    }
                }
                else if(command.code == Gen3Command.special)
                {
                    script.Add(ParseSpecialCommand(command, metadata));
                }
                else if(command.code == Gen3Command.copyvarifnotzero)
                {
                    // Check for give item multi-command
                    if (TryParseGiveItemMultiCommand(rom, command, out GiveItemCommand giveItemCommand))
                    {
                        script.Add(giveItemCommand);
                    }
                    else
                    {
                        script.Add(command);
                    }
                }
                else if(command.code == Gen3Command.givePokemon)
                {
                    var givePokemonCommand = new GivePokemonCommand()
                    {
                        pokemon = (Pokemon)command.ArgData(0),
                        level = (byte)command.ArgData(1),
                        heldItem = (Item)command.ArgData(2)
                    };
                    // Mark command type
                    if ((int)givePokemonCommand.pokemon > 10000)
                    {
                        givePokemonCommand.type = (int)givePokemonCommand.pokemon > 32768 ? GivePokemonCommand.Type.Variable : GivePokemonCommand.Type.Unknown;
                    }
                    // Add new give pokemon command
                    script.Add(givePokemonCommand);
                }
                else if(command.code == Gen3Command.giveEgg)
                {
                    // Add new give egg event
                    script.Add(new GiveEggCommand() { pokemon = (Pokemon)command.ArgData(0) });
                }
                else if(command.code == Gen3Command.pokemart || command.code == Gen3Command.pokemart2 || command.code == Gen3Command.pokemart3)
                {
                    // Add new poke mart event
                    script.Add(ParseShopCommand(rom, command));
                }
                else if(command.code == Gen3Command.setberrytree)
                {
                    script.Add(ParseBerryTreeCommand(rom, command));
                }
                else // Not a special code, just push the command
                {
                    script.Add(command);
                }
            }
            rom.LoadOffset();
            return script;
        }

        private bool TryParseGiveItemMultiCommand(Rom rom, Gen3Command command1, out GiveItemCommand giveItemMultiCommand)
        {
            giveItemMultiCommand = null;
            // If the first command isn't setting variable 0x8000 or the next command is end return false
            if (command1.ArgData(0) != Gen3Command.itemTypeVar || rom.Peek() == Gen3Command.end)
                return false;
            rom.SaveOffset();
            var command2 = ReadCommand(rom);
            // If the second command copyvarifnotzero with a target of var 0x8001 return false
            if (command2.code != Gen3Command.copyvarifnotzero || command2.ArgData(0) != Gen3Command.itemQuantityVar)
            {
                rom.LoadOffset();
                return false;
            }
            var command3 = ReadCommand(rom);
            // If the third command isn't callstd with an valid give item arg return false
            if (command3.code != Gen3Command.callstd || (command3.ArgData(0) != CallStd.giveItemObtain && command3.ArgData(0) != CallStd.giveItemFind))
            {
                rom.LoadOffset();
                return false;
            }
            // Valid giveitem multicommand found
            giveItemMultiCommand = new GiveItemCommand()
            {
                item = (Item)command1.ArgData(1),
                amount = command2.ArgData(1),
                messageType = (GiveItemCommand.MessageType)command3.ArgData(0),
            };
            // Mark command type
            if((int)giveItemMultiCommand.item > 10000)
            {
                giveItemMultiCommand.type = (int)giveItemMultiCommand.item > 32768 ? GiveItemCommand.Type.Variable : GiveItemCommand.Type.Unknown;
            }
            rom.DumpOffset();
            return true;
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

        private Command ParseSpecialCommand(Gen3Command command, RomMetadata metadata)
        {
            int specialCode = command.ArgData(0);
            if (metadata.IsFireRedOrLeafGreen)
            {
                return specialCode switch
                {
                    Gen3Command.specialGiveNationalDexFrlg => new GivePokedexCommand { Type = GivePokedexCommand.PokedexType.National },
                    Gen3Command.specialGiveRegionalDexFrlg => new GivePokedexCommand { Type = GivePokedexCommand.PokedexType.Regional },
                    _ => command,
                };
            }
            else if(metadata.IsEmerald)
            {
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
        private Gen3Command ReadCommand(Rom rom)
        {
            var command = new Gen3Command() { code = rom.ReadByte() };
            if (!Gen3Command.commandMap.ContainsKey(command.code))
            {
                Logger.main.Error("Unrecognized script command code: " + command.code);
                return command;
            }
            ReadArgs(ref command, rom, Gen3Command.commandMap[command.code]);
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
    }
}