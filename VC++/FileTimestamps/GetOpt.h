/*++
Copyright (C) 2007 - 1015 Hong Liu
Check out http://ScissorTools.WordPress.com/ for updates.

    THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    PURPOSE.

--*/


/*
    getopt -- get commandline options

    features:
        Allow either one or both long form and short form of one option
        Allow each parameter to take one extra parameter, e.g. --file <your file name>
        Allow short parameters to pass through, e.g. -abcdefg   <--- each represent one option (if they take extra parameters, just type them after)
        Allow extra parameter not specified in the CmdOptions array
        Allow to ignore the unknown commands

*/

#pragma once

#include <windows.h>

typedef struct _CmdOptions
{
    WCHAR       wszLong[31];                                // IN:  the long format of the param, prefix "--" is not in the string
    WCHAR       wszShort;                                   // IN:  the short format of the param, prefix "-" or "/" is not in
    WCHAR       wszHelp[MAX_PATH];                          // IN:  help message
    BOOL        bExist;                                     // OUT: the param exist in the commandline?
    BOOL        bHasParam;                                  // IN:  does it take one extra parameter?
    WCHAR       wszParam[MAX_PATH];                         // OUT: the extra parameter, NULL if bHasParam == FALSE
} CmdOptions, *PCmdOptions;

extern BOOL     CmdOptions_IgnoreUnknown;                   // ignore unknown commands? if FALSE: return E_INVALIDARG
extern BOOL     CmdOptions_HasWildChar;                     // TRUE if the parameter contains '?' or '*'
extern WCHAR    CmdOptions_ExtraParam[MAX_PATH];            // store extra parameter


#ifndef ARRAY_SIZE
    #define ARRAY_SIZE( __x )          sizeof(__x) / sizeof(__x[0])
#endif


// *--------------------------------------------------------------------------------------------------------------
extern VOID WINAPI getopt_long_Help(
                            __in                        INT         nCmd,
                            __deref_inout_ecount(nCmd)  PCmdOptions pCmd );

// *--------------------------------------------------------------------------------------------------------------
// For short parameters, support fall-through, e.g. -12345678, provided these parameters do NOT take extra parameters.

extern HRESULT WINAPI getopt_long_win(
                            __in                        INT         argc,
                            __deref_in                  LPCWSTR    *argv,
                            __in                        INT         nCmd,
                            __deref_inout_ecount(nCmd)  PCmdOptions pCmd );

