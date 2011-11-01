

@echo off
cd /d %~dp0

"%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe" TrackFolderChanges\TrackFolderChanges.sln  /p:Configuration=Release



set zip=%cd%\TrackFolderChanges.zip

del "%zip%"

cd TrackFolderChanges\bin\Release

%Apps%\7-Zip\7z.exe a "%zip%" TrackFolderChanges.exe TrackFolderChanges.exe.config -aoa




