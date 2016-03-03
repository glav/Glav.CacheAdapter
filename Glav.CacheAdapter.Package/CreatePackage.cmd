echo off
echo.
echo Copying latest files from Release directory

mkdir lib > NUL:
copy "..\Glav.CacheAdapter\bin\Release\Glav.CacheAdapter.dll" "lib\net45\" /y > NUL:
copy "..\Glav.CacheAdapter\Readme.txt" "content\CacheAdapter-ReadMe.txt" /y > NUL:
copy "..\Glav.CacheAdapter.ExampleUsage\Program.cs" "content\Example-CacheAdapterUsage.cs" /y > NUL:
echo ....Done.
echo.
echo Creating Glav.CacheAdapter.Core Package
echo ***************************************
"..\Nuget\Nuget.exe" pack Glav.CacheAdapter.Core.nuspec
echo ....Done.
echo.
echo Creating Glav.CacheAdapter Package
echo **********************************
"..\Nuget\Nuget.exe" pack Glav.CacheAdapter.nuspec
echo ....Done.
echo.
echo Updating Local Package Repository
echo *********************************
xcopy "*.nupkg" "..\..\..\My Nuget Packages\*.*" /y /q
echo.
echo Package Update process complete.
echo.
pause
