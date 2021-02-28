using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.Writing
{
    using Scripting.GenIII;
    using DataStructures.Scripts;
    using DataStructures;
    public class Gen3ScriptWriter
    {
        public void Write(Script script, Rom rom, int offset, Gen3RomWriter.RepointList repointList)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            foreach(var command in script)
            {
                switch (command)
                {
                    case GotoIfCommand gotoIf:
                        rom.WriteByte(Gen3Command.gotoif);
                        rom.WriteByte(gotoIf.condition);
                        rom.WritePointer(gotoIf.offset);
                        Write(gotoIf.script, rom, gotoIf.offset, repointList);
                        break;
                    case GotoCommand @goto:
                        rom.WriteByte(Gen3Command.@goto);
                        rom.WritePointer(@goto.offset);
                        Write(@goto.script, rom, @goto.offset, repointList);
                        break;
                    case GiveItemCommand giveItem:
                        rom.WriteByte(Gen3Command.copyvarifnotzero);
                        rom.WriteUInt16(Gen3Command.itemTypeVar);
                        rom.WriteUInt16((int)giveItem.item);
                        rom.WriteByte(Gen3Command.copyvarifnotzero);
                        rom.WriteUInt16(Gen3Command.itemQuantityVar);
                        rom.WriteUInt16(giveItem.amount);
                        rom.WriteByte(Gen3Command.callstd);
                        rom.WriteByte((byte)giveItem.messageType);
                        break;
                    case GivePokemonCommand givePokemon:
                        rom.WriteByte(Gen3Command.givePokemon);
                        rom.WriteUInt16((int)givePokemon.pokemon);
                        rom.WriteByte(givePokemon.level);
                        rom.WriteUInt16((int)givePokemon.heldItem);
                        rom.SetBlock(9, 0x00);
                        break;
                    case GiveEggCommand giveEggCommand:
                        rom.WriteByte(Gen3Command.giveEgg);
                        rom.WriteUInt16((int)giveEggCommand.pokemon);
                        break;
                    case Gen3Command gen3Command:
                        rom.WriteByte(gen3Command.code);
                        foreach(var arg in gen3Command.args)
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
    }
}
