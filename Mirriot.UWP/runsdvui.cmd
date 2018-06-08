cd /d "C:\Projects\Miriot\Source\Mirriot.UWP" &msbuild "Mirriot.UWP.csproj" /t:sdvViewer /p:configuration="Debug" /p:platform=ARM
exit %errorlevel% 