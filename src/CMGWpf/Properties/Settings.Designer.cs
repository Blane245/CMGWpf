//------------------------------------------------------------------------------
// Define the application and user level settings
//      Application level settings
//          Version - the version of the CMG application
//          
//      User level settings
//          CMGSoundFontLocation - the location of the SoundFont files
//          CMGRecentFiles - the recent file list joined with |
//          CMGRecordFormat - the format of Audio files
//          CMGIsSnap - whether interval snapping is enabled
//          CMGSnapIncrement - the increment for interval snapping
//          WindowLeft - the left position of the main window   
//          WindowTop - the top position of the main window
//          WindowWidth - the width of the main window
//          WindowHeight - the height of the main window
//          
//------------------------------------------------------------------------------

namespace CMGWpf.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.12.0.0")]
    public sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("4.1.0")]
        public string Version {
            get {
                return ((string)(this["Version"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("C:\\SoundFonts")]
        public string CMGSoundFontLocation
        {
            get
            {
                return ((string)(this["CMGSoundFontLocation"]));
            }
            set
            {
                this["CMGSoundFontLocation"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("mp3")]
        public string CMGRecordFormat
        {
            get
            {
                return ((string)(this["CMGRecordFormat"]));
            }
            set
            {
                this["CMGRecordFormat"] = value;
            }
        }
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("true")]
        public string CMGIsSnap
        {
            get
            {
                return ((string)(this["CMGIsSnap"]));
            }
            set
            {
                this["CMGIsSnap"] = value;
            }
        }
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public string CMGSnapIncrement
        {
            get
            {
                return ((string)(this["CMGSnapIncrement"]));
            }
            set
            {
                this["CMGSnapIncrement"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string CMGRecentFiles
        {
            get
            {
                return ((string)(this["CMGRecentFiles"]));
            }
            set
            {
                this["CMGRecentFiles"] = value;
            }
        }
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double WindowLeft
        {
            get
            {
                return ((double)(this["WindowLeft"]));
            }
            set
            {
                this["WindowLeft"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double WindowTop
        {
            get
            {
                return ((double)(this["WindowTop"]));
            }
            set
            {
                this["WindowTop"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1024")]
        public double WindowWidth
        {
            get
            {
                return ((double)(this["WindowWidth"]));
            }
            set
            {
                this["WindowWidth"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("768")]
        public double WindowHeight
        {
            get
            {
                return ((double)(this["WindowHeight"]));
            }
            set
            {
                this["WindowHeight"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Maximized")]
        public string WindowState
        {
            get
            {
                return ((string)(this["WindowState"]));
            }
            set
            {
                this["WindowState"] = value;
            }
        }

        //TODO define other settings

    }
}
