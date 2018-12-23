using System;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MemTools
{
    class Memory
    {
        [DllImport("kernel32.dll", EntryPoint = "CloseHandle")]
        private static extern bool _CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", EntryPoint = "CreateRemoteThread")]
        private static extern IntPtr _CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, UIntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        private static extern IntPtr _FindWindow(string classname, string windowtitle);
        [DllImport("kernel32.dll", EntryPoint = "GetExitCodeThread")]
        private static extern bool _GetExitCodeThread(IntPtr hThread, out uint dwExitCode);
        [DllImport("kernel32.dll", EntryPoint = "GetModuleHandle")]
        private static extern IntPtr _GetModuleHandle(string lpModuleName);
        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        private static extern UIntPtr _GetProcAddress(IntPtr hModule, string procName);
        [DllImport("user32.dll", EntryPoint = "GetWindowText")]
        private static extern int _GetWindowText(IntPtr hWnd, StringBuilder buf, int nMaxCount);
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
        private static extern bool ReadProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, [Out] IntPtr lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll", EntryPoint = "ResetEvent")]
        private static extern bool _ResetEvent(IntPtr hEvent);
        [DllImport("kernel32.dll", EntryPoint = "SetEvent")]
        private static extern bool _SetEvent(IntPtr hEvent);
        [DllImport("kernel32.dll", EntryPoint = "TerminateThread")]
        private static extern bool _TerminateThread(IntPtr hThread, uint dwExitCode);
        [DllImport("kernel32.dll", EntryPoint = "WaitForSingleObject", SetLastError = true)]
        private static extern int _WaitForSingleObject(IntPtr hObject, uint milliseconds);
        [DllImport("kernel32.dll", EntryPoint = "VirtualAllocEx")]
        private static extern UIntPtr _VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll", EntryPoint = "VirtualFreeEx")]
        private static extern bool _VirtualFreeEx(IntPtr hProcess, UIntPtr lpAddress, uint dwSize, uint dwFreeType);

        [DllImport("kernel32.dll")]
        private static extern IntPtr CreateEvent(IntPtr lpSecurity, bool bManualReset, bool bInitialState, string lpEventName);
        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumProcessModules(int handle, IntPtr modules, int size, ref int needed);

        public static IntPtr FindWindow(string classname, string title)
        {
            return _FindWindow(classname, title);
        }
        public static IntPtr FindWindow(string either)
        {
            IntPtr hWnd = IntPtr.Zero;
            hWnd = _FindWindow(either, null);
            if (hWnd != IntPtr.Zero) return hWnd;
            hWnd = _FindWindow(null, either);
            if (hWnd != IntPtr.Zero) return hWnd;
            return IntPtr.Zero;
        }

        public static int[] GetProcessIdsByProcessName(string processName)
        {
            if (processName.IndexOf(".exe") != -1)
                processName = processName.Remove(processName.Length - 4);

            Process[] processesByName = Process.GetProcessesByName(processName);
            if (processesByName.Length == 0) return null;

            int[] numArray = new int[processesByName.Length];
            for (int i = 0; i < processesByName.Length; i++)
                numArray[i] = processesByName[i].Id;

            return numArray;
        }

        public static int GetProcessIdByProcessName(string processname)
        {
            int[] processIdsByProcessName = GetProcessIdsByProcessName(processname);
            if (processIdsByProcessName == null) return 0;
            return processIdsByProcessName[0];
        }

        public static IntPtr OpenProcess(int ProcessID)
        {
            if (ProcessID == 0) return IntPtr.Zero;
            return OpenProcess(0x1F0FFF, false, ProcessID);
        }

        public static bool ReadMemory(Int64 Address, ref byte[] buffer)
        {
            return ReadProcessMemory(Globals.ProcessHandle, (UIntPtr)Address, buffer, (UIntPtr)buffer.Length, IntPtr.Zero);
        }
        public static bool ReadMemory(Int64 Address, ref byte[] buffer, int size)
        {
            return ReadProcessMemory(Globals.ProcessHandle, (UIntPtr)Address, buffer, (UIntPtr)size, IntPtr.Zero);
        }
        public static bool ReadMemory(long Address, [Out] IntPtr lpBuffer, int size)
        {
            return ReadProcessMemory(Globals.ProcessHandle, (UIntPtr)Address, lpBuffer, (UIntPtr)size, IntPtr.Zero);
        }

        public static byte ReadByte(long Address)
        {
            byte[] buffer = new byte[1];
            if (ReadMemory(Address, ref buffer, 1))
                return buffer[0];
            return 0xff;
        }
        public static float ReadFloat(long Address)
        {
            byte[] buffer = new byte[4];
            if (ReadMemory(Address, ref buffer, 4))
                return BitConverter.ToSingle(buffer, 0);
            return float.MaxValue;
        }
        public static object ReadObject(long Address, Type type)
        {
            int cb = Marshal.SizeOf(type);
            IntPtr lpBuffer = Marshal.AllocHGlobal(cb);
            if (!ReadMemory(Address, lpBuffer, cb))
            {
                return null;
            }
            object obj2 = Marshal.PtrToStructure(lpBuffer, type);
            Marshal.FreeHGlobal(lpBuffer);
            return obj2;
        }
        public static string ReadString(long Address, int len)
        {
            byte[] buffer = new byte[len];
            if (ReadMemory(Address, ref buffer, len))
            {
                string str = Encoding.ASCII.GetString(buffer);
                if (str.IndexOf('\0') != -1)
                    return str.Remove(str.IndexOf('\0'));
                return str;
            }
            return string.Empty;
        }
        public static uint ReadUInt(long Address)
        {
            byte[] buffer = new byte[4];
            if (ReadMemory(Address, ref buffer, 4))
                return BitConverter.ToUInt32(buffer, 0);
            return uint.MaxValue;
        }
        public static UInt64 ReadUInt64(Int64 Address)
        {
            byte[] buffer = new byte[8];
            if (ReadMemory(Address, ref buffer, 8))
                return BitConverter.ToUInt64(buffer, 0);
            return UInt64.MaxValue;
        }

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(IntPtr hProcess, UIntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, out int lpNumberOfBytesWritten);
        public static bool WriteMemory(UIntPtr Address, byte[] buffer)
        {
            int written;
            return WriteProcessMemory(Globals.ProcessHandle, Address, buffer, (UIntPtr)buffer.Length, out written);
        }
        public static bool WriteMemory(UIntPtr Address, byte[] buffer, int size)
        {
            int written;
            return WriteProcessMemory(Globals.ProcessHandle, Address, buffer, (UIntPtr)size, out written);
        }
        public static bool WriteMemory(UIntPtr Address, byte[] buffer, int size, out int written)
        {
            return WriteProcessMemory(Globals.ProcessHandle, Address, buffer, (UIntPtr)size, out written);
        }
        public static bool WriteMemory(UIntPtr Address, byte value)
        {
            return WriteMemory(Address, BitConverter.GetBytes((short)value), 1);
        }
        public static bool WriteMemory(UIntPtr Address, double value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(value));
        }
        public static bool WriteMemory(UIntPtr Address, short value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(value), 2);
        }
        public static bool WriteMemory(UIntPtr Address, int value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(value), 4);
        }
        public static bool WriteMemory(UIntPtr Address, Int64 value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(value));
        }
        public static bool WriteMemory(UIntPtr Address, sbyte value)
        {
            return WriteMemory(Address, BitConverter.GetBytes((short)value), 1);
        }
        public static bool WriteMemory(UIntPtr Address, float value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(value));
        }
        public static bool WriteMemory(UIntPtr Address, string value)
        {
            return WriteMemory(Address, Encoding.UTF8.GetBytes(value));
        }
        public static bool WriteMemory(UIntPtr Address, ushort value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(value), 2);
        }
        public static bool WriteMemory(UIntPtr Address, uint value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(value), 4);
        }
        public static bool WriteMemory(UIntPtr Address, ulong value)
        {
            return WriteMemory(Address, BitConverter.GetBytes(value));
        }

        public static UIntPtr GetProcAddress(IntPtr modulehandle, string procname)
        {
            return _GetProcAddress(modulehandle, procname);
        }

        public static UIntPtr GetProcAddress(string modulename, string procname)
        {
            IntPtr modulehandle = _GetModuleHandle(modulename);
            if (modulehandle == IntPtr.Zero)
                return UIntPtr.Zero;

            return _GetProcAddress(modulehandle, procname);
        }

        public static UIntPtr VirtualAllocEx(IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect)
        {
            return _VirtualAllocEx(Globals.ProcessHandle, lpAddress, dwSize, flAllocationType, flProtect);
        }

        public static bool VirtualFreeEx(UIntPtr lpAddress, uint dwSize, uint dwFreeType)
        {
            return _VirtualFreeEx(Globals.ProcessHandle, lpAddress, dwSize, dwFreeType);
        }

        public static IntPtr CreateRemoteThread(IntPtr lpThreadAttributes, uint dwStackSize, UIntPtr lpStartAddress, UIntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId)
        {
            return _CreateRemoteThread(Globals.ProcessHandle, lpThreadAttributes, dwStackSize, lpStartAddress, lpParameter, dwCreationFlags, lpThreadId);
        }

        public static int WaitForSingleObject(IntPtr hObject, uint milliseconds)
        {
            return _WaitForSingleObject(hObject, milliseconds);
        }

        public static bool GetExitCodeThread(IntPtr hThread, out uint dwExitCode)
        {
            return _GetExitCodeThread(hThread, out dwExitCode);
        }

        public static bool CloseHandle(IntPtr hObject)
        {
            return _CloseHandle(hObject);
        }
    }
}
