; Inno Setup Script - 标签页收藏管理器 (Multi-Arch)
; Requires Inno Setup 6+
; Usage: iscc installer-arch.iss /DArch=x64 /DRid=win-x64

#ifndef Arch
  #define Arch "x64"
#endif
#ifndef Rid
  #define Rid "win-x64"
#endif

#define MyAppName "标签页收藏管理器"
#define MyAppVersion "0.0.1"
#define MyAppPublisher "BookmarkManager"
#define MyAppExeName "BookmarkManager.exe"

[Setup]
AppId={{B7A3C4D5-E6F7-48A9-B0C1-D2E3F4567890}
AppName={#MyAppName} ({#Arch})
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppCopyright=Copyright (C) 2026
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
OutputDir=bin\Release
OutputBaseFilename=BookmarkManager-Setup-{#Arch}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=classic
WizardSizePercent=110
SetupIconFile=app.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
UninstallDisplayName={#MyAppName} ({#Arch})
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
DisableProgramGroupPage=yes
WizardImageFile=compiler:WizClassicImage.bmp
WizardSmallImageFile=compiler:WizClassicSmallImage.bmp

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Messages]
WelcomeLabel1=欢迎安装 [name]
WelcomeLabel2=此向导将引导您完成 [name] 的安装过程。%n%n建议在继续之前关闭所有其他应用程序。
FinishedLabel=安装程序已完成 [name] 的安装。%n%n点击完成关闭向导。
FinishedHeadingLabel=安装完成！

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\{#Rid}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName} ({#Arch})"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\卸载 {#MyAppName} ({#Arch})"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "启动 {#MyAppName}"; Flags: nowait postinstall skipifsilent

[InstallDelete]
Type: filesandordirs; Name: "{app}"

[Code]
function InitializeSetup: Boolean;
begin
  Result := True;
end;
