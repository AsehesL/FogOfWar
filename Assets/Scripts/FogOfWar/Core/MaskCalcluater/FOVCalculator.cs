using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.FogOfWar
{
    /// <summary>
    /// FOV蒙版计算器
    /// </summary>
    internal abstract class FOVCalculator : MaskCalcluatorBase
    {
        protected Queue<FOWMapPos> m_Queue;

        protected List<int> m_Arrives;

        public FOVCalculator()
        {
            m_Queue = new Queue<FOWMapPos>();
            m_Arrives = new List<int>();
        }

        protected sealed override void RealtimeCalculate(FOWFieldData field, FOWMap map)
        {
            Vector3 worldPosition = field.position;
            float radiusSq = field.radiusSquare;

            int x = Mathf.FloorToInt((worldPosition.x - map.beginPosition.x) * map.invDeltaX);
            int z = Mathf.FloorToInt((worldPosition.z - map.beginPosition.z) * map.invDeltaZ);

            if (x < 0 || x >= map.texWidth)
                return;
            if (z < 0 || z >= map.texHeight)
                return;
            if (map.mapData[x, z])
            {
                return;
            }
            m_Queue.Clear();
            m_Arrives.Clear();

            m_Queue.Enqueue(new FOWMapPos(x, z));
            m_Arrives.Add(z * map.texWidth + x);
            map.maskTexture.SetAsVisible(x, z);

            while (m_Queue.Count > 0)
            {
                var root = m_Queue.Dequeue();
                if (map.mapData[root.x, root.y])
                {
                    if (PreRayCast(map, root, x, z))
                    {
                        int index = root.y*map.texWidth + root.x;
                        if (!m_Arrives.Contains(index))
                            m_Arrives.Add(index);
                        map.maskTexture.SetAsVisible(root.x, root.y);
                    }
                    else
                        RayCast(map, root, x, z, field);
                    continue;
                }
                SetVisibleAtPosition(map, root.x - 1, root.y, x, z, radiusSq);
                SetVisibleAtPosition(map, root.x, root.y - 1, x, z, radiusSq);
                SetVisibleAtPosition(map, root.x + 1, root.y, x, z, radiusSq);
                SetVisibleAtPosition(map, root.x, root.y + 1, x, z, radiusSq);
            }
        }

        public override void Release()
        {
            m_Queue.Clear();
            m_Queue = null;
            m_Arrives.Clear();
            m_Arrives = null;
        }

        private bool PreRayCast(FOWMap map, FOWMapPos pos, int centX, int centZ)
        {
            float k = ((float) (pos.y - centZ))/(pos.x - centX);
            if (k < -0.414f && k >= -2.414f)
            {
                return !IsVisible(map, pos.x + 1, pos.y + 1) && !IsVisible(map, pos.x - 1, pos.y - 1);
            }else if (k < -2.414f || k >= 2.414f)
            {
                return !IsVisible(map, pos.x + 1, pos.y) && !IsVisible(map, pos.x - 1, pos.y);
            }else if (k < 2.414f && k >= 0.414f)
            {
                return !IsVisible(map, pos.x + 1, pos.y - 1) && !IsVisible(map, pos.x - 1, pos.y + 1);
            }
            else
            {
                return !IsVisible(map, pos.x, pos.y + 1) && !IsVisible(map, pos.x, pos.y - 1);
            }
        }

        private bool IsVisible(FOWMap map, int x, int y)
        {
            if (x < 0 || x >= map.texWidth)
                return false;
            if (y < 0 || y >= map.texHeight)
                return false;
            return !map.mapData[x, y];
        }

        protected abstract void RayCast(FOWMap map, FOWMapPos pos, int centX, int centZ,
            FOWFieldData field);

        private void SetVisibleAtPosition(FOWMap map, int x, int z, int centX, int centZ, float radiusSq)
        {
            if (x < 0 || z < 0 || x >= map.texWidth || z >= map.texHeight)
                return;
            if (!IsInRange(map, x, z, centX, centZ, radiusSq))
                return;
            int index = z * map.texWidth + x;
            if (m_Arrives.Contains(index))
                return;
            m_Arrives.Add(index);
            m_Queue.Enqueue(new FOWMapPos(x, z));
            map.maskTexture.SetAsVisible(x, z);
        }
        
        protected void SetInvisibleLine(FOWMap map, int beginx, int beginy, int endx, int endy, int centX, int centZ, float rsq)
        {
            int dx = Mathf.Abs(endx - beginx);
            int dy = Mathf.Abs(endy - beginy);
            //int x, y;
            int step = ((endy < beginy && endx >= beginx) || (endy >= beginy && endx < beginx)) ? -1 : 1;
            int p, twod, twodm;
            int pv1, pv2, to;
            int x, y;
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
            {
                x = pv1;
                y = pv2;
            }
            else
            {
                x = pv2;
                y = pv1;
            }
            SetInvisibleAtPosition(map, x, y);
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
                {
                    x = pv1;
                    y = pv2;
                }
                else
                {
                    x = pv2;
                    y = pv1;
                }
                if (!IsInRange(map, x, y, centX, centZ, rsq))
                {
                    return;
                }
                SetInvisibleAtPosition(map, x, y);
            }

        }

        private bool IsInRange(FOWMap map, int x, int z, int centX, int centZ, float radiusSq)
        {
            float r = (x - centX) * (x - centX) * map.deltaX * map.deltaX + (z - centZ) * (z - centZ) * map.deltaZ * map.deltaZ;
            if (r > radiusSq)
                return false;
            return true;
        }

        private void SetInvisibleAtPosition(FOWMap map, int x, int z)
        {
            int index = z * map.texWidth + x;
            if (m_Arrives.Contains(index) == false)
            {
                m_Arrives.Add(index);
            }
        }
    }
}