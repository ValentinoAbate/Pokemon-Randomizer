using System;

namespace PokemonRandomizer.Backend.Writing
{
    using DataStructures;
    using Utilities;
    using GenIII.Constants.ElementNames;
    public class Gen3MapWriter
    {
        private readonly Gen3ScriptWriter scriptWriter;
        public Gen3MapWriter(Gen3ScriptWriter scriptWriter)
        {
            this.scriptWriter = scriptWriter;
        }
        /// <summary>
        /// Write the maps back to the file. Currently edits in place, and does not support exapansion of Map Bank Table.
        /// Currently only writes Map Header data.
        /// </summary>
        public void WriteMapData(RomData data, Rom rom, XmlManager info, RomMetadata metadata)
        {
            int bankPtrOffset = info.FindOffset(ElementNames.mapBankPointers, rom);
            if (bankPtrOffset == Rom.nullPointer)
                return;
            int labelOffset = info.FindOffset(ElementNames.mapLabels, rom);
            if (labelOffset == Rom.nullPointer)
                return;
            // Construct map data structures
            for (int i = 0; i < data.MapBanks.Length; ++i)
            {
                int bankPtr = rom.ReadPointer(bankPtrOffset + (i * Rom.pointerSize));
                WriteBank(data.MapBanks[i], rom, metadata, bankPtr, labelOffset);
            }
        }

        /// <summary>
        /// Write the maps in this bank back to the file. Currently edits in place, and does not support exapansion of the given map table
        /// </summary>
        private void WriteBank(Map[] maps, Rom rom, RomMetadata metadata, int bankOffset, int labelOffset)
        {
            for (int i = 0; i < maps.Length; ++i)
            {
                int mapOffset = rom.ReadPointer(bankOffset + (i * Rom.pointerSize));
                WriteMap(maps[i], rom, metadata, mapOffset, labelOffset);
            }
        }
        /// <summary>
        /// Write a map to the rom. Currently only writes header data
        /// </summary>
        private void WriteMap(Map map, Rom rom, RomMetadata metadata, int mapOffset, int labelOffset)
        {
            #region Write Header Data
            rom.Seek(mapOffset);
            rom.WritePointer(map.mapDataOffset);
            rom.WritePointer(map.eventDataOffset);
            rom.WritePointer(map.mapScriptsOffset);
            rom.WritePointer(map.connectionOffset);
            rom.WriteUInt16(map.music);
            rom.WriteUInt16(map.mapIndex);
            rom.WriteByte(map.labelIndex);
            rom.WriteByte(map.visibility);
            rom.WriteByte((byte)map.weather);
            rom.WriteByte((byte)map.mapType);
            rom.WriteByte(map.unknown);
            rom.WriteByte(map.unknown2);
            rom.WriteByte(map.showLabelOnEntry);
            rom.WriteByte(map.battleField);
            #endregion

            #region Write Non-Header Data
            // TODO: Write map names

            // TODO: Map Data

            // Event Data
            if (map.eventData != null)
                WriteMapEventData(rom, map.eventData, map.eventDataOffset, metadata);
            // Connections
            if (map.connections != null)
                WriteMapConnectionData(rom, map.connections, map.connectionOffset);
            // TODO: Script Data
            #endregion
        }

        private void WriteMapEventData(Rom rom, MapEventData eventData, int offset, RomMetadata metadata)
        {
            rom.Seek(offset);
            // TODO: Allow expansion for this data
            // Map event data header
            rom.WriteByte(eventData.numNpcEvents);
            rom.WriteByte(eventData.numWarpEvents);
            rom.WriteByte(eventData.numTriggerEvents);
            rom.WriteByte(eventData.numSignEvents);
            rom.WritePointer(eventData.npcEventOffset);
            rom.WritePointer(eventData.warpEventOffset);
            rom.WritePointer(eventData.triggerEventOffset);
            rom.WritePointer(eventData.signEventOffset);

            // Write Non-Header Data
            // Write NPC Events
            if (eventData.npcEventOffset != Rom.nullPointer)
            {
                rom.Seek(eventData.npcEventOffset);
                foreach(var npc in eventData.npcEvents)
                {
                    rom.WriteByte(npc.npcEventIndex);
                    rom.WriteUInt16(npc.spriteSetIndex);
                    rom.WriteByte(npc.unknown1);
                    rom.WriteUInt16(npc.xPos);
                    rom.WriteUInt16(npc.yPos);
                    rom.WriteByte(npc.unknown2);
                    rom.WriteByte(npc.movementType);
                    rom.WriteByte(npc.movement);
                    rom.WriteByte(npc.unknown3);
                    rom.WriteByte(npc.isTrainer);
                    rom.WriteByte(npc.unknown4);
                    rom.WriteUInt16(npc.trainerViewRadius);
                    rom.WritePointer(npc.scriptOffset);
                    rom.WriteUInt16(npc.personID);
                    rom.WriteByte(npc.unknown5);
                    rom.WriteByte(npc.unknown6);
                    if (npc.script != null)
                    {
                        scriptWriter.Write(npc.script, rom, npc.scriptOffset, metadata);
                    }
                }
            }
            // Write Warp Events
            if (eventData.warpEventOffset != Rom.nullPointer)
            {
                rom.Seek(eventData.warpEventOffset);
                foreach (var warp in eventData.warpEvents)
                {
                    rom.WriteUInt16(warp.xPos);
                    rom.WriteUInt16(warp.yPos);
                    rom.WriteByte(warp.unknown);
                    rom.WriteByte(warp.warpIndex);
                    rom.WriteByte(warp.mapIndex);
                    rom.WriteByte(warp.bankIndex);
                }
            }
            // Write Trigger Events
            if (eventData.triggerEventOffset != Rom.nullPointer)
            {
                rom.Seek(eventData.triggerEventOffset);
                foreach (var trigger in eventData.triggerEvents)
                {
                    rom.WriteUInt16(trigger.xPos);
                    rom.WriteUInt16(trigger.yPos);
                    rom.WriteUInt16(trigger.unknownUInt16);
                    rom.WriteUInt16(trigger.variableIndex);
                    rom.WriteUInt16(trigger.variableValue);
                    rom.WriteUInt16(trigger.unknownUInt162);
                    rom.WritePointer(trigger.scriptOffset);
                }
            }
            // Write Sign Events
            if (eventData.signEventOffset != Rom.nullPointer)
            {
                rom.Seek(eventData.signEventOffset);
                foreach(var sign in eventData.signEvents)
                {
                    rom.WriteUInt16(sign.xPos);
                    rom.WriteUInt16(sign.yPos);
                    rom.WriteByte(sign.height);
                    rom.WriteByte((byte)sign.signType);
                    rom.WriteByte(sign.unknown1);
                    rom.WriteByte(sign.unknown2);
                    if (sign.IsHiddenItem)
                    {
                        rom.WriteUInt16((int)sign.hiddenItem);
                        rom.WriteByte(sign.hiddenID);
                        rom.WriteByte(sign.hiddenItemAmount);
                    }
                    else if(sign.signType == MapEventData.SignEvent.Type.SecretBase)
                    {
                        rom.WriteByte(sign.secretBaseID);
                        rom.WriteBlock(sign.unknownSecretBaseBlock);
                    }
                    else // Script type
                    {
                        rom.WritePointer(sign.scriptOffset);
                    }
                }
            }
        }
        private void WriteMapConnectionData(Rom rom, ConnectionData data, int offset)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            rom.WriteUInt32(data.connections.Count);
            rom.WritePointer(data.dataOffset);
            // TODO: allow for adding connections
            rom.Seek(data.dataOffset);
            foreach(var connection in data.connections)
            {
                rom.WriteUInt32((int)connection.type);
                rom.WriteUInt32(connection.offset);
                rom.WriteByte(connection.bankId);
                rom.WriteByte(connection.mapId);
                rom.WriteUInt16(connection.unknown);
            }
            rom.LoadOffset();
        }
    }
}
