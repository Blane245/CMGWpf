//------------------------------------------------------------------------------
// Define the application and user level settings
//      Application level settings
//          Version - the version of the CMG application
//          CMGSoundFontLocation - the location of the SoundFont files
//          DbServer - the url of the CMG database server
//          DbPort - the port number of the CMG database server
//      User level settings
//          CMGRecentFiles - the recent file list joined with |
//          AudioFormat - the format of Audio files
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
        [global::System.Configuration.DefaultSettingValueAttribute("4.0")]
        public string Version {
            get {
                return ((string)(this["Version"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://blane-latitude-7290")]
        public string DbServer
        {
            get
            {
                return ((string)(this["DbServer"]));
            }
        }
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("8081")]
        public string DbPort
        {
            get
            {
                return ((string)(this["DbPort"]));
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
        [global::System.Configuration.DefaultSettingValueAttribute("mp3")]
        public string AudioFormat
        {
            get
            {
                return ((string)(this["AudioFormat"]));
            }
            set
            {
                this["AudioFormat"] = value;
            }
        }
        //TODO define other settings

    }
}
