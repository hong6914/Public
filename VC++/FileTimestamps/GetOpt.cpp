/*++
Copyright (C) 2007 - 1015 Hong Liu
Check out http://ScissorTools.WordPress.com/ for updates.

    THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    PURPOSE.

--*/

// getopt
//

#include <windows.h>
#include <wchar.h>
#include <strsafe.h>

#include "GetOpt.h"

BOOL    CmdOptions_IgnoreUnknown = TRUE;                    // ignore unknown commands? if FALSE: return E_INVALIDARG
BOOL    CmdOptions_HasWildChar = FALSE;                     // TRUE if the parameter contains '?' or '*'
WCHAR   CmdOptions_ExtraParam[MAX_PATH] = { 0 };            // store extra parameter


// *--------------------------------------------------------------------------------------------------------------
extern VOID WINAPI getopt_long_Help(
                            __in                        INT         nCmd,
                            __deref_inout_ecount(nCmd)  PCmdOptions pCmd )
{
    PCmdOptions pOne = pCmd;

    fwprintf( stdout, L"\
All parameters are NOT case-sensitive.\n\
\nOptions:\n\n\
short  long\t\tNote\n\
=====  ====\t\t====\n" );

    for( INT i = 0; i < nCmd; i++, pOne++ )
    {
        fwprintf( stdout, L"/%c -%c  --%-15s%s.\n",
            pOne->wszShort,
            pOne->wszShort,
            pOne->wszLong,
            pOne->wszHelp );
    }

    fwprintf( stdout, L"\n" );
}


// *--------------------------------------------------------------------------------------------------------------

extern HRESULT WINAPI getopt_long_win(
                            __in                        INT         argc,
                            __deref_in                  LPCWSTR    *argv,
                            __in                        INT         nCmd,
                            __deref_inout_ecount(nCmd)  PCmdOptions pCmd )
{
    HRESULT     hr = S_OK;
    INT         i = 0,
                j = 0,
                k = 0;
    BOOL        bFindIt = FALSE;
    PCmdOptions pOne = pCmd;

    if( NULL == argv ||
        NULL == pCmd )
        return E_INVALIDARG;

    if( 1 == argc )                                         // no param
    {
        return hr;
    }

    for( i = 0; i < nCmd; i++, pOne++ )                     // either long or short command should be specified
    {
        if( NULL == pOne->wszLong &&
            0x00 == pOne->wszShort )
            return E_INVALIDARG;

        pOne->bExist = FALSE;
    }

    for( i = 1; i < argc; i++ )
    {
        LPCWSTR pStr = argv[i];

        bFindIt = FALSE;

        if( NULL == pStr )                                  // skip NULL parameter
        {
            continue;
        }

        if( L'-' == pStr[0] &&
            L'-' == pStr[1] )                               // long format
        {
            pStr += 2;                                      // skip the prefix
            for( j = 0; j < nCmd; j++ )
            {
                if( 0 == _wcsicmp( pStr, pCmd[j].wszLong ) )
                {
                    pCmd[j].bExist = bFindIt = TRUE;
                    break;
                }
            }

            if( bFindIt )
            {
                if( pCmd[j].bHasParam )
                {
                    if( ++i >= argc )                       // miss extra param
                    {
                        hr = E_INVALIDARG;
                        break;
                    }
                    else
                        hr = StringCchCopyW( pCmd[j].wszParam, MAX_PATH, argv[i] );
                }
            }
            else
            {
                hr = E_INVALIDARG;
            }

            if( FAILED(hr) )
                break;
        }

        else

        if( L'/' == pStr[0] ||
           (L'-' == pStr[0] &&
            L'-' != pStr[1]) )                              // short format
        {
            for( k = 1; pStr[k] && k < MAX_PATH; k++ )      // support fall-through, e.g. -12345678 for all short format parameters
            {
                bFindIt = FALSE;

                for( j = 0; j < nCmd; j++ )                 // look for each match
                {
                    if( towupper( pStr[k] ) == towupper( pCmd[j].wszShort ) )
                    {
                        pCmd[j].bExist = bFindIt = TRUE;
                        break;
                    }
                }  // for

                if( bFindIt )
                {
                    if( pCmd[j].bHasParam )
                    {
                        if( ++i >= argc )                   // miss extra param
                        {
                            hr = E_INVALIDARG;
                            break;
                        }
                        else
                            hr = StringCchCopyW( pCmd[j].wszParam, MAX_PATH, argv[i] );
                    }
                }
                else
                {
                    hr = E_INVALIDARG;
                }

                if( FAILED(hr) )
                    break;
            }  // for
        }

        else if( CmdOptions_IgnoreUnknown && ! CmdOptions_ExtraParam[0] )
        {
            StringCchCopyW( CmdOptions_ExtraParam, ARRAY_SIZE(CmdOptions_ExtraParam), pStr );
            bFindIt = TRUE;

            if( NULL != wcschr( CmdOptions_ExtraParam, L'?' ) ||
                NULL != wcschr( CmdOptions_ExtraParam, L'*' ) )
            {
                CmdOptions_HasWildChar = TRUE;
            }
        }

        else
        {
            hr = E_INVALIDARG;
        }

        if( ! bFindIt && SUCCEEDED(hr) )                                        // in case someone type a command in "valiid" format but invalid in syntax
        {
            hr = E_INVALIDARG;
        }

        if( FAILED(hr) )
            break;
    }  // for

    return hr;
}

