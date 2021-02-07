using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonRandomizer.UI
{
    public abstract class GroupDataView<T> where T : GroupDataModel
    {
        public abstract void SetModel(T model);

        public abstract T CloneModel(T model);
    }
}
