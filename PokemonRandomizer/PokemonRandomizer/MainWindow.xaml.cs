using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Collections.Generic;

namespace PokemonRandomizer
{
    using Backend.DataStructures;
    using Backend.EnumTypes;
    using Backend.Reading;
    using Backend.Utilities;
    using Backend.Utilities.Debug;
    using Backend.Writing;
    using PokemonRandomizer.AppSettings;
    using UI;
    using UI.Models;
    using UI.Views;
    using Windows;
    using static Settings;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public const string version = "v0.2.5";
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

        public bool LogNotEmpty => Logger.main.Count > 0;

        #endregion

        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();

        private RomData RomData { get; set; }
        private Rom Rom { get; set; }
        private RomMetadata Metadata
        {
            get => metadata;
            set
            {
                metadata = value;
                appSettings.Metadata = value;
            }
        }
        private RomMetadata metadata;
        private XmlManager RomInfo { get; set; }
        private RomParser Parser { get; set; }

        private string[] LastRandomizationInfo { get; set; }

        private const string openRomFileFilter = "Rom Files (*.gba,*.nds)|*gba;*nds|" + gbaRomFileFilter + "|" + ndsRomFileFilter;
        private const string gbaRomFileFilter = "GBA Roms (*.gba)|*.gba";
        private const string ndsRomFileFilter = "NDS Roms (*.nds)|*.nds";
        private const string textFileFilter = "txt files (*.txt)|*.txt";
        private const string csvFileFilter = "csv files (*.csv)|*.csv";
        private const string saveRomPrompt = "Save Rom";
        private const string openRomPrompt = "Open Rom";
        private const string openRomError = "Failed to open rom: ";

        private readonly RandomizerDataModel randomizerData = new RandomizerDataModel();
        private readonly TmHmTutorModel tmHmData = new TmHmTutorModel();
        private readonly SpecialPokemonDataModel specialPokemonData = new SpecialPokemonDataModel(new StartersDataModel(), new InGameTradesDataModel(), new GiftPokemonDataModel());
        private readonly PokemonTraitsModel pokemonData = new PokemonTraitsModel();
        private readonly WildEncounterDataModel wildEncounterData = new WildEncounterDataModel();
        // Later have preset ones for special groups
        private readonly TrainerDataModel[] trainerDataModels = new TrainerDataModel[]
        {
            new TrainerDataModel(TrainerCategory.Trainer, DataModel.defaultName),
            new TrainerDataModel(TrainerCategory.AceTrainer, "Ace Trainers"),
            new TrainerDataModel(TrainerCategory.Rival, "Rivals"),
            new TrainerDataModel(TrainerCategory.GymLeader, "Gym Leaders"),
            new TrainerDataModel(TrainerCategory.EliteFour, "Elite Four"),
            new TrainerDataModel(TrainerCategory.Champion, "Champion"),
        };
        private readonly ItemDataModel itemData = new ItemDataModel();
        private readonly WeatherDataModel weatherData = new WeatherDataModel();
        private readonly MiscDataModel miscData = new MiscDataModel();


        public Settings AppSettings => UseHardCodedSettings ? hardCodedSettings : appSettings;

#if DEBUG
        public bool UseHardCodedSettings { get; set; } = true;
#else
        public bool UseHardCodedSettings { get; set; } = false;
#endif
        private readonly HardCodedSettings hardCodedSettings;
        private readonly AppSettings.AppSettings appSettings;

        private int errorCount = 0;

        public MainWindow()
        {
            hardCodedSettings = new HardCodedSettings(randomizerData);
            appSettings = new AppSettings.AppSettings(randomizerData, specialPokemonData, tmHmData, pokemonData, wildEncounterData, trainerDataModels, itemData, weatherData, miscData);

            IsROMLoaded = false;
            InitializeComponent();
            this.DataContext = this;
            RandomizerView.Content = new RandomizerDataView(randomizerData);
            // Create pokemon traits UI
            var pokemonTraitsGroup = new GroupUI<PokemonTraitsDataView, PokemonTraitsModel>(TraitsGroupsPanel, TraitsViewPanel, pokemonData);
            pokemonTraitsGroup.SetAddButtonVisibility(Visibility.Hidden);
            TmHmTutorView.Content = new TmHmTutorDataView(tmHmData);
            WildPokemonView.Content = new WildEncounterDataView(wildEncounterData);
            var trainerTraitsGroup = new GroupUI<TrainerDataView, TrainerDataModel>(TrainerGroups, TrainerView, trainerDataModels);
            trainerTraitsGroup.SetAddButtonVisibility(Visibility.Hidden);
            ItemsView.Content = new ItemDataView(itemData);
            WeatherView.Content = new WeatherDataView(weatherData);
            Logger.main.OnLog += OnLog;
        }

        private void OnLog(Logger.LogData data)
        {
            if(data.level != Logger.Level.Info)
            {
                if(data.level == Logger.Level.Error)
                {
                    ++errorCount;
                    lblInfoBoxErrorCount.Content = errorCount + (errorCount == 1 ? " Error" : " Errors");
                }
                SetInfoBox(data.ToString());
            }
            if(Logger.main.Count == 1)
            {
                OnPropertyChanged("LogNotEmpty");
            }
        }

        private void SetInfoBox(string content)
        {
            lblInfoBoxContent.Content = content;
        }

        private void InitializeUI(RomData data, RomMetadata metadata)
        {
            var pokemon = new List<Pokemon>(data.PokemonNationalDexOrder.Length + 1);
            pokemon.Add(Pokemon.None);
            pokemon.AddRange(data.PokemonNationalDexOrder.Select(p => p.species));
            SpecialPokemonView.Content = new SpecialPokemonDataView(specialPokemonData, data.PokemonNames, pokemon);
            MiscView.Content = new MiscDataView(miscData, metadata);
        }

        private bool GetRomData(byte[] rawRom)
        {
            // Initialize ROM metadata
            RomMetadata metadata = new RomMetadata(rawRom);
            // Read ROM data
            if (metadata.Gen == Generation.III)
            {
                RomInfo = new XmlManager(PokemonRandomizer.Resources.RomInfo.RomInfo.Gen3RomInfo);
                RomInfo.SetSearchRoot(metadata.Code + metadata.Version.ToString());
                //Initalize Rom file wrapper
                var freeSpaceByte = (byte)RomInfo.HexAttr("freeSpace", "byte");
                var searchStartOffset = RomInfo.HexAttr("freeSpace", "startAddy");
                Rom = new Rom(rawRom, freeSpaceByte, searchStartOffset);
                // Parse the file
                Parser = new Gen3RomParser();
                RomData = Parser.Parse(Rom, metadata, RomInfo);
            }
            else
            {
                Logger.main.Error("Failed to open rom - unsupported generation (" + metadata.Gen.ToString() + ")");
                return false;
            }
            IsROMLoaded = true;
            // Log open and set info box
            string msg = "Rom opened: " + metadata.Name + " (" + metadata.Code + ")";
            Logger.main.Info(msg);
            SetInfoBox(msg);
            // Cache metadata and last randomization info
            LastRandomizationInfo = RomData.ToStringArray();
            Metadata = metadata;
            InitializeUI(RomData, Metadata);
            return true;
        }

        private byte[] GetRandomizedRom()
        {
            var copyData = Parser.Parse(Rom, Metadata, RomInfo);
            var randomzier = new Backend.Randomization.Randomizer(copyData, AppSettings);
            var randomizedData = randomzier.Randomize();
            LastRandomizationInfo = randomizedData.ToStringArray();
            if(Metadata.Gen == Generation.III)
            {
                var writer = new Gen3RomWriter();
                return writer.Write(randomizedData, Rom, Metadata, RomInfo, AppSettings).File;
            }
            throw new Exception("Attempting to write randomized data to ROM of unsupported generation (" + Metadata.Gen.ToString() + ")");
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

#region Menu Functions

        private void OpenROM(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = openRomFileFilter,
                Title = openRomPrompt
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    GetRomData(File.ReadAllBytes(openFileDialog.FileName));
                }
                catch(IOException exception)
                {
                    Logger.main.Error(openRomError + exception.Message);
                }
            }
        }

        private void SaveFile<T>(string path, string name, T[] file, Action<string, T[]> writeFn, string writingMsg = null)
        {
            SaveFile(path, name, () => file, writeFn, writingMsg);
        }

        private void SaveFile<T>(string path, string name, Func<T[]> fileFn, Action<string, T[]> writeFn, string writingMsg = null)
        {
            MainMenu.IsEnabled = false;
            MainTabControl.IsEnabled = false;
            SetInfoBox(writingMsg ?? "Saving " + name.ToLower() + "...");
            backgroundWorker.DoWork += (_, _2) =>
            {
                writeFn(path, fileFn());
            };
            backgroundWorker.RunWorkerCompleted += (_, error) =>
            {
                if (error.Error != null)
                {
                    Logger.main.Error("Failed to save " + name.ToLower() + ": " + error.Error.Message);
                }
                // Log open and set info box
                string msg = name + " saved to " + path;
                Logger.main.Info(msg);
                SetInfoBox(msg);
                MainMenu.IsEnabled = true;
                MainTabControl.IsEnabled = true;
            };
            backgroundWorker.RunWorkerAsync();
        }

        private void WriteRom(Func<byte[]> fileFn, string message)
        {
            string filter = gbaRomFileFilter;
            if (Metadata.Gen == Generation.IV)
                filter = ndsRomFileFilter;
            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = saveRomPrompt
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName, "Rom", fileFn, File.WriteAllBytes, message);     
            }
        }

        private void SaveROM(object sender, RoutedEventArgs e)
        {
            WriteRom(GetRandomizedRom, "Randomizing...");
        }

        private void SaveCleanROM(object sender, RoutedEventArgs e)
        {
            if (Metadata.Gen == Generation.III)
            {
                var writer = new Gen3RomWriter();
                WriteRom(() => writer.Write(RomData, Rom, Metadata, RomInfo, AppSettings).File, "Saving Clean Rom...");
            }
        }

        private void DiffRoms(object sender, RoutedEventArgs e)
        {
            if(Rom == null)
            {
                Logger.main.Error("Diff Error: Diff cannot be run with no open rom");
                return;
            }
            var openFileDialog = new OpenFileDialog
            {
                Filter = openRomFileFilter,
                Title = openRomPrompt
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Open Rom to diff
                    var rawRom2 = File.ReadAllBytes(openFileDialog.FileName);
                    var metadata2 = new RomMetadata(rawRom2);
                    if(metadata2.Gen != Metadata.Gen)
                    {
                        Logger.main.Error("Diff Error: Roms have different generations");
                        return;
                    }
                    var rom2 = new Rom(rawRom2, 0x00, 0x00); // No free space data, will do later
                    // Diff Roms
                    var diffData = RomDiff.Diff(Rom, rom2);
                    // Save diff file
                    var saveFileDialog = new SaveFileDialog
                    {
                        Filter = textFileFilter,
                        Title = "Save Diff File",
                        FileName = "diff",
                    };
                    if (saveFileDialog.ShowDialog() == true)
                    {
                        SaveFile(saveFileDialog.FileName, "Diff", diffData.Readout().ToArray(), File.WriteAllLines);
                    }
                }
                catch (IOException exception)
                {
                    Logger.main.Error(openRomError + exception.Message);
                }
            }
        }

        private void SaveLog(object sender, RoutedEventArgs e)
        {
            if (Logger.main.Count <= 0)
                return;
            // Save Log File
            var saveFileDialog = new SaveFileDialog
            {
                Filter = textFileFilter,
                Title = "Save Log File",
                FileName = "log",
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName, "Log", Logger.main.FullLogText.ToArray(), File.WriteAllLines);

            }
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            Logger.main.Clear();
            errorCount = 0;
            lblInfoBoxErrorCount.Content = string.Empty;
            OnPropertyChanged("LogNotEmpty");
        }

        private void GenerateInfoDoc(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = textFileFilter,
                Title = "Generate Info Docs"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName, "Info file", LastRandomizationInfo, File.WriteAllLines);
            }
        }

        private void ConvertTable(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = csvFileFilter,
                Title = "Convert csv"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                TableReader.TableToDictFormat(openFileDialog.FileName, ',', 15);
            }

        }

        private void ShowAboutWindow(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };
            aboutWindow.ShowDialog();
        }

#endregion
    }
}
