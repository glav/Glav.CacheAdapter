echo off
echo.
echo Copying latest files from Release directory

copy "D:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter\bin\Release\Glav.CacheAdapter.dll" "D:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package\lib" /y > NUL:
copy "D:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter\Readme.txt" "D:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package\content\CacheAdapter-ReadMe.txt" /y > NUL:
copy "D:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.ExampleUsage\Program.cs" "D:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package\content\Example-CacheAdapterUsage.cs" /y > NUL:
echo ....Done.
echo.
echo Creating Glav.CacheAdapter.Core Package
echo ***************************************
cd "d:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package"
"d:\data\Development\Dev Resources\Nuget\NuGet.exe" pack Glav.CacheAdapter.Core.nuspec
echo ....Done.
echo.
echo Creating Glav.CacheAdapter Package
echo **********************************
cd "d:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package"
"d:\data\Development\Dev Resources\Nuget\NuGet.exe" pack Glav.CacheAdapter.nuspec
echo ....Done.
echo.
echo Updating Local Package Repository
echo *********************************
xcopy "D:\Data\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package\*.nupkg" "D:\Data\Development\My Nuget Packages" /y /q
echo.
echo Package Update process complete.
echo.
pause
