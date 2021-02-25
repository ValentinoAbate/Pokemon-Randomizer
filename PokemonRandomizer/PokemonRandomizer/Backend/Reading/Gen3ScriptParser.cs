using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonRandomizer.Backend.Scripting.GenIII;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.DataStructures.Scripts;

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
                byte code = rom.ReadByte();
                var rawCommand = ReadCommand(code, rom);
                switch (code)
                {
                    case Gen3Command.copyvarifnotzero:
                        // Check for give item multi-command (TODO: make own function)
                        if(rawCommand.args[0].data == 0x8000)
                        {
                            if (rom.Peek() == Gen3Command.end)
                            {
                                break;
                            }
                            rom.SaveOffset();
                            var command2 = ReadCommand(rom.ReadByte(), rom);
                            if(command2.code == Gen3Command.copyvarifnotzero && command2.args[0].data == 0x8001)
                            {
                                var command3 = ReadCommand(rom.ReadByte(), rom);
                                if (rom.Peek() == Gen3Command.end)
                                {
                                    rom.LoadOffset();
                                    break;
                                }
                                if (command3.code == Gen3Command.callstd && (command3.args[0].data == 0x01 || command3.args[0].data == 0x00))
                                {
                                    script.Add(new GiveItemCommand()
                                    {
                                        item = (Item)rawCommand.args[1].data,
                                        amount = command2.args[1].data
                                    });
                                }
                            }
                            rom.LoadOffset();
                        }
                        break;
                    default: // Unknown command code, read all data into unknown command
                        script.Add(new UnknownCommand(new byte[] { code })); // Will later need to contain arg data as well
                        break;
                }
            }
            rom.LoadOffset();
            return script;
        }

        public Gen3Command ReadCommand(byte code, Rom rom)
        {
            var command = new Gen3Command() { code = code };
            ReadArgs(ref command, rom, Gen3Command.commandMap[code]);
            return command;
        }
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
                if (type == Gen3Command.Arg.TrainerBattle)
                {
                    byte trainerType = (byte)command.args[command.args.Count - 1].data;
                    var trainerArgList = Gen3Command.trainerCommandMap[Gen3Command.trainerCommandMap.ContainsKey(trainerType) ? trainerType : (byte)0x00];
                    ReadArgs(ref command, rom, trainerArgList);
                }
            }
        }
    }
}