using System;
using System.Collections.Generic;

namespace PokemonRandomizer.Backend.DataStructures.Scripts
{
    public class Script : List<Command>
    {
        public void ApplyRecursively(Action<Command> action)
        {
            foreach(var command in this)
            {
                switch (command)
                {
                    case GotoCommand @goto:
                        @goto.script?.ApplyRecursively(action);
                        break;
                    case CallCommand call:
                        call.script?.ApplyRecursively(action);
                        break;
                    case TrainerBattleCommand trainerBattleCommand:
                        trainerBattleCommand.postBattleScript?.ApplyRecursively(action);
                        goto default;
                    default:
                        action(command);
                        break;
                }
            }
        }
    }
}
