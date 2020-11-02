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
using PokemonRandomizer.CultureUtils;
using PokemonRandomizer.Backend.DataStructures;
using PokemonRandomizer.Backend.EnumTypes;
using PokemonRandomizer.Backend.Utilities;

namespace PokemonRandomizer
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

        public CompositeCollection TypeTraitWeightDropDown
        {
            get => new CompositeCollection
            {
                new ComboBoxItem() {Content="None", ToolTip="The type has an equal chance of being normally effective, not very effective, super effect, or no effect vs any given type (and vice versa)"},
                new Separator(),
                new ComboBoxItem() {Content="Weak", ToolTip="Like average, but less likely to be super effective, more likely to be not very effective or have no effect. Less likely to resist or negate other types"},
                new ComboBoxItem() {Content="Average", ToolTip="The type's chance of being a certain effectivess vs a type is the average occurence of that effectiveness in the Base ROM"},
                new ComboBoxItem() {Content="Powerful", ToolTip="Like average, but more likely to be super effective, less likely to be not very effective or have no effect. More likely to resist or negate other types"},
                new ComboBoxItem() {Content="Arceus-Like", ToolTip="Like powerful, but ridiculous. Use with caution"},
                new Separator(),
                new ComboBoxItem() {Content="Glass Cannon", ToolTip="Likely to be super effective versus other types, but also likely to be weak against other types"},
                new ComboBoxItem() {Content="Tank", ToolTip="Likely to resist/negate other types, but not likely to be super effective"},
                new Separator(),
                new ComboBoxItem() {Content="Average (per type)", ToolTip="More likely to be strong against types that already have many weaknesses, more likely to be weak to types that already have many strengths"},
                new ComboBoxItem() {Content="Per type (inverted)", ToolTip="More likely to be weak to types that already have many weaknesses, more likely to be strong against types that already have many strengths"},
                new Separator(),
                new ComboBoxItem() {Content="Dynamic", IsEnabled=false, ToolTip="Ranomly choose a weighting (by another weighting)"},
                new ComboBoxItem() {Content="Custom", IsEnabled=false, ToolTip="Make your own weighting!" }
            };
        }
        public CompositeCollection TypeWeightDropDown
        {
            get => new CompositeCollection
            {
                new ComboBoxItem() { Content="None", ToolTip="Each type has an equal chance of being picked" },
                new ComboBoxItem() { Content="Type Occurence (Any)", ToolTip="Each type's weight is its number of occurences as a single, primary, or secondary type in the base ROM" },
                new ComboBoxItem() { Content="Type Occurence (Single)", ToolTip="Each type's weight is its number of occurences as a single type in the base ROM" },
                new ComboBoxItem() { Content="Type Occurence (Primary)", ToolTip="Each type's weight is its number of occurences as a primary type (the first type on a dual-typed pokemon) in the base ROM" },
                new ComboBoxItem() { Content="Type Occurence (Secondary)", ToolTip="Each type's weight is its number of occurences as a secondary type (the second type on a dual-typed pokemon) in the base ROM" },
            };
        }
        #endregion

        private RomData Data { get; set; }
        private RomMetadata Metadata { get; set; }

        private string[] LastRandomizationInfo { get; set; }

        private const string romFileFilter = "GBA ROM|*.gba";
        private const string saveRomPrompt = "Save ROM";
        private const string openRomPrompt = "Open ROM";

        public MainWindow()
        {
            IsROMLoaded = false;
            InitializeComponent();
            this.DataContext = this;
        }


        private bool GetRomData(byte[] rawRom)
        {
            // Initialize ROM metadata
            RomMetadata metadata = new RomMetadata(rawRom);
            // Read ROM data
            if (metadata.Gen == Generation.III)
            {
                XmlManager info = new XmlManager(PokemonRandomizer.Resources.RomInfo.RomInfo.Gen3RomInfo);
                info.SetSearchRoot(metadata.Code + metadata.Version.ToString());
                //Initalize Rom file wrapper
                var freeSpaceByte = (byte)info.HexAttr("freeSpace", "byte");
                var searchStartOffset = info.HexAttr("freeSpace", "startAddy");
                var rom = new Rom(rawRom, freeSpaceByte, searchStartOffset);
                // Parse the file
                Data = Backend.Reading.Gen3RomParser.Parse(rom, metadata, info);
            }
            else
            {
                lblMessageBoxContent.Content = "Failed to open ROM - unsupported generation: " + metadata.Gen.ToString();
                return false;
            }
            IsROMLoaded = true;
            lblMessageBoxContent.Content = "ROM opened: " + metadata.Name + " (" + metadata.Code + ")";
            LastRandomizationInfo = Data.ToStringArray();
            Metadata = metadata;
            return true;
        }

        private void OpenRomNoWindow(string path)
        {
            GetRomData(File.ReadAllBytes(path));
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
            var openFileDialog = new OpenFileDialog
            {
                Filter = romFileFilter,
                Title = openRomPrompt
            };
            if (openFileDialog.ShowDialog() == true)
            {
                GetRomData(File.ReadAllBytes(openFileDialog.FileName));
            }
        }

        private void Save_ROM(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = romFileFilter,
                Title = saveRomPrompt
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                var randomzier = new Backend.Randomization.Randomizer(Data, new Settings(this));
                var randomizedData = randomzier.Randomize();
                File.WriteAllBytes(saveFileDialog.FileName, Backend.Writing.Gen3RomWriter.Write(randomizedData));
                LastRandomizationInfo = randomizedData.ToStringArray();
            }
        }

        private void Generate_Info_Doc(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "txt file|*.txt",
                Title = "Generate Info Docs"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, LastRandomizationInfo);
            }
        }

        private void Convert_Table(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = ".csv file|*.csv",
                Title = "Convert csv"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                Backend.Utilities.TableReader.TableToDictFormat(openFileDialog.FileName, ',', 15);
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

        private void Open_ROM_And_Save_Bypass(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = romFileFilter,
                Title = "Open ROM and Save (Bypass Randomization)"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                GetRomData(File.ReadAllBytes(openFileDialog.FileName));
                var saveFileDialog = new SaveFileDialog
                {
                    Filter = romFileFilter,
                    Title = saveRomPrompt,
                    FileName = "testROM.gba"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllBytes(saveFileDialog.FileName, Backend.Writing.Gen3RomWriter.Write(Data));
                }
            }
        }
    }
    public static class Commands
    {
        public static readonly RoutedCommand addTreeItem = new RoutedCommand();
        public static readonly RoutedCommand rmTreeItem = new RoutedCommand();
    }
}
