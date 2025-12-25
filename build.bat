@echo off
echo ========================================
echo LLM Desktop Overlay Build Script - WPF Version
echo ========================================
echo.

REM Check if .NET 8 is installed
echo Checking for .NET 8 installation...
dotnet --version >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: .NET 8 is not installed or not in PATH
    echo Please install .NET 8 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo.
echo Building LLM Desktop Overlay (WPF)...

REM Clean previous builds
echo Cleaning previous builds...
if exist "bin" rmdir /s /q "bin"
if exist "obj" rmdir /s /q "obj"

REM Restore NuGet packages
echo Restoring packages...
dotnet restore

REM Build the project
echo Building project in Release mode...
dotnet build --configuration Release

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: Build failed!
    pause
    exit /b 1
)

echo.
echo Build completed successfully!
echo.
echo Executable location: bin\Release\net8.0-windows\LLMOverlay.exe
echo.

REM Ask if user wants to run the application
set /p run="Do you want to run the application now? (Y/N): "
if /i "%run%"=="Y" (
    echo.
    echo Starting LLM Desktop Overlay...
    start "" "bin\Release\net8.0-windows\LLMOverlay.exe"
)

echo.
echo Build process completed.
pause