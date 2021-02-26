using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.DataStructures.Scripts;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Scripting.GenIII;
using System;

namespace PokemonRandomizer.Backend.Reading
{
    public class Gen3ScriptParser
    {
        public Script Parse(Rom rom, int offset)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            var script = new Script();
            while(rom.Peek() != Gen3Command.end)
            {
                var rawCommand = ReadCommand(rom);
                switch (rawCommand.code)
                {
                    case Gen3Command.copyvarifnotzero:
                        // Check for give item multi-command
                        if(TryParseGiveItemMultiCommand(rom, rawCommand, out GiveItemCommand giveItemCommand))
                        {
                            script.Add(giveItemCommand);
                        }
                        break;
                    default: // Unknown command code, read all data into unknown command
                        script.Add(new UnknownCommand(new byte[] { rawCommand.code })); // Will later need to contain arg data as well
                        break;
                }
            }
            rom.LoadOffset();
            return script;
        }

        public bool TryParseGiveItemMultiCommand(Rom rom, Gen3Command command1, out GiveItemCommand giveItemMultiCommand)
        {
            giveItemMultiCommand = null;
            // If the first command isn't setting variable 0x8000 or the next command is end return false
            if (command1.args[0].data != 0x8000 || rom.Peek() == Gen3Command.end)
                return false;
            rom.SaveOffset();
            var command2 = ReadCommand(rom);
            // If the second command copyvarifnotzero with a target of var 0x8001 return false
            if (command2.code != Gen3Command.copyvarifnotzero || command2.args[0].data != 0x8001)
            {
                rom.LoadOffset();
                return false;
            }
            var command3 = ReadCommand(rom);
            // If the third command isn't callstd with an valid give item arg return false
            if (command3.code != Gen3Command.callstd || (command3.args[0].data != CallStd.giveItemObtain && command3.args[0].data != CallStd.giveItemFind))
            {
                rom.LoadOffset();
                return false;
            }
            // Valid giveitem multicommand found
            giveItemMultiCommand = new GiveItemCommand()
            {
                item = (Item)command1.args[1].data,
                amount = command2.args[1].data,
                messageType = (GiveItemCommand.MessageType)command3.args[0].data,
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
            if (Gen3Command.commandMap.ContainsKey(command.code))
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
                command.args.Add(type switch
                {
                    Gen3Command.Arg.Byte => new Gen3Command.Argument(rom.ReadByte(), type),
                    Gen3Command.Arg.Word => new Gen3Command.Argument(rom.ReadUInt16(), type),
                    Gen3Command.Arg.Long => new Gen3Command.Argument(rom.ReadUInt32(), type),
                    Gen3Command.Arg.Pointer => new Gen3Command.Argument(rom.ReadPointer(), type),
                    Gen3Command.Arg.TrainerBattle => new Gen3Command.Argument(rom.ReadByte(), type),
                    _ => throw new NotImplementedException(),
                });
                // Trainer battle commands have different argument layouts based on the first byte
                if (type == Gen3Command.Arg.TrainerBattle)
                {
                    byte trainerType = (byte)command.args[command.args.Count - 1].data;
                    if (Gen3Command.trainerCommandMap.TryGetValue(trainerType, out var trainerArgList))
                    {
                        ReadArgs(ref command, rom, trainerArgList);
                    }
                    else // Fallback to basic trainer args
                    {
                        ReadArgs(ref command, rom, Gen3Command.trainerCommandMap[0x00]);
                    } 
                }
            }
        }
    }
}