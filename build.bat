@echo off
echo ========================================
echo LLM Desktop Overlay Build Script - Windows 11
echo ========================================
echo.

REM Check Windows version
echo Verifying Windows 11 compatibility...
ver | find "10.0.2" > nul
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: This application is optimized for Windows 11
    echo Please ensure you are running Windows 11 (Build 22000+)
    echo.
)

REM Check if .NET 6 is installed
echo Checking for .NET 6 installation...
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET 6 is not installed or not in PATH
    echo Please install .NET 6 from: https://dotnet.microsoft.com/download/dotnet/6.0
    pause
    exit /b 1
)

echo Found .NET 6 installation
echo.

REM Check if Visual Studio build tools are available
echo Checking for Visual Studio build tools...
where msbuild >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo WARNING: MSBuild not found in PATH
    echo Trying to use Visual Studio Developer Command Prompt...
    call "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\Enterprise\Common7\Tools\VsDevCmd.bat" >nul 2>&1
    if %ERRORLEVEL% NEQ 0 (
        call "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\Professional\Common7\Tools\VsDevCmd.bat" >nul 2>&1
    )
    if %ERRORLEVEL% NEQ 0 (
        call "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\Community\Common7\Tools\VsDevCmd.bat" >nul 2>&1
    )
)

echo.

REM Clean previous builds
echo Cleaning previous builds...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"
echo Clean completed
echo.

REM Restore NuGet packages
echo Restoring NuGet packages...
dotnet restore
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Failed to restore NuGet packages
    pause
    exit /b 1
)
echo Packages restored successfully
echo.

REM Build the project
echo Building LLM Overlay application for Windows 11...
dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo Build completed successfully
echo.

REM Create run script
echo Creating run script...
echo @echo off > run.bat
echo echo Starting LLM Desktop Overlay for Windows 11... >> run.bat
echo echo. >> run.bat
echo if exist "bin\x64\Release\net6.0-windows10.0.19041.0\LLMOverlay.exe" ( >> run.bat
echo     start "" "bin\x64\Release\net6.0-windows10.0.19041.0\LLMOverlay.exe" >> run.bat
echo ) else ( >> run.bat
echo     echo ERROR: Application executable not found >> run.bat
echo     echo Please run build.bat first >> run.bat
echo     pause >> run.bat
echo ) >> run.bat
echo.
echo Run script created: run.bat
echo.

REM Display success message
echo ========================================
echo BUILD COMPLETED SUCCESSFULLY!
echo ========================================
echo.
echo Executable location:
echo bin\x64\Release\net6.0-windows10.0.19041.0\LLMOverlay.exe
echo.
echo To run the application:
echo 1. Double-click run.bat
echo 2. Or run: bin\x64\Release\net6.0-windows10.0.19041.0\LLMOverlay.exe
echo.
echo Note: Requires .NET 8.0 Runtime to be installed on the target system
echo.
echo Next steps:
echo 1. Launch the application
echo 2. Click the settings button (⚙️)
echo 3. Enter your API key for your chosen LLM provider
echo 4. Start chatting!
echo.
pause