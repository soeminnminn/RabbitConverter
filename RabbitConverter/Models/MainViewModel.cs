using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Threading.Tasks;
using Microsoft.Win32;
using RabbitConverter.Observable;
using RabbitConverter.Commands;

namespace RabbitConverter.Models
{
    public class MainViewModel : ObservableObject
    {
        #region Variables
        private static readonly string SAMPLE_UNI = @"သီဟိုဠ်မှ ဉာဏ်ကြီးရှင်သည် အာယုဝဍ္ဎနဆေးညွှန်းစာကို ဇလွန်ဈေးဘေး ဗာဒံပင်ထက် အဓိဋ္ဌာန်လျက် ဂဃနဏဖတ်ခဲ့သည်။";
        private static readonly string SAMPLE_ZAWGYI = @"သီဟိုဠ္မွ ဉာဏ္ႀကီးရွင္သည္ အာယုဝၯနေဆးၫႊန္းစာကို ဇလြန္ေဈးေဘး ဗာဒံပင္ထက္ အဓိ႒ာန္လ်က္ ဂဃနဏဖတ္ခဲ့သည္။";

        private OpenFileDialog openFileDialog = new()
        {
            Title = "Open file",
            Filter = "Text File|*.txt|All Files|*.*",
            CheckFileExists = true
        };

        private SaveFileDialog saveFileDialog = new()
        {
            Title = "Save file",
            Filter = "Text File|*.txt|All Files|*.*",
            OverwritePrompt = true,
            AddExtension = true,
        };

        private bool textWrapping = true;
        private bool isProcessing = false;
        private Rabbit rabbit = new();
        private string zawgyiText = SAMPLE_ZAWGYI;
        private string unicodeText = SAMPLE_UNI;

        #endregion

        #region Constructors
        public MainViewModel()
        {
        }
        #endregion

        #region Properties
        public bool IsProcessing
        {
            get => isProcessing;
            set { SetProperty(ref isProcessing, value); }
        }

        public string ZawgyiText
        {
            get => zawgyiText;
            set 
            { 
                if (SetProperty(ref zawgyiText, value, nameof(ZawgyiText), OnZawgyiChanged))
                {
                    OnPropertyChanged(nameof(IsEmpty));
                }
            }
        }

        public string UnicodeText
        {
            get => unicodeText;
            set 
            { 
                if (SetProperty(ref unicodeText, value, nameof(UnicodeText), OnUnicodeChanged))
                {
                    OnPropertyChanged(nameof(IsEmpty));
                }
            }
        }

        public ICommand OpenCommand
        {
            get => new Command<string>(OnOpenFile);
        }

        public ICommand SaveCommand
        {
            get => new Command<string>(OnSaveFile, CanSaveFile);
        }

        public ICommand ClearCommand
        {
            get => new Command(OnClear, CanClear);
        }

        public TextWrapping WordWrap
        {
            get => textWrapping ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        public bool IsWordWrap
        {
            get => textWrapping;
            set 
            {
                if (SetProperty(ref textWrapping, value))
                {
                    OnPropertyChanged(nameof(WordWrap));
                }
            }
        }

        public bool IsEmpty
        {
            get => string.IsNullOrEmpty(zawgyiText) || string.IsNullOrEmpty(unicodeText);
        }
        #endregion

        #region Methods
        private static bool IsTextFile(out Encoding encoding, string filePath, int windowSize = 10240)
        {
            FileStream fileStream;
            try
            {
                fileStream = File.OpenRead(filePath);
            }
            catch
            {
                encoding = null;
                return false;
            }

            var rawData = new byte[windowSize];
            var text = new char[windowSize];
            var isText = true;

            // Read raw bytes
            var rawLength = fileStream.Read(rawData, 0, rawData.Length);
            fileStream.Seek(0, SeekOrigin.Begin);

            // Detect encoding correctly (from Rick Strahl's blog)
            // http://www.west-wind.com/weblog/posts/2007/Nov/28/Detecting-Text-Encoding-for-StreamReader
            encoding = rawData[0] switch
            {
                0x00 when rawData[1] == 0x00 && rawData[2] == 0xfe && rawData[3] == 0xff => Encoding.UTF32,
                // 0x2b when rawData[1] == 0x2f && rawData[2] == 0x76 => Encoding.UTF7,
                0xef when rawData[1] == 0xbb && rawData[2] == 0xbf => Encoding.UTF8,
                0xfe when rawData[1] == 0xff => Encoding.Unicode,
                _ => Encoding.Default
            };

            // Read text and detect the encoding
            using (var streamReader = new StreamReader(fileStream))
            {
                streamReader.Read(text, 0, text.Length);
            }

            using var memoryStream = new MemoryStream();
            using var streamWriter = new StreamWriter(memoryStream, encoding);
            // Write the text to a buffer
            streamWriter.Write(text);
            streamWriter.Flush();

            // Get the buffer from the memory stream for comparision
            var memoryBuffer = memoryStream.GetBuffer();

            // Compare only bytes read
            for (var i = 0; i < rawLength && isText; i++)
            {
                isText = rawData[i] == memoryBuffer[i];
            }

            return isText;
        }

        private void OnOpenFile(string parameter)
        {
            ConvertTypes convert = parameter == "zawgyi" ? ConvertTypes.ZawgyiOne : parameter == "unicode" ? ConvertTypes.Unicode : ConvertTypes.None;
            if (convert == ConvertTypes.ZawgyiOne)
            {
                openFileDialog.Title = "Open file (Zawgyi-One)";
            }
            else if (convert == ConvertTypes.Unicode)
            {
                openFileDialog.Title = "Open file (Unicode)";
            }

            if (openFileDialog.ShowDialog(Application.Current.MainWindow) == true)
            {
                ConvertFile(openFileDialog.FileName, convert);
            }
        }

        private bool CanSaveFile(string parameter)
        {
            if (parameter == "zawgyi")
                return !string.IsNullOrEmpty(zawgyiText);
            if (parameter == "unicode")
                return !string.IsNullOrEmpty(unicodeText);

            return !string.IsNullOrEmpty(zawgyiText) || !string.IsNullOrEmpty(unicodeText);
        }

        private void OnSaveFile(string parameter)
        {
            ConvertTypes convert = parameter == "zawgyi" ? ConvertTypes.ZawgyiOne : parameter == "unicode" ? ConvertTypes.Unicode : ConvertTypes.None;
            if (convert == ConvertTypes.ZawgyiOne)
            {
                saveFileDialog.Title = "Save file (Zawgyi-One)";
            }
            else if (convert == ConvertTypes.Unicode)
            {
                saveFileDialog.Title = "Save file (Unicode)";
            }
            if (saveFileDialog.ShowDialog(Application.Current.MainWindow) == true)
            {
                SaveFile(saveFileDialog.FileName, convert);
            }
        }

        private bool CanClear() => !IsEmpty;

        private void OnClear()
        {
            Clear();
        }

        private async void OnZawgyiChanged()
        {
            if (IsProcessing) return;
            IsProcessing = true;

            if (!string.IsNullOrEmpty(zawgyiText))
            {
                string converted = string.Empty;
                await Task.Factory.StartNew(() =>
                {
                    converted = rabbit.Zg2Uni(zawgyiText);
                });
                UnicodeText = converted;
            }

            IsProcessing = false;
        }

        private async void OnUnicodeChanged()
        {
            if (IsProcessing) return;
            IsProcessing = true;

            if (!string.IsNullOrEmpty(unicodeText))
            {
                string converted = string.Empty;
                await Task.Factory.StartNew(() =>
                {
                    converted = rabbit.Uni2Zg(unicodeText);
                });
                ZawgyiText = converted;
            }

            IsProcessing = false;
        }

        private async void ConvertFile(string fileName, ConvertTypes convertFrom)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            if (!File.Exists(fileName)) return;

            if (IsProcessing) return;
            IsProcessing = true;

            List<string> fileZawgyi = new();
            List<string> fileUnicode = new();

            await Task.Factory.StartNew(() =>
            {
                if (IsTextFile(out Encoding encoding, fileName) && encoding != Encoding.ASCII)
                {
                    FileInfo file = new(fileName);
                    using (var reader = new StreamReader(file.OpenRead()))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();
                            if (string.IsNullOrEmpty(line))
                            {
                                fileZawgyi.Add(line);
                                fileUnicode.Add(line);
                            }
                            else if(convertFrom == ConvertTypes.ZawgyiOne)
                            {
                                fileZawgyi.Add(line);
                                fileUnicode.Add(rabbit.Zg2Uni(line));
                            }
                            else if (convertFrom == ConvertTypes.Unicode)
                            {
                                fileZawgyi.Add(rabbit.Uni2Zg(line));
                                fileUnicode.Add(line);
                            }
                        }
                    }
                }
            });

            ZawgyiText = string.Join(Environment.NewLine, fileZawgyi.ToArray());
            UnicodeText = string.Join(Environment.NewLine, fileUnicode.ToArray());

            IsProcessing = false;
        }

        private async void SaveFile(string fileName, ConvertTypes convert)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            if (File.Exists(fileName)) return;

            string text = convert == ConvertTypes.ZawgyiOne ? zawgyiText : unicodeText;
            if (string.IsNullOrEmpty(text)) return;

            if (IsProcessing) return;
            IsProcessing = true;
            
            await Task.Factory.StartNew(() => 
            {
                FileInfo file = new(fileName);

                try
                {
                    if (file.Exists)
                    {
                        file.Delete();
                    }

                    if (!file.Directory.Exists)
                    {
                        file.Directory.Create();
                    }

                    using (var stream = new StreamWriter(file.Create(), Encoding.UTF8))
                    {
                        stream.Write(text);
                        stream.Flush();
                    }
                }
                catch (Exception)
                { }
            });

            IsProcessing = false;
        }

        public void Clear()
        {
            if (IsProcessing) return;
            IsProcessing = true;

            ZawgyiText = string.Empty;
            UnicodeText = string.Empty;

            IsProcessing = false;
        }
        #endregion

        #region Nested Types
        private enum ConvertTypes
        {
            None,
            Unicode,
            ZawgyiOne
        }
        #endregion
    }
}
