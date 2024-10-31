using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.ldtk_gamemap.Editor.Settings
{
    public class LdtkGamemapSettings : ScriptableObject
    {
        private const string ResourcesFolder = "Settings";
        private const string FileName = nameof(LdtkGamemapSettings);
        private static readonly string ResourceLoadPath = $"{ResourcesFolder}/{FileName}";

        public static readonly string SettingsPath = $"Assets/Resources/{ResourcesFolder}";
        public static readonly string LdtkGamemapSettingsPath = $"{SettingsPath}/{FileName}.asset";

        [SerializeField]
        private string _ldtkRelativeProjectPath;
        [SerializeField]
        private string _gamemapLayer = "Pathway";
        [SerializeField]
        private float _mapScale = 1.0f;
        [SerializeField]
        private Vector2 _mapOffset = Vector2.zero;

        public string LdtkRelativeProjectPath => _ldtkRelativeProjectPath;
        public string LdtkProjectPath => Path.Combine(Application.dataPath, LdtkRelativeProjectPath);
        public string GamemapLayer => _gamemapLayer;
        public float MapScale => _mapScale;
        public Vector2 MapOffset => _mapOffset;

        public static string[] SettingFields => new string[]
        {
            nameof(_ldtkRelativeProjectPath),
            nameof(_gamemapLayer),
            nameof(_mapScale),
            nameof(_mapOffset),
        };

        public static LdtkGamemapSettings Load()
        {
            return Resources.Load<LdtkGamemapSettings>(ResourceLoadPath);
        }
    }
}
