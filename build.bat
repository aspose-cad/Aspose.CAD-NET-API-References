powershell Set-ExecutionPolicy Bypass -Scope Process -Force; [System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))

choco install docfx
choco install markdownlint-cli

dotent restore
copy /Y "%USERPROFILE%\.nuget\packages\aspose.cad\23.5.0\lib\netstandard2.0\*.*" src

docfx metadata
docfx build