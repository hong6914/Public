


Get/Set File and Folder Datetimes
Written By Hong Liu
Check out https://github.com/hong6914/Public for updates.



Description
===========

This software is a tiny tool that could both get and set the timestamps of files and
folders on NTFS dile system. It can work through files and folders recursively.

Run the software without any parameters from command line to get the full help on the
usage.

Usage:  <this EXE> [options]

All parameters are NOT case-sensitive.

Options:

short  long             Note
=====  ====             ====
/c -c  --RLastCreate    Read last create time of the file.
/w -w  --RLastWrite     Read last write time of the file.
/a -a  --RLastAccess    Read last access time of the file.
/1 -1  --WLastCreate    Write last create time of the file.
/2 -2  --WLastWrite     Write last write time of the file.
/3 -3  --WLastAccess    Write last access time of the file.
/r -r  --recursive      Recursively surf the sub-folders.
/f -f  --folder         Change root folder's Date/Time as well.
/s -s  --SubFolder      Change all the sub-folders' Date/Time as well.
/v -v  --Verify         Verify the changes, redo it up to 3 times if failed.


Example:

<this EXE>    a.dll /a                  Display last access time of a.dll.
<this EXE>    a.dll                     Display all three times of a.dll.
<this EXE>    a.dll /1 "2011/1/2 3:45:56" Modify creation time of a.dll to
                                            2011/1/2 3:45:56.
<this EXE>    c:\Windows\*.txt /a       Display last access time of all the txt files.
<this EXE>    c:\Windows\*.txt /a -r    Display last access time of all the txt files,
                                            recursively.
<this EXE>    c:\Windows\*.txt /ar      Save As Above.
<this EXE>    c:\Windows\*.txt /r /3 "2011/1/2 3:45:56"
                                        Modify last access time of ALL .txt files
                                        (recursively) to 2011/1/2 3:45:56.
<this EXE> -s c:\Windows\*.txt /r /3 "2011/1/2 3:45:56"
                                        Save As Above. Also modify the sub-folders'
                                            Date/Time to 2011/1/2 3:45:56.
<this EXE> -fs c:\Windows\*.txt /r /3 "2011/1/2 3:45:56"
                                        Save As Above. Also modify all sub-folders
                                        as well as c:\Windows to 2011/1/2 3:45:56.
<this EXE> /fsr c:\Windows\*.txt /3 "2011/1/2 3:45:56"
                                        Save As Above.
<this EXE> /v /fsr c:\Windows\*.txt /3 "2011/1/2 3:45:56"
                                        Save As Above. Also verify the changes for up to 3 times.



System Requirement
==================

This software works on Windows XP and above, that is Windows XP, Server 2003,
Windows Vista, Windows Server 2008, Windows 7, Windows 8 and Server 2012.

Both 32-bit and 64-bit systems are supported.



License
=======

This software is released as freeware. You are allowed to freely distribute this
software via floppy disk, CD-ROM, Internet, or in any other way, as long as you
don't charge anything for this. If you distribute this software, you must include
all files in the distribution package, without any modification.



Disclaimer
==========

The software is provided "AS IS" without any warranty, either expressed
or implied, including, but not limited to, the implied warranties of
merchantability and fitness for a particular purpose. The author will not
be liable for any special, incidental, consequential or indirect damages
due to loss of data or any other reason.


