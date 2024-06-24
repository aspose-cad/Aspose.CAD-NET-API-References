set serve=
for %%i in (%*) do (
    if "%%i"=="--serve" set serve=--serve
)

cd ../src

dotnet tool install -g docfx

dotnet restore
dotnet build -c Release -f net8.0


docfx metadata docfx.json
docfx build docfx.json %serve%

cd ../scripts