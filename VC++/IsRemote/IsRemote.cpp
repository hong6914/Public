/*++
Copyright (C) 2007 - 1015 Hong Liu
Check out https://github.com/hong6914/Public/ for updates.

    THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    PURPOSE.

--*/

// IsRemote.cpp : check to see if the file or folder is on which device (USB/CD/network/local/mounted, etc.).
//

#include <windows.h>
#include <wchar.h>
#include <strsafe.h>
#include <Shlwapi.h>
#include "ntapi.h"


#ifndef ARRAY_SIZE
    #define ARRAY_SIZE( __x )          sizeof(__x) / sizeof(__x[0])
#endif

static FILE_FS_DEVICE_INFORMATION device_info = { 0 };


// *----------------------------------------------------------------------------
__inline HRESULT HRESULT_FROM_LAST_ERROR( VOID )
{
    DWORD dwError = GetLastError();
    return HRESULT_FROM_WIN32( dwError );
}


// *----------------------------------------------------------------------------
VOID PrintHelp()
{
    wprintf( L"\
\n\n\rTool to check if the file or folder is on network/USB/Floppy/CD/Mounted drive, etc.\n\
\n\rExample:\n\n\r\
<this EXE>    <File or Folder>\n\n" );
}


// *----------------------------------------------------------------------------
HRESULT GetFileObjectProperty(
                            __deref_in      HANDLE              hFileHandle,
                            __deref_out     PFILE_FS_DEVICE_INFORMATION pdevice_info )
{
    IO_STATUS_BLOCK       status_block = { 0 };
    NTSTATUS              status = STATUS_SUCCESS;
    FuncNtQueryVolumeInformationFile pNtQueryVolumeInformationFile = NULL;

    if( INVALID_HANDLE_VALUE == hFileHandle ||
        NULL == pdevice_info )
        return E_INVALIDARG;

    HMODULE hNTDLL = GetModuleHandleW( L"ntdll" );          // The call does not increment NTDLL's load count as mentioned in MSDN
    if( NULL == hNTDLL ||
        NULL == ( pNtQueryVolumeInformationFile = (FuncNtQueryVolumeInformationFile) GetProcAddress( hNTDLL, "NtQueryVolumeInformationFile" ) ) )
    {
        return FALSE;
    }

    status = pNtQueryVolumeInformationFile ( hFileHandle, & status_block, pdevice_info, sizeof(FILE_FS_DEVICE_INFORMATION), FileFsDeviceInformation );

    return ( NT_SUCCESS( status )? S_OK : E_FAIL );
}


// *----------------------------------------------------------------------------
VOID PrintoutResult(
                            __deref_in      PFILE_FS_DEVICE_INFORMATION pdevice_info )
{
    if( NULL == pdevice_info )
        return;

    for( INT i = 0; i < 7; i++ )
    {
        if( 0 != ( pdevice_info->Characteristics & FS_DEVICE_CHARACTERISTICS_VALUE[i] ) )
        {
            wprintf( L"\t%s\n", FS_DEVICE_CHARACTERISTICS_STR[i] );
        }
    }

}


// *----------------------------------------------------------------------------
HRESULT GetFileFullPathNameW(
                            __deref_in      LPCWSTR             pwszObjectName, // possible relative path to the file/folder
                            __deref_out     LPWSTR              pwszFullName )  // full UNC path to the file/folder
{
    HRESULT         hr          = S_OK;
    LPWSTR          lpFilePart  = NULL;

    if( NULL == pwszObjectName ||
        NULL == pwszFullName )
        return E_INVALIDARG;
 
    if( 0 == GetFullPathNameW( pwszObjectName, MAX_PATH, pwszFullName, NULL ) ||
        ! PathFileExistsW( pwszFullName ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

Done:
    return hr;
}


// *----------------------------------------------------------------------------
HRESULT CheckFileObjectTypeW(
                            __deref_in      LPCWSTR             pwszObjectName,
                            __deref_out     BOOL              * pObjectIsFile )
{
    HRESULT         hr      = S_OK;
    WCHAR           wszFullName[MAX_PATH] = { 0 };

    if( NULL == pwszObjectName ||
        NULL == pObjectIsFile )
        return E_INVALIDARG;

    if( FAILED( hr = GetFileFullPathNameW( pwszObjectName, wszFullName ) ) )
        goto Done;

    DWORD dwFileAttrs = GetFileAttributesW( pwszObjectName );

    if( INVALID_FILE_ATTRIBUTES == dwFileAttrs )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    else if( dwFileAttrs & FILE_ATTRIBUTE_DIRECTORY )
    {
        *pObjectIsFile = FALSE;
    }
    else if( dwFileAttrs & FILE_ATTRIBUTE_ARCHIVE ||
             dwFileAttrs & FILE_ATTRIBUTE_NORMAL ||
             dwFileAttrs & FILE_ATTRIBUTE_READONLY )
    {
        *pObjectIsFile = TRUE;
    }
    else                                                                        // unknown file object
    {
        hr = E_UNEXPECTED;
    }

Done:
    return hr;
}


// *----------------------------------------------------------------------------
HRESULT WorkOnOneFile(
                            __deref_in      LPCWSTR             pwszFileName )
{
    HRESULT     hr = S_OK;
    HANDLE      hFile = INVALID_HANDLE_VALUE;

    if( NULL == pwszFileName )
        return E_INVALIDARG;

    if( ( INVALID_HANDLE_VALUE == ( hFile = CreateFileW( pwszFileName,
                                                         GENERIC_READ | GENERIC_WRITE,
                                                         0, 
                                                         NULL, 
                                                         OPEN_EXISTING, 
                                                         FILE_ATTRIBUTE_ARCHIVE | FILE_ATTRIBUTE_NORMAL | FILE_ATTRIBUTE_READONLY | FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM | FILE_FLAG_SEQUENTIAL_SCAN,
                                                         NULL ) ) ) ||
         FAILED( GetFileObjectProperty( hFile, & device_info ) ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    wprintf( L"\nFile \"%s\" is on\n\n", pwszFileName );
    PrintoutResult( & device_info );

Done:
    if( INVALID_HANDLE_VALUE != hFile )
    {
        CloseHandle( hFile );
        hFile = INVALID_HANDLE_VALUE;
    }

    return hr;
}


// *----------------------------------------------------------------------------
// Assume the folder path is the absolute path

HRESULT WorkOnOneDirectory(
                            __deref_in      LPCWSTR             pwszDirName )
{
    HRESULT                     hr   = S_OK;
    HANDLE                      hFile = INVALID_HANDLE_VALUE;
    WCHAR                       wszTemp[MAX_PATH] = { 0 };
    size_t                      dwSize = 0;

    if( NULL == pwszDirName ||
        0x00 == pwszDirName[0] )
        return E_INVALIDARG;

    // make a copy of the path
    if( FAILED( StringCchLengthW( pwszDirName, STRSAFE_MAX_CCH,  &dwSize ) ) ||
        FAILED( StringCchCopyW( (LPWSTR) wszTemp, ARRAY_SIZE(wszTemp), pwszDirName ) ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    // if not a qualified path, find it and check again
    if( FALSE == PathIsDirectoryW( wszTemp ) )
    {
        LPWSTR pTemp = (LPWSTR) wszTemp + dwSize;

        while( pTemp != (LPWSTR) wszTemp && L'\\' != *pTemp )
        {
            pTemp --;
        }

        if( pTemp == (LPWSTR) wszTemp )                                         // BUG: the whole "path" is a file name?
        {
            return E_INVALIDARG;
        }

        if( L'\\' == *pTemp )
        {
            *pTemp = 0x00;

            if( FALSE == PathIsDirectoryW( wszTemp ) )                          // still not a valid path
            {
                return E_INVALIDARG;
            }
        }
    }

    if( INVALID_HANDLE_VALUE == ( hFile = CreateFileW(  (LPCWSTR) wszTemp,
                                                        GENERIC_READ | GENERIC_WRITE,
                                                        FILE_SHARE_READ|FILE_SHARE_WRITE,
                                                        NULL,
                                                        OPEN_EXISTING,
                                                        FILE_ATTRIBUTE_NORMAL|FILE_FLAG_BACKUP_SEMANTICS,
                                                        0) ) ||
        FAILED( GetFileObjectProperty( hFile, & device_info ) ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    wprintf( L"\nFolder \"%s\" is on\n\n", pwszDirName );
    PrintoutResult( & device_info );

Done:
    if( INVALID_HANDLE_VALUE != hFile )
    {
        CloseHandle( hFile );
        hFile = INVALID_HANDLE_VALUE;
    }

    return hr;
}


// *----------------------------------------------------------------------------
int wmain(	__in                int argc,
            __in_ecount(argc)   LPWSTR argv[] )
{
    HRESULT     hr = S_OK;
    WCHAR       TheObjectPath[MAX_PATH] = { 0 };
    BOOL        bFileIsFile = FALSE;
    size_t      dwSize = 0;

    if( 2 != argc )
    {
        PrintHelp();
        hr = E_INVALIDARG;
        goto Done;
    }

    if( FAILED( hr = GetFileFullPathNameW( argv[1], TheObjectPath ) ) ||
        FAILED( hr = CheckFileObjectTypeW( TheObjectPath, & bFileIsFile ) ) )
    {
        goto Done;
    }

    if( FALSE == bFileIsFile )
    {
        hr = WorkOnOneDirectory( TheObjectPath );
    }
    else
    {
        hr = WorkOnOneFile( TheObjectPath );
    }

Done:
    wprintf( L"\nExit with code = 0x%08X\n", (DWORD) hr );
    return hr;
}
