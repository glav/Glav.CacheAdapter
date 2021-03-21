echo off
echo.
echo Copying latest files from Release directory

mkdir lib > NUL:
copy "..\..\..\Glav.CacheAdapter\bin\Release\Glav.CacheAdapter.dll" "lib\net45\" /y > NUL:
copy "..\..\..\Glav.CacheAdapter\bin\Release\Glav.CacheAdapter.dll" "lib\net472\" /y > NUL:
copy "..\..\..\Glav.CacheAdapter\Readme.txt" "content\CacheAdapter-ReadMe.txt" /y > NUL:
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\Program.cs" "content\CacheAdapterExamples\Example-CacheAdapterUsage.cs" /y > NUL:
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\ConsoleHelper.cs" "content\CacheAdapterExamples\ConsoleHelper.cs" /y > NUL:

copy "..\..\..\Glav.CacheAdapter.ExampleUsage\HammerTheCache.cs" "content\CacheAdapterExamples\HammerTheCache.cs" /y > NUL:
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\InMemoryLogger.cs" "content\CacheAdapterExamples\InMemoryLogger.cs" /y > NUL:
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\SimpleUsageAsync.cs" "content\CacheAdapterExamples\SimpleUsageAsync.cs" /y > NUL:
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\SimpleUsageWithDependencies.cs" "content\CacheAdapterExamples\SimpleUsageWithDependencies.cs" /y > NUL:
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\SimpleUsageWithTests.cs" "content\CacheAdapterExamples\SimpleUsageWithTests.cs" /y > NUL:
copy "..\..\..\Glav.CacheAdapter.ExampleUsage\SomeData.cs" "content\CacheAdapterExamples\SomeData.cs" /y > NUL:

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
