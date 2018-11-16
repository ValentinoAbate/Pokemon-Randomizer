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
using System.IO;

namespace PokemoneEmeraldRandomizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Backend.ROMData Data { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Open_ROM(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GBA ROM files|*.gba";
            openFileDialog.Title = "Open ROM";
            if (openFileDialog.ShowDialog() == true)
            {
                byte[] rawROM = File.ReadAllBytes(openFileDialog.FileName);
                Data = Backend.ROMParser.Parse(rawROM);
            }

        }

        private void Save_ROM(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "GBA ROM files|*.gba";
            saveFileDialog.Title = "Save ROM";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, Backend.ROMGenerator.GenerateROM(Data));
            }
        }
    }
}
