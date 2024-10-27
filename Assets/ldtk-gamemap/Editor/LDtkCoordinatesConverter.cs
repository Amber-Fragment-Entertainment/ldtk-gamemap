using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.ldtk_gamemap.Editor
{
    public static class LDtkCoordinatesConverter
    {
        public static Vector2 LevelPosition(Vector2Int pixelPos, int pixelHeight, int pixelsPerUnit)
        {
            pixelPos += Vector2Int.up * pixelHeight;
            return (Vector2)NegateY(pixelPos) / pixelsPerUnit;
        }

        private static Vector2Int NegateY(Vector2Int pos)
        {
            return new Vector2Int
            {
                x = pos.x,
                y = -pos.y
            };
        }
    }
}
