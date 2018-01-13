using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.FogOfWar
{
    internal class FOVSimple : FOVCalculator
    {
        protected override void RayCast(FOWMap map, FOWMapPos pos, int centX, int centZ, FOWFieldData field)
        {
            float r = field.radius/map.deltaX;
            Vector2 dir = new Vector2(pos.x - centX, pos.y - centZ);
            float l = dir.magnitude;
            if (r - l <= 0)
                return;
            dir = dir.normalized * (r - l);
            int x = pos.x + (int)dir.x;
            int y = pos.y + (int)dir.y;

            SetInvisibleLine(map, pos.x, pos.y, x, y);
        }

        private void SetInvisibleLine(FOWMap map, int beginx, int beginy, int endx, int endy)
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
                SetInvisibleAtPosition(map, pv1, pv2);
            else
                SetInvisibleAtPosition(map, pv2, pv1);
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
                    SetInvisibleAtPosition(map, pv1, pv2);
                else
                    SetInvisibleAtPosition(map, pv2, pv1);
            }

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