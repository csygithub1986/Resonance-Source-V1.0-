; �ű��� Inno Setup �ű��� ���ɣ�
; �йش��� Inno Setup �ű��ļ�����ϸ��������İ����ĵ���

#define MyAppName "Linbo_DAC"
#define MyAppVersion "1.0"
#define MyAppExeName "Resonance.exe"

[Setup]
; ע: AppId��ֵΪ������ʶ��Ӧ�ó���
; ��ҪΪ������װ����ʹ����ͬ��AppIdֵ��
; (�����µ�GUID����� ����|��IDE������GUID��)
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
Source: "E:\��Ƶг��\Resonance Source V1.0\Resonance\bin\Release\DynamicDataDisplay.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\��Ƶг��\Resonance Source V1.0\Resonance\bin\Release\itextsharp.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\��Ƶг��\Resonance Source V1.0\Resonance\bin\Release\MahApps.Metro.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\��Ƶг��\Resonance Source V1.0\Resonance\bin\Release\Resonance.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "E:\��Ƶг��\Resonance Source V1.0\Resonance\bin\Release\DynamicDataDisplay.dll"; DestDir: "{app}"; Flags: ignoreversion
; ע��: ��Ҫ���κι���ϵͳ�ļ���ʹ�á�Flags: ignoreversion��

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"


