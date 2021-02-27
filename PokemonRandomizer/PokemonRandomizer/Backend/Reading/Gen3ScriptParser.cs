using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.Reading
{
    using DataStructures;
    using DataStructures.Scripts;
    using EnumTypes;
    using Scripting.GenIII;

    public class Gen3ScriptParser
    {
        public Script Parse(Rom rom, int offset)
        {
            var visited = new HashSet<int>();
            var script = Parse(rom, offset, ref visited);
            return script;
        }

        private Script Parse(Rom rom, int offset, ref HashSet<int> visited)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            var script = new Script();
            bool reachedEnd = false;
            while (!reachedEnd)
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
                        gotoCommand.script = Parse(rom, gotoCommand.offset, ref visited);
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
                        gotoCommand.script = Parse(rom, gotoCommand.offset, ref visited);
                    }
                }
                else if(command.code == Gen3Command.copyvarifnotzero)
                {
                    // Check for give item multi-command
                    if (TryParseGiveItemMultiCommand(rom, command, out GiveItemCommand giveItemCommand))
                    {
                        script.Add(giveItemCommand);
                    }
                }
                else if(command.code == Gen3Command.givePokemon)
                {
                    // Add new give pokemon command
                    script.Add(new GivePokemonCommand()
                    {
                        pokemon = (PokemonSpecies)command.ArgData(0),
                        level = (byte)command.ArgData(1),
                        heldItem = (Item)command.ArgData(2)
                    });
                }
                else if(command.code == Gen3Command.giveEgg)
                {
                    // Add new give egg event
                    script.Add(new GiveEggCommand() { pokemon = (PokemonSpecies)command.ArgData(0) });
                }
                else // Not a special code, just push the command
                {
                    script.Add(command);
                }
            }
            rom.LoadOffset();
            return script;
        }

        public bool TryParseGiveItemMultiCommand(Rom rom, Gen3Command command1, out GiveItemCommand giveItemMultiCommand)
        {
            giveItemMultiCommand = null;
            // If the first command isn't setting variable 0x8000 or the next command is end return false
            if (command1.ArgData(0) != 0x8000 || rom.Peek() == Gen3Command.end)
                return false;
            rom.SaveOffset();
            var command2 = ReadCommand(rom);
            // If the second command copyvarifnotzero with a target of var 0x8001 return false
            if (command2.code != Gen3Command.copyvarifnotzero || command2.ArgData(0) != 0x8001)
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
            rom.LoadOffset();
            return true;
        }

        /// <summary>
        /// Read a raw command from the ROM
        /// </summary>
        public Gen3Command ReadCommand(Rom rom)
        {
            var command = new Gen3Command() { code = rom.ReadByte() };
            if (!Gen3Command.commandMap.ContainsKey(command.code))
            {
                // Todo: log error
                return command;
            }
            ReadArgs(ref command, rom, Gen3Command.commandMap[command.code]);
            return command;
        }

        /// <summary>
        /// Read the command args depending on the command code
        /// </summary>
        public void ReadArgs(ref Gen3Command command, Rom rom, Gen3Command.Arg[] typeArr)
        {
            foreach (var type in typeArr)
            {
                // Trainer battle commands have different argument layouts based on the first byte
                if (type == Gen3Command.Arg.TrainerBattle)
                {
                    var trainerType = new Gen3Command.Argument(rom.ReadByte(), Gen3Command.Arg.Byte);
                    command.args.Add(trainerType);
                    if (Gen3Command.trainerCommandMap.TryGetValue(trainerType.data, out var trainerArgList))
                    {
                        ReadArgs(ref command, rom, trainerArgList);
                    }
                    else // Fallback to basic trainer args
                    {
                        ReadArgs(ref command, rom, Gen3Command.trainerCommandMap[0x00]);
                    }
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