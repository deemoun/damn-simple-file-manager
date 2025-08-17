using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DamnSimpleFileManager.Utils
{
    public static class ShellContextMenu
    {
        private const uint CMF_NORMAL = 0x00000000;
        private const uint TPM_LEFTALIGN = 0x0000;
        private const uint TPM_RETURNCMD = 0x0100;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern IntPtr CreatePopupMenu();

        [DllImport("user32.dll")]
        private static extern bool DestroyMenu(IntPtr hMenu);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern uint TrackPopupMenuEx(IntPtr hmenu, uint uFlags, int x, int y, IntPtr hwnd, IntPtr lptpm);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHParseDisplayName(
            string name,
            IntPtr pbc,
            out IntPtr ppidl,
            uint sfgaoIn,
            out uint psfgaoOut);

        [DllImport("shell32.dll")]
        private static extern int SHBindToParent(IntPtr pidl, ref Guid riid, out IntPtr ppv, out IntPtr ppidlLast);

        [DllImport("ole32.dll")]
        private static extern void CoTaskMemFree(IntPtr pv);

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214e6-0000-0000-c000-000000000046")]
        private interface IShellFolder
        {
            void ParseDisplayName(IntPtr hwnd, IntPtr pbc, [MarshalAs(UnmanagedType.LPWStr)] string pszDisplayName, uint pchEaten, out IntPtr ppidl, ref uint pdwAttributes);
            void EnumObjects(IntPtr hwnd, int grfFlags, out IntPtr ppenumIDList);
            void BindToObject(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);
            void BindToStorage(IntPtr pidl, IntPtr pbc, ref Guid riid, out IntPtr ppv);
            [PreserveSig] int CompareIDs(int lParam, IntPtr pidl1, IntPtr pidl2);
            void CreateViewObject(IntPtr hwndOwner, ref Guid riid, out IntPtr ppv);
            void GetAttributesOf(uint cidl, IntPtr apidl, ref uint rgfInOut);
            void GetUIObjectOf(IntPtr hwndOwner, uint cidl, IntPtr apidl, ref Guid riid, IntPtr rgfReserved, out IntPtr ppv);
            void GetDisplayNameOf(IntPtr pidl, uint uFlags, out IntPtr pName);
            void SetNameOf(IntPtr hwnd, IntPtr pidl, [MarshalAs(UnmanagedType.LPWStr)] string pszName, uint uFlags, out IntPtr ppidlOut);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("000214e4-0000-0000-c000-000000000046")]
        private interface IContextMenu
        {
            [PreserveSig]
            int QueryContextMenu(IntPtr hmenu, uint indexMenu, uint idCmdFirst, uint idCmdLast, uint uFlags);
            void InvokeCommand(ref CMINVOKECOMMANDINFOEX pici);
            void GetCommandString(uint idcmd, uint uflags, IntPtr reserved, IntPtr commandString, int cch);
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct CMINVOKECOMMANDINFOEX
        {
            public int cbSize;
            public int fMask;
            public IntPtr hwnd;
            public IntPtr lpVerb;
            public IntPtr lpParameters;
            public IntPtr lpDirectory;
            public IntPtr lpTitle;
            public int nShow;
            public int dwHotKey;
            public IntPtr hIcon;
            public IntPtr lpTitleW;
            public IntPtr lpVerbW;
            public IntPtr lpParametersW;
            public IntPtr lpDirectoryW;
            public IntPtr lpTitleW2;
            public POINT ptInvoke;
        }

        private static readonly Guid IID_IShellFolder = new("000214E6-0000-0000-C000-000000000046");
        private static readonly Guid IID_IContextMenu = new("000214E4-0000-0000-C000-000000000046");

        public static void ShowForPaths(string[] paths, Window owner)
        {
            if (paths.Length == 0)
                return;

            GetCursorPos(out POINT pt);
            IntPtr hwnd = new WindowInteropHelper(owner).Handle;

            IntPtr[] pidls = new IntPtr[paths.Length];
            for (int i = 0; i < paths.Length; i++)
            {
                SHParseDisplayName(paths[i], IntPtr.Zero, out pidls[i], 0, out _);
            }

            SHBindToParent(pidls[0], ref IID_IShellFolder, out IntPtr parentPtr, out IntPtr firstItem);
            IShellFolder parent = (IShellFolder)Marshal.GetObjectForIUnknown(parentPtr);

            IntPtr apidl = Marshal.AllocCoTaskMem(IntPtr.Size * paths.Length);
            Marshal.WriteIntPtr(apidl, 0, firstItem);
            for (int i = 1; i < paths.Length; i++)
            {
                SHBindToParent(pidls[i], ref IID_IShellFolder, out _, out IntPtr rel);
                Marshal.WriteIntPtr(apidl, i * IntPtr.Size, rel);
            }

            parent.GetUIObjectOf(hwnd, (uint)paths.Length, apidl, ref IID_IContextMenu, IntPtr.Zero, out IntPtr menuPtr);
            IContextMenu menu = (IContextMenu)Marshal.GetObjectForIUnknown(menuPtr);

            IntPtr hMenu = CreatePopupMenu();
            menu.QueryContextMenu(hMenu, 0, 1, 0x7FFF, CMF_NORMAL);

            uint cmd = TrackPopupMenuEx(hMenu, TPM_LEFTALIGN | TPM_RETURNCMD, pt.X, pt.Y, hwnd, IntPtr.Zero);
            if (cmd != 0)
            {
                CMINVOKECOMMANDINFOEX invoke = new()
                {
                    cbSize = Marshal.SizeOf<CMINVOKECOMMANDINFOEX>(),
                    hwnd = hwnd,
                    lpVerb = (IntPtr)(cmd - 1),
                    nShow = 1,
                    ptInvoke = pt
                };
                menu.InvokeCommand(ref invoke);
            }

            DestroyMenu(hMenu);

            Marshal.ReleaseComObject(menu);
            Marshal.ReleaseComObject(parent);
            Marshal.FreeCoTaskMem(apidl);
            foreach (var pidl in pidls)
            {
                CoTaskMemFree(pidl);
            }
        }
    }
}

