using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace ASL.FogOfWar
{
    internal class CircularMask : MaskCalcluatorBase
    {
        public override void Calculate(FOWFieldData field, FOWMap map)
        {
            Vector3 worldPosition = field.position;
            int rx = (int) (field.radius/map.deltaX);
            int rz = (int) (field.radius/map.deltaZ);
            float rs = rx*rx;
            int x = Mathf.FloorToInt((worldPosition.x - map.beginPosition.x) / map.deltaX);
            int z = Mathf.FloorToInt((worldPosition.z - map.beginPosition.z) / map.deltaZ);

            int beginx = Mathf.Max(0, x - rx);
            int beginy = Mathf.Max(0, z - rz);
            int endx = Mathf.Min(map.texWidth, x + rx);
            int endy = Mathf.Min(map.texHeight, z + rz);

            for (int i = beginx; i < endx; i++)
            {
                for (int j = beginy; j < endy; j++)
                {
                    int dx = i - x;
                    int dy = j - z;
                    float rads = dx * dx + dy * dy;
                    if (rads <= rs)
                        map.maskTexture.SetAsVisible(i, j);
                }
            }
        }

        public override void Release()
        {
           
        }
    }
}
