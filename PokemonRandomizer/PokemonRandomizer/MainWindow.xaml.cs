using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace PokemonRandomizer
{
    using Backend.DataStructures;
    using Backend.EnumTypes;
    using Backend.Reading;
    using Backend.Utilities;
    using Backend.Utilities.Debug;
    using Backend.Writing;
    using PokemonRandomizer.AppSettings;
    using System.Diagnostics;
    using System.IO.Compression;
    using System.Text;
    using System.Windows.Controls;
    using UI;
    using UI.Json;
    using UI.Models;
    using UI.Views;
    using Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public const string version = "v1.0-beta.4";
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
        private readonly JsonSerializerOptions serializerOptions = new JsonSerializerOptions();

        private readonly InfoFileGenerator infoFileGenerator = new InfoFileGenerator();

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
        private const string settingsFileFilter = "Randomizer Settings (*.pkrs)|*.pkrs";
        private const string csvFileFilter = "csv files (*.csv)|*.csv";
        private const string saveRomPrompt = "Save Rom";
        private const string openRomPrompt = "Open Rom";
        private const string openRomError = "Failed to open Rom";
        private const string checkAboutHelpMessage = "Please check Help->About for a list of supported ROMs";
        private const string debugSeed = "DEBUG";

        private ApplicationDataModel AppData { get; set; }

        public Settings AppSettings => UseHardCodedSettings ? hardCodedSettings : appSettings;

        public bool UseHardCodedSettings 
        { 
            get => useHardcodedSettings;
            set
            {
                useHardcodedSettings = value;
                HardcodedSettingsIndicator.Text = value ? "(Hardcoded Mode)" : string.Empty;
            } 
        }

        private bool useHardcodedSettings;

        private HardCodedSettings hardCodedSettings;
        private AppSettings.AppSettings appSettings;

        private int errorCount = 0;

        public MainWindow()
        {

            IsROMLoaded = false;
            InitializeComponent();
            this.DataContext = this;
            ToolTipService.ShowDurationProperty.OverrideMetadata(typeof(DependencyObject), new FrameworkPropertyMetadata(int.MaxValue));
            serializerOptions.Converters.Add(new WeightedSetJsonConverter());
            AppData = new ApplicationDataModel();
            InitializeAppData();
            UseHardCodedSettings = useHardcodedSettings;

            Logger.main.OnLog += OnLog;
        }

        private readonly List<Logger.LogData> logData = new List<Logger.LogData>();
        private void OnLog(Logger.LogData data)
        {
            logData.Add(data);
        }
        private void ProcessLogData()
        {
            if (logData.Count <= 0)
                return;
            int oldErrorCount = errorCount;
            string latestErrorMsg = string.Empty;
            foreach(var data in logData)
            {
                if (data.level == Logger.Level.Error)
                {
                    ++errorCount;
                    latestErrorMsg = data.message;
                }
            }
            if(errorCount != oldErrorCount)
            {
                lblInfoBoxErrorCount.Content = errorCount + (errorCount == 1 ? " Error" : " Errors");
                SetInfoBox(latestErrorMsg);
            }
            OnPropertyChanged("LogNotEmpty");
            logData.Clear();
        }
        private void LogError(string message)
        {
            Logger.main.Error(message);
            ProcessLogData();
        }

        private void LogInfo(string message)
        {
            Logger.main.Info(message);
            ProcessLogData();
        }

        private void SetInfoBox(string content)
        {
            lblInfoBoxContent.Content = content;
        }

        private void InitializeAppData()
        {
            if (hardCodedSettings == null)
            {
                hardCodedSettings = new HardCodedSettings(AppData);
            }
            else
            {
                hardCodedSettings.UpdateData(AppData);
            }
            if (appSettings == null)
            {
                appSettings = new AppSettings.AppSettings(AppData);
            }
            else
            {
                appSettings.UpdateData(AppData);
            }
            InitializeUI();
            if (IsROMLoaded)
            {
                InitializeRomDependentUI(RomData, Metadata);
            }
        }

        private void InitializeUI()
        {
            VariantPokemonView.Content = new VariantPokemonDataView(AppData.VariantPokemonData);
            PokemonTraitsView.Content = new PokemonTraitsDataView(AppData.PokemonData);
            TmHmTutorView.Content = new TmHmTutorDataView(AppData.TmHmTutorData);
            WildPokemonView.Content = new WildEncounterDataView(AppData.WildEncounterData);
            var trainerTraitsGroup = new GroupUI<TrainerDataView, TrainerDataModel>(TrainerGroups, TrainerView, AppData.TrainerDataModels);
            trainerTraitsGroup.SetAddButtonVisibility(Visibility.Hidden);
            ItemsView.Content = new ItemDataView(AppData.ItemData);
            WeatherView.Content = new WeatherDataView(AppData.WeatherData);
        }

        private void InitializeRomDependentUI(RomData data, RomMetadata metadata)
        {
            var pokemon = new List<Pokemon>(data.PokemonNationalDexOrder.Length + 1);
            pokemon.Add(Pokemon.None);
            pokemon.AddRange(data.PokemonNationalDexOrder.Select(p => p.species));
            SpecialPokemonView.Content = new SpecialPokemonDataView(AppData.SpecialPokemonData, data.PokemonNames, pokemon);
            MiscView.Content = new MiscDataView(AppData.MiscData, metadata);
        }

        private void SetLastRandomizationInfo(RomData data, RomMetadata metadata, bool includeSettingsString)
        {
            if (includeSettingsString)
            {
                LastRandomizationInfo = infoFileGenerator.GenerateInfoFile(data, metadata, UseHardCodedSettings ? "Hardcoded" : GenerateCompressedPresetString());
            }
            else
            {
                LastRandomizationInfo = infoFileGenerator.GenerateInfoFile(data, metadata);
            }
        }

        private void GetRomData(byte[] rawRom)
        {
            // Initialize ROM metadata
            RomMetadata metadata = new RomMetadata(rawRom);
            // Read ROM data
            if (metadata.Gen == Generation.III)
            {
                if(!(metadata.IsEmerald || (metadata.IsFireRed && metadata.Version == 0)))
                {
                    throw new Exception($"Unsupported Rom (Code {metadata.Code}{metadata.Version}). {checkAboutHelpMessage}");
                }
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
                throw new Exception($"Unsupported generation (Gen {metadata.Gen}). {checkAboutHelpMessage}");
            }

            // Cache metadata and last randomization 
            SetLastRandomizationInfo(RomData, metadata, false);
            Metadata = metadata;
        }

        private byte[] GetRandomizedRom(string seed)
        {
            var copyData = Parser.Parse(Rom, Metadata, RomInfo);
            var randomzier = new Backend.Randomization.Randomizer(copyData, AppSettings, seed);
            var randomizedData = randomzier.Randomize();
            SetLastRandomizationInfo(randomizedData, Metadata, true);
            if(Metadata.Gen == Generation.III)
            {
                var writer = new Gen3RomWriter();
                return writer.Write(randomizedData, Rom, Metadata, RomInfo, AppSettings).File;
            }
            throw new Exception($"Attempting to write randomized data to Rom of unsupported generation (Gen {Metadata.Gen})");
        }

#region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string prop)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

#endregion

#region Menu Functions

        private void SetUIEnabled(bool enabled)
        {
            MainMenu.IsEnabled = enabled;
            MainTabControl.IsEnabled = enabled && IsROMLoaded;
        }

        private void PauseUIAndRunInBackground(string progressMsg, Action work, Action onSuccess, Action<Exception> onError)
        {
            SetUIEnabled(false);
            SetInfoBox(progressMsg);
            void DoWork(object _, DoWorkEventArgs _2)
            {
                backgroundWorker.DoWork -= DoWork;
                work?.Invoke();
            }
            void RunWorkerCompleted(object _, RunWorkerCompletedEventArgs args)
            {
                backgroundWorker.DoWork -= DoWork;
                backgroundWorker.RunWorkerCompleted -= RunWorkerCompleted;
                if (args.Error != null)
                {
                    onError?.Invoke(args.Error);
                }
                else
                {
                    onSuccess?.Invoke();
                }
                SetUIEnabled(true);
            }
            backgroundWorker.DoWork += DoWork;
            backgroundWorker.RunWorkerCompleted += RunWorkerCompleted;
            backgroundWorker.RunWorkerAsync();
        }

        private void OpenROM(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = openRomFileFilter,
                Title = openRomPrompt
            };
            if (openFileDialog.ShowDialog() == true)
            {
                void Open() => GetRomData(File.ReadAllBytes(openFileDialog.FileName));
                void Error(Exception e) => LogError($"{openRomError}: {e.Message}");
                void Success()
                {
                    IsROMLoaded = true;
                    // Log open and set info box
                    string msg = $"Rom opened: {metadata.Name} ({metadata.Code})"; ;
                    LogInfo(msg);
                    SetInfoBox(msg);
                    InitializeRomDependentUI(RomData, Metadata);
                }
                PauseUIAndRunInBackground("Opening Rom...", Open, Success, Error);
            }
        }

        private void SaveFile<T>(string path, string name, T[] file, Action<string, T[]> writeFn, string writingMsg = null, Action<bool> onComplete = null)
        {
            SaveFile(path, name, () => file, writeFn, writingMsg, onComplete);
        }

        private void SaveFile<T>(string path, string name, Func<T[]> fileFn, Action<string, T[]> writeFn, string writingMsg = null, Action<bool> onComplete = null)
        {
            void Save() => writeFn(path, fileFn());
            void Error(Exception e)
            {
                LogError($"Failed to save {name.ToLower()}: {e.Message}");
                onComplete?.Invoke(false);
            }
            void Success()
            {
                // Log open and set info box
                string msg = $"{name} saved to {path}";
                LogInfo(msg);
                SetInfoBox(msg);
                onComplete?.Invoke(true);
            }
            PauseUIAndRunInBackground(writingMsg ?? $"Saving {name.ToLower()}...", Save, Success, Error);
        }

        private void WriteRom(Func<byte[]> fileFn, string message, Action<bool> onComplete = null)
        {
            string filter = gbaRomFileFilter;
            if (Metadata.Gen == Generation.IV)
                filter = ndsRomFileFilter;
            var saveFileDialog = new SaveFileDialog
            {
                Filter = filter,
                Title = saveRomPrompt,
#if DEBUG
                FileName = "testROM"
#endif
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName, "Rom", fileFn, File.WriteAllBytes, message, onComplete);     
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }

        private void ReenableUIOnFailure(bool success)
        {
            if (!success)
            {
                SetUIEnabled(true);
            }
        }

        private void PostRandomizationUIFlow(bool success)
        {
            if (!success)
            {
                SetUIEnabled(true);
                return;
            }
            //void AskToSaveSetttingsFile(bool success)
            //{
            //    var promptWindow = new PromptWindow()
            //    {
            //        Owner = this
            //    };
            //    bool? result = promptWindow.ShowDialog("Save Settings File?", "The Settings File contains the randomizer settings you used.");
            //    if(result == true)
            //    {
            //        SavePreset(null);
            //    }
            //}
            var promptWindow = new PromptWindow()
            {
                Owner = this
            };
            bool? result = promptWindow.ShowDialog("Save Randomization Info File?", "The Randomization Info File contains your seed, settings string, and randomization results.");
            if (result == true)
            {
                GenerateInfoDoc(null);
            }
        }

        private const string randomizingProgressMessage = "Randomizing...";
        private void SaveROM(object sender, RoutedEventArgs e)
        {
            var inputWindow = new InputPromptWindow()
            {
                Owner = this
            };
            SetUIEnabled(false);
            if(inputWindow.ShowDialog("Use Seed?", "Enter a seed, or leave blank for a random seed", "Confirm", "Cancel") == true)
            {
                
                if (UseHardCodedSettings)
                {
                    WriteRom(() => GetRandomizedRom(inputWindow.Output), randomizingProgressMessage, ReenableUIOnFailure);
                }
                else
                {
                    WriteRom(() => GetRandomizedRom(inputWindow.Output), randomizingProgressMessage, PostRandomizationUIFlow);
                }
            }
            else
            {
                SetUIEnabled(true);
            }
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
                LogError("Diff Error: Diff cannot be run with no open rom");
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
                        LogError("Diff Error: Roms have different generations");
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
                    LogError($"{openRomError}: {exception.Message}");
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
                SaveFile(saveFileDialog.FileName, "Log", Logger.main.FullLogText, File.WriteAllLines);
            }
        }

        private void ClearLog(object sender, RoutedEventArgs e)
        {
            Logger.main.Clear();
            errorCount = 0;
            lblInfoBoxErrorCount.Content = string.Empty;
            OnPropertyChanged("LogNotEmpty");
        }

        private void QuickRandomize(object sender, RoutedEventArgs e)
        {
            void QuickRand() => GetRandomizedRom(debugSeed);
            void Error(Exception e)
            {
                LogError($"Quick Randomization Error: {e.Message}");
            }
            void Success()
            {
                // Log open and set info box
                string msg = "Quick randomization complete";
                LogInfo(msg);
                SetInfoBox(msg);
            }
            PauseUIAndRunInBackground("Quick randomizing...", QuickRand, Success, Error);
        }

        private void GenerateInfoDoc(object sender, RoutedEventArgs e)
        {
            GenerateInfoDoc(null);
        }

        private void GenerateInfoDoc(Action<bool> onComplete)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = textFileFilter,
                Title = "Generate Info Docs",
                FileName = "info",
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName, "Info file", LastRandomizationInfo, File.WriteAllLines, null, onComplete);
            }
            else
            {
                onComplete?.Invoke(false);
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

        private void SavePreset(object sender, RoutedEventArgs e)
        {
            SavePreset(null);
        }

        private void SavePreset(Action<bool> onComplete)
        {
            // Save Log File
            var saveFileDialog = new SaveFileDialog
            {
                Filter = settingsFileFilter,
                Title = "Save Randomizer Settings",
                FileName = "settings",
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFile(saveFileDialog.FileName, "Settings", () => new string[] { GenerateCompressedPresetString() }, File.WriteAllLines, null, onComplete);
            }
            else
            {
                onComplete?.Invoke(false);
            }
        }

        private void CopyPresetStringToClipboard(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GenerateCompressedPresetString());
        }

        private string GeneratePresetString()
        {
            return JsonSerializer.Serialize(AppData, serializerOptions);
        }

        private string GenerateCompressedPresetString()
        {
            var compressed = CompressBytes(Encoding.Unicode.GetBytes(GeneratePresetString()));
            return Convert.ToBase64String(compressed);
        }

        private static byte[] CompressBytes(byte[] bytes)
        {
            using (var outputStream = new MemoryStream())
            {
                using (var compressStream = new BrotliStream(outputStream, CompressionLevel.Optimal))
                {
                    compressStream.Write(bytes, 0, bytes.Length);
                }
                outputStream.Flush();
                return outputStream.ToArray();
            }
        }

        private static byte[] DecompressBytes(byte[] bytes)
        {
            using (var inputStream = new MemoryStream(bytes))
            {
                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new BrotliStream(inputStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                    }
                    outputStream.Flush();
                    return outputStream.ToArray();
                }
            }
        }

        private void LoadPreset(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = settingsFileFilter,
                Title = "Load Randomizer Preset"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                void Load()
                {
                    var file = File.ReadAllLines(openFileDialog.FileName);
                    if (file.Length <= 0)
                    {
                        throw new IOException("Preset load error: Empty preset file");
                    }
                    try
                    {
                        DecompressAndApplyPreset(file[0]);
                    }
                    catch
                    {
                        ApplyPreset(file[0]);
                    }
                }
                void Error(Exception e) => LogError($"Preset load error: {e.Message}");
                void Success()
                {
                    OnPresetLoaded($"Preset loaded: {openFileDialog.FileName}");
                }
                PauseUIAndRunInBackground("Loading preset...", Load, Success, Error);
            }
        }

        private void LoadPresetString(object sender, RoutedEventArgs e)
        {
            var stringPrompt = new InputPromptWindow
            {
                Owner = this
            };
            SetUIEnabled(false);
            if(stringPrompt.ShowDialog("Load Settings String?", "Paste Settings String", "Ok", "Cancel") == true && !string.IsNullOrWhiteSpace(stringPrompt.Output))
            {
                void Load()
                {
                    DecompressAndApplyPreset(stringPrompt.Output);
                }
                void Error(Exception e) => LogError($"Preset load error: {e.Message}");
                void Success()
                {
                    OnPresetLoaded("Preset string loaded successfully");
                }
                PauseUIAndRunInBackground("Loading preset...", Load, Success, Error);
            }
            else
            {
                SetUIEnabled(true);
            }
        }

        private void OnPresetLoaded(string msg)
        {
            // Log open and set info box
            LogInfo(msg);
            SetInfoBox(msg);
            InitializeAppData();
        }

        private void DecompressAndApplyPreset(string compressedPreset)
        {
            var compressed = Convert.FromBase64String(compressedPreset);
            var decompressed = DecompressBytes(compressed);
            ApplyPreset(Encoding.Unicode.GetString(decompressed));

        }

        private void ApplyPreset(string preset)
        {
            AppData = JsonSerializer.Deserialize<ApplicationDataModel>(preset, serializerOptions);
        }

        private void LoadDefaultPreset(object sender, RoutedEventArgs e)
        {
            AppData = new ApplicationDataModel();
            OnPresetLoaded("Default preset loaded successfully");
        }

        private void ShowAboutWindow(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow
            {
                Owner = this
            };
            SetUIEnabled(false);
            aboutWindow.ShowDialog();
            SetUIEnabled(true);
        }

        private void GoToDownloadPage(object sender, RoutedEventArgs e)
        {
            string url = "https://github.com/ValentinoAbate/Pokemon-Randomizer/releases";
            Process.Start(new ProcessStartInfo(url));
            e.Handled = true;
        }

        #endregion


    }
}
