using UnityEngine;
using System.Collections;

namespace ASL.FogOfWar
{
    /// <summary>
    /// 战争迷雾纹理类
    /// </summary>
    internal class FOWMaskTexture
    {
        private enum UpdateMark
        {
            None,
            Changed,
            EndUpdate,
        }

        /// <summary>
        /// 战争迷雾纹理：R通道叠加所有已探索区域，G通道为当前更新的可见区域，B通道为上一次更新的可见区域
        /// </summary>
        public Texture2D texture
        {
            get { return m_MaskTexture; }
        }

        private Texture2D m_MaskTexture;

        private byte[] m_MaskCache;
        //private byte[,] m_Visible;
        private Color[] m_ColorBuffer;

        //private bool m_IsUpdated;
        private UpdateMark m_UpdateMark;

        private int m_Width;
        private int m_Height;

        public FOWMaskTexture(int width, int height)
        {
            m_Width = width;
            m_Height = height;
            m_MaskCache = new byte[width* height];
            m_ColorBuffer = new Color[width*height];
            //m_Visible = new byte[width, height];
        }

        public void SetAsVisible(int x, int y)
        {
           // m_MaskCache[x, y] = 1;
            m_MaskCache[y*m_Width + x] = 1;
            m_UpdateMark = UpdateMark.Changed;
        }

        public void MarkAsUpdated()
        {
            if (m_UpdateMark != UpdateMark.Changed)
            {
                m_UpdateMark = UpdateMark.EndUpdate;

            }
            else
            {
                for (int i = 0; i < m_Width; i++)
                {
                    for (int j = 0; j < m_Height; j++)
                    {
                        bool isVisible = m_MaskCache[j * m_Width + i] == 1;
                        Color origin = m_ColorBuffer[j * m_Width + i];
                        origin.r = Mathf.Clamp01(origin.r + origin.g);
                        origin.b = origin.g;
                        origin.g = isVisible ? 1 : 0;
                        m_ColorBuffer[j * m_Width + i] = origin;
                        //m_Visible[i, j] = (byte)(isVisible ? 1 : 0);
                        m_MaskCache[j * m_Width + i] = 0;
                    }
                }
            }
        }

        public bool IsVisible(int x, int y)
        {
            if (x < 0 || x >= m_Width || y < 0 || y >= m_Height)
                return false;
            return m_ColorBuffer[y*m_Width + x].g > 0.5f;
            //return m_Visible[x, y] == 1;
        }

        public bool RefreshTexture()
        {
            if (m_UpdateMark == UpdateMark.None)
                return false;
            if (m_UpdateMark == UpdateMark.EndUpdate)
                return true;
            //bool isNew = false;
            if (m_MaskTexture == null)
            {
                m_MaskTexture = GenerateTexture();
                //isNew = true;
            }
            //for (int i = 0; i < m_MaskTexture.width; i++)
            //{
            //    for (int j = 0; j < m_MaskTexture.height; j++)
            //    {
            //        bool isVisible = m_MaskCache[i, j] == 1;
            //        Color origin = isNew ? Color.black : m_MaskTexture.GetPixel(i, j);
            //        origin.r = Mathf.Clamp01(origin.r + origin.g);
            //        origin.b = origin.g;
            //        origin.g = isVisible ? 1 : 0;
            //        m_MaskTexture.SetPixel(i, j, origin);
            //        m_Visible[i, j] = (byte) (isVisible ? 1 : 0);
            //        m_MaskCache[i, j] = 0;
            //    }
            //}
            m_MaskTexture.SetPixels(m_ColorBuffer);
            m_MaskTexture.Apply();
            m_UpdateMark = UpdateMark.None;
            return true;
        }

        public void Release()
        {
            if (m_MaskTexture != null)
                Object.Destroy(m_MaskTexture);
            m_MaskTexture = null;
            m_MaskCache = null;
            m_ColorBuffer = null;
            //m_Visible = null;
        }

        private Texture2D GenerateTexture()
        {
            Texture2D tex = new Texture2D(m_Width, m_Height, TextureFormat.RGB24, false);
            tex.wrapMode = TextureWrapMode.Clamp;
            //for (int i = 0; i < tex.width; i++)
            //{
            //    for (int j = 0; j < tex.height; j++)
            //    {
            //        m_ColorBuffer[j*m_Width + i] = Color.black;
            //    }
            //}
            return tex;
        }
    }
}