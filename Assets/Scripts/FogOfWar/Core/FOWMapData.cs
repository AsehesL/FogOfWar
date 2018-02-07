using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.FogOfWar
{
    public interface IFOWMapData
    {
        /// <summary>
        /// 地图数据是否为预生成
        /// </summary>
        bool IsPregeneration { get; }

        void GenerateMapData(float beginx, float beginy, float deltax, float deltay, float heightRange);

        bool this[int i, int j] { get; }
    }
    
    public class FOWMapData : IFOWMapData
    {
        public bool IsPregeneration
        {
            get { return false; }
        }

        public int Width
        {
            get { return m_Width; }
        }

        public int Height
        {
            get { return m_Height; }
        }

        public bool this[int i, int j]
        {
            get
            {
                return m_MapData[i, j]; 
                
            }
        }

        private bool[,] m_MapData;

        private int m_Width;
        private int m_Height;

        public FOWMapData(int width, int height)
        {
            m_MapData = new bool[width, height];
            m_Width = width;
            m_Height = height;
        }

        public void GenerateMapData(float beginx, float beginy, float deltax, float deltay, float heightRange)
        {
            for (int i = 0; i < m_Width; i++)
            {
                for (int j = 0; j < m_Height; j++)
                {
                    float x = beginx + i * deltax + deltax / 2;
                    float y = beginy + j * deltay + deltay / 2;
                    Ray ray = new Ray(new Vector3(x, beginy + heightRange, y), Vector3.down);
                    if (Physics.Raycast(ray, heightRange))
                    {
                        m_MapData[i, j] = true;
                    }
                    else
                    {
                        //m_MapData[i, j] = false;
                    }
                }
            }
        }
    }
}