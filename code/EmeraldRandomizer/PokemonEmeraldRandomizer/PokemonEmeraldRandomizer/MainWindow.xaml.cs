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

namespace PokemonEmeraldRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
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
    }
}
