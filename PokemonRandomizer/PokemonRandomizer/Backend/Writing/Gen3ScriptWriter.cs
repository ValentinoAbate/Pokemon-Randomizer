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
    using EnumTypes;
    public class Gen3ScriptWriter
    {
        private readonly Func<Item, Item> itemRemap;
        public Gen3ScriptWriter(Func<Item, Item> itemRemap)
        {
            this.itemRemap = itemRemap;
        }
        public void Write(Script script, Rom rom, int offset)
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
                        Write(gotoIf.script, rom, gotoIf.offset);
                        break;
                    case GotoCommand @goto:
                        rom.WriteByte(Gen3Command.@goto);
                        rom.WritePointer(@goto.offset);
                        Write(@goto.script, rom, @goto.offset);
                        break;
                    case GiveItemCommand giveItem:
                        rom.WriteByte(Gen3Command.copyvarifnotzero);
                        rom.WriteUInt16(Gen3Command.itemTypeVar);
                        rom.WriteUInt16((int)itemRemap(giveItem.item));
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
                        rom.WriteUInt16((int)itemRemap(givePokemon.heldItem));
                        rom.SetBlock(9, 0x00);
                        break;
                    case GiveEggCommand giveEggCommand:
                        rom.WriteByte(Gen3Command.giveEgg);
                        rom.WriteUInt16((int)giveEggCommand.pokemon);
                        break;
                    case ShopCommand shopCommand:
                        WriteShopCommand(rom, shopCommand);
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

        private void WriteShopCommand(Rom rom, ShopCommand command)
        {
            rom.WriteByte(command.code);
            if(command.shop.items.Count <= command.shop.OriginalSize)
            {
                rom.WritePointer(command.shopOffset);
                // Write the shop contents
                rom.SaveAndSeekOffset(command.shopOffset);
                foreach(var item in command.shop.items)
                {
                    rom.WriteUInt16((int)itemRemap(item));
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
                    dataBlock.WriteUInt16((int)itemRemap(item));
                }
                dataBlock.WriteUInt16((int)Item.None);
                // Attempt to write shop data block in free space
                var newOffset = rom.WriteInFreeSpace(dataBlock.File);
                // If successful, write the new offset, else write the old one
                rom.WritePointer(newOffset != null ? (int)newOffset : command.shopOffset);
            }
        }
    }
}
