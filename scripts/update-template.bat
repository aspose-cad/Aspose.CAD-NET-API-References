echo off

call _build-template.bat
set srcDir=..\src\templates\aspose-modern\public
set destDir=..\_site\public

rem Remove all contents of the destination directory recursively
rd /s /q "%destDir%"

rem Recreate the destination directory
mkdir "%destDir%"

rem Copy all files and directories from source to destination
xcopy "%srcDir%" "%destDir%" /e /i /h /y

echo Operation completed.