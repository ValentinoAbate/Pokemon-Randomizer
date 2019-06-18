using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PokemonRandomizer.UserControls
{
    /// <summary>
    /// Interaction logic for MutationRateSlider.xaml
    /// </summary>
    public partial class MutationRateSlider : UserControl
    {

        public double Value { get => slMutRate.Value; }


        public MutationRateSlider()
        {
            InitializeComponent();
        }
    }
}
