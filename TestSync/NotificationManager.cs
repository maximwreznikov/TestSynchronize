using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TestSync
{
    /// <summary>
    /// Class for send message SYNC_MESSAGE for all apllication instances
    /// </summary>
    class NotificationManager
    {
        public event Action SyncState;

        private const int SYNC_MESSAGE = 0xA123;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SendMessage(IntPtr hwnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            // filter the SYNC_MESSAGE
            if (msg == SYNC_MESSAGE)
            {
                if(SyncState != null)
                    SyncState();
            }

            return IntPtr.Zero;
        }

        public void UpdateAll()
        {
            //get this running process
            Process proc = Process.GetCurrentProcess();
            //get all other (possible) running instances
            Process[] processes = Process.GetProcessesByName(proc.ProcessName);

            if (processes.Length > 1)
            {
                //iterate through all running target applications
                foreach (Process p in processes)
                {
                    if (p.Id != proc.Id)
                    {
                        //now send the SYNC_MESSAGE to the running instance
                        SendMessage(p.MainWindowHandle, SYNC_MESSAGE, IntPtr.Zero, IntPtr.Zero);
                    }
                }
            }
        }

        public static void CloseAllInstances()
        {
            //get this running process
            Process proc = Process.GetCurrentProcess();
            //get all other (possible) running instances
            Process[] processes = Process.GetProcessesByName(proc.ProcessName);


            //iterate through all running target applications
            foreach (Process p in processes)
            {
                p.Kill();
            }
        }
    }
}
