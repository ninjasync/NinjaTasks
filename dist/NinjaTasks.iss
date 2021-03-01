#define AppSourceDir '..\NinjaTasks.App.Wpf\bin\Release'
#define MainExe 'NinjaTasks.exe'
#define ApplicationName 'NinjaTasks'
#define ApplicationId 'NinjaTasks'

#include "common.iss"

[Setup]
AppPublisher=NinjaTasks Contributors
AppPublisherURL=
AppCopyright=Copyright (c) 2015-2019 NinjaTasks Contributors
DefaultGroupName=Ninja

ArchitecturesInstallIn64BitMode=x64

;WizardImageFile=Logo.bmp
;WizardImageStretch=no
;WizardImageBackColor=$FFFFFF

DisableProgramGroupPage=yes
OutputDir=.

PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog

DisableDirPage=no
;InfoBeforeFile=License.rtf
;Compression=none


[Components]
;Name: "normal"; Description: "Normal (data in user directory)";              Flags:exclusive
Name: "portable"; Description: "Portable (data in installation directory)";  
Name: "encryption"; Description: "Enable database encryption"; 

[Files]
Source: {#AppSourceDir}\*; DestDir: {app}; Excludes: NLog*.config,*.pdb,*.vshost.*,*.xml,*.ini,*.sqlite,*.lnk,*.obfusc*,{#MergedDlls},System.Windows.Interactivity.resources.dll; Flags: ignoreversion recursesubdirs
Source: {#AppSourceDir}\NLog-release.config; DestDir: {app}; DestName: NLog.config;Flags: ignoreversion

[Dirs]
Name: "{app}\portable-data"; Components: portable
; encryption marker:
Name: "{app}\empty"; Components: encryption    


[Code]
function InitializeSetup(): Boolean;
begin
    if not IsDotNetDetected('v4.7.2', 0) then begin
        MsgBox('{#ApplicationName} requires Microsoft .NET Framework 4.7.2'#13#13
            'Please use Windows Update to install this version,'#13
            'and then re-run the {#ApplicationName} setup program.', mbInformation, MB_OK);
        result := true;  { set to false to stop setup; 
                           but it seems better to let 
                           the user continue on his own. }        
    end else
        result := true;
end;
