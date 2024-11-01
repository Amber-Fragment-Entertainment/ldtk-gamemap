using Assets.ldtk_gamemap.Editor.Settings;
using ldtk;
using ldtk_simplified;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Assets.ldtk_gamemap.Editor
{
    public class MenuItems
    {
        private const string MapDirName = "Map";
        private static readonly string AbsoluteMapDirPath = $"{Application.dataPath}/{MapDirName}";
        private const string TopLevelMapGameObjectName = "[MAP]";

        [MenuItem("GameObject/LDtk/Create game map")]
        private static void CreateGameMap()
        {
            var settings = LdtkGamemapSettings.Load();
            
            if(OpenProjectSettingsIfNotConfigured(settings))
            {
                return;
            }

            var reader = new LDtkReader.LDtkReader(settings.LdtkProjectPath);
            var levels = reader.LdtkJson.Levels;

            var simplifiedData = levels.Select(level => new System.Tuple<Level, SimplifiedData>(level, reader.GetSimplifiedDataForLevel(level)));
            var backgroundImagePaths = levels.Select(level => new System.Tuple<Level, string>(level, reader.PathwayImagePath(level)));

            EnsureMapDirExists();
            DestroyTopLevelMapGameObject();

            CopyBackgroundFiles(backgroundImagePaths, settings);
            AddSpritesToScene(simplifiedData, settings);
        }

        private static bool OpenProjectSettingsIfNotConfigured(LdtkGamemapSettings settings)
        {
            var ldtkProjectPath = settings == null ? null : settings.LdtkProjectPath;
            if (string.IsNullOrEmpty(ldtkProjectPath))
            {
                Debug.LogError("LDtk project path not configured.");
                SettingsService.OpenProjectSettings(LdtkGamemapSettingsRegister.SettingsPath);
                return true;
            }

            if (!File.Exists(settings.LdtkProjectPath))
            {
                Debug.LogError("LDtk project at relative path doesn't exist.");
                SettingsService.OpenProjectSettings(LdtkGamemapSettingsRegister.SettingsPath);
                return true;
            }
            return false;
        }

        private static void EnsureMapDirExists()
        {
            AssetDatabase.DeleteAsset(UnityPath.Combine("Assets", MapDirName));
            AssetDatabase.CreateFolder("Assets", MapDirName);
        }

        private static void DestroyTopLevelMapGameObject()
        {
            var go = GameObject.Find(TopLevelMapGameObjectName);
            GameObject.DestroyImmediate(go);
        }

        private static void CopyBackgroundFiles(IEnumerable<System.Tuple<Level, string>> backgroundImagePaths, LdtkGamemapSettings settings)
        {
            foreach (var backgroundImagePath in backgroundImagePaths)
            {
                var level = backgroundImagePath.Item1;
                var targetFileName = SpriteAssetFilename(level, settings);
                var targetFilePath = UnityPath.Combine(AbsoluteMapDirPath, targetFileName);
                FileUtil.CopyFileOrDirectory(backgroundImagePath.Item2, targetFilePath);
            }

            AssetDatabase.Refresh();

            foreach (var backgroundImagePath in backgroundImagePaths)
            {
                var level = backgroundImagePath.Item1;
                var assetTargetFilePath = SpriteAssetPath(level, settings);
                ConfigureSprite(assetTargetFilePath);
            }

            AssetDatabase.Refresh();
        }

        private static void ConfigureSprite(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);

            textureSettings.spriteMode = (int)SpriteDrawMode.Simple;
            textureSettings.textureType = TextureImporterType.Sprite;
            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            textureSettings.spriteAlignment = (int)SpriteAlignment.BottomCenter;

            importer.SetTextureSettings(textureSettings);
            importer.SaveAndReimport();
        }

        private static void AddSpritesToScene(IEnumerable<System.Tuple<Level, SimplifiedData>> simplifiedData, LdtkGamemapSettings settings)
        {
            var parent = new GameObject(TopLevelMapGameObjectName);

            foreach (var simplified in simplifiedData)
            {
                var level = simplified.Item1;
                var assetTargetFilePath = SpriteAssetPath(level, settings);
                AddSpriteToScene(simplified.Item2, assetTargetFilePath, parent, settings);
            }
        }

        private static void AddSpriteToScene(SimplifiedData simplifiedData, string spritePath, GameObject parent, LdtkGamemapSettings settings)
        {
            var UnityWorldCoord = new Vector2Int((int)simplifiedData.X, (int)simplifiedData.Y);
            var unityPos = LDtkCoordinatesConverter.LevelPosition(UnityWorldCoord, (int)simplifiedData.Height, settings.MapScale);

            var go = new GameObject(simplifiedData.Identifier, typeof(SpriteRenderer));
            go.transform.position = unityPos + settings.MapOffset;
            go.transform.parent = parent.transform;
            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            spriteRenderer.sprite = sprite;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(simplifiedData.Width * settings.MapScale, simplifiedData.Height * settings.MapScale);
        }

        private static string SpriteAssetFilename(Level level, LdtkGamemapSettings settings) => $"{level.Identifier}_{settings.GamemapLayer}.png";

        private static string SpriteAssetPath(Level level, LdtkGamemapSettings settings)
        {
            var targetFileName = SpriteAssetFilename(level, settings);
            var assetTargetFilePath = UnityPath.Combine("Assets", MapDirName, targetFileName);
            return assetTargetFilePath;
        }
    }
}
