﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Resonance.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "15.0.1.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?>
<ArrayOfString xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <string>0.3</string>
  <string>0.5</string>
  <string>0.7</string>
  <string>0.9</string>
  <string>1.0</string>
  <string>1.1</string>
  <string>1.3</string>
  <string>1.5</string>
  <string>1.7</string>
  <string>2.0</string>
</ArrayOfString>")]
        public global::System.Collections.Specialized.StringCollection VoltageList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["VoltageList"]));
            }
            set {
                this["VoltageList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool EnableSweepWindow {
            get {
                return ((bool)(this["EnableSweepWindow"]));
            }
            set {
                this["EnableSweepWindow"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int DischargeUnit {
            get {
                return ((int)(this["DischargeUnit"]));
            }
            set {
                this["DischargeUnit"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("150")]
        public double MinCalibVelocity {
            get {
                return ((double)(this["MinCalibVelocity"]));
            }
            set {
                this["MinCalibVelocity"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("200")]
        public double MaxCalibVelocity {
            get {
                return ((double)(this["MaxCalibVelocity"]));
            }
            set {
                this["MaxCalibVelocity"] = value;
            }
        }
    }
}
