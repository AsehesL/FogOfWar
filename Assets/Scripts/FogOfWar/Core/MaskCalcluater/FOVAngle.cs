using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.FogOfWar
{
    internal class FOVAngle : MaskCalcluatorBase
    {
        private Queue<FOWMapPos> m_Queue;
        private Queue<FOWMapPos> m_RayCastQueue;

        private float[] m_SortAngle;

        public FOVAngle()
        {
            m_Queue = new Queue<FOWMapPos>();
            m_RayCastQueue = new Queue<FOWMapPos>();

            m_SortAngle = new float[4];
        }

        public override void Release()
        {
            m_Queue.Clear();
            m_Queue = null;
            m_RayCastQueue.Clear();
            m_RayCastQueue = null;
            m_SortAngle = null;
        }

        public override void CalculateFOV(List<int> arrives, FOWFieldData field, FOWMap map)
        {
            Vector3 worldPosition = field.position;
            float radiusSq = field.radiusSquare;

            int x = Mathf.FloorToInt((worldPosition.x - map.beginPosition.x)/ map.deltaX);
            int z = Mathf.FloorToInt((worldPosition.z - map.beginPosition.z)/ map.deltaZ);

            if (x < 0 || x >= map.texWidth)
                return;
            if (z < 0 || z >= map.texHeight)
                return;
            if (map.mapData[x, z] != 0)
            {
                return;
            }
            m_Queue.Clear();
            arrives.Clear();

            m_Queue.Enqueue(new FOWMapPos(x, z));
            arrives.Add(z* map.texWidth + x);
            map.maskTexture.SetAsVisible(x, z);

            while (m_Queue.Count > 0)
            {
                var root = m_Queue.Dequeue();
                if (map.mapData[root.x, root.y] != 0)
                {
                    RayCast(arrives, map, root, x, z, radiusSq);
                    continue;
                }
                SetVisibleAtPosition(arrives, map, root.x - 1, root.y, x, z, radiusSq);
                SetVisibleAtPosition(arrives, map, root.x, root.y - 1, x, z, radiusSq);
                SetVisibleAtPosition(arrives, map, root.x + 1, root.y, x, z, radiusSq);
                SetVisibleAtPosition(arrives, map, root.x, root.y + 1, x, z, radiusSq);
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
        private void RayCast(List<int> arrives, FOWMap map, FOWMapPos pos, int centX, int centZ, float radiusSq)
        {

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
            arrives.Add(index);
            while (m_RayCastQueue.Count > 0)
            {
                FOWMapPos root = m_RayCastQueue.Dequeue();

                if (root.x - 1 >= 0 && (curAngle >= 90 || curAngle < -90))
                {
                    SetInvisibleAtPosition(arrives, map, root.x - 1, root.y, centX, centZ, radiusSq);
                }
                if (root.x - 1 >= 0 && root.y - 1 >= 0 && curAngle <= -90 && curAngle >= -180)
                {
                    SetInvisibleAtPosition(arrives, map, root.x - 1, root.y - 1, centX, centZ, radiusSq);
                }
                if (root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -180)
                {
                    SetInvisibleAtPosition(arrives, map, root.x, root.y - 1, centX, centZ, radiusSq);
                }
                if (root.x + 1 < map.texWidth && root.y - 1 >= 0 && curAngle <= 0 && curAngle >= -90)
                {
                    SetInvisibleAtPosition(arrives, map, root.x + 1, root.y - 1, centX, centZ, radiusSq);
                }
                if (root.x + 1 < map.texWidth && curAngle >= -90 && curAngle <= 90)
                {
                    SetInvisibleAtPosition(arrives, map, root.x + 1, root.y, centX, centZ, radiusSq);
                }
                if (root.x + 1 < map.texWidth && root.y + 1 < map.texHeight && curAngle >= 0 && curAngle <= 90)
                {
                    SetInvisibleAtPosition(arrives, map, root.x + 1, root.y + 1, centX, centZ, radiusSq);
                }
                if (root.y + 1 < map.texHeight && curAngle >= 0 && curAngle <= 180)
                {
                    SetInvisibleAtPosition(arrives, map, root.x, root.y + 1, centX, centZ, radiusSq);
                }
                if (root.x - 1 >= 0 && root.y + 1 < map.texHeight && curAngle >= 90 && curAngle <= 180)
                {
                    SetInvisibleAtPosition(arrives, map, root.x - 1, root.y + 1, centX, centZ, radiusSq);
                }
            }
        }


        private void SetVisibleAtPosition(List<int> arrives, FOWMap map, int x, int z, int centX, int centZ, float radiusSq)
        {
            if (x < 0 || z < 0 || x >= map.texWidth || z >= map.texHeight)
                return;
            float r = (x - centX) * (x - centX) * map.deltaX * map.deltaX + (z - centZ) * (z - centZ) * map.deltaZ * map.deltaZ;
            if (r > radiusSq)
                return;
            int index = z * map.texWidth + x;
            if (arrives.Contains(index))
                return;
            arrives.Add(index);
            m_Queue.Enqueue(new FOWMapPos(x, z));
            map.maskTexture.SetAsVisible(x, z);
        }

        private void SetInvisibleAtPosition(List<int> arrives, FOWMap map, int x, int z, int centX, int centZ, float radiusSq)
        {
            int index = z * map.texWidth + x;
            float r = (x - centX) * (x - centX) * map.deltaX * map.deltaX + (z - centZ) * (z - centZ) * map.deltaZ * map.deltaZ;
            if (r > radiusSq)
                return;
            if (arrives.Contains(index) == false)
            {
                if (IsPositionInvisible(map, x - centX, z - centZ))
                {
                    m_RayCastQueue.Enqueue(new FOWMapPos(x, z));
                    arrives.Add(index);
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