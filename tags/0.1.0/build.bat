
set FILES=src\gpsData.cs src\guiWindows.cs src\main.cs src\i18n.cs

set CSC_OPTS=/debug- /doc:doc\drohneDoc.xml /nologo /out:Drohne.exe /target:winexe /win32icon:share\pixmaps\drohne_16_transp.ico

csc.exe %CSC_OPTS% %FILES%

pause

: vim:fileformat=dos
