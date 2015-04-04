/*++
Copyright (C) 2007 - 1015 Hong Liu
Check out https://github.com/hong6914/Public/ for updates.

    THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
    KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
    IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
    PURPOSE.

--*/

// undocumented native NT functions, mostly exported by NTDLL.DLL

// Please install Platform SDK first

#pragma once

#include <windows.h>
#include <Winternl.h>


typedef enum _FS_INFORMATION_CLASS {
    FileFsVolumeInformation = 1,
    FileFsLabelInformation,
    FileFsSizeInformation,
    FileFsDeviceInformation,
    FileFsAttributeInformation,
    FileFsControlInformation,
    FileFsFullSizeInformation,
    FileFsObjectIdInformation,
    FileFsMaximumInformation
} FS_INFORMATION_CLASS, * PFS_INFORMATION_CLASS;


//
// Define the various device characteristics flags
//

typedef enum _FS_DEVICE_CHARACTERISTICS {
    FS_FILE_REMOVABLE_MEDIA          = 0x00000001,
    FS_FILE_READ_ONLY_DEVICE         = 0x00000002,
    FS_FILE_FLOPPY_DISKETTE          = 0x00000004,
    FS_FILE_WRITE_ONCE_MEDIA         = 0x00000008,
    FS_FILE_REMOTE_DEVICE            = 0x00000010,
    FS_FILE_DEVICE_IS_MOUNTED        = 0x00000020,
    FS_FILE_VIRTUAL_VOLUME           = 0x00000040
} FS_DEVICE_CHARACTERISTICS, * PFS_DEVICE_CHARACTERISTICS;

ULONG FS_DEVICE_CHARACTERISTICS_VALUE [7] = {
    FS_FILE_REMOVABLE_MEDIA,
    FS_FILE_READ_ONLY_DEVICE,
    FS_FILE_FLOPPY_DISKETTE,
    FS_FILE_WRITE_ONCE_MEDIA,
    FS_FILE_REMOTE_DEVICE,
    FS_FILE_DEVICE_IS_MOUNTED,
    FS_FILE_VIRTUAL_VOLUME
};

LPCWSTR FS_DEVICE_CHARACTERISTICS_STR[7] = {
/* 0 */ L"Removable Media",
/* 1 */ L"Read-only Device",
/* 2 */ L"Floppy Diskette",
/* 3 */ L"Write-once Media",
/* 4 */ L"Remote Device",
/* 5 */ L"Mounted Device",
/* 6 */ L"Virtual Volume"
};

typedef struct _FILE_FS_DEVICE_INFORMATION {
    ULONG DeviceType;
    ULONG Characteristics;
} FILE_FS_DEVICE_INFORMATION, * PFILE_FS_DEVICE_INFORMATION;

#define STATUS_SUCCESS                          ((NTSTATUS) 0x00000000L)



// *--------------------------------------------------------------------------------------------------------------
typedef NTSTATUS (WINAPI * FuncNtQueryVolumeInformationFile ) (
                                                            __in            HANDLE                      FileHandle,
                                                            __deref_out     PIO_STATUS_BLOCK            IoStatusBlock,
                                                            __deref_out     PVOID                       FileSystemInformation,
                                                            __in            ULONG                       Length,
                                                            __in            FS_INFORMATION_CLASS        FileSystemInformationClass );