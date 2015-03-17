@ECHO OFF

SETLOCAL ENABLEEXTENSIONS

:: =============================================================================
::
:: NT shell script to reset settings on your local dev box
::      1. Azure storage emulator
::      2. MS SQL Express
::
:: BIG Hammer :-)
::
:: Sleep.exe could be found in MS Windows SDK
::
::
:: Written By Hong Liu
:: Check out https://github.com/hong6914/Public for updates.
::
:: =============================================================================


@ECHO CALL "%ProgramFiles%\Microsoft SDKs\Windows Azure\.NET SDK\v2.3\bin\setenv.cmd"
      CALL "%ProgramFiles%\Microsoft SDKs\Windows Azure\.NET SDK\v2.3\bin\setenv.cmd"
IF NOT ERRORLEVEL 0 ( GOTO :EOF )

@ECHO.
@ECHO.
@ECHO "%ProgramFiles%\Microsoft SDKs\Azure\Emulator\csrun.exe" /devstore:shutdown
      "%ProgramFiles%\Microsoft SDKs\Azure\Emulator\csrun.exe" /devstore:shutdown

"%~d0%~p0sleep.exe" 5

@ECHO.
@ECHO.
@ECHO NET STOP MSSQL$SQLEXPRESS
      NET STOP MSSQL$SQLEXPRESS

@ECHO.
@ECHO.
@ECHO NET START MSSQL$SQLEXPRESS
      NET START MSSQL$SQLEXPRESS

@ECHO.
@ECHO.
@ECHO Reset Storage Emulator ...
@ECHO.
@ECHO dsinit.exe /forcecreate /silent
      dsinit.exe /forcecreate /silent

"%~d0%~p0sleep.exe" 5

@ECHO.
@ECHO.
@ECHO "%ProgramFiles%\Microsoft SDKs\Azure\Emulator\csrun.exe" /devstore:start
      "%ProgramFiles%\Microsoft SDKs\Azure\Emulator\csrun.exe" /devstore:start

EXIT

:EOF