using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.FogOfWar
{
    internal class FOVLineTo : MaskCalcluatorBase
    {
        private Queue<FOWMapPos> m_Queue;

        public override void Release()
        {
            m_Queue = new Queue<FOWMapPos>();
        }

        public override void CalculateFOV(List<int> arrives, FOWFieldData field, FOWMap map)
        {
            Vector3 worldPosition = field.position;
            float radiusSq = field.radiusSquare;
            float radius = field.radius;

            int x = Mathf.FloorToInt((worldPosition.x - map.beginPosition.x) / map.deltaX);
            int z = Mathf.FloorToInt((worldPosition.z - map.beginPosition.z) / map.deltaZ);

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
            arrives.Add(z * map.texWidth + x);
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
        private void RayCast(List<int> arrives, FOWMap map, FOWMapPos pos, int centX, int centZ, float radius)
        {

            Vector2 dir = new Vector2(pos.x - centX, pos.y - centZ);
            float l = dir.magnitude;
            if (radius - l <= 0)
                return;
            dir = dir.normalized * (radius - l);
            int x = pos.x + (int)dir.x;
            int y = pos.y + (int)dir.y;

            SetInvisibleLine(arrives, map, pos.x, pos.y, x, y);
        }

        private void SetInvisibleLine(List<int> arrives, FOWMap map, int beginx, int beginy, int endx, int endy)
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
                SetInvisibleAtPosition(arrives, map, pv1, pv2);
            else
                SetInvisibleAtPosition(arrives, map, pv2, pv1);
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
                    SetInvisibleAtPosition(arrives, map, pv1, pv2);
                else
                    SetInvisibleAtPosition(arrives, map, pv2, pv1);
            }

        }

        private void SetInvisibleAtPosition(List<int> arrives, FOWMap map, int x, int z)
        {
            int index = z * map.texWidth + x;
            if (arrives.Contains(index) == false)
            {
                arrives.Add(index);
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

    }
}