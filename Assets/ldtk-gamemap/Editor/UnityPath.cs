using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ldtk_gamemap.Editor
{
    public static class UnityPath
    {
        public static string Combine(params string[] paths)
        {
            var path = Path.Combine(paths);
            return path.Replace("\\", "/");
        }
    }
}
