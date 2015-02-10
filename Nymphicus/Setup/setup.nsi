!addplugindir ".\"

!include "MUI2.nsh"
!include "checkDotNetFull.nsh"
!include LogicLib.nsh
!include UAC.nsh


; The name of the installer
Name "Nymphicus"

; The file to write
OutFile "Setup-Nymphicus.exe"



; The default installation directory
InstallDir "$PROGRAMFILES\lI Ghun\Nymphcius\"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\lI Ghun\Nymphicus" "Install_Dir"

; Request application privileges for Windows Vista
; Back to user for mixed use
RequestExecutionLevel user

Function .onInit
uac_tryagain:
!insertmacro UAC_RunElevated
#MessageBox mb_TopMost "0=$0 1=$1 2=$2 3=$3"
${Switch} $0
${Case} 0
	${IfThen} $1 = 1 ${|} Quit ${|} ;we are the outer process, the inner process has done its work, we are done
	${IfThen} $3 <> 0 ${|} ${Break} ${|} ;we are admin, let the show go on
	${If} $1 = 3 ;RunAs completed successfully, but with a non-admin user
		MessageBox mb_IconExclamation|mb_TopMost|mb_SetForeground "This installer requires admin access, try again" /SD IDNO IDOK uac_tryagain IDNO 0
	${EndIf}
	;fall-through and die
${Case} 1223
	MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "This installer requires admin privileges, aborting!"
	Quit
${Case} 1062
	MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "Logon service not running, aborting!"
	Quit
${Default}
	MessageBox mb_IconStop|mb_TopMost|mb_SetForeground "Unable to elevate , error $0"
	Quit
${EndSwitch}
FunctionEnd


;--------------------------------

  !define MUI_ABORTWARNING



!define MUI_HEADERIMAGE
!define MUI_HEADERIMAGE_BITMAP "logoSetupSmall.bmp"
!define MUI_WELCOMEFINISHPAGE_BITMAP "logoSetupBig.bmp"
!define MUI_WELCOMEPAGE_TITLE "Nymphicus"
!define MUI_WELCOMEPAGE_TEXT "See all your posts in your own views$\r$\n$\r$\nPlease stop any instance of Nymphicus prior to installing this version."
!define MUI_STARTMENUPAGE_DEFAULTFOLDER "lI Ghun\Nymphicus"
!define MUI_ICON "..\Images\NymphicusIcon.ico"
!define MUI_UNICON "uninstall.ico"


Var StartMenuFolder
; Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "License.txt"
  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY

  !define MUI_STARTMENUPAGE_REGISTRY_ROOT "HKCU" 
  !define MUI_STARTMENUPAGE_REGISTRY_KEY "Software\lI Ghun\Nymphicus" 
  !define MUI_STARTMENUPAGE_REGISTRY_VALUENAME "Start Menu Folder"
  !insertmacro MUI_PAGE_STARTMENU Application $StartMenuFolder
  !insertmacro MUI_PAGE_INSTFILES
;  !define MUI_FINISHPAGE_RUN "Nymphicus.exe"
  !define MUI_FINISHPAGE_RUN
  !define MUI_FINISHPAGE_RUN_FUNCTION FinishRun   
  !insertmacro MUI_PAGE_FINISH
  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH



Function FinishRun
!insertmacro UAC_AsUser_ExecShell "" "Nymphicus.exe" "" "" ""
FunctionEnd

;--------------------------------




!insertmacro MUI_LANGUAGE "English"

; LoadLanguageFile "${NSISDIR}\Contrib\Language files\English.nlf"
;--------------------------------
;Version Information

  VIProductVersion "2.2.0.0"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "Nymphicus"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "Sven Walther"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "© 2011-2015 Sven Walther"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "The App.net, Twitter, Facebook and QUOTE.fm client your wanna have"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "2.3.0"


Function un.UninstallDirs
    Exch $R0 ;input string
    Exch
    Exch $R1 ;maximum number of dirs to check for
    Push $R2
    Push $R3
    Push $R4
    Push $R5
       IfFileExists "$R0\*.*" 0 +2
       RMDir "$R0"
     StrCpy $R5 0
    top:
     StrCpy $R2 0
     StrLen $R4 $R0
    loop:
     IntOp $R2 $R2 + 1
      StrCpy $R3 $R0 1 -$R2
     StrCmp $R2 $R4 exit
     StrCmp $R3 "\" 0 loop
      StrCpy $R0 $R0 -$R2
       IfFileExists "$R0\*.*" 0 +2
       RMDir "$R0"
     IntOp $R5 $R5 + 1
     StrCmp $R5 $R1 exit top
    exit:
    Pop $R5
    Pop $R4
    Pop $R3
    Pop $R2
    Pop $R1
    Pop $R0
FunctionEnd




; The stuff to install
Section "Nymphicus"

  SectionIn RO
  
 Call CheckAndInstallDotNet

SetOutPath "$INSTDIR\\Images"
  File "..\Images\Nymphicus_icon_512_freigestellt.ico"
  File "..\Images\Nymphicus_icon_512_freigestellt.png"

  SetOutPath "$INSTDIR\\Images\\ExternalServices"
  File /r "..\Images\ExternalServices\*"

  SetOutPath "$INSTDIR\\Images\\GoogleReader"
  File /r "..\Images\GoogleReader\*"
 
  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  

  ; Put file there
  File "Documentation.URL"
  File "..\Images\NymphicusIcon.ico"
  File "..\Nymphicus.exe"
 ; File "..\Nymphicus.ico"
  File "..\Nymphicus.pdb"
  File "..\Nymphicus.exe.config"
  File "LICENSE.txt"
  File "Documentation.ico"
  
  File "..\Facebook.dll"
  File "..\Facebook.xml"

  

    File /r "..\ExternalLibraries\*"
    File "..\Hamm*"
	
	File "..\TweetS*"
	File "..\Newt*"
	File "..\AppNet*"

  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\Nymphicus" "Install_Dir" "$INSTDIR"
  delete "$INSTDIR\Hammock.dll"
  delete "$INSTDIR\Hammock.pdb"

  ; Enable WebBrowser not being quirks mode
  WriteRegDWORD HKCU "Software\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION" "Nymphicus.exe" 8888
  WriteRegDWORD HKCU "Software\Microsoft\Internet Explorer\MAIN\FeatureControl\FEATURE_BROWSER_EMULATION" "Nymphicus.vshost.exe" 8888
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Nymphicus" "DisplayName" "Nymphicus"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Nymphicus" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Nymphicus" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\Nymphicus" "NoRepair" 1
  WriteUninstaller "uninstall.exe"

SectionEnd

; Optional section (can be disabled by the user)
Section "Start Menu Shortcuts"

!insertmacro MUI_STARTMENU_WRITE_BEGIN Application

  SetShellVarContext all
  CreateDirectory "$SMPROGRAMS\$StartMenuFolder"
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\Nymphicus.lnk" "$INSTDIR\Nymphicus.exe" "" "$INSTDIR\Images\Nymphicus_icon_512_freigestellt.ico" 0
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\Documentation.lnk" "$INSTDIR\Documentation.URL" "" $INSTDIR\Documentation.ico" 0
  CreateShortCut "$SMPROGRAMS\$StartMenuFolder\\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  SetShellVarContext current
  
!insertmacro MUI_STARTMENU_WRITE_END 
SectionEnd


;--------------------------------

; Uninstaller

Section "Uninstall"

  
  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\li ghun\Nymphicus"
  DeleteRegKey HKLM "Software\li ghun\Nymphicus"
  ; Remove files and uninstaller
  Delete $INSTDIR\*.*

  ; Remove shortcuts, if any
  !insertmacro MUI_STARTMENU_GETFOLDER Application $StartMenuFolder
    
	SetShellVarContext all
  Delete "$SMPROGRAMS\$StartMenuFolder\\*.*"
  SetShellVarContext current
  


  DeleteRegKey HKCU "Software\li ghun\Nymphicus"


  ; Remove directories used
   ; RMDir "$SMPROGRAMS\$StartMenuFolder"
Push 10 #maximum amount of directories to remove
  Push "$SMPROGRAMS\$StartMenuFolder" #input string
    Call un.UninstallDirs

   
  ; RMDir "$INSTDIR"
  
  Push 10 #maximum amount of directories to remove
  Push $INSTDIR #input string
    Call un.UninstallDirs


SectionEnd
