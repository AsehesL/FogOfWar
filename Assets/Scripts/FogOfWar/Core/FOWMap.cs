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

            int xRange = Mathf.FloorToInt(radius/Mathf.FloorToInt(xSize/texWidth));
            int zRange = Mathf.FloorToInt(radius / Mathf.FloorToInt(zSize / texHeight));

            int left = xRange;
            int right = xRange;
            int top = zRange;
            int down = zRange;
            if (x >= 0 && x < xRange)
            {
                left = x;
            }
            if (x < texWidth && x >= texWidth - xRange)
            {
                right = texWidth - 1 - x;
            }
            if (z >= 0 && z < zRange)
            {
                down = z;
            }
            if (z < texHeight && z >= texHeight - zRange)
            {
                top = texHeight - 1 - z;
            }
            int centX = left;
            int centY = down;

            int[,] temp = new int[left + right + 1, top + down + 1];

            m_Queue.Enqueue(new FOWMapPos(0, 0));
            temp[centX, centY] = -1;

            while (m_Queue.Count > 0)
            {
                var root = m_Queue.Dequeue();
                if (m_MapData[x + root.x, z + root.y] != 0)
                {
                    RayCast(root, centX, centY, left + right + 1, top + down + 1, deltaXSize, deltaZSize, temp);
                    continue;
                }
                if (centX + root.x - 1 >= 0)
                {
                    if (temp[centX + root.x - 1, centY + root.y] == 0)
                    {
                        m_Queue.Enqueue(new FOWMapPos(root.x - 1, root.y));
                        temp[centX + root.x - 1, centY + root.y] = -1;
                        m_MaskTexture.SetAsVisible(x + root.x - 1, z + root.y);
                    }
                }
                if (centY + root.y - 1 >= 0)
                {
                    if (temp[centX + root.x, centY + root.y - 1] == 0)
                    {
                        m_Queue.Enqueue(new FOWMapPos(root.x, root.y - 1));
                        temp[centX + root.x, centY + root.y - 1] = -1;
                        m_MaskTexture.SetAsVisible(x + root.x, z + root.y - 1);
                    }
                }
                if (centX + root.x + 1 < left + right + 1)
                {
                    if (temp[centX + root.x + 1, centY + root.y] == 0)
                    {
                        m_Queue.Enqueue(new FOWMapPos(root.x + 1, root.y));
                        temp[centX + root.x + 1, centY + root.y] = -1;
                        m_MaskTexture.SetAsVisible(x + root.x + 1, z + root.y);
                    }
                }
                if (centY + root.y + 1 < top + down + 1)
                {
                    if (temp[centX + root.x, centY + root.y + 1] == 0)
                    {
                        m_Queue.Enqueue(new FOWMapPos(root.x, root.y + 1));
                        temp[centX + root.x, centY + root.y + 1] = -1;
                        m_MaskTexture.SetAsVisible(x + root.x, z + root.y + 1);
                    }
                }
            }
            
        }

        private void RayCast(FOWMapPos pos, int centX, int centY, int w, int h, float deltaXSize, float deltaZSize, int[,] map)
        {
            m_SortAngle[0] = Mathf.Atan2((pos.y * deltaZSize + deltaZSize / 2), (pos.x * deltaXSize - deltaXSize / 2)) * Mathf.Rad2Deg;
            m_SortAngle[1] = Mathf.Atan2((pos.y * deltaZSize - deltaZSize / 2), (pos.x * deltaXSize - deltaXSize / 2)) * Mathf.Rad2Deg;
            m_SortAngle[2] = Mathf.Atan2((pos.y * deltaZSize + deltaZSize / 2), (pos.x * deltaXSize + deltaXSize / 2)) * Mathf.Rad2Deg;
            m_SortAngle[3] = Mathf.Atan2((pos.y * deltaZSize - deltaZSize / 2), (pos.x * deltaXSize + deltaXSize / 2)) * Mathf.Rad2Deg;
            float cAngle = Mathf.Atan2((pos.y * deltaZSize), (pos.x * deltaXSize)) * Mathf.Rad2Deg;
            SortAngle();

            m_RayCastQueue.Clear();
            m_RayCastQueue.Enqueue(pos);
            map[centX + pos.x, centY + pos.y] = 1;
            while (m_RayCastQueue.Count > 0)
            {
                FOWMapPos root = m_RayCastQueue.Dequeue();

                if (centX + root.x - 1 >= 0 && (cAngle >= 90 || cAngle < -90))
                {
                    if (map[centX + root.x - 1, centY + root.y] == 0)
                    {
                        if (AddLegalPos(root.x - 1, root.y, deltaXSize, deltaZSize))
                        {
                            m_RayCastQueue.Enqueue(new FOWMapPos(root.x - 1, root.y));
                            map[centX + root.x - 1, centY + root.y] = 1;
                        }
                    }
                }
                if (centX + root.x - 1 >= 0 && centY + root.y - 1 >= 0 && cAngle <= -90 && cAngle >= -180)
                {
                    if (map[centX + root.x - 1, centY + root.y - 1] == 0)
                    {
                        if (AddLegalPos(root.x - 1, root.y - 1, deltaXSize, deltaZSize))
                        {
                            m_RayCastQueue.Enqueue(new FOWMapPos(root.x - 1, root.y - 1));
                            map[centX + root.x - 1, centY + root.y - 1] = 1;
                        }
                    }
                }
                if (centY + root.y - 1 >= 0 && cAngle <= 0 && cAngle >= -180)
                {
                    if (map[centX + root.x, centY + root.y - 1] == 0)
                    {
                        if (AddLegalPos(root.x, root.y - 1, deltaXSize, deltaZSize))
                        {
                            m_RayCastQueue.Enqueue(new FOWMapPos(root.x, root.y - 1));
                            map[centX + root.x, centY + root.y - 1] = 1;
                        }
                    }
                }
                if (centX + root.x + 1 < w && centY + root.y - 1 >= 0 && cAngle <= 0 && cAngle >= -90)
                {
                    if (map[centX + root.x + 1, centY + root.y - 1] == 0)
                    {
                        if (AddLegalPos(root.x + 1, root.y - 1, deltaXSize, deltaZSize))
                        {
                            m_RayCastQueue.Enqueue(new FOWMapPos(root.x + 1, root.y - 1));
                            map[centX + root.x + 1, centY + root.y - 1] = 1;
                        }
                    }
                }
                if (centX + root.x + 1 < w && cAngle >= -90 && cAngle <= 90)
                {
                    if (map[centX + root.x + 1, centY + root.y] == 0)
                    {
                        if (AddLegalPos(root.x + 1, root.y, deltaXSize, deltaZSize))
                        {
                            m_RayCastQueue.Enqueue(new FOWMapPos(root.x + 1, root.y));
                            map[centX + root.x + 1, centY + root.y] = 1;
                        }
                    }
                }
                if (centX + root.x + 1 < w && centY + root.y + 1 < h && cAngle >= 0 && cAngle <= 90)
                {
                    if (map[centX + root.x + 1, centY + root.y + 1] == 0)
                    {
                        if (AddLegalPos(root.x + 1, root.y + 1, deltaXSize, deltaZSize))
                        {
                            m_RayCastQueue.Enqueue(new FOWMapPos(root.x + 1, root.y + 1));
                            map[centX + root.x + 1, centY + root.y + 1] = 1;
                        }
                    }
                }
                if (centY + root.y + 1 < h && cAngle >= 0 && cAngle <= 180)
                {
                    if (map[centX + root.x, centY + root.y + 1] == 0)
                    {
                        if (AddLegalPos(root.x, root.y + 1, deltaXSize, deltaZSize))
                        {
                            m_RayCastQueue.Enqueue(new FOWMapPos(root.x, root.y + 1));
                            map[centX + root.x, centY + root.y + 1] = 1;
                        }
                    }
                }
                if (centX + root.x - 1 >= 0 && centY + root.y + 1 < h && cAngle >= 90 && cAngle <= 180)
                {
                    if (map[centX + root.x - 1, centY + root.y + 1] == 0)
                    {
                        if (AddLegalPos(root.x - 1, root.y + 1, deltaXSize, deltaZSize))
                        {
                            m_RayCastQueue.Enqueue(new FOWMapPos(root.x - 1, root.y + 1));
                            map[centX + root.x - 1, centY + root.y + 1] = 1;
                        }
                    }
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