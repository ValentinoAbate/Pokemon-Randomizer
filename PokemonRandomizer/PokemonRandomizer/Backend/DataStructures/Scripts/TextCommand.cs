using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public abstract class TextCommand : Command
    {
        public abstract string Text { get; set; }
        public int OriginalLength { get; private set; }

        public void SetOriginalValues()
        {
            OriginalLength = Text?.Length ?? 0;
        }
    }
}
