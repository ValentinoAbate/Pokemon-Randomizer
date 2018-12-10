using Microsoft.Win32;
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
using System.ComponentModel;
using System.IO;
using PokemonEmeraldRandomizer.CultureUtils;

namespace PokemonEmeraldRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region XAML Properties for bindings
        private bool _isROMLoaded;
        public bool IsROMLoaded
        {
            get => _isROMLoaded;
            private set
            {
                _isROMLoaded = value;
                OnPropertyChanged("IsROMLoaded");
            }
        }
        #endregion

        private Backend.ROMData Data { get; set; }
        private Backend.ROMData RandomizedData { get; set; }

        public MainWindow()
        {
            IsROMLoaded = false;
            InitializeComponent();
            this.DataContext = this;
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        #endregion

        #region Routed Commands
        // ExecutedRoutedEventHandler for the custom command.
        private void CmdAddTreeItem(object sender, ExecutedRoutedEventArgs e)
        {
            var target = e.Source as ItemsControl;
            if (target != null)
            {
                var add = new TreeViewItem()
                {
                    Header = "Group " + (target.Items.Count + 1),
                    Style = e.Parameter as Style
                };
                target.Items.Add(add);
                add.IsSelected = true;
            }
        }

        // ExecutedRoutedEventHandler for the custom command.
        private void CmdRmTreeItem(object sender, ExecutedRoutedEventArgs e)
        {
            //var target = (e.Parameter as ContextMenu)?.PlacementTarget as TreeViewItem;
            var target = e.Source as TreeViewItem;
            if (target != null)
                (target.Parent as ItemsControl).Items.Remove(target);
        }

        // CanExecuteRoutedEventHandler for the custom command.
        private void AddMutationTargetCmdCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = e.Source is TreeViewItem;
        }
        #endregion

        private void Open_ROM(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GBA ROM files|*.gba";
            openFileDialog.Title = "Open ROM";
            if (openFileDialog.ShowDialog() == true)
            {
                byte[] rawROM = File.ReadAllBytes(openFileDialog.FileName);
                Data = Backend.ROMParser.Parse(rawROM);
                RandomizedData = Data;
                IsROMLoaded = true;
                lblMessageBoxContent.Content = "Pokemon Emerald ROM opened";
            }
        }

        private void Save_ROM(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "GBA ROM|*.gba";
            saveFileDialog.Title = "Save ROM";
            if (saveFileDialog.ShowDialog() == true)
            {
                RandomizedData = Backend.ROMDataMutator.Mutate(Data, this);
                File.WriteAllBytes(saveFileDialog.FileName, Backend.ROMWriter.Write(RandomizedData));
            }
        }

        private void Generate_Info_Doc(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "txt file|*.txt";
            saveFileDialog.Title = "Generate Info Docs";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, RandomizedData.ToStringArray());
            }
        }

        private void Convert_Table(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ".csv files|*.csv";
            openFileDialog.Title = "Convert csv";
            if (openFileDialog.ShowDialog() == true)
            {
                Backend.TableReader.TableToDictFormat(openFileDialog.FileName, ',', 15);
            }

        }

        private void SeedCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            tbSeed.Visibility = (bool)cbSeed.IsChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Select_On_Right_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as TreeViewItem;
            item.IsSelected = true;
        }
    }
    public static class Commands
    {
        public static readonly RoutedCommand addTreeItem = new RoutedCommand();
        public static readonly RoutedCommand rmTreeItem = new RoutedCommand();
    }

    class PercentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                float fltValue = System.Convert.ToSingle(value);
                return string.Format("{0:P1}",fltValue);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return float.Parse((value as string).RemovePercent()) / 100;
        }
    }
}
