using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PokemonRandomizer.UI
{
    public abstract class GroupDataView<T> : DataView<T> where T : DataModel
    {
        private Panel parent;
        private readonly Dictionary<T, Panel> modelViews = new Dictionary<T, Panel>();
        private Panel currView;

        public virtual void Initialize(Panel parent, params T[] initialModels)
        {
            this.parent = parent;
            for(int i = 0; i < initialModels.Length; ++i)
            {
                var model = initialModels[i];
                if (i == 0)
                {
                    SetModel(model);
                }
                else
                {
                    modelViews.Add(model, CreateModelView(model));
                }
            }
        }

        public void SetModel(T model)
        {
            if(!modelViews.ContainsKey(model))
            {
                modelViews.Add(model, CreateModelView(model));
            }
            if(currView != null)
            {
                parent.Children.Remove(currView);
            }
            currView = modelViews[model];
            parent.Children.Add(currView);
        }

        public abstract Panel CreateModelView(T model);

        public abstract T CloneModel(T model);
    }
}
