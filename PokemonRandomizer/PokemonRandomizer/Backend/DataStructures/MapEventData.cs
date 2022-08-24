using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
	using Scripts;
	public class MapEventData
    {
		public byte numNpcEvents;
		public byte numWarpEvents;
		public byte numTriggerEvents;
		public byte numSignEvents;
		public int npcEventOffset;
		public int warpEventOffset;
		public int triggerEventOffset;
		public int signEventOffset;

		public readonly List<NpcEvent> npcEvents = new List<NpcEvent>();
		public readonly List<WarpEvent> warpEvents = new List<WarpEvent>();
		public readonly List<TriggerEvent> triggerEvents = new List<TriggerEvent>();
		public readonly List<SignEvent> signEvents = new List<SignEvent>();

        public class NpcEvent
        {
			public byte npcEventIndex;
			public int spriteSetIndex; // Could possibly be just 1 byte, with an unknown byte after it
			public byte unknown1;
			public int xPos;
			public int yPos;
			public byte unknown2; // Seems to be 1 or 3 a lot
			public byte movementType;
			public byte movement; // Not sure if this is range, speed etc
			public byte unknown3;
			public bool IsTrainer => isTrainer == 0x01;
			public byte isTrainer;
			public byte unknown4;
			public int trainerViewRadius; // Is "PlantID" for berry trees (Not sure what this is)
			public int scriptOffset;
			public int personID; // Not sure what this is used for, possibly is a script argument?
			public byte unknown5;
			public byte unknown6;
			public Script script;
		}

        public class SignEvent
        {
            public enum Type
            { 
				Script,
				ScriptPlayerFacingUp,
				ScriptPlayerFacingDown,
				ScriptPlayerFacingRight,
				ScriptPlayerFacingLeft,
				HiddenItem1,
				HiddenItem2,
				HiddenItem3,
				SecretBase,
			}

			public bool IsHiddenItem => signType is Type.HiddenItem1 or Type.HiddenItem2 or Type.HiddenItem3;
			public bool IsScript => signType is Type.Script or Type.ScriptPlayerFacingUp or Type.ScriptPlayerFacingDown or Type.ScriptPlayerFacingRight or Type.ScriptPlayerFacingLeft;

			public int xPos;
			public int yPos;
			public byte height;
			public Type signType;
			public byte unknown1;
			public byte unknown2;

			// Script variables
			public int scriptOffset;
			public Script script;

			// Hidden item variables
			public EnumTypes.Item hiddenItem;
			public byte hiddenID;
			/// <summary>
			/// 1 less than the amount of item(s) to be picked up. Add 1 for the actual amount.
			/// </summary>
			public byte hiddenItemAmount;

			// Secret Base Variables
			public byte secretBaseID;
			public byte[] unknownSecretBaseBlock; // Seems to always be 0x00 may not contain data
		}

        public class WarpEvent
        {
			public int xPos;
			public int yPos;
			public byte unknown; // Is usually 0, 1, or 3. Could be layer / collision? Could be to do with the arrow graphic
			// Which warp tile to go to on target map/bank
			public byte warpIndex;
			// Map and bank to warp to
			public byte mapIndex;
			public byte bankIndex;
		}

		public class TriggerEvent : IHasWeather
        {
			private static readonly Dictionary<Map.Weather, int> weatherToInternal = new()
			{
				{ Map.Weather.ClearWithCloudsInWater, 1 },
				{ Map.Weather.Clear, 2 },
				{ Map.Weather.Rain, 3 },
				{ Map.Weather.Snow, 4 },
				{ Map.Weather.RainThunderstorm, 5 },
				{ Map.Weather.MistSteady, 6 },
				{ Map.Weather.MistFromTopRight, 7 },
				{ Map.Weather.FallingAsh, 8 },
				{ Map.Weather.Sandstorm, 9 },
				{ Map.Weather.Cloudy, 10 },
				{ Map.Weather.StrongSunlight, 11 },
				{ Map.Weather.RainSometimes1, 20 },
				{ Map.Weather.RainSometimes2, 21 },
			};
			private static readonly Dictionary<int, Map.Weather> internalToWeather;
			static TriggerEvent()
            {
				internalToWeather = new(weatherToInternal.Count);
				foreach(var kvp in weatherToInternal)
                {
					internalToWeather[kvp.Value] = kvp.Key;
                }
            }
			public bool IsWeatherTrigger => scriptOffset == Rom.nullPointer;
			public Map.Weather Weather
            {
				get => IsWeatherTrigger && internalToWeather.ContainsKey(variableIndex) ? internalToWeather[variableIndex] : Map.Weather.House;
                set
                {
					if (!IsWeatherTrigger)
						return;
					IntendedWeather = value;
					if (weatherToInternal.ContainsKey(value))
                    {
						variableIndex = weatherToInternal[value];
					}
                }
            }
			public Map.Weather IntendedWeather { get; private set; } = Map.Weather.House;
			public int xPos;
			public int yPos;
			public int unknownUInt16;
			public int variableIndex; // May be some sort of flag
			public int variableValue;
			public int unknownUInt162;
			public int scriptOffset;
			public Script script;
		}
	}
}
