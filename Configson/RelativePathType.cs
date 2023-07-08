using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Configson
{
    public enum RelativePathType
    {
        /// <summary>
        /// Paths are relative to the process working directory.
        /// </summary>
        RelativeToCurrentWorkingDirectory = 0,
        /// <summary>
        /// Paths are relative to the app's directory (typically the directory where the main executable is stored)
        /// </summary>
        RelativeToAppBaseDirectory,
        /// <summary>
        /// Paths are relative to the system's %appdata% (XDG_CONFIG_HOME) folder. On Windows, that's usually ~/AppData/Roaming. On Linux, usually ~/.config.
        /// </summary>
        RelativeToAppData
    }
}