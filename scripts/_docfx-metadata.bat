set LOG_LEVEL=%1
if "%LOG_LEVEL%"=="" set LOG_LEVEL=verbose

cd ../src
echo "=============== METADATA ===================="
docfx metadata docfx.json --logLevel %LOG_LEVEL%
cd ../scripts
