using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.FogOfWar
{
    /// <summary>
    /// 精确FOV计算器
    /// </summary>
    internal class FOVAccurate : FOVCalculator
    {
        private Queue<FOWMapPos> m_RayCastQueue;

        private float[] m_SortAngle;

        public FOVAccurate() : base()
        {
            m_RayCastQueue = new Queue<FOWMapPos>();

            m_SortAngle = new float[4];
        }

        public override void Release()
        {
            base.Release();
            m_RayCastQueue.Clear();
            m_RayCastQueue = null;
            m_SortAngle = null;
        }

        protected override void RayCast(FOWMap map, FOWMapPos pos, int centX, int centZ, FOWFieldData field)
        {
            float radiusSq = field.radiusSquare;
            int x = pos.x - centX;
            int z = pos.y - centZ;
            m_SortAngle[0] = Mathf.Atan2((z * map.deltaZ + map.deltaZ / 2), (x * map.deltaX - map.deltaX / 2)) * Mathf.Rad2Deg;
            m_SortAngle[1] = Mathf.Atan2((z * map.deltaZ - map.deltaZ / 2), (x * map.deltaX - map.deltaX / 2)) * Mathf.Rad2Deg;
            m_SortAngle[2] = Mathf.Atan2((z * map.deltaZ + map.deltaZ / 2), (x * map.deltaX + map.deltaX / 2)) * Mathf.Rad2Deg;
            m_SortAngle[3] = Mathf.Atan2((z * map.deltaZ - map.deltaZ / 2), (x * map.deltaX + map.deltaX / 2)) * Mathf.Rad2Deg;
            float curAngle = Mathf.Atan2((z * map.deltaZ), (x * map.deltaX)) * Mathf.Rad2Deg;
            SortAngle();

            m_RayCastQueue.Clear();
            m_RayCastQueue.Enqueue(pos);
            int index = pos.y * map.texWidth + pos.x;
            m_Arrives.Add(index);
            while (m_RayCastQueue.Count > 0)
            {
                FOWMapPos root = m_RayCastQueue.Dequeue();

                if (root.x - 1 >= 0 && (curAngle >= 90 || curAngle < -90))
                {
                    SetInvisibleAtPosition(map, root.x - 1, root.y, centX, centZ, radiusSq);
                }
                if (root.x - 1 >= 0 && root.y - 1 >= 0 && curAngle <= -90 && curAngle >= -180)
                {
                    SetInvisibleAtPosition(map, root.x - 1, root.y - 1, centX, centZ, radiusSq);
                }
                if (root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -180)
                {
                    SetInvisibleAtPosition(map, root.x, root.y - 1, centX, centZ, radiusSq);
                }
                if (root.x + 1 < map.texWidth && root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -90)
                {
                    SetInvisibleAtPosition(map, root.x + 1, root.y - 1, centX, centZ, radiusSq);
                }
                if (root.x + 1 < map.texWidth && curAngle >= -90 && curAngle <= 90)
                {
                    SetInvisibleAtPosition(map, root.x + 1, root.y, centX, centZ, radiusSq);
                }
                if (root.x + 1 < map.texWidth && root.y + 1 < map.texHeight && curAngle >= 0 && curAngle <= 90)
                {
                    SetInvisibleAtPosition(map, root.x + 1, root.y + 1, centX, centZ, radiusSq);
                }
                if (root.y + 1 < map.texHeight && curAngle >= 0 && curAngle <= 180)
                {
                    SetInvisibleAtPosition(map, root.x, root.y + 1, centX, centZ, radiusSq);
                }
                if (root.x - 1 >= 0 && root.y + 1 < map.texHeight && curAngle >= 90 && curAngle <= 180)
                {
                    SetInvisibleAtPosition(map, root.x - 1, root.y + 1, centX, centZ, radiusSq);
                }
            }
        }

        private void SetInvisibleAtPosition(FOWMap map, int x, int z, int centX, int centZ, float radiusSq)
        {
            int index = z * map.texWidth + x;
            float r = (x - centX) * (x - centX) * map.deltaX * map.deltaX + (z - centZ) * (z - centZ) * map.deltaZ * map.deltaZ;
            if (r > radiusSq)
                return;
            if (m_Arrives.Contains(index) == false)
            {
                if (IsPositionInvisible(map, x - centX, z - centZ))
                {
                    m_RayCastQueue.Enqueue(new FOWMapPos(x, z));
                    m_Arrives.Add(index);
                }
            }
        }

        private bool IsPositionInvisible(FOWMap map, int x, int y)
        {
            float angle = Mathf.Atan2((y * map.deltaZ), (x * map.deltaX)) * Mathf.Rad2Deg;
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