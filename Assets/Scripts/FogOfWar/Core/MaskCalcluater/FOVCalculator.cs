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

        public sealed override void Calculate(FOWFieldData field, FOWMap map)
        {
            Vector3 worldPosition = field.position;
            float radiusSq = field.radiusSquare;

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
            m_Arrives.Clear();

            m_Queue.Enqueue(new FOWMapPos(x, z));
            m_Arrives.Add(z * map.texWidth + x);
            map.maskTexture.SetAsVisible(x, z);

            while (m_Queue.Count > 0)
            {
                var root = m_Queue.Dequeue();
                if (map.mapData[root.x, root.y] != 0)
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
            return map.mapData[x, y] == 0;
        }

        protected abstract void RayCast(FOWMap map, FOWMapPos pos, int centX, int centZ,
            FOWFieldData field);

        private void SetVisibleAtPosition(FOWMap map, int x, int z, int centX, int centZ, float radiusSq)
        {
            if (x < 0 || z < 0 || x >= map.texWidth || z >= map.texHeight)
                return;
            float r = (x - centX) * (x - centX) * map.deltaX * map.deltaX + (z - centZ) * (z - centZ) * map.deltaZ * map.deltaZ;
            if (r > radiusSq)
                return;
            int index = z * map.texWidth + x;
            if (m_Arrives.Contains(index))
                return;
            m_Arrives.Add(index);
            m_Queue.Enqueue(new FOWMapPos(x, z));
            map.maskTexture.SetAsVisible(x, z);
        }
    }
}