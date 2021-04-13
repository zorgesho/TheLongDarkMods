:: %1 $(ConfigurationName)
:: %2 $(ProjectName)
:: %3 $(ProjectDir)
:: %4 $(TargetPath)
:: %5 $(TargetDir)

echo %1 | findstr /c:"testbuild" 1>nul && goto :eof

set mods_path="c:\games\thelongdark\mods"
if not exist %mods_path% goto :eof

echo =====
xcopy %5*.* %mods_path%\ /e /y