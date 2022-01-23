using MelonLoader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace UnoSingleConsole
{
    public class SingleConsole : MelonMod
    {

        public override void OnApplicationStart()
        {
            LoggerInstance.Msg(ParentProcessUtilities.GetParentProcess().ProcessName + " started me!");
        }

        public override void OnPreferencesSaved()
        {
            var modParentProc = ParentProcessUtilities.GetParentProcess().ProcessName.ToLower();
            if (modParentProc != "ubisoftgamelauncher")
            {
                LoggerInstance.Msg("Seemes like UbisoftGameLauncher didn't launch me, time to commit dos");

                var processes = Process.GetProcesses();
                foreach(Process proc in processes)
                {
                    var parentProc = ParentProcessUtilities.GetParentProcess(proc.Handle);

                    if (parentProc?.Id == Process.GetCurrentProcess().Id && proc.ProcessName == "conhost")
                    {
                        proc.Kill();
                    }
                }
                Process.GetCurrentProcess().Kill();
            }
        }

    }

    /// <summary>
    /// A utility class to determine a process parent.
    /// 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ParentProcessUtilities
    {
        // These members must match PROCESS_BASIC_INFORMATION
        internal IntPtr Reserved1;
        internal IntPtr PebBaseAddress;
        internal IntPtr Reserved2_0;
        internal IntPtr Reserved2_1;
        internal IntPtr UniqueProcessId;
        internal IntPtr InheritedFromUniqueProcessId;

        [DllImport("ntdll.dll")]
        private static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ParentProcessUtilities processInformation, int processInformationLength, out int returnLength);

        /// <summary>
        /// Gets the parent process of the current process.
        /// </summary>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess()
        {
            return GetParentProcess(Process.GetCurrentProcess().Handle);
        }

        /// <summary>
        /// Gets the parent process of specified process.
        /// </summary>
        /// <param name="id">The process id.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(int id)
        {
            Process process = Process.GetProcessById(id);
            return GetParentProcess(process.Handle);
        }

        /// <summary>
        /// Gets the parent process of a specified process.
        /// </summary>
        /// <param name="handle">The process handle.</param>
        /// <returns>An instance of the Process class.</returns>
        public static Process GetParentProcess(IntPtr handle)
        {
            ParentProcessUtilities pbi = new ParentProcessUtilities();
            int returnLength;
            int status = NtQueryInformationProcess(handle, 0, ref pbi, Marshal.SizeOf(pbi), out returnLength);
            if (status != 0)
                throw new Win32Exception(status);

            try
            {
                return Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
            }
            catch (ArgumentException)
            {
                // not found
                return null;
            }
        }
    }
}
