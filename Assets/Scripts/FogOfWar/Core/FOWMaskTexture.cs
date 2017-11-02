using UnityEngine;
using System.Collections;

namespace ASL.FogOfWar
{
    internal class FOWMaskTexture
    {

        public Texture2D texture { get { return m_MaskTexture; } }

        private Texture2D m_MaskTexture;

        private byte[,] m_MaskCache;

        private bool m_IsUpdated;

        private int m_Width;
        private int m_Height;

        public FOWMaskTexture(int width, int height)
        {
            //m_MaskTexture = new Texture2D(width, height);
            m_Width = width;
            m_Height = height;
            m_MaskCache = new byte[width, height];
            //for (int i = 0; i < m_MaskTexture.width; i++)
            //{
            //    for (int j = 0; j < m_MaskTexture.height; j++)
            //    {
            //        m_MaskTexture.SetPixel(i, j, Color.black);
            //    }
            //}
            //m_MaskTexture.Apply();
        }

        public void SetAsVisible(int x, int y)
        {
            m_MaskCache[x, y] = 1;
            m_IsUpdated = true;
        }

        public bool RefreshTexture()
        {
            if (!m_IsUpdated)
                return false;
            bool isNew = false;
            if (m_MaskTexture == null)
            {
                m_MaskTexture = new Texture2D(m_Width, m_Height);
                m_MaskTexture.wrapMode = TextureWrapMode.Clamp;
                isNew = true;
            }
            for (int i = 0; i < m_MaskTexture.width; i++)
            {
                for (int j = 0; j < m_MaskTexture.height; j++)
                {
                    bool isVisible = m_MaskCache[i, j] == 1;
                    Color origin = isNew ? Color.black : m_MaskTexture.GetPixel(i, j);
                    origin.r = Mathf.Clamp01(origin.r + origin.g);
                    origin.b = origin.g;
                    origin.g = isVisible ? 1 : 0;
                    m_MaskTexture.SetPixel(i, j, origin);
                    m_MaskCache[i, j] = 0;
                }
            }
            m_MaskTexture.Apply();
            m_IsUpdated = false;
            return true;
        }
    }
}