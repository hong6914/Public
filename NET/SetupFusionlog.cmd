@ECHO OFF

SETLOCAL ENABLEEXTENSIONS

:: =============================================================================
::
:: NT shell script to setup FusionLog for .NET assembly binding traces.
::
:: Written By Hong Liu
:: Check out https://github.com/hong6914/Public for updates.
::
::
:: Note:
::      You need to be the local admin to be able to make it work properly.
::
::  Example Usage:
::      <this script>  blabla       <--- turn it ON, default log folder is C:\FusionLog\
::      <this script>  aaa C:\dummy <--- turn it ON and set log folder as C:\dummy\
::                                          Will create C:\dummy\ if it does NOT exist.
::      <this script>  0            <--- turn it OFF
::
:: =============================================================================

::
:: Global environment variables
::
SET sControl=%1
SET sLogFolder=%2

IF "%sLogFolder%" EQU "" (
    SET sLogFolder=C:\FusionLog
)

IF "%sControl%" EQU "" (
    @ECHO.
    @ECHO.
    @ECHO ------------------------------------------------------------------
    @ECHO NT shell script to setup FusionLog for .NET assembly binding logs.
    @ECHO ------------------------------------------------------------------
    @ECHO.
    @ECHO Usage: %~nx0 Flag [Folder Name]
    @ECHO   Flag: 0 == turn OFF  anything else == turn ON
    @ECHO.
    @ECHO   Folder Name: For FusionLog to store log files. Optional.
    @ECHO       If not specified, it is C:\FusionLog\ by default.
    @ECHO.
    @ECHO.
    @ECHO  Example Usage:
    @ECHO      [this script]  blabla        Turn it ON, default log folder is C:\FusionLog\
    @ECHO      [this script]  aaa C:\dummy  Turn it ON and set log folder as C:\dummy\
    @ECHO                                       Will create C:\dummy\ if it does NOT exist.
    @ECHO      [this script]  0             turn it OFF
    @ECHO.
    @ECHO.

    EXIT /B 1
)

IF "%sControl%" EQU "0" (
    @ECHO.
    @ECHO.
    @ECHO ------------------------------------------------------------------
    @ECHO Disable the Fusion logging...
    @ECHO.
    @ECHO.
    @ECHO REG ADD HKLM\Software\Microsoft\Fusion /V EnableLog /T REG_DWORD /D 0 /F
          REG ADD HKLM\Software\Microsoft\Fusion /V EnableLog /T REG_DWORD /D 0 /F
    GOTO :Done
)


@ECHO.
@ECHO.
@ECHO ------------------------------------------------------------------
@ECHO Enable the Fusion logging...
@ECHO.
@ECHO.

IF NOT EXIST "%sLogFolder%" (
    @ECHO.
    @ECHO.
    @ECHO ERROR ^>^>^> Log folder "%sLogFolder%" does NOT exist --- creating it...
    @ECHO.
    @ECHO.
    @ECHO MD "%sLogFolder%"
          MD "%sLogFolder%"
) ELSE (
    @ECHO.
    @ECHO.
    @ECHO   Log folder sets to %sLogFolder%
    @ECHO.
    @ECHO.
)

@ECHO REG ADD HKLM\Software\Microsoft\Fusion /V EnableLog /T REG_DWORD /D 1 /F
      REG ADD HKLM\Software\Microsoft\Fusion /V EnableLog /T REG_DWORD /D 1 /F
@ECHO.
@ECHO REG ADD HKLM\Software\Microsoft\Fusion /V LogFailures /T REG_DWORD /D 1 /F
      REG ADD HKLM\Software\Microsoft\Fusion /V LogFailures /T REG_DWORD /D 1 /F
@ECHO.
@ECHO REG ADD HKLM\Software\Microsoft\Fusion /V LogResourceBinds /T REG_DWORD /D 1 /F
      REG ADD HKLM\Software\Microsoft\Fusion /V LogResourceBinds /T REG_DWORD /D 1 /F
@ECHO.
@ECHO REG ADD HKLM\Software\Microsoft\Fusion /V LogPath /T REG_SZ /D "%sLogFolder%\\" /F
      REG ADD HKLM\Software\Microsoft\Fusion /V LogPath /T REG_SZ /D "%sLogFolder%\\" /F
@ECHO.

:Done

@ECHO.
@ECHO.
@ECHO ------------------------------------------------------------------
@ECHO Done.
@ECHO.
@ECHO.

EXIT /B 0