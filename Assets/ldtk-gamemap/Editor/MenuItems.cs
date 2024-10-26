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

        [MenuItem("GameObject/LDtk/Create game map")]
        private static void CreateGameMap()
        {
            var reader = new LDtkReader.LDtkReader("C:\\Gamedev\\ldtk test\\worldmap.ldtk");
            var levels = reader.LdtkJson.Levels;

            var simplifiedData = levels.Select(level => new System.Tuple<Level, SimplifiedData>(level, reader.GetSimplifiedDataForLevel(level)));
            var backgroundImagePaths = levels.Select(level => new System.Tuple<Level, string>(level, reader.PathwayImagePath(level)));

            EnsureMapDirExists();

            CopyBackgroundFiles(backgroundImagePaths);
            AddSpritesToScene(simplifiedData);
        }

        private static void EnsureMapDirExists()
        {
            AssetDatabase.DeleteAsset(UnityPath.Combine("Assets", MapDirName));
            AssetDatabase.CreateFolder("Assets", MapDirName);
        }

        private static void CopyBackgroundFiles(IEnumerable<System.Tuple<Level, string>> backgroundImagePaths)
        {
            foreach (var backgroundImagePath in backgroundImagePaths)
            {
                var level = backgroundImagePath.Item1;
                var targetFileName = SpriteAssetFilename(level);
                var targetFilePath = UnityPath.Combine(AbsoluteMapDirPath, targetFileName);
                FileUtil.CopyFileOrDirectory(backgroundImagePath.Item2, targetFilePath);
            }

            AssetDatabase.Refresh();

            foreach (var backgroundImagePath in backgroundImagePaths)
            {
                var level = backgroundImagePath.Item1;
                var assetTargetFilePath = SpriteAssetPath(level);
                ConfigureSprite(assetTargetFilePath);
            }

            AssetDatabase.Refresh();
        }

        private static void ConfigureSprite(string assetPath)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            var textureSettings = new TextureImporterSettings();
            importer.ReadTextureSettings(textureSettings);

            textureSettings.spriteMeshType = SpriteMeshType.FullRect;
            textureSettings.spriteAlignment = (int)SpriteAlignment.BottomCenter;

            importer.SetTextureSettings(textureSettings);
            importer.SaveAndReimport();
        }

        private static void AddSpritesToScene(IEnumerable<System.Tuple<Level, SimplifiedData>> simplifiedData)
        {
            foreach (var simplified in simplifiedData)
            {
                var level = simplified.Item1;
                var assetTargetFilePath = SpriteAssetPath(level);
                AddSpriteToScene(simplified.Item2, assetTargetFilePath);
            }
        }

        private static void AddSpriteToScene(SimplifiedData simplifiedData, string spritePath)
        {
            var UnityWorldCoord = new Vector2Int((int)simplifiedData.X, (int)simplifiedData.Y);
            var unityPos = LDtkCoordinatesConverter.LevelPosition(UnityWorldCoord, (int)simplifiedData.Height, 1);
            //CoordinatesConverterLevelPosition(Vector2Int pixelPos, int pixelHeight, int pixelsPerUnit)

            var go = new GameObject(simplifiedData.Identifier, typeof(SpriteRenderer));
            go.transform.position = unityPos; // new Vector2(simplifiedData.X, simplifiedData.Y);
            var spriteRenderer = go.GetComponent<SpriteRenderer>();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            spriteRenderer.sprite = sprite;
            spriteRenderer.drawMode = SpriteDrawMode.Sliced;
            spriteRenderer.size = new Vector2(simplifiedData.Width, simplifiedData.Height);
        }

        private static string SpriteAssetFilename(Level level) => $"{level.Identifier}_Pathway.png";

        private static string SpriteAssetPath(Level level)
        {
            var targetFileName = SpriteAssetFilename(level);
            var assetTargetFilePath = UnityPath.Combine("Assets", MapDirName, targetFileName);
            return assetTargetFilePath;
        }
    }
}
