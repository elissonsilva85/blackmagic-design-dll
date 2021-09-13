if "%SwitchersSDK_Include_DIR%"=="" (set SwitchersSDK_Include_DIR=%cd%\include)

set "VSCMD_START_DIR=%cd%"
call "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\VC\Auxiliary\Build\vcvars64.bat"

mkdir build
cd build
cmake -G "Visual Studio 16 2019" -A x64 -DSWITCHERS_SDK_INCLUDE_DIR="%SwitchersSDK_Include_DIR%" ..
cmake --build . --config Release
