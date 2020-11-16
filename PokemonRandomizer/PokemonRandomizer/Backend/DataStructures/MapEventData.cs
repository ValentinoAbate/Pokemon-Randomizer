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
	}
}
