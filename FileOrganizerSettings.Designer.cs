﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FileOrganizer {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class FileOrganizerSettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static FileOrganizerSettings defaultInstance = ((FileOrganizerSettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new FileOrganizerSettings())));
        
        public static FileOrganizerSettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("%HOMEPATH%\\Pictures\\Scans")]
        public string FilesToOrganizeDirectory {
            get {
                return ((string)(this["FilesToOrganizeDirectory"]));
            }
            set {
                this["FilesToOrganizeDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string OrganizedFilesDirectory {
            get {
                return ((string)(this["OrganizedFilesDirectory"]));
            }
            set {
                this["OrganizedFilesDirectory"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("%Y_%M_%D - ")]
        public string OrganizedFileNamePrefix {
            get {
                return ((string)(this["OrganizedFileNamePrefix"]));
            }
            set {
                this["OrganizedFileNamePrefix"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(" - pg %P")]
        public string OrganizedFileNameSuffix {
            get {
                return ((string)(this["OrganizedFileNameSuffix"]));
            }
            set {
                this["OrganizedFileNameSuffix"] = value;
            }
        }
    }
}
