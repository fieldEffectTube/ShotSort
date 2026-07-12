using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace ShotSort.Properties
{
    internal static class Resources
    {
        private static Icon? _appIcon;

        internal static Icon AppIcon
        {
            get
            {
                if (_appIcon == null)
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    var stream = assembly.GetManifestResourceStream("ShotSort.app.ico");
                    if (stream != null)
                        _appIcon = new Icon(stream);
                    else
                        _appIcon = SystemIcons.Application;
                }
                return _appIcon;
            }
        }
    }
}
