
@echo off
setlocal EnableExtensions
cd /d "%~dp0"

REM SonarQube configuration
set "SONAR_URL=http://localhost:9000"
set "PROJECT_KEY=Insurance_App"

REM Validate SonarQube token
if not defined SONAR_TOKEN (
    echo ERROR: SONAR_TOKEN is not configured.
    goto error
)

REM Remove previous test results
if exist "TestResults" rmdir /s /q "TestResults"
mkdir "TestResults"

REM Start SonarQube analysis
echo.
echo **********************
echo Starting analysis...
echo **********************

dotnet sonarscanner begin ^
  /k:"%PROJECT_KEY%" ^
  /d:sonar.host.url="%SONAR_URL%" ^
  /d:sonar.token="%SONAR_TOKEN%" ^
  /d:sonar.verbose=true ^
  /d:sonar.cs.vstest.reportsPaths="TestResults/**/*.trx" ^
  /d:sonar.cs.vscoveragexml.reportsPaths="TestResults/coverage.xml"

if errorlevel 1 goto error

REM Build the solution
echo.
echo **********************
echo Building solution...
echo **********************

dotnet build --no-incremental
if errorlevel 1 goto error

REM Run unit tests and collect coverage
echo.
echo **********************
echo Running unit tests and collecting coverage...
echo **********************

dotnet-coverage collect ^
  "dotnet test --no-build --logger trx --results-directory TestResults" ^
  -f xml ^
  -o "TestResults/coverage.xml"

if errorlevel 1 goto error

REM Validate coverage report
if not exist "TestResults\coverage.xml" (
    echo ERROR: Coverage report was not generated.
    goto error
)

REM Upload analysis results
echo.
echo **********************
echo Uploading results...
echo **********************

dotnet sonarscanner end ^
  /d:sonar.token="%SONAR_TOKEN%"

if errorlevel 1 goto error

echo.
echo **********************
echo Analysis completed successfully!
echo **********************
echo Results: %SONAR_URL%/dashboard?id=%PROJECT_KEY%

goto finish

:error
echo.
echo ERROR: SonarQube analysis failed!
endlocal
exit /b 1

:finish
echo.
endlocal
exit /b 0