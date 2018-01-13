using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace ASL.FogOfWar
{
    /// <summary>
    /// 战争迷雾地图类
    /// </summary>
    internal class FOWMap
    {
        /// <summary>
        /// 地图数据（1表示障碍物）
        /// </summary>
        public byte[,] mapData { get { return m_MapData; } }

        public FOWMaskTexture maskTexture { get { return m_MaskTexture; } }

        public Vector3 beginPosition { get { return m_BeginPosition; } }

        public float deltaX { get { return m_DeltaX; } }

        public float deltaZ { get { return m_DeltaZ; } }
        public int texWidth { get { return m_TexWidth; } }
        public int texHeight { get { return m_TexHdight; } }
        
        private byte[,] m_MapData;

        /// <summary>
        /// 迷雾纹理
        /// </summary>
        private FOWMaskTexture m_MaskTexture;

        
        private List<int> m_Arrives = new List<int>();

        //private float[] m_SortAngle = new float[4];

        /// <summary>
        /// 在线程池中计算FOV
        /// </summary>
        private WaitCallback m_FOVCalculator;

        private Vector3 m_BeginPosition;
        
        private float m_DeltaX;
        private float m_DeltaZ;
        private int m_TexWidth;
        private int m_TexHdight;

        /// <summary>
        /// FOV计算器
        /// </summary>
        private MaskCalcluatorBase m_CalculaterBase;

        private object m_Lock;

        public FOWMap(Vector3 begionPosition, float xSize, float zSize, int texWidth, int texHeight, float heightRange)
        {
            m_FOVCalculator = new WaitCallback(this.CalculateFOV);

            m_MapData = new byte[texWidth, texHeight];
            m_MaskTexture = new FOWMaskTexture(texWidth, texHeight);

            m_DeltaX = xSize / texWidth;
            m_DeltaZ = zSize / texHeight;
            m_BeginPosition = begionPosition;
            m_TexWidth = texWidth;
            m_TexHdight = texHeight;

            m_CalculaterBase = new FOVAngle();

            GenerateMapData(heightRange);

            m_Lock = new object();
        }

        /// <summary>
        /// 生成地图数据
        /// </summary>
        /// <param name="heightRange">高度范围</param>
        private void GenerateMapData(float heightRange)
        {
            for (int i = 0; i < m_TexWidth; i++)
            {
                for (int j = 0; j < m_TexHdight; j++)
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
        }

        /// <summary>
        /// 刷新战争迷雾纹理，成功则返回true
        /// </summary>
        /// <returns></returns>
        public bool RefreshFOWTexture()
        {
            lock (m_Lock)
            {
                return m_MaskTexture.RefreshTexture();
            }
        }

        /// <summary>
        /// 获得战争迷雾纹理
        /// </summary>
        /// <returns></returns>
        public Texture2D GetFOWTexture()
        {
            return m_MaskTexture.texture;
        }

        /// <summary>
        /// 根据视野数据设置可见
        /// </summary>
        /// <param name="fieldData">视野数据</param>
        //public void SetVisible(FOWFieldData fieldData)
        public void SetVisible(List<FOWFieldData> fieldDatas)
        {
            //lock (m_Lock)
            {
                ThreadPool.QueueUserWorkItem(m_FOVCalculator, fieldDatas);
            }
        }

        /// <summary>
        /// 指定坐标是否在地图中可见
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool IsVisibleInMap(int x, int z)
        {
            return m_MaskTexture.IsVisible(x, z);
        }
        
        public void Release()
        {
            lock (m_Lock)
            {
                if (m_MaskTexture != null)
                    m_MaskTexture.Release();
                m_MaskTexture = null;
                m_MapData = null;
                m_CalculaterBase.Release();
                m_CalculaterBase = null;
                //m_SortAngle = null;
                //if (m_Queue != null)
                //    m_Queue.Clear();
                //if (m_RayCastQueue != null)
                //    m_RayCastQueue.Clear();
                if (m_Arrives != null)
                    m_Arrives.Clear();
                //m_Queue = null;
                //m_RayCastQueue = null;
                m_Arrives = null;
                m_FOVCalculator = null;
            }
            m_Lock = null;
        }

        /// <summary>
        /// 在子线程计算视野
        /// </summary>
        /// <param name="state">参数（视野数据）</param>
        private void CalculateFOV(object state)
        {
            if (state == null)
                return;
            var dt = (List<FOWFieldData>)state;
            lock (m_Lock)
            {
                for (int i = 0; i < dt.Count; i++)
                {
                    if (dt[i] == null)
                        continue;
                    m_CalculaterBase.CalculateFOV(m_Arrives, dt[i], this);
                }
                m_MaskTexture.MarkAsUpdated();
            }
        }

        /// <summary>
        /// 射线检测，将障碍物后全部设为不可见
        /// TODO:待优化，实际上不必全部设置
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="centX"></param>
        /// <param name="centZ"></param>
        /// <param name="radiusSq"></param>
        private void RayCast(FOWMapPos pos, int centX, int centZ, float radius)
        {
            Vector2 dir = new Vector2(pos.x - centX, pos.y - centZ);
            float l = dir.magnitude;
            if (radius - l <= 0)
                return;
            dir = dir.normalized*(radius - l);
            int x = pos.x + (int)dir.x;
            int y = pos.y + (int)dir.y;

            SetInvisibleLine(pos.x, pos.y, x, y);

            //int x = pos.x - centX;
            //int z = pos.y - centZ;
            //m_SortAngle[0] = Mathf.Atan2((z* m_DeltaZ + m_DeltaZ / 2), (x* m_DeltaX - m_DeltaX / 2))*Mathf.Rad2Deg;
            //m_SortAngle[1] = Mathf.Atan2((z* m_DeltaZ - m_DeltaZ / 2), (x* m_DeltaX - m_DeltaX / 2))*Mathf.Rad2Deg;
            //m_SortAngle[2] = Mathf.Atan2((z* m_DeltaZ + m_DeltaZ / 2), (x* m_DeltaX + m_DeltaX / 2))*Mathf.Rad2Deg;
            //m_SortAngle[3] = Mathf.Atan2((z* m_DeltaZ - m_DeltaZ / 2), (x* m_DeltaX + m_DeltaX / 2))*Mathf.Rad2Deg;
            //float curAngle = Mathf.Atan2((z* m_DeltaZ), (x* m_DeltaX))*Mathf.Rad2Deg;
            //SortAngle();

            //m_RayCastQueue.Clear();
            //m_RayCastQueue.Enqueue(pos);
            //int index = pos.y*m_TexWidth + pos.x;
            //m_Arrives.Add(index);
            //while (m_RayCastQueue.Count > 0)
            //{
            //    FOWMapPos root = m_RayCastQueue.Dequeue();

            //    if (root.x - 1 >= 0 && (curAngle >= 90 || curAngle < -90))
            //    {
            //        SetInvisibleAtPosition(root.x - 1, root.y, centX, centZ, radiusSq);
            //    }
            //    if (root.x - 1 >= 0 && root.y - 1 >= 0 && curAngle <= -90 && curAngle >= -180)
            //    {
            //        SetInvisibleAtPosition(root.x - 1, root.y - 1, centX, centZ, radiusSq);
            //    }
            //    if (root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -180)
            //    {
            //        SetInvisibleAtPosition(root.x, root.y - 1, centX, centZ, radiusSq);
            //    }
            //    if (root.x + 1 < m_TexWidth && root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -90)
            //    {
            //        SetInvisibleAtPosition(root.x + 1, root.y - 1, centX, centZ, radiusSq);
            //    }
            //    if (root.x + 1 < m_TexWidth && curAngle >= -90 && curAngle <= 90)
            //    {
            //        SetInvisibleAtPosition(root.x + 1, root.y, centX, centZ, radiusSq);
            //    }
            //    if (root.x + 1 < m_TexWidth && root.y + 1 < m_TexHdight && curAngle >= 0 && curAngle <= 90)
            //    {
            //        SetInvisibleAtPosition(root.x + 1, root.y + 1, centX, centZ, radiusSq);
            //    }
            //    if (root.y + 1 < m_TexHdight && curAngle >= 0 && curAngle <= 180)
            //    {
            //        SetInvisibleAtPosition(root.x, root.y + 1, centX, centZ, radiusSq);
            //    }
            //    if (root.x - 1 >= 0 && root.y + 1 < m_TexHdight && curAngle >= 90 && curAngle <= 180)
            //    {
            //        SetInvisibleAtPosition(root.x - 1, root.y + 1, centX, centZ, radiusSq);
            //    }
            //}
        }

        

        private void SetInvisibleLine(int beginx, int beginy, int endx, int endy)
        {
            int dx = Mathf.Abs(endx - beginx);
            int dy = Mathf.Abs(endy - beginy);
            //int x, y;
            int step = ((endy < beginy && endx >= beginx) || (endy >= beginy && endx < beginx)) ? -1 : 1;
            int p, twod, twodm;
            int pv1, pv2, to;
            if (dy < dx)
            {
                p = 2 * dy - dx;
                twod = 2 * dy;
                twodm = 2 * (dy - dx);
                if (beginx > endx)
                {
                    pv1 = endx;
                    pv2 = endy;
                    endx = beginx;
                }
                else
                {
                    pv1 = beginx;
                    pv2 = beginy;
                }
                to = endx;
            }
            else
            {
                p = 2 * dx - dy;
                twod = 2 * dx;
                twodm = 2 * (dx - dy);
                if (beginy > endy)
                {
                    pv2 = endx;
                    pv1 = endy;
                    endy = beginy;
                }
                else
                {
                    pv2 = beginx;
                    pv1 = beginy;
                }
                to = endy;
            }
            if (dy < dx)
                SetInvisibleAtPosition(pv1, pv2);
            else
                SetInvisibleAtPosition(pv2, pv1);
            while (pv1 < to)
            {
                pv1++;
                if (p < 0)
                    p += twod;
                else
                {
                    pv2 += step;
                    p += twodm;
                }
                if (dy < dx)
                    SetInvisibleAtPosition(pv1, pv2);
                else
                    SetInvisibleAtPosition(pv2, pv1);
            }

        }

        private void SetInvisibleAtPosition(int x, int z)
        {
            int index = z * m_TexWidth + x;
            if (m_Arrives.Contains(index) == false)
            {
                m_Arrives.Add(index);
            }
        }

        /// <summary>
        /// 将指定坐标点设置为不可见
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="centX"></param>
        /// <param name="centZ"></param>
        /// <param name="radiusSq"></param>
        //private void SetInvisibleAtPosition(int x, int z, int centX, int centZ, float radiusSq)
        //{
        //    int index = z * m_TexWidth + x;
        //    float r = (x - centX) * (x - centX) * m_DeltaX * m_DeltaX + (z - centZ) * (z - centZ) * m_DeltaZ * m_DeltaZ;
        //    if (r > radiusSq)
        //        return;
        //    if (m_Arrives.Contains(index) == false)
        //    {
        //        if (IsPositionInvisible(x - centX, z - centZ))
        //        {
        //            m_RayCastQueue.Enqueue(new FOWMapPos(x, z));
        //            m_Arrives.Add(index);
        //        }
        //    }
        //}

        //private bool IsPositionInvisible(int x, int y)
        //{
        //    float angle = Mathf.Atan2((y * m_DeltaZ), (x * m_DeltaX)) * Mathf.Rad2Deg;
        //    //if (angle < 0) angle += 360;
        //    bool isEnd = (m_SortAngle[0] - m_SortAngle[3]) >= 180;
        //    float minAngle = isEnd ? m_SortAngle[1] : m_SortAngle[3];
        //    float maxAngle = isEnd ? m_SortAngle[2] : m_SortAngle[0];
        //    if (isEnd)
        //    {
        //        if (angle >= minAngle && angle <= 180)
        //            return true;
        //        if (angle <= maxAngle && angle >= -180)
        //            return true;
        //        return false;
        //    }
        //    if (angle >= minAngle && angle <= maxAngle)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        //private void SortAngle()
        //{
        //    for (int i = 0; i < 4; i++)
        //    {
        //        for (int j = i + 1; j < 4; j++)
        //        {
        //            if (m_SortAngle[i] < m_SortAngle[j])
        //            {
        //                float tmp = m_SortAngle[i];
        //                m_SortAngle[i] = m_SortAngle[j];
        //                m_SortAngle[j] = tmp;
        //            }
        //        }
        //    }
        //}

    }

}