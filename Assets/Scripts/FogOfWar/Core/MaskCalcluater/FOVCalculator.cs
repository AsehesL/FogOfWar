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