// If you wanna reference the class here, comment the #define statement and make it un-effective.
//
// Written By Hong Liu
// Check out https://github.com/hong6914/Public for updates.
//
// Create dump of the process for further analysis in windbg or other debugging tools.
//
//    MiniDumpType.Normal = nimi dump
//
//    MiniDumpType.WithFullMemory = full memory dump
//


#define Demo


using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MiniDumps
{
    public enum MiniDumpType
    {
        Normal = 0x00000000,
        WithDataSegs = 0x00000001,
        WithFullMemory = 0x00000002,
        WithHandleData = 0x00000004,
        FilterMemory = 0x00000008,
        ScanMemory = 0x00000010,
        WithUnloadedModules = 0x00000020,
        WithIndirectlyReferencedMemory = 0x00000040,
        FilterModulePaths = 0x00000080,
        WithProcessThreadData = 0x00000100,
        WithPrivateReadWriteMemory = 0x00000200,
        WithoutOptionalData = 0x00000400,
        WithFullMemoryInfo = 0x00000800,
        WithThreadInfo = 0x00001000,
        WithCodeSegs = 0x00002000,
        WithoutAuxiliaryState = 0x00004000,
        WithFullAuxiliaryState = 0x00008000,
        CustomDump = 0x7FFFffff                         // this is NOT defined by Windows. We add this one just for easier processing
    }


    public static class MiniDump
    {
        [DllImport("DbgHelp.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
        private static extern Boolean MiniDumpWriteDump
                                        (   IntPtr hProcess,
                                            Int32 processId,
                                            IntPtr fileHandle,
                                            MiniDumpType dumpType,
                                            IntPtr excepInfo,
                                            IntPtr userInfo,
                                            IntPtr extInfo);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [StructLayout(LayoutKind.Sequential)]
        [Serializable]
        struct MinidumpExceptionInfo
        {
            public Int32 ThreadId;
            public IntPtr ExceptionPointers;
            public string MachineName;
            public string ExecutablePath;
            public string ExecutableName;
            public string[] Modules;
            public string MoreInfo;
        }

        private struct CreateDumpArgs
        {
            public string DumpPath;
            public MiniDumpType DumpType;
            public string fileName;
        }


        private static int mDumpError;
        private static CreateDumpArgs mArgs;
        private static MinidumpExceptionInfo mMei;


        /// <summary>
        /// Outpout debug message to Windows' debug port.
        /// It calls Kernel32!OutputDebugString() at last, so make sure you use SysInternals!DbgView.exe to check the messages.
        /// </summary>
        /// <param name="string sMessage"></param>
        /// <returns>void</returns>

        public static void OutputDebugMsg(string sMessage)
        {
            StackTrace st = new StackTrace(false);
            string sCaller = st.GetFrame(1).GetMethod().Name;
            Debug.WriteLine(string.Format("{0}: {1}", sCaller, sMessage));
        }


        /// <summary>
        /// Create one thread that generates dumps for current process:
        ///     One is the standard minidump, another is the custom dump message.
        /// </summary>
        /// <param name="String dumpPath">path to store the dumps.</param>
        /// <param name="MiniDumpType dumpType">refer to enum MiniDumpType</param>
        /// <returns>bool</returns>
        
        public static bool TryDumpMe(String dumpPath, MiniDumpType dumpType)
        {
            if (!Directory.Exists(dumpPath))
            {
                OutputDebugMsg(string.Format("{0} does NOT exist", dumpPath));
                return false;
            }
            
            Process me = Process.GetCurrentProcess();
            DateTime dt = DateTime.Now;

            mArgs.DumpPath = dumpPath;
            mArgs.DumpType = dumpType;
            mMei.MachineName = Environment.GetEnvironmentVariable("COMPUTERNAME");
            mArgs.fileName = string.Format("{0}_{1}_{2}_{3}{4}{5}_{6}_{7}_{8}",
                                Environment.GetEnvironmentVariable("COMPUTERNAME"),
                                mMei.MachineName,
                                me.MainModule.ModuleName,
                                dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second);
            
            mMei.ThreadId = GetCurrentThreadId();
            mMei.ExceptionPointers = Marshal.GetExceptionPointers();
            mMei.ExecutablePath = me.MainModule.FileName;  // AppDomain.CurrentDomain.ToString();  //BUGBUG
            mMei.ExecutableName = me.MainModule.ModuleName;
            mMei.Modules = new string[me.Modules.Count];
            for (int i = 0; i < me.Modules.Count; i++)
            {
                mMei.Modules[i] = (string) me.Modules[i].FileName.Clone();
            }

            mMei.MoreInfo = string.Format("Handle Count = {0}\r\nMinWorkingSet = 0x{1:X8}\r\nMinWorkingSet = 0x{2:X8}",
                    me.HandleCount, me.MinWorkingSet, me.MaxWorkingSet);
            mMei.MoreInfo = string.Format("{0}\r\nPrivateMemorySize = 0x{1:X16}\r\nVirtualMemorySize = 0x{2:X16}",
                    mMei.MoreInfo, me.PrivateMemorySize64, me.VirtualMemorySize64);
            mMei.MoreInfo = string.Format("{0}\r\nStart Args = {1}\r\nStart Time = {2}",
                    mMei.MoreInfo, me.StartInfo.Arguments, me.StartTime.ToLocalTime().ToString());
            Thread t = new Thread(new ThreadStart(CreateDump));
            t.Start();
            t.Join();
            return mDumpError == 0;
        }


        /// <summary>
        /// Call DbgHelp.dll to generate dumps for current process.
        /// 
        /// Make sure the version of DbgHelp.dll > v6.0. v6 contains a bug that when calling MiniDumpWriteDump() on
        ///     current process, one deadlock will be triggered to hang the process.
        /// 
        ///  Better install windbg first to overwrite the one shipped with the OS.
        /// </summary>
        /// <returns>void</returns>
    
        private static void CreateDump()
        {
            using (TextWriter tw = new StreamWriter(string.Format("{0}\\{1}.txt", mArgs.DumpPath, mArgs.fileName)))
            {
                tw.WriteLine(string.Format("ThreadID = {0}\r\n", mMei.ThreadId));
                tw.WriteLine(string.Format("ExecutablePath = {0}\r\n", mMei.ExecutablePath));
                tw.WriteLine(string.Format("ExecutableName = {0}\r\n", mMei.ExecutableName));
                tw.WriteLine(string.Format("MachineName = {0}\r\n", mMei.MachineName));
                tw.WriteLine("Modules\r\n");
                for (int i = 0; i < mMei.Modules.Length; i++ )
                    tw.WriteLine(string.Format("\t{0}", mMei.Modules[i]));

                tw.WriteLine("\r\nMore Info\r\n");
                tw.WriteLine(mMei.MoreInfo);
            }

            Process process = Process.GetCurrentProcess();
            IntPtr myCustomDump = Marshal.AllocHGlobal(Marshal.SizeOf(mMei));
            Marshal.StructureToPtr(mMei, myCustomDump, false);
            Boolean res = false;

            using (FileStream stream = new FileStream(string.Format("{0}\\{1}.dmp", mArgs.DumpPath, mArgs.fileName), FileMode.Create))
            {
                res = MiniDumpWriteDump(
                                    process.Handle,
                                    process.Id,
                                    stream.SafeFileHandle.DangerousGetHandle(),
                                    MiniDumpType.Normal,
                                    IntPtr.Zero,  // mini dump
                                    IntPtr.Zero,
                                    IntPtr.Zero);
            }

            mDumpError = res ? 0 : Marshal.GetLastWin32Error();
            Marshal.FreeHGlobal(myCustomDump);
        }
    }


#if Demo

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                int i = 0;
                int j = 1 / i;
            }
            catch (Exception)
            {
                MiniDump.TryDumpMe("C:\\Temp", MiniDumpType.Normal);
            }
        }
    }
#endif


}
