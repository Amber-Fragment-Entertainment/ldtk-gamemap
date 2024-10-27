using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace Assets.ldtk_gamemap.Editor.Settings
{
    static class LdtkGamemapSettingsRegister
    {
        public static readonly string NoProjectFileSelectedLabel = "<No project file selected>";
        public static readonly string SettingsPath = "Project/LdtkGamemap Settings";

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var fields = LdtkGamemapSettings.SettingFields;

            var provider = new SettingsProvider(SettingsPath, SettingsScope.Project)
            {
                guiHandler = (searchContext) =>
                {
                    var serializedSettings = GetSerializedSettings();
                    var settings = GetOrCreateSettings();

                    var ldtkProjectPath = serializedSettings.FindProperty("_ldtkRelativeProjectPath");
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(string.IsNullOrEmpty(ldtkProjectPath.stringValue) ? NoProjectFileSelectedLabel : $"Project relative path: {ldtkProjectPath.stringValue}");
                    if (GUILayout.Button("Pick ldtk project file"))
                    {
                        var path = EditorUtility.OpenFilePanelWithFilters("LDtk file Location", "", new string[] { "LDtk project", "ldtk" });
                        if (path != null)
                        {
                            var relativePath = GetRelativePath(path, Application.dataPath);
                            ldtkProjectPath.stringValue = relativePath;
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (!File.Exists(settings.LdtkProjectPath))
                    {
                        EditorGUILayout.HelpBox("Relative path doesn't point to any file", MessageType.Error);
                    }

                    EditorGUILayout.PropertyField(serializedSettings.FindProperty("_gamemapLayer"));

                    serializedSettings.ApplyModifiedPropertiesWithoutUndo();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(fields.Select(ObjectNames.NicifyVariableName))
            };

            return provider;
        }

        internal static LdtkGamemapSettings GetOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<LdtkGamemapSettings>(LdtkGamemapSettings.LdtkGamemapSettingsPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<LdtkGamemapSettings>();
                CreateDirectories(LdtkGamemapSettings.SettingsPath);
                AssetDatabase.CreateAsset(settings, LdtkGamemapSettings.LdtkGamemapSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }

        private static void CreateDirectories(string path)
        {
            var entries = path.Split('/');

            if (entries.Length == 0)
            {
                return;
            }

            var parentPath = entries[0];

            for (var idx = 1; idx < entries.Length; idx++)
            {
                var entry = entries[idx];
                var dir = $"{parentPath}/{entry}";

                if (!AssetDatabase.IsValidFolder(dir))
                {
                    AssetDatabase.CreateFolder(parentPath, entry);
                }

                parentPath = dir;
            }
        }

        private static string GetRelativePath(string filespec, string folder)
        {
            var pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            var folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
