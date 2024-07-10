echo off

set force=
for %%i in (%*) do (
    if "%%i"=="-f" set force=--force
)

call _build.bat %force%