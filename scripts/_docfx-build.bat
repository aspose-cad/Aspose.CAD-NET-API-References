set LOG_LEVEL=%1
if "%LOG_LEVEL%"=="" set LOG_LEVEL=verbose

set serve=%2

cd ../src
echo "================ BUILD ======================"
docfx build docfx.json --logLevel %LOG_LEVEL% %serve%
cd ../scripts
