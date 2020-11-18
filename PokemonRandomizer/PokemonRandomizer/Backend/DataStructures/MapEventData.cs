using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures
{
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
			public int trainerViewRadius;
			public int scriptOffset;
			public int personID; // Not sure what this is used for, possibly is a script argument?
			public byte unknown5;
			public byte unknown6;
		}

        public class SignEvent
        { 
		
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

        }
	}
}
