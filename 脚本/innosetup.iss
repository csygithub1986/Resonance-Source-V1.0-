; 脚本由 Inno Setup 脚本向导 生成！
; 有关创建 Inno Setup 脚本文件的详细资料请查阅帮助文档！

#define MyAppName "Linbo_DAC"
#define MyAppVersion "1.0"
#define MyAppExeName "Resonance.exe"

[Setup]
; 注: AppId的值为单独标识该应用程序。
; 不要为其他安装程序使用相同的AppId值。
; (生成新的GUID，点击 工具|在IDE中生成GUID。)
AppId={{10A40F28-33B7-44BB-9E80-8C19A1800561}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
DefaultDirName={pf}\{#MyAppName}
DefaultGroupName={#MyAppName}
OutputBaseFilename=setup
Compression=lzma
SolidCompression=yes

[Languages]
Name: "chinesesimp"; MessagesFile: "compiler:Default.isl"
Name: "english"; MessagesFile: "compiler:Languages\English.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 0,6.1

[Files]
Source: "E:\变频谐振\Resonance Source V1.0\Resonance\bin\Release\DynamicDataDisplay.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\变频谐振\Resonance Source V1.0\Resonance\bin\Release\itextsharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\变频谐振\Resonance Source V1.0\Resonance\bin\Release\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\变频谐振\Resonance Source V1.0\Resonance\bin\Release\Resonance.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\变频谐振\Resonance Source V1.0\Resonance\bin\Release\DynamicDataDisplay.dll"; DestDir: "{app}"; Flags: ignoreversion
; 注意: 不要在任何共享系统文件上使用“Flags: ignoreversion”

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"


