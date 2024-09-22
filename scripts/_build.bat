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

cd templates
node build.js
cd ..

SET PLUGINS_DIR=templates\aspose-modern\plugins\

dotnet restore
dotnet build -c %CONFIGURATION% -f %FRAMEWORK%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\Aspose.CAD.* %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\Docfx.Aspose.Plugins.* %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\LastModifiedPostProcessor.* %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\LibGit2Sharp.dll %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\HtmlAgilityPack.dll %PLUGINS_DIR%
copy /Y Docfx.Boostrap\bin\%CONFIGURATION%\%FRAMEWORK%\runtimes\%RUNTIME%\native\* %PLUGINS_DIR%

echo "=============== METADATA ===================="
docfx metadata docfx.json --logLevel %LOG_LEVEL%

echo "================ BUILD ======================"
docfx build docfx.json --logLevel %LOG_LEVEL% %serve%

if not defined serve (
    call bootstrap\Docfx.Aspose.Tools\bin\%CONFIGURATION%\%FRAMEWORK%\Docfx.Aspose.Tools.exe sitemap "../_site/sitemap.xml" "docfx.json"
REM    call bootstrap\Docfx.Aspose.Tools\bin\%CONFIGURATION%\%FRAMEWORK%\Docfx.Aspose.Tools.exe verify "../_site/sitemap.xml" --server "http://localhost:8081" --dry-run
)

cd ../scripts