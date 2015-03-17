/*++
Copyright (C) 2007 - 1015 Hong Liu
Check out http://ScissorTools.WordPress.com/ for updates.

    THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    PURPOSE.

--*/

// GetFileTimestamp.cpp : Defines the entry point for the console application.
//
// Internally, Windows uses UTC time to store the info, and "dir" displays the last write time in local time format.
//
//

#define _CRT_SECURE_NO_WARNINGS

#include <windows.h>
#include <wchar.h>
#include <strsafe.h>
#include <Shlwapi.h>
#include "GetOpt.h"


#define FILE_ATTRIBUTE_RSH	( FILE_ATTRIBUTE_SYSTEM | FILE_ATTRIBUTE_READONLY | FILE_ATTRIBUTE_HIDDEN )


// The original file/folder name will save to <CmdOptions_ExtraParam>

enum APP_OPERATION {
    APP_OPERATION_READ_LAST_CREATION_TIME = 0,
    APP_OPERATION_READ_LAST_WRITE_TIME,
    APP_OPERATION_READ_LAST_ACCESS_TIME,
    APP_OPERATION_WRITE_LAST_CREATION_TIME,
    APP_OPERATION_WRITE_LAST_WRITE_TIME,
    APP_OPERATION_WRITE_LAST_ACCESS_TIME,
    APP_OPERATION_RECURSIVE,
    APP_OPERATION_ROOT_FOLDER,
    APP_OPERATION_SUB_FOLDER,
    APP_OPERATION_VERIFY,
    APP_OPERATION_SIZE
};

CmdOptions MyParams[APP_OPERATION_SIZE] = {
/* 0 */    { L"RLastCreate",L'c', L"Read last create time of the file",                     TRUE,  FALSE,  NULL },
/* 1 */    { L"RLastWrite", L'w', L"Read last write time of the file",                      TRUE,  FALSE,  NULL },
/* 2 */    { L"RLastAccess",L'a', L"Read last access time of the file",                     TRUE,  FALSE,  NULL },
/* 3 */    { L"WLastCreate",L'1', L"Write last create time of the file",                    FALSE, TRUE,   L"" },
/* 4 */    { L"WLastWrite", L'2', L"Write last write time of the file",                     FALSE, TRUE,   L"" },
/* 5 */    { L"WLastAccess",L'3', L"Write last access time of the file",                    FALSE, TRUE,   L"" },
/* 6 */    { L"recursive",  L'r', L"Recursively surf the sub-folders",                      FALSE, FALSE,  NULL },
/* 7 */    { L"folder",     L'f', L"Change root folder's Date/Time as well",                FALSE, FALSE,  NULL },
/* 8 */    { L"SubFolder",  L's', L"Change all the sub-folders' Date/Time as well",         FALSE, FALSE,  NULL },
/* 9 */    { L"Verify",     L'v', L"Verify the changes, redo it up to 3 times if failed",   FALSE, FALSE,  NULL },
};


LPCWSTR FILETIME_ACTION_STR[APP_OPERATION_SIZE] = {
/* 0 */    L"Last Creation Time",
/* 1 */    L"Last Write Time",
/* 2 */    L"Last Access Time",
/* 3 */    L"Last Creation Time",
/* 4 */    L"Last Write Time",
/* 5 */    L"Last Access Time",
/* 6 */    L"",
/* 7 */    L"",
/* 8 */    L"",
/* 9 */    L"",
};

const INT nRetry = 3;


// *----------------------------------------------------------------------------
__inline HRESULT HRESULT_FROM_LAST_ERROR( VOID )
{
    DWORD dwError = GetLastError();
    return HRESULT_FROM_WIN32( dwError );
}


// *----------------------------------------------------------------------------
HRESULT CompareFILETIME( LPFILETIME pft1, LPFILETIME pft2 )
{
    if( NULL == pft1 ||
        NULL == pft2 )
        return E_INVALIDARG;

    return ( pft1->dwHighDateTime == pft2->dwHighDateTime &&
        pft1->dwLowDateTime == pft2->dwLowDateTime )? S_OK : E_FAIL;
}


// *----------------------------------------------------------------------------
VOID PrintHelp()
{
    wprintf( L"\
\n\n\rTool to read or update the times of the file(s) and folder(s).\n\n\r" );
    getopt_long_Help( APP_OPERATION_SIZE, MyParams );

    wprintf( L"\
\n\rExample:\n\n\r\
<this EXE>    a.dll /a                  Display last access time of a.dll.\n\r\
<this EXE>    a.dll                     Display all three times of a.dll.\n\r\
<this EXE>    a.dll /1 \"2011/1/2 3:45:56\" Modify creation time of a.dll to\n\r\
                                            2011/1/2 3:45:56.\n\r\
<this EXE>    c:\\Windows\\*.txt /a       Display last access time of all the txt files.\n\r\
<this EXE>    c:\\Windows\\*.txt /a -r    Display last access time of all the txt files,\n\r\
                                            recursively.\r\n\
<this EXE>    c:\\Windows\\*.txt /ar      Save As Above.\n\r\
<this EXE>    c:\\Windows\\*.txt /r /3 \"2011/1/2 3:45:56\"\n\r\
                                        Modify last access time of ALL .txt files\n\r\
                                        (recursively) to 2011/1/2 3:45:56.\n\r\
<this EXE> -s c:\\Windows\\*.txt /r /3 \"2011/1/2 3:45:56\"\n\r\
                                        Save As Above. Also modify the sub-folders'\n\r\
                                            Date/Time to 2011/1/2 3:45:56.\n\r\
<this EXE> -fs c:\\Windows\\*.txt /r /3 \"2011/1/2 3:45:56\"\n\r\
                                        Save As Above. Also modify all sub-folders\n\r\
                                        as well as c:\\Windows to 2011/1/2 3:45:56.\n\r\
<this EXE> /fsr c:\\Windows\\*.txt /3 \"2011/1/2 3:45:56\"\n\r\
                                        Save As Above.\n\r\
<this EXE> /v /fsr c:\\Windows\\*.txt /3 \"2011/1/2 3:45:56\"\n\r\
                                        Save As Above. Also verify the changes for up to 3 times.\n\r\
\r\n\n" );
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
 
    // possible win32 API bug in GetFullPathName() that lpFilePart does NOT get freed at the end, though the space is allocated by the function
    // If someone keeps calling GetFullPathName() for a while, the process may crash at last.
    // Right now we just ignore it

    if( 0 == GetFullPathNameW( pwszObjectName, MAX_PATH, pwszFullName, &lpFilePart ) ||
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
HRESULT OutputTimestamp( 
                            __deref_in      LPCWSTR             pwszFileName,
                                            DWORD               iAction,
                            __deref_in      LPFILETIME          pFiletime )
{
    HRESULT     hr = S_OK;
    FILETIME    ft = { 0 };
    SYSTEMTIME  st = { 0 };

    if( NULL == pwszFileName ||
        APP_OPERATION_SIZE <= iAction ||
        NULL == pFiletime )
        return E_INVALIDARG;

    if( FALSE == FileTimeToLocalFileTime( pFiletime, &ft ) ||
        FALSE == FileTimeToSystemTime( &ft, & st ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    wprintf( L"%20s\t%d/%d/%d %d:%d:%d\n", FILETIME_ACTION_STR[iAction], st.wYear, st.wMonth, st.wDay, st.wHour, st.wMinute, st.wSecond );

Done:
    return hr;
}


// *----------------------------------------------------------------------------
HRESULT SetTimeOnOneFile( HANDLE hFile, PBY_HANDLE_FILE_INFORMATION pfiFile )
{
    HRESULT     hr = S_OK;
    FILETIME    ft = { 0 },
                ftUTC[3] = { {0}, {0}, {0} };
    SYSTEMTIME  st = { 0 };

    if( INVALID_HANDLE_VALUE == hFile ||
        NULL == pfiFile )
        return E_INVALIDARG;

    memcpy_s( & ftUTC[0], sizeof(FILETIME), & pfiFile->ftCreationTime, sizeof(FILETIME) );
    memcpy_s( & ftUTC[1], sizeof(FILETIME), & pfiFile->ftLastWriteTime, sizeof(FILETIME) );
    memcpy_s( & ftUTC[2], sizeof(FILETIME), & pfiFile->ftLastAccessTime, sizeof(FILETIME) );

    for( DWORD i = APP_OPERATION_WRITE_LAST_CREATION_TIME; i <= APP_OPERATION_WRITE_LAST_ACCESS_TIME; i++ )
    {
        if( MyParams[i].bExist )
        {
            swscanf_s( MyParams[i].wszParam, L"%d/%d/%d %d:%d:%d", &(st.wYear), &(st.wMonth), &(st.wDay), &(st.wHour), &(st.wMinute), &(st.wSecond) );
            if( 0 == st.wYear ||
                0 == st.wMonth ||
                0 == st.wDay )
            {
                wprintf( L"\nERROR: Invalid DateTime stamp: %s\n", MyParams[i].wszParam );
                return E_INVALIDARG;
            }

            if( FALSE == SystemTimeToFileTime( & st, & ft ) ||
                FALSE == LocalFileTimeToFileTime( & ft, & ftUTC[i - APP_OPERATION_WRITE_LAST_CREATION_TIME] ) )
            {
                hr = HRESULT_FROM_LAST_ERROR();
                goto Done;
            }
        }
    }

    if( FALSE == SetFileTime( hFile, & ftUTC[0], & ftUTC[2], & ftUTC[1] ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
    }

Done:
    return hr;
}


// *----------------------------------------------------------------------------
HRESULT VerifyTimeOnOneFile( HANDLE hFile, PBY_HANDLE_FILE_INFORMATION pfiFile )
{
    HRESULT     hr = S_OK;
    BY_HANDLE_FILE_INFORMATION fiFileNew = { 0 };

    if( INVALID_HANDLE_VALUE == hFile ||
        NULL == pfiFile )
        return E_INVALIDARG;

    if( FALSE == GetFileInformationByHandle( hFile, & fiFileNew ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    if( FAILED( CompareFILETIME( &(fiFileNew.ftCreationTime), &(pfiFile->ftCreationTime) ) ) ||
        FAILED( CompareFILETIME( &(fiFileNew.ftLastAccessTime), &(pfiFile->ftLastAccessTime ) ) ) ||
        FAILED( CompareFILETIME( &(fiFileNew.ftLastWriteTime), &(pfiFile->ftLastWriteTime ) ) ) )
    {
        hr = E_FAIL;
    }

Done:
    return hr;
}


// *----------------------------------------------------------------------------

HRESULT WorkOnOneFile(
                            __in            SIZE_T              pwszFileExtension,
                            __deref_in      LPCWSTR             pwszFileName )
{
    HRESULT     hr = S_OK;
    BY_HANDLE_FILE_INFORMATION  fiFile = { 0 },
                                fiOriginal = { 0 };
    HANDLE      hFile = INVALID_HANDLE_VALUE;
    INT         iRetries = 1;

    if( 0 == pwszFileExtension ||
        NULL == pwszFileName )
        return E_INVALIDARG;

    // GetFileTime()/GetFileInformationByHandle() returns UTC time, and we need to convert to local time for display

    if( ( INVALID_HANDLE_VALUE == ( hFile = CreateFileW( pwszFileName,
                                                         GENERIC_READ | GENERIC_WRITE,
                                                         0, 
                                                         NULL, 
                                                         OPEN_EXISTING, 
                                                         FILE_ATTRIBUTE_ARCHIVE | FILE_ATTRIBUTE_NORMAL | FILE_ATTRIBUTE_READONLY | FILE_ATTRIBUTE_HIDDEN | FILE_ATTRIBUTE_SYSTEM | FILE_FLAG_SEQUENTIAL_SCAN,
                                                         NULL ) ) ) ||
        ( FALSE == GetFileInformationByHandle( hFile, & fiFile ) ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    memcpy_s(& fiOriginal, sizeof(BY_HANDLE_FILE_INFORMATION), & fiFile, sizeof(BY_HANDLE_FILE_INFORMATION) );

    wprintf( L"\n----- %s -----\r\n", pwszFileName );

    if( TRUE == MyParams[APP_OPERATION_READ_LAST_CREATION_TIME].bExist )
    {
        hr = OutputTimestamp( pwszFileName, APP_OPERATION_READ_LAST_CREATION_TIME, & fiFile.ftCreationTime );
    }
    if( TRUE == MyParams[APP_OPERATION_READ_LAST_ACCESS_TIME].bExist )
    {
        hr = OutputTimestamp( pwszFileName, APP_OPERATION_READ_LAST_ACCESS_TIME, & fiFile.ftLastAccessTime );
    }
    if( TRUE == MyParams[APP_OPERATION_READ_LAST_WRITE_TIME].bExist )
    {
        hr = OutputTimestamp( pwszFileName, APP_OPERATION_READ_LAST_WRITE_TIME, & fiFile.ftLastWriteTime );
    }

    if( TRUE == MyParams[APP_OPERATION_WRITE_LAST_CREATION_TIME].bExist ||
        TRUE == MyParams[APP_OPERATION_WRITE_LAST_WRITE_TIME].bExist ||
        TRUE == MyParams[APP_OPERATION_WRITE_LAST_ACCESS_TIME].bExist )
    {
        hr = SetTimeOnOneFile( hFile, &fiFile );
        if( FALSE == MyParams[APP_OPERATION_VERIFY].bExist )
        {
            goto Done;
        }

        while( iRetries <= nRetry )
        {
            Sleep( 1000 );                                                      // sleep for one second

            if( SUCCEEDED( hr = VerifyTimeOnOneFile( hFile, &fiFile ) ) )
                break;

            iRetries ++;
            hr = SetTimeOnOneFile( hFile, &fiFile );
        }
    }

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

HRESULT SetDateTimeOnOneDirectory(
                            __deref_in      LPCWSTR             pwszDirName )
{
    HRESULT                     hr   = S_OK;
    HANDLE                      hFile = INVALID_HANDLE_VALUE;
    BY_HANDLE_FILE_INFORMATION  fiFile = { 0 };
    DWORD                       dwFileAttr = 0;
    WCHAR                       wszTemp[MAX_PATH] = { 0 };
    size_t                      dwSize = 0;
    INT                         iRetries = 1;

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
                                                        0) ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    if( FALSE == GetFileInformationByHandle( hFile, &fiFile ) )
    {
        hr = HRESULT_FROM_LAST_ERROR();
        goto Done;
    }

    if( INVALID_FILE_ATTRIBUTES == ( dwFileAttr = GetFileAttributesW( (LPCWSTR) wszTemp ) ) )
    {
        dwFileAttr = 0;
    }
    if( FILE_ATTRIBUTE_RSH & dwFileAttr )
    {
        dwFileAttr &= ~FILE_ATTRIBUTE_RSH;
    }

    if( TRUE == MyParams[APP_OPERATION_WRITE_LAST_CREATION_TIME].bExist ||
        TRUE == MyParams[APP_OPERATION_WRITE_LAST_WRITE_TIME].bExist ||
        TRUE == MyParams[APP_OPERATION_WRITE_LAST_ACCESS_TIME].bExist )
    {
        hr = SetTimeOnOneFile( hFile, &fiFile );
        if( FALSE == MyParams[APP_OPERATION_VERIFY].bExist )
        {
            goto Done;
        }

        while( iRetries <= nRetry )
        {
            Sleep( 1000 );                                                      // sleep for one second

            if( SUCCEEDED( hr = VerifyTimeOnOneFile( hFile, &fiFile ) ) )
                break;

            iRetries ++;
            hr = SetTimeOnOneFile( hFile, &fiFile );
        }
    }

Done:
    if( INVALID_HANDLE_VALUE != hFile )
    {
        CloseHandle( hFile );
        hFile = INVALID_HANDLE_VALUE;
    }

    return hr;
}


// *----------------------------------------------------------------------------

HRESULT WorkOnOneDirectory(
                            __deref_in      LPCWSTR             pwszDirName,
                            __deref_in      LPCWSTR             pwszFileExtension )
{
    HRESULT             hr = S_OK;
    WCHAR               sPath[MAX_PATH] = { 0 };
    WCHAR               sFile[MAX_PATH] = { 0 };
    HANDLE              hFind = INVALID_HANDLE_VALUE;
    BOOL                bKeep = TRUE;
    WIN32_FIND_DATAW    FindFileData = { 0 };
    size_t              dwSize = 0;

    if( NULL == pwszDirName ||
        NULL == pwszFileExtension )
        return E_INVALIDARG;

    wprintf( L"\n====================\n%s\n====================\n\n", pwszDirName );

    if( TRUE == MyParams[APP_OPERATION_ROOT_FOLDER].bExist &&                   // change current folder?
        FAILED( hr = SetDateTimeOnOneDirectory( pwszDirName ) ) )
    {
        goto Done;
    }

    StringCchLengthW( pwszDirName, STRSAFE_MAX_CCH,  &dwSize );
    if( L'\\' == pwszDirName[dwSize] )
    {
        StringCchPrintfW( sPath, MAX_PATH, L"%s%s", pwszDirName, pwszFileExtension );
    }
    else
    {
        StringCchPrintfW( sPath, MAX_PATH, L"%s\\%s", pwszDirName, pwszFileExtension );
    }

    hFind = FindFirstFileW( sPath, &FindFileData );

    while( INVALID_HANDLE_VALUE != hFind && bKeep )
    {
        if( L'.' != FindFileData.cFileName[0] &&                                // skip . & ..
            ! (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) &&
            ! (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) )
        {
            StringCchPrintfW( sFile, MAX_PATH, L"%s\\%s", pwszDirName, FindFileData.cFileName );
            (VOID) WorkOnOneFile( ARRAY_SIZE(sFile), sFile );                   // ignore the errors here so we can continue
        }

        bKeep = FindNextFileW( hFind, &FindFileData );
    }  // while

    FindClose( hFind );

    if( TRUE == MyParams[APP_OPERATION_RECURSIVE].bExist ||                     // recursive?
        TRUE == MyParams[APP_OPERATION_SUB_FOLDER].bExist )                     // change child folder?
    {
        if( L'\\' == pwszDirName[dwSize] )
        {
            StringCchPrintfW( sPath, MAX_PATH, L"%s*.*", pwszDirName );
        }
        else
        {
            StringCchPrintfW( sPath, MAX_PATH, L"%s\\*.*", pwszDirName );
        }

        bKeep = TRUE;
        hFind = FindFirstFileW( sPath, &FindFileData );

        while( INVALID_HANDLE_VALUE != hFind && bKeep )
        {
            if( L'.' != FindFileData.cFileName[0] &&                            // skip . & ..
                (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) )
            {
                StringCchPrintfW( sFile, MAX_PATH, L"%s\\%s", pwszDirName, FindFileData.cFileName );

                if( TRUE == MyParams[APP_OPERATION_SUB_FOLDER].bExist )         // change child folder?
                {
                    (VOID) SetDateTimeOnOneDirectory( sFile );                  // ignore the errors here so we can continue
                }

                if( TRUE == MyParams[APP_OPERATION_RECURSIVE].bExist )          // recursive?
                {
                    (VOID) WorkOnOneDirectory( sFile, pwszFileExtension );      // ignore the errors here so we can continue
                }
            }

            bKeep = FindNextFileW( hFind, &FindFileData );
        }  // while

        FindClose( hFind );
    }

Done:
    return hr;
}


// *----------------------------------------------------------------------------
int wmain(	__in                int argc,
            __in_ecount(argc)   LPWSTR argv[] )
{
    HRESULT     hr = S_OK;
    WCHAR       TheObjectPath[MAX_PATH] = { 0 };
    WCHAR       FileExtensionName[MAX_PATH] = { 0 };
    BOOL        bFileIsFile = FALSE;
    BOOL        bHasOption = FALSE;
    size_t      dwSize = 0;

    if( 1 == argc ||
        FAILED( hr = getopt_long_win( argc, (LPCWSTR*) argv, APP_OPERATION_SIZE, MyParams ) ) )
    {
        PrintHelp();
        hr = E_INVALIDARG;
        goto Done;
    }

    // if no options, by default will display all the three attributes
    for( INT i = APP_OPERATION_READ_LAST_CREATION_TIME; i < APP_OPERATION_SIZE; i++ )
    {
        if( TRUE == MyParams[i].bExist )
        {
            bHasOption = TRUE;
            break;
        }
    }
    if( FALSE == bHasOption )                                                   // no options
    {
        MyParams[APP_OPERATION_READ_LAST_CREATION_TIME].bExist = MyParams[APP_OPERATION_READ_LAST_ACCESS_TIME].bExist = MyParams[APP_OPERATION_READ_LAST_WRITE_TIME].bExist = TRUE;
    }

    if( TRUE == CmdOptions_HasWildChar )
    {
        StringCchLengthW( CmdOptions_ExtraParam, STRSAFE_MAX_CCH,  &dwSize );

        LPWSTR  pStr = &( CmdOptions_ExtraParam[dwSize] );
        WCHAR   wszCurrentDir[MAX_PATH] = { 0 };

        GetCurrentDirectoryW( ARRAY_SIZE(wszCurrentDir), wszCurrentDir );

        // we try to find the path + file & extension first
        while( pStr >= CmdOptions_ExtraParam && L'\\' != pStr[0] )  pStr --;
        if( pStr < CmdOptions_ExtraParam )                                      // there is no path
        {
            if( FAILED( hr = GetFileFullPathNameW( wszCurrentDir, TheObjectPath ) ) )
            {
                goto Done;
            }

            StringCchCopyW( FileExtensionName, ARRAY_SIZE(FileExtensionName), CmdOptions_ExtraParam );
        }
        else                                                                    // we try to seperate path from file name/extension
        {
            LPWSTR pStr2 = CmdOptions_ExtraParam;
            LPWSTR pStr3 = TheObjectPath;
            while( pStr2 < pStr )
                *(pStr3 ++) = *(pStr2 ++);
            pStr2 ++;
            pStr3 = FileExtensionName;
            while( 0x00 != *pStr2 )
                *(pStr3 ++) = *(pStr2 ++);
        }
    }
    else
    {
        if( FAILED( hr = GetFileFullPathNameW( CmdOptions_ExtraParam, TheObjectPath ) ) ||
            FAILED( hr = CheckFileObjectTypeW( TheObjectPath, & bFileIsFile ) ) )
        {
            goto Done;
        }
        StringCchCopyW( FileExtensionName, ARRAY_SIZE(FileExtensionName), L"*.*" );
    }

    if( TRUE == MyParams[APP_OPERATION_ROOT_FOLDER].bExist &&               // change current folder?
        FAILED( hr = SetDateTimeOnOneDirectory( TheObjectPath ) ) )
    {
        goto Done;
    }
    
    if( FALSE == bFileIsFile )
    {
        hr = WorkOnOneDirectory( TheObjectPath, FileExtensionName );
    }
    else
    {
        hr = WorkOnOneFile( ARRAY_SIZE(TheObjectPath), TheObjectPath );
    }

Done:
    wprintf( L"\nExit with code = 0x%08X\n", (DWORD) hr );
    return hr;
}

