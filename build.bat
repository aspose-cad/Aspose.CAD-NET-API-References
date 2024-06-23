echo off

dotnet tool install -g docfx

mkdir src || cd src

dotnet restore
dotnet build -c Release -f net8.0

REM docfx metadata docfx.json
docfx build docfx.json --serve

cd ..