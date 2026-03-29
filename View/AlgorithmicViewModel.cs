//using CMGWpf.Model.Generators;
//using CMGWpf.Services;
//using CMGWpf.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Text;

//namespace CMGWpf.View
//{
//    public class AlgorithmicViewModel(Algorithmic generator) : ViewModelBase
//    {
//        public Algorithmic UIGenerator => generator.Clone(generator.Parent);
//        public static ObservableCollection<string> SoundFontFileNames => GlobalService.Instance.SoundFontFileNames;

//        private string soundFontFileName = string.Empty;
//        public string SoundFontFileName
//        {
//            get { return soundFontFileName; }
//            set
//            {
//                if (soundFontFileName != value)
//                {
//                    UIGenerator.SoundFont = SoundFontUtilities.GetSoundFont(value);
//                    PresetNames = new ObservableCollection<string>(UIGenerator.SoundFont.Presets.Select);
//                    OnPropertyChanged(nameof(PresetNames));
//                    soundFontFileName = value;
//                    OnPropertyChanged();
//                }
//            }
//        }
//        private ObservableCollection<string> presetNames = [];
//        public ObservableCollection<string> PresetNames
//        {
//            get { return presetNames; }
//            set { presetNames = value; OnPropertyChanged(); }
//        }
//        private string presetName = string.Empty;
//        public string PresetName
//        {
//            get { return presetName; }
//            //TODO load the preset from the soundfont file and populate the generator parameters with the preset values
//            set { presetName = value; OnPropertyChanged(); }
//        }

//    }
//}
