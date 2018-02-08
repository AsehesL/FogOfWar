using UnityEngine;
using System.Collections;
using System;

namespace ASL.FogOfWar
{
    [System.Serializable]
    public class FOWPregenerationMapData : MonoBehaviour, IFOWMapData
    {
        public bool isPregeneration
        {
            get { return false; }
        }

        public bool this[int i, int j]
        {
            get { return m_MapData[j * width + i]; }
        }
        
        public int width;
        public int height;
        [SerializeField]
        private bool[] m_MapData;

        public void GenerateMapData(float beginx, float beginy, float deltax, float deltay, float heightRange)
        {
            m_MapData = new bool[width*height];
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    m_MapData[j* width + i] = FOWUtils.IsObstacle(beginx, beginy, deltax, deltay, heightRange, i, j);
                }
            }
        }
    }
}