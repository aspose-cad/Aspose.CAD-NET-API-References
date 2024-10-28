set serve=
set force=
for %%i in (%*) do (
    if "%%i"=="--serve" set serve=--serve
    if "%%i"=="--force" set force=--force
)

REM error, warning, info, verbose, diagnostic
SET LOG_LEVEL=verbose
REM linux-x64
SET RUNTIME=win-x64
SET FRAMEWORK=net8.0
SET CONFIGURATION=Release

cd ../src

if "%force%"=="--force" (
    rmdir /S /Q api
    rmdir /S /Q ..\_site
	
    dotnet tool install -g docfx
    dotnet tool update docfx -g
	
	cd templates
	call npm install
	cd ..
)

cd ../scripts
CALL _build-template.bat
cd ../src

SET PLUGINS_DIR=templates\aspose-modern\plugins\

dotnet nuget locals all --clear
dotnet restore --no-cache

dotnet build -c %CONFIGURATION% -f %FRAMEWORK%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\Aspose.CAD.* %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\Docfx.Aspose.Plugins.* %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\LastModifiedPostProcessor.* %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\LibGit2Sharp.dll %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\HtmlAgilityPack.dll %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\runtimes\%RUNTIME%\native\* %PLUGINS_DIR%

cd ../scripts
CALL _docfx-metadata.bat %LOG_LEVEL%
cd ../src

cd ../scripts
CALL _docfx-build.bat %LOG_LEVEL% %serve%
cd ../src

CALL bootstrap\Docfx.Aspose.Tools\bin\%CONFIGURATION%\%FRAMEWORK%\Docfx.Aspose.Tools.exe index "../_site/index.json" "docfx.json"

if not defined serve (
    CALL bootstrap\Docfx.Aspose.Tools\bin\%CONFIGURATION%\%FRAMEWORK%\Docfx.Aspose.Tools.exe sitemap "../_site/sitemap.xml" "docfx.json"
REM    CALL bootstrap\Docfx.Aspose.Tools\bin\%CONFIGURATION%\%FRAMEWORK%\Docfx.Aspose.Tools.exe verify "../_site/sitemap.xml" --server "http://localhost:8081" --dry-run
)

cd ../scripts