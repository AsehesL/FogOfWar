using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

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

    internal struct FOWFieldData
    {
        public Vector3 position;
        public float radius;

        public FOWFieldData(Vector3 position, float radius)
        {
            this.position = position;
            this.radius = radius;
        }
    }

    internal class FOWMap
    {

        //public FOWMaskTexture maskTexture { get { return m_MaskTexture; } }

        private byte[,] m_MapData;

        private FOWMaskTexture m_MaskTexture;

        private Queue<FOWMapPos> m_Queue = new Queue<FOWMapPos>();
        private Queue<FOWMapPos> m_RayCastQueue = new Queue<FOWMapPos>();
        private List<int> m_Arrives = new List<int>();

        private float[] m_SortAngle = new float[4];

        private Thread m_Thread;
        private object m_Lock;

        private Queue<FOWFieldData> m_OpenFOVList = new Queue<FOWFieldData>();

        private Vector3 m_BeginPosition;
        
        private float m_DeltaX;
        private float m_DeltaZ;
        private int m_TexWidth;
        private int m_TexHdight;

        public FOWMap(Vector3 begionPosition, float xSize, float zSize, int texWidth, int texHeight, float heightRange)
        {
            m_Lock = new object();
            m_Thread = new Thread(FOVAction);

            m_MapData = new byte[texWidth, texHeight];
            m_MaskTexture = new FOWMaskTexture(texWidth, texHeight);

            m_DeltaX = xSize / texWidth;
            m_DeltaZ = zSize / texHeight;
            m_BeginPosition = begionPosition;
            m_TexWidth = texWidth;
            m_TexHdight = texHeight;

            for (int i = 0; i < texWidth; i++)
            {
                for (int j = 0; j < texHeight; j++)
                {
                    float x = m_BeginPosition.x + i * m_DeltaX + m_DeltaX / 2;
                    float y = m_BeginPosition.y + j * m_DeltaZ + m_DeltaZ / 2;
                    Ray ray = new Ray(new Vector3(x, m_BeginPosition.y + heightRange, y), Vector3.down);
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

            m_Thread.Start();
        }

        public bool RefreshTexture()
        {
            lock (m_Lock)
            {
                return m_MaskTexture.RefreshTexture();
            }
        }

        public Texture2D GetTexture()
        {
            return m_MaskTexture.texture;
        }

        public void OpenFOV(Vector3 worldPosition, float radius)
        {
            lock (m_Lock)
            {
                m_OpenFOVList.Enqueue(new FOWFieldData(worldPosition, radius));
            }
            
        }

        public bool IsVisibleInMap(int x, int z)
        {
            return m_MaskTexture.IsVisible(x, z);
        }

        public void Release()
        {
            if (m_Thread != null)
                m_Thread.Abort();
            m_Thread = null;
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
            if (m_OpenFOVList != null)
                m_OpenFOVList.Clear();
            m_Queue = null;
            m_RayCastQueue = null;
            m_Arrives = null;
            m_OpenFOVList = null;
            m_Lock = null;
        }

        private void FOVAction()
        {
            while (true)
            {
                lock (m_Lock)
                {
                    if (m_OpenFOVList != null && m_OpenFOVList.Count > 0)
                    {
                        while(m_OpenFOVList.Count>0)
                        {
                            var dt = m_OpenFOVList.Dequeue();
                            Vector3 worldPosition = dt.position;
                            float radius = dt.radius;
                            int x = Mathf.FloorToInt((worldPosition.x - m_BeginPosition.x) / m_DeltaX);
                            int z = Mathf.FloorToInt((worldPosition.z - m_BeginPosition.z) / m_DeltaZ);

                            if (x < 0 || x >= m_TexWidth)
                                continue;
                            if (z < 0 || z >= m_TexHdight)
                                continue;
                            if (m_MapData[x, z] != 0)
                                continue;
                            m_Queue.Clear();
                            m_Arrives.Clear();

                            m_Queue.Enqueue(new FOWMapPos(x, z));
                            m_Arrives.Add(z * m_TexWidth + x);

                            while (m_Queue.Count > 0)
                            {
                                var root = m_Queue.Dequeue();
                                if (m_MapData[root.x, root.y] != 0)
                                {
                                    RayCast(root, x, z, radius);
                                    continue;
                                }
                                SetAsVisible(root.x - 1, root.y, x, z, radius);
                                SetAsVisible(root.x, root.y - 1, x, z, radius);
                                SetAsVisible(root.x + 1, root.y, x, z, radius);
                                SetAsVisible(root.x, root.y + 1, x, z, radius);
                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }

        private void SetAsVisible(int x, int z, int centX, int centZ, float radius)
        {
            if (x < 0 || z < 0 || x >= m_TexWidth || z >= m_TexHdight)
                return;
            float r = Mathf.Sqrt((x - centX)*(x - centX)* m_DeltaX * m_DeltaX + (z - centZ)*(z - centZ)* m_DeltaZ * m_DeltaZ);
            if (r > radius)
                return;
            int index = z * m_TexWidth + x;
            if (m_Arrives.Contains(index))
                return;
            m_Arrives.Add(index);
            m_Queue.Enqueue(new FOWMapPos(x, z));
            m_MaskTexture.SetAsVisible(x, z);
        }

        private void RayCast(FOWMapPos pos, int centX, int centZ, float radius)
        {
            int x = pos.x - centX;
            int z = pos.y - centZ;
            m_SortAngle[0] = Mathf.Atan2((z* m_DeltaZ + m_DeltaZ / 2), (x* m_DeltaX - m_DeltaX / 2))*Mathf.Rad2Deg;
            m_SortAngle[1] = Mathf.Atan2((z* m_DeltaZ - m_DeltaZ / 2), (x* m_DeltaX - m_DeltaX / 2))*Mathf.Rad2Deg;
            m_SortAngle[2] = Mathf.Atan2((z* m_DeltaZ + m_DeltaZ / 2), (x* m_DeltaX + m_DeltaX / 2))*Mathf.Rad2Deg;
            m_SortAngle[3] = Mathf.Atan2((z* m_DeltaZ - m_DeltaZ / 2), (x* m_DeltaX + m_DeltaX / 2))*Mathf.Rad2Deg;
            float curAngle = Mathf.Atan2((z* m_DeltaZ), (x* m_DeltaX))*Mathf.Rad2Deg;
            SortAngle();

            m_RayCastQueue.Clear();
            m_RayCastQueue.Enqueue(pos);
            int index = pos.y*m_TexWidth + pos.x;
            m_Arrives.Add(index);
            while (m_RayCastQueue.Count > 0)
            {
                FOWMapPos root = m_RayCastQueue.Dequeue();

                if (root.x - 1 >= 0 && (curAngle >= 90 || curAngle < -90))
                {
                    SetAsRaycast(root.x - 1, root.y, centX, centZ, radius);
                }
                if (root.x - 1 >= 0 && root.y - 1 >= 0 && curAngle <= -90 && curAngle >= -180)
                {
                    SetAsRaycast(root.x - 1, root.y - 1, centX, centZ, radius);
                }
                if (root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -180)
                {
                    SetAsRaycast(root.x, root.y - 1, centX, centZ, radius);
                }
                if (root.x + 1 < m_TexWidth && root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -90)
                {
                    SetAsRaycast(root.x + 1, root.y - 1, centX, centZ, radius);
                }
                if (root.x + 1 < m_TexWidth && curAngle >= -90 && curAngle <= 90)
                {
                    SetAsRaycast(root.x + 1, root.y, centX, centZ, radius);
                }
                if (root.x + 1 < m_TexWidth && root.y + 1 < m_TexHdight && curAngle >= 0 && curAngle <= 90)
                {
                    SetAsRaycast(root.x + 1, root.y + 1, centX, centZ, radius);
                }
                if (root.y + 1 < m_TexHdight && curAngle >= 0 && curAngle <= 180)
                {
                    SetAsRaycast(root.x, root.y + 1, centX, centZ, radius);
                }
                if (root.x - 1 >= 0 && root.y + 1 < m_TexHdight && curAngle >= 90 && curAngle <= 180)
                {
                    SetAsRaycast(root.x - 1, root.y + 1, centX, centZ, radius);
                }
            }
        }

        private void SetAsRaycast(int x, int z, int centX, int centZ, float radius)
        {
            int index = z * m_TexWidth + x;
            float r = Mathf.Sqrt((x - centX) * (x - centX) * m_DeltaX * m_DeltaX + (z - centZ) * (z - centZ) * m_DeltaZ * m_DeltaZ);
            if (r > radius)
                return;
            if (m_Arrives.Contains(index) == false)
            {
                if (AddLegalPos(x - centX, z - centZ))
                {
                    m_RayCastQueue.Enqueue(new FOWMapPos(x, z));
                    m_Arrives.Add(index);
                }
            }
        }

        private bool AddLegalPos(int x, int y)
        {
            float angle = Mathf.Atan2((y * m_DeltaZ), (x * m_DeltaX)) * Mathf.Rad2Deg;
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