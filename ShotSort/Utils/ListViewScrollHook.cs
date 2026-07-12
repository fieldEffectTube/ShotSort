using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShotSort.Utils
{
    public class ListViewScrollHook : NativeWindow
    {
        public event EventHandler? Scrolled;

        private const int WM_VSCROLL = 0x115;
        private const int WM_HSCROLL = 0x114;
        private const int WM_MOUSEWHEEL = 0x020A;

        public ListViewScrollHook(ListView listView)
        {
            AssignHandle(listView.Handle);
            listView.HandleCreated += (s, e) => AssignHandle(listView.Handle);
            listView.HandleDestroyed += (s, e) => ReleaseHandle();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_VSCROLL || m.Msg == WM_HSCROLL || m.Msg == WM_MOUSEWHEEL)
            {
                Scrolled?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
