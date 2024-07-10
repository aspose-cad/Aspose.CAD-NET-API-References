set serve=
set force=
for %%i in (%*) do (
    if "%%i"=="--serve" set serve=--serve
    if "%%i"=="--force" set force=--force
)

cd ../src


if "%force%"=="--force" (
    rmdir /S /Q api
    rmdir /S /Q ..\_site
)

dotnet tool install -g docfx

dotnet restore
dotnet build -c Release -f net8.0


docfx metadata docfx.json
docfx build docfx.json --verbose %serve%

cd ../scripts