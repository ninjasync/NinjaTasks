using System;
using System.IO;

namespace NinjaTools.GUI.Wpf
{
    public static class AppPath
    {
        private const string PortablePath = "portable-data";

        public static string GetDataPath(string appname, string publisher = "Ninja")
        {
            string dir;

            if (Directory.Exists(PortablePath))
                dir = "portable-data";
            else
            {
                dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), publisher,
                    appname);
                Directory.CreateDirectory(dir);
            }

            return Path.GetFullPath(dir);
        }
    }
}
