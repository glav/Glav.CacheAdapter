echo off
echo.
echo Copying latest files from Release directory

mkdir lib
copy "..\..\..\Glav.CacheAdapter\bin\Release\Glav.CacheAdapter.dll" "lib\net472\" /y
copy "..\..\..\Glav.CacheAdapter\Readme.txt" "content\CacheAdapter-ReadMe.txt" /y
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\Program.cs" "content\CacheAdapterExamples\Example-CacheAdapterUsage.cs" /y
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\ConsoleHelper.cs" "content\CacheAdapterExamples\ConsoleHelper.cs" /y

copy "..\..\..\Glav.CacheAdapter.ExampleUsage\HammerTheCache.cs" "content\CacheAdapterExamples\HammerTheCache.cs" /y
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\InMemoryLogger.cs" "content\CacheAdapterExamples\InMemoryLogger.cs" /y
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\SimpleUsageAsync.cs" "content\CacheAdapterExamples\SimpleUsageAsync.cs" /y
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\SimpleUsageWithDependencies.cs" "content\CacheAdapterExamples\SimpleUsageWithDependencies.cs" /y
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\SimpleUsageWithTests.cs" "content\CacheAdapterExamples\SimpleUsageWithTests.cs" /y
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\SomeData.cs" "content\CacheAdapterExamples\SomeData.cs" /y

echo ....Done.
echo.
echo Creating Glav.CacheAdapter Package
echo **********************************
"..\..\..\Nuget\Nuget.exe" pack Glav.CacheAdapter.nuspec
echo ....Done.
echo.
echo Updating Local Package Repository
echo *********************************
xcopy "*.nupkg" "..\..\..\..\..\My Nuget Packages\*.*" /y /q
echo.
echo Package Update process complete.
echo.
pause
