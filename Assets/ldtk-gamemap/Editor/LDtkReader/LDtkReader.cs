using ldtk;
using ldtk_simplified;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ldtk_gamemap.Editor.LDtkReader
{
    public class LDtkReader
    {
        private string _projectfilePath;

        public LdtkJson LdtkJson { get; }

        public LDtkReader(string path)
        {
            _projectfilePath = path;

            var project = File.ReadAllText(_projectfilePath);
            LdtkJson = LdtkJson.FromJson(project);
        }

        public SimplifiedData GetSimplifiedDataForLevel(Level level)
        {
            var path = DataJsonPath(level);
            var content = File.ReadAllText(path);
            return SimplifiedData.FromJson(content);
        }

        public string DataJsonPath(Level level)
        {
            var parentDirPath = Path.GetDirectoryName(_projectfilePath);
            var contentDir = Path.GetFileNameWithoutExtension(_projectfilePath);
            return UnityPath.Combine(parentDirPath, contentDir, "simplified", level.Identifier, "data.json");
        }

        public string PathwayImagePath(Level level)
        {
            var parentDirPath = Path.GetDirectoryName(_projectfilePath);
            var contentDir = Path.GetFileNameWithoutExtension(_projectfilePath);
            return UnityPath.Combine(parentDirPath, contentDir, "simplified", level.Identifier, "Pathway.png");
        }
    }
}
