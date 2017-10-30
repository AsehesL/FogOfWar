using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.FogOfWar
{
    internal class FOWMap
    {
        public byte[,] mapData { get { return m_MapData; } }

        private byte[,] m_MapData;

        public FOWMap()
        {

        }

        public void GenerateMap(Vector3 position, float xSize, float zSize, int texWidth,
            int texHeight,
            float heightRange)
        {
            m_MapData = new byte[texWidth, texHeight];

            float deltax = xSize / texWidth;
            float deltay = zSize / texHeight;
            Vector3 origin = position - new Vector3(xSize / 2, 0, zSize / 2);

            for (int i = 0; i < texWidth; i++)
            {
                for (int j = 0; j < texHeight; j++)
                {
                    float x = origin.x + i * deltax + deltax / 2;
                    float y = origin.y + j * deltay + deltay / 2;
                    Ray ray = new Ray(new Vector3(x, origin.y + heightRange, y), Vector3.down);
                    if (Physics.Raycast(ray, heightRange))
                    {
                        m_MapData[i, j] = 1;
                    }
                    else
                    {
                        m_MapData[i, j] = 0;
                    }
                }
            }
        }

    }

}