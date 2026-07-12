using System.Windows.Forms;

namespace ShotSort.Core
{
    public static class HotkeyManager
    {
        public enum HotkeyAction
        {
            None,
            Previous,
            Next,
            MarkSelected,
            MarkKept,
            CtrlAction,
            RotateClockwise
        }

        public static HotkeyAction ParseKey(Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Left:
                    return HotkeyAction.Previous;
                case Keys.Right:
                    return HotkeyAction.Next;
                case Keys.Up:
                    return HotkeyAction.MarkSelected;
                case Keys.Down:
                    return HotkeyAction.MarkKept;
                // Ctrl 单独按下 / Space / Delete 均触发 CtrlAction
                case Keys.Control:
                case Keys.ControlKey:
                case Keys.Space:
                case Keys.Delete:
                    return HotkeyAction.CtrlAction;
                case Keys.PageDown:
                    return HotkeyAction.RotateClockwise;
                default:
                    return HotkeyAction.None;
            }
        }
    }
}
