echo Copy files ...
copy DbNotifyer\bin\Release\Hardcodet.Wpf.TaskbarNotification DbFactory\Bin\Release
copy SQLiteLibrary\bin\Release\System.Data.SQLite.dll DbFactory\bin\Release
copy SQLiteLibrary\SQLite.Interop.dll DbFactory\bin\Release
copy MySQLLibrary\bin\Release\MySql.Data.dll DbFactory\bin\Release
copy MySQLLibrary\bin\Release\MySql.Data.Entity.EF5.dll DbFactory\bin\Release
copy MySQLLibrary\bin\Release\MySql.Data.Entity.EF6.dll DbFactory\bin\Release
copy MySQLLibrary\bin\Release\MySql.Fabric.Plugin.dll DbFactory\bin\Release

echo Cleanup Binaries folder ...
del Binaries\*.*  /s /q

echo Building Project ...
"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" DatabaseFactory.sln /p:Configuration=Release /t:Rebuild


echo Cleanup Release folder ...
del _Release\*.*  /s /q

echo Copy new dll files ...
copy DbFactory\bin\Release\*.dll Binaries

copy Binaries\Hardcodet.Wpf.TaskbarNotification.dll _Release
copy Binaries\System.Data.SQLite.dll _Release
copy SQLiteLibrary\SQLite.Interop.dll _Release
copy Binaries\MySql.Data.dll _Release


echo Merging Binaries ...
"C:\Program Files (x86)\Microsoft\ILMerge\ILMerge.exe" /ndebug /copyattrs /targetplatform:4.0,"C:\Windows\Microsoft.NET\Framework64\v4.0.30319" /out:_Release\TPDev.DatabaseFactory.dll Binaries\DatabaseFactory.dll Binaries\DbInterface.dll Binaries\DbLogger.dll Binaries\DbNotifyer.dll Binaries\MySQLLibrary.dll Binaries\SQLiteLibrary.dll Binaries\SQLLibrary.dll Binaries\OracleLibrary.dll

echo finished!
pause