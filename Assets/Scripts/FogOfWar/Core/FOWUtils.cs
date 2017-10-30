using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.FogOfWar
{
    internal static class FOWUtils 
    {
        public static void DrawFogOfWarGizmos(Vector3 position, float xSize, float zSize, int texWidth, int texHeight,
            float heightRange)
        {
            if (heightRange <= 0)
                return;
            if (xSize <= 0 || zSize <= 0 || texWidth <= 0 || texHeight <= 0)
                return;
            Gizmos.color = Color.green;

            float deltax = xSize / texWidth;
            float deltay = zSize / texHeight;

            Vector3 origin = position - new Vector3(xSize / 2, 0, zSize / 2);

            for (int i = 0; i <= texWidth; i++)
            {
                Vector3 b = origin + new Vector3(i * deltax, 0, 0);
                Vector3 t = origin + new Vector3(i * deltax, 0, zSize);
                Gizmos.DrawLine(b, t);
            }
            for (int j = 0; j <= texHeight; j++)
            {
                Vector3 b = origin + new Vector3(0, 0, j * deltay);
                Vector3 t = origin + new Vector3(xSize, 0, j * deltay);
                Gizmos.DrawLine(b, t);
            }

            Gizmos.color = Color.blue;

            Gizmos.DrawWireCube(position+Vector3.up*heightRange/2, new Vector3(xSize, heightRange, zSize));
        }
    }
}