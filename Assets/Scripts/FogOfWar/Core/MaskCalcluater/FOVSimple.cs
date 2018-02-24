using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.FogOfWar
{
    internal class FOVSimple : FOVCalculator
    {
        protected override void RayCast(FOWMap map, FOWMapPos pos, int centX, int centZ, FOWFieldData field)
        {
            float r = field.radius*map.invDeltaX;
            Vector2 dir = new Vector2(pos.x - centX, pos.y - centZ);
            float l = dir.magnitude;
            if (r - l <= 0)
                return;
            dir = dir.normalized * (r - l);
            int x = pos.x + (int)dir.x;
            int y = pos.y + (int)dir.y;

            SetInvisibleLine(map, pos.x, pos.y, x, y, centX, centZ, field.radiusSquare);
        }


    }
}