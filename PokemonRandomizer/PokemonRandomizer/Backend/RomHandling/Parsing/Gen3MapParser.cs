using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;
using PokemonRandomizer.Backend.Constants;
using System;
using PokemonRandomizer.Backend.Utilities.Debug;

namespace PokemonRandomizer.Backend.RomHandling.Parsing
{
    public class Gen3MapParser
    {
        private readonly Gen3ScriptParser scriptParser;
        public Gen3MapParser(Gen3ScriptParser scriptParser)
        {
            this.scriptParser = scriptParser;
        }
        // Read Map Banks
        public Map[][] ReadMapBanks(Rom rom, XmlManager info, RomMetadata metadata)
        {
            // Read data from XML file
            int bankPtrOffset = info.FindOffset(ElementNames.mapBankPointers, rom);
            if (bankPtrOffset == Rom.nullPointer)
                return Array.Empty<Map[]>();
            int labelOffset = info.FindOffset(ElementNames.mapLabels, rom);
            if (labelOffset == Rom.nullPointer)
                return Array.Empty<Map[]>();
            Map[][] mapBanks = new Map[info.Num(ElementNames.mapBankPointers)][];
            int[] bankLengths = info.IntArrayAttr(ElementNames.maps, "bankLengths");
            // Construct map data structures
            for (int i = 0; i < mapBanks.Length; ++i)
            {
                int bankPtr = rom.ReadPointer(bankPtrOffset + i * Rom.pointerSize);
                mapBanks[i] = ReadMapBank(rom, metadata, bankPtr, bankLengths[i], labelOffset);
            }
            return mapBanks;
        }
        private Map[] ReadMapBank(Rom rom, RomMetadata metadata, int offset, int numMaps, int labelOffset)
        {
            Map[] maps = new Map[numMaps];
            for (int i = 0; i < maps.Length; ++i)
            {
                int mapAddy = rom.ReadPointer(offset + i * Rom.pointerSize);
                maps[i] = ReadMap(rom, metadata, mapAddy, labelOffset);
            }
            return maps;
        }
        private Map ReadMap(Rom rom, RomMetadata metadata, int offset, int labelOffset)
        {
            rom.SaveOffset();
            rom.Seek(offset);

            #region Construct Map With Header Data

            var map = new Map()
            {
                mapDataOffset = rom.ReadPointer(),
                eventDataOffset = rom.ReadPointer(),
                mapScriptsOffset = rom.ReadPointer(),
                connectionOffset = rom.ReadPointer(),
                music = rom.ReadUInt16(),
                mapIndex = rom.ReadUInt16(),
                labelIndex = rom.ReadByte(),
                visibility = rom.ReadByte(),
                weather = (Map.Weather)rom.ReadByte(),
                mapType = (Map.Type)rom.ReadByte(),
                unknown = rom.ReadByte(),
                unknown2 = rom.ReadByte(),
                showLabelOnEntry = rom.ReadByte(),
                battleField = rom.ReadByte(),
            };

            #endregion

            #region Read Non-Header Data

            // Map Label (name)
            if (metadata.IsRubySapphireOrEmerald)
            {
                // Read Map Label (RSE)
                rom.Seek(rom.ReadPointer(labelOffset + map.labelIndex * 8 + 4));
                map.Name = rom.ReadVariableLengthString();
            }
            else if (metadata.IsFireRedOrLeafGreen)
            {
                // Don't know why this magic number is here
                const int frlgMapLabelsStart = 0x58;
                // Read Map Label (FRLG)
                rom.Seek(rom.ReadPointer(labelOffset + (map.labelIndex - frlgMapLabelsStart) * 4));
                map.Name = rom.ReadVariableLengthString();
            }
            // Map Data
            if (map.mapDataOffset != Rom.nullPointer)
                map.mapData = ReadMapData(rom, metadata, map.mapDataOffset);
            // Event Data
            if (map.eventDataOffset != Rom.nullPointer)
                map.eventData = ReadMapEventData(rom, map.eventDataOffset, metadata);
            // Script Data
            if (map.mapScriptsOffset != Rom.nullPointer)
                map.scriptData = ReadMapScriptData(rom, map.mapScriptsOffset, metadata);
            // Connections
            if (map.connectionOffset != Rom.nullPointer)
                map.connections = ReadMapConnectionData(rom, map.connectionOffset);
            #endregion

            map.SetOriginalValues();
            rom.LoadOffset();
            return map;
        }
        private MapData ReadMapData(Rom rom, RomMetadata metadata, int offset)
        {
            var mapData = new MapData();
            rom.Seek(offset);
            mapData.width = rom.ReadUInt32();
            mapData.height = rom.ReadUInt32();
            mapData.borderTileOffset = rom.ReadPointer();
            mapData.mapTilesOffset = rom.ReadPointer();
            mapData.globalTileSetOffset = rom.ReadPointer();
            mapData.localTileSetOffset = rom.ReadPointer();
            var border = rom.ReadBits(4, 2);
            mapData.borderWidth = border[0];
            mapData.borderHeight = border[1];
            mapData.secondarySize = mapData.borderWidth + 0xA0;
            if (metadata.IsRubySapphireOrEmerald)
            {
                const int rseBorderWidthAndHeight = 2;
                mapData.borderWidth = rseBorderWidthAndHeight;
                mapData.borderHeight = rseBorderWidthAndHeight;
            }
            return mapData;
        }
        private MapEventData ReadMapEventData(Rom rom, int offset, RomMetadata metadata)
        {
            var eventData = new MapEventData();
            rom.Seek(offset);
            eventData.numNpcEvents = rom.ReadByte();
            eventData.numWarpEvents = rom.ReadByte();
            eventData.numTriggerEvents = rom.ReadByte();
            eventData.numSignEvents = rom.ReadByte();
            eventData.npcEventOffset = rom.ReadPointer();
            eventData.warpEventOffset = rom.ReadPointer();
            eventData.triggerEventOffset = rom.ReadPointer();
            eventData.signEventOffset = rom.ReadPointer();

            // Read Non-Header Data
            // Read NPC Events
            if (eventData.npcEventOffset != Rom.nullPointer)
            {
                rom.Seek(eventData.npcEventOffset);
                for (int i = 0; i < eventData.numNpcEvents; i++)
                {
                    var npc = new MapEventData.NpcEvent
                    {
                        npcEventIndex = rom.ReadByte(),
                        spriteSetIndex = rom.ReadUInt16(),
                        unknown1 = rom.ReadByte(),
                        xPos = rom.ReadUInt16(),
                        yPos = rom.ReadUInt16(),
                        unknown2 = rom.ReadByte(),
                        movementType = rom.ReadByte(),
                        movement = rom.ReadByte(),
                        unknown3 = rom.ReadByte(),
                        isTrainer = rom.ReadByte(),
                        unknown4 = rom.ReadByte(),
                        trainerViewRadius = rom.ReadUInt16(),
                        scriptOffset = rom.ReadPointer(),
                        personID = rom.ReadUInt16(),
                        unknown5 = rom.ReadByte(),
                        unknown6 = rom.ReadByte(),
                    };
                    if (npc.scriptOffset != Rom.nullPointer)
                    {
                        npc.script = scriptParser.Parse(rom, npc.scriptOffset, metadata);
                    }
                    eventData.npcEvents.Add(npc);
                }
            }
            // Read Warp Events
            if (eventData.warpEventOffset != Rom.nullPointer)
            {
                rom.Seek(eventData.warpEventOffset);
                for (int i = 0; i < eventData.numWarpEvents; i++)
                {
                    eventData.warpEvents.Add(new MapEventData.WarpEvent
                    {
                        xPos = rom.ReadUInt16(),
                        yPos = rom.ReadUInt16(),
                        unknown = rom.ReadByte(),
                        warpIndex = rom.ReadByte(),
                        mapIndex = rom.ReadByte(),
                        bankIndex = rom.ReadByte(),
                    });
                }
            }
            // Read Trigger Events
            if (eventData.triggerEventOffset != Rom.nullPointer)
            {
                rom.Seek(eventData.triggerEventOffset);
                for (int i = 0; i < eventData.numTriggerEvents; i++)
                {
                    var trigger = new MapEventData.TriggerEvent
                    {
                        xPos = rom.ReadUInt16(),
                        yPos = rom.ReadUInt16(),
                        unknownUInt16 = rom.ReadUInt16(),
                        variableIndex = rom.ReadUInt16(),
                        variableValue = rom.ReadUInt16(),
                        unknownUInt162 = rom.ReadUInt16(),
                        scriptOffset = rom.ReadPointer(),
                    };
                    if (trigger.scriptOffset != Rom.nullPointer)
                    {
                        trigger.script = scriptParser.Parse(rom, trigger.scriptOffset, metadata);
                    }
                    eventData.triggerEvents.Add(trigger);
                }
            }
            // Read Sign Events
            if (eventData.signEventOffset != Rom.nullPointer)
            {
                rom.Seek(eventData.signEventOffset);
                for (int i = 0; i < eventData.numSignEvents; i++)
                {
                    var signEvent = new MapEventData.SignEvent
                    {
                        xPos = rom.ReadUInt16(),
                        yPos = rom.ReadUInt16(),
                        height = rom.ReadByte(),
                        signType = (MapEventData.SignEvent.Type)rom.ReadByte(),
                        unknown1 = rom.ReadByte(),
                        unknown2 = rom.ReadByte(),
                    };
                    if (signEvent.IsHiddenItem)
                    {
                        signEvent.hiddenItem = (Item)rom.ReadUInt16();
                        signEvent.hiddenID = rom.ReadByte();
                        signEvent.hiddenItemAmount = rom.ReadByte();
                    }
                    else if (signEvent.signType == MapEventData.SignEvent.Type.SecretBase)
                    {
                        signEvent.secretBaseID = rom.ReadByte();
                        signEvent.unknownSecretBaseBlock = rom.ReadBlock(3);
                    }
                    else // Script type
                    {
                        signEvent.scriptOffset = rom.ReadPointer();
                    }
                    eventData.signEvents.Add(signEvent);
                }
            }

            return eventData;
        }
        private MapScriptData ReadMapScriptData(Rom rom, int offset, RomMetadata metadata)
        {
            var mapScriptData = new MapScriptData();
            rom.Seek(offset);

            var type = (MapScriptData.Type)rom.ReadByte();
            while(type != MapScriptData.Type.NoScripts)
            {
                int scriptOffset = rom.ReadPointer();
                var mapScript = new MapScriptData.MapScript()
                {
                    type = type,
                    scriptOffset = scriptOffset,
                };
                if (scriptOffset != Rom.nullPointer)
                {
                    if (mapScript.IsSimpleScriptType)
                    {
                        mapScript.script = scriptParser.Parse(rom, scriptOffset, metadata);
                    } 
                    else if (mapScript.IsFlagValueScriptType)
                    {
                        rom.SaveAndSeekOffset(mapScript.scriptOffset);
                        mapScript.flag = rom.ReadUInt16();
                        mapScript.value = rom.ReadUInt16();
                        mapScript.scriptOffset2 = rom.ReadPointer();
                        rom.LoadOffset();
                        if(mapScript.scriptOffset2 != Rom.nullPointer)
                        {
                            mapScript.script = scriptParser.Parse(rom, mapScript.scriptOffset2, metadata);
                        }
                    }
                    else
                    {
                        Logger.main.Info($"Map Script With Unknown Type: {type}");
                    }
                }
                mapScriptData.scripts.Add(mapScript);
                type = (MapScriptData.Type)rom.ReadByte();
            }
            return mapScriptData;
        }
        private ConnectionData ReadMapConnectionData(Rom rom, int offset)
        {
            rom.SaveOffset();
            rom.Seek(offset);
            ConnectionData data = new ConnectionData
            {
                initialNumConnections = rom.ReadUInt32(),
                dataOffset = rom.ReadPointer()
            };
            rom.Seek(data.dataOffset);
            for (int i = 0; i < data.initialNumConnections; ++i)
            {
                // Read the connection
                var connection = new Connection
                {
                    type = (Connection.Type)rom.ReadUInt32(),
                    offset = rom.ReadUInt32(),
                    bankId = rom.ReadByte(),
                    mapId = rom.ReadByte(),
                    unknown = rom.ReadUInt16()
                };
                data.connections.Add(connection);
            }
            rom.LoadOffset();
            return data;
        }
    }
}
