namespace ShotSort.Properties
{
    internal static class Resources
    {
        private static System.Resources.ResourceManager? _manager;

        internal static System.Resources.ResourceManager ResourceManager
        {
            get
            {
                if (_manager == null)
                    _manager = new System.Resources.ResourceManager("ShotSort.Properties.Resources", typeof(Resources).Assembly);
                return _manager;
            }
        }

        internal static System.Drawing.Icon AppIcon
        {
            get => (System.Drawing.Icon?)ResourceManager.GetObject("AppIcon") ?? System.Drawing.SystemIcons.Application;
        }
    }
}
