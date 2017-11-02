using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.FogOfWar
{
    internal struct FOWMapPos
    {
        public int x;
        public int y;

        public FOWMapPos(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    internal class FOWMap
    {
        public byte[,] mapData { get { return m_MapData; } }

        public FOWMaskTexture maskTexture { get { return m_MaskTexture; } }

        private byte[,] m_MapData;

        private FOWMaskTexture m_MaskTexture;

        private Queue<FOWMapPos> m_Queue = new Queue<FOWMapPos>();
        private Queue<FOWMapPos> m_RayCastQueue = new Queue<FOWMapPos>();
        private List<int> m_Arrives = new List<int>();

        private float[] m_SortAngle = new float[4];

        public FOWMap(Vector3 position, float xSize, float zSize, int texWidth,
            int texHeight,
            float heightRange)
        {
            m_MapData = new byte[texWidth, texHeight];
            m_MaskTexture = new FOWMaskTexture(texWidth, texHeight);

            float deltax = xSize / texWidth;
            float deltay = zSize / texHeight;
            Vector3 originPos = position - new Vector3(xSize / 2, 0, zSize / 2);

            for (int i = 0; i < texWidth; i++)
            {
                for (int j = 0; j < texHeight; j++)
                {
                    float x = originPos.x + i * deltax + deltax / 2;
                    float y = originPos.y + j * deltay + deltay / 2;
                    Ray ray = new Ray(new Vector3(x, originPos.y + heightRange, y), Vector3.down);
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

        public void OpenFOV(Vector3 worldPosition, Vector3 fowPos, float xSize, float zSize, int texWidth,
            int texHeight, float radius)
        {
            float deltaXSize = xSize/texWidth;
            float deltaZSize = zSize/texHeight;

            Vector3 originPos = fowPos - new Vector3(xSize / 2, 0, zSize / 2);
            int x = Mathf.FloorToInt((worldPosition.x - originPos.x)/ deltaXSize);
            int z = Mathf.FloorToInt((worldPosition.z - originPos.z)/ deltaZSize);

            if (x < 0 || x >= texWidth)
                return;
            if (z < 0 || z >= texHeight)
                return;
            if (m_MapData[x, z] != 0)
                return;
            m_Queue.Clear();
            m_Arrives.Clear();

            m_Queue.Enqueue(new FOWMapPos(x, z));
            m_Arrives.Add(z*texWidth + x);

            while (m_Queue.Count > 0)
            {
                var root = m_Queue.Dequeue();
                if (m_MapData[root.x, root.y] != 0)
                {
                    RayCast(root, x, z, texWidth, texHeight, deltaXSize, deltaZSize, radius);
                    continue;
                }
                SetAsVisible(root.x - 1, root.y, x, z, texWidth, texHeight, deltaXSize, deltaZSize, radius);
                SetAsVisible(root.x, root.y - 1, x, z, texWidth, texHeight, deltaXSize, deltaZSize, radius);
                SetAsVisible(root.x + 1, root.y, x, z, texWidth, texHeight, deltaXSize, deltaZSize, radius);
                SetAsVisible(root.x, root.y + 1, x, z, texWidth, texHeight, deltaXSize, deltaZSize, radius);

            }
            
        }

        public bool IsVisibleInMap(int x, int z)
        {
            return m_MaskTexture.IsVisible(x, z);
        }

        public void Release()
        {
            if (m_MaskTexture != null)
                m_MaskTexture.Release();
            m_MaskTexture = null;
            m_MapData = null;
            m_SortAngle = null;
            if (m_Queue != null)
                m_Queue.Clear();
            if (m_RayCastQueue != null)
                m_RayCastQueue.Clear();
            if (m_Arrives != null)
                m_Arrives.Clear();
            m_Queue = null;
            m_RayCastQueue = null;
            m_Arrives = null;
        }

        private void SetAsVisible(int x, int z,int centX, int centZ, int texWidth, int texHeight, float deltaXSize, float deltaZSize, float radius)
        {
            if (x < 0 || z < 0 || x >= texWidth || z >= texHeight)
                return;
            float r = Mathf.Sqrt((x - centX)*(x - centX)*deltaXSize*deltaXSize + (z - centZ)*(z - centZ)*deltaZSize*deltaZSize);
            if (r > radius)
                return;
            int index = z * texWidth + x;
            if (m_Arrives.Contains(index))
                return;
            m_Arrives.Add(index);
            m_Queue.Enqueue(new FOWMapPos(x, z));
            m_MaskTexture.SetAsVisible(x, z);
        }

        private void RayCast(FOWMapPos pos, int centX, int centZ, int texWidth, int texHeight, float deltaXSize,
            float deltaZSize, float radius)
        {
            int x = pos.x - centX;
            int z = pos.y - centZ;
            m_SortAngle[0] = Mathf.Atan2((z*deltaZSize + deltaZSize/2), (x*deltaXSize - deltaXSize/2))*Mathf.Rad2Deg;
            m_SortAngle[1] = Mathf.Atan2((z*deltaZSize - deltaZSize/2), (x*deltaXSize - deltaXSize/2))*Mathf.Rad2Deg;
            m_SortAngle[2] = Mathf.Atan2((z*deltaZSize + deltaZSize/2), (x*deltaXSize + deltaXSize/2))*Mathf.Rad2Deg;
            m_SortAngle[3] = Mathf.Atan2((z*deltaZSize - deltaZSize/2), (x*deltaXSize + deltaXSize/2))*Mathf.Rad2Deg;
            float curAngle = Mathf.Atan2((z*deltaZSize), (x*deltaXSize))*Mathf.Rad2Deg;
            SortAngle();

            m_RayCastQueue.Clear();
            m_RayCastQueue.Enqueue(pos);
            int index = pos.y*texWidth + pos.x;
            m_Arrives.Add(index);
            while (m_RayCastQueue.Count > 0)
            {
                FOWMapPos root = m_RayCastQueue.Dequeue();

                if (root.x - 1 >= 0 && (curAngle >= 90 || curAngle < -90))
                {
                    SetAsRaycast(root.x - 1, root.y, centX, centZ, texWidth, texHeight, deltaXSize, deltaZSize, radius);
                }
                if (root.x - 1 >= 0 && root.y - 1 >= 0 && curAngle <= -90 && curAngle >= -180)
                {
                    SetAsRaycast(root.x - 1, root.y - 1, centX, centZ, texWidth, texHeight, deltaXSize, deltaZSize,
                        radius);
                }
                if (root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -180)
                {
                    SetAsRaycast(root.x, root.y - 1, centX, centZ, texWidth, texHeight, deltaXSize, deltaZSize, radius);
                }
                if (root.x + 1 < texWidth && root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -90)
                {
                    SetAsRaycast(root.x + 1, root.y - 1, centX, centZ, texWidth, texHeight, deltaXSize, deltaZSize,
                        radius);
                }
                if (root.x + 1 < texWidth && curAngle >= -90 && curAngle <= 90)
                {
                    SetAsRaycast(root.x + 1, root.y, centX, centZ, texWidth, texHeight, deltaXSize, deltaZSize, radius);
                }
                if (root.x + 1 < texWidth && root.y + 1 < texHeight && curAngle >= 0 && curAngle <= 90)
                {
                    SetAsRaycast(root.x + 1, root.y + 1, centX, centZ, texWidth, texHeight, deltaXSize, deltaZSize,
                        radius);
                }
                if (root.y + 1 < texHeight && curAngle >= 0 && curAngle <= 180)
                {
                    SetAsRaycast(root.x, root.y + 1, centX, centZ, texWidth, texHeight, deltaXSize, deltaZSize, radius);
                }
                if (root.x - 1 >= 0 && root.y + 1 < texHeight && curAngle >= 90 && curAngle <= 180)
                {
                    SetAsRaycast(root.x - 1, root.y + 1, centX, centZ, texWidth, texHeight, deltaXSize, deltaZSize,
                        radius);
                }
            }
        }

        private void SetAsRaycast(int x, int z, int centX, int centZ, int texWidth, int texHeight, float deltaXSize, float deltaZSize, float radius)
        {
            int index = z * texWidth + x;
            float r = Mathf.Sqrt((x - centX) * (x - centX) * deltaXSize * deltaXSize + (z - centZ) * (z - centZ) * deltaZSize * deltaZSize);
            if (r > radius)
                return;
            if (m_Arrives.Contains(index) == false)
            {
                if (AddLegalPos(x - centX, z - centZ, deltaXSize, deltaZSize))
                {
                    m_RayCastQueue.Enqueue(new FOWMapPos(x, z));
                    m_Arrives.Add(index);
                }
            }
        }

        private bool AddLegalPos(int x, int y, float deltaXSize, float deltaZSize)
        {
            float angle = Mathf.Atan2((y * deltaZSize), (x * deltaXSize)) * Mathf.Rad2Deg;
            //if (angle < 0) angle += 360;
            bool isEnd = (m_SortAngle[0] - m_SortAngle[3]) >= 180;
            float minAngle = isEnd ? m_SortAngle[1] : m_SortAngle[3];
            float maxAngle = isEnd ? m_SortAngle[2] : m_SortAngle[0];
            if (isEnd)
            {
                if (angle >= minAngle && angle <= 180)
                    return true;
                if (angle <= maxAngle && angle >= -180)
                    return true;
                return false;
            }
            if (angle >= minAngle && angle <= maxAngle)
            {
                return true;
            }
            return false;
        }

        private void SortAngle()
        {
            for (int i = 0; i < 4; i++)
            {
                for (int j = i + 1; j < 4; j++)
                {
                    if (m_SortAngle[i] < m_SortAngle[j])
                    {
                        float tmp = m_SortAngle[i];
                        m_SortAngle[i] = m_SortAngle[j];
                        m_SortAngle[j] = tmp;
                    }
                }
            }
        }

    }

}