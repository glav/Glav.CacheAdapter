echo off
echo.
echo Copying latest files from Release directory

copy "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter\bin\Release\Glav.CacheAdapter.dll" "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package\lib" /y > NUL:
copy "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter\Readme.txt" "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package\content\CacheAdapter-ReadMe.txt" /y > NUL:
copy "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.ExampleUsage\Program.cs" "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package\content\Example-CacheAdapterUsage.cs" /y > NUL:
echo ....Done.
echo.
echo Creating Glav.CacheAdapter.Core Package
echo ***************************************
cd "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package"
"C:\Development\Dev Resources\Nuget\Nuget.exe" pack Glav.CacheAdapter.Core.nuspec
echo ....Done.
echo.
echo Creating Glav.CacheAdapter Package
echo **********************************
cd "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package"
"C:\Development\Dev Resources\Nuget\Nuget.exe" pack Glav.CacheAdapter.nuspec
echo ....Done.
echo.
echo Updating Local Package Repository
echo *********************************
xcopy "C:\Development\Dev Projects\CacheAdapter-Repository\Glav.CacheAdapter.Package\*.nupkg" "C:\Development\My Nuget Packages" /y /q
echo.
echo Package Update process complete.
echo.
pause
