using System;

namespace MemTools
{
    public class Globals
    {
        private static int ProcessID_ = -1;
        public static int ProcessID
        {
            get { return ProcessID_; }
            set { ProcessID_ = value; }
        }

        private static IntPtr ProcessHandle_ = IntPtr.Zero;
        public static IntPtr ProcessHandle
        {
            get { return ProcessHandle_; }
            set { ProcessHandle_ = value; }
        }
    }
}
