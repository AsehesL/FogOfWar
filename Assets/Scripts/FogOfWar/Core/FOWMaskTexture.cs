using UnityEngine;
using System.Collections;

namespace ASL.FogOfWar
{
    internal class FOWMaskTexture
    {

        private Texture2D m_MaskTexture;

        private byte[,] m_MaskCache;

        public FOWMaskTexture(int width, int height)
        {
            m_MaskTexture = new Texture2D(width, height);
            m_MaskCache = new byte[width, height];
            for (int i = 0; i < m_MaskTexture.width; i++)
            {
                for (int j = 0; j < m_MaskTexture.height; j++)
                {
                    m_MaskTexture.SetPixel(i, j, Color.black);
                }
            }
            m_MaskTexture.Apply();
        }

        public void SetAsVisible(int x, int y)
        {
            m_MaskCache[x, y] = 1;
        }

        public Texture2D GetMaskTexture(bool refresh)
        {
            if (!refresh)
                return m_MaskTexture;
            else
            {
                for (int i = 0; i < m_MaskTexture.width; i++)
                {
                    for (int j = 0; j < m_MaskTexture.height; j++)
                    {
                        m_MaskTexture.SetPixel(i, j, m_MaskCache[i, j] == 1 ? Color.white : Color.black);
                        m_MaskCache[i, j] = 0;
                    }
                }
                m_MaskTexture.Apply();
                return m_MaskTexture;
            }
        }
    }
}