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
			public bool isTrainer;
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

			public bool IsHiddenItem => signType == Type.HiddenItem1 || signType == Type.HiddenItem2 || signType == Type.HiddenItem3;

			public int xPos;
			public int yPos;
			public byte height;
			public Type signType;
			public byte unknown1;
			public byte unknown2;

			// Script variables
			public int scriptOffset;

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
			public byte unknown; // Seems to usually by 3?
			// Which warp tile to go to on target map/bank
			public byte warpIndex;
			// Map and bank to warp to
			public byte mapIndex;
			public byte bankIndex;
		}

		public class TriggerEvent
        {
			public int xPos;
			public int yPos;
			public int unknownUInt16;
			public int variableIndex; // May be some sort of flag
			public int variableValue;
			public int unknownUInt162;
			public int scriptOffset;
		}
	}
}
