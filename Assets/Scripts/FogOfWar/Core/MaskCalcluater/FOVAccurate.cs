using UnityEngine;
using System.Collections;

namespace ASL.FogOfWar
{
    /// <summary>
    /// 精确FOV计算器算法2
    /// </summary>
    internal class FOVAccurate : FOVCalculator
    {

        public FOVAccurate() : base()
        {
        }

        protected override void RayCast(FOWMap map, FOWMapPos pos, int centX, int centZ, FOWFieldData field)
        {
            int x = pos.x - centX;
            int z = pos.y - centZ;

            float corner1x, corner1z, corner2x, corner2z;
            GetCornersPos(x, z, out corner1x, out corner1z, out corner2x, out corner2z);
            RaycastLine(map, pos, centX, centZ, corner1x, corner1z, field);
            RaycastLine(map, pos, centX, centZ, corner2x, corner2z, field);
            
        }

        private void GetCornersPos(int x, int z, out float corner1x, out float corner1z, out float corner2x, out float corner2z)
        {
            corner1x = 0;
            corner1z = 0;
            corner2x = 0;
            corner2z = 0;
            if (x == 0 && z != 0)
            {
                corner1x = -0.5f;
                corner2x = 0.5f;
                if (z >= 0)
                {
                    corner1z = z - 0.5f;
                    corner2z = z - 0.5f;
                }
                else
                {
                    corner1z = z + 0.5f;
                    corner2z = z + 0.5f;
                }
            }
            else if (x != 0 && z == 0)
            {
                corner1z = -0.5f;
                corner2z = 0.5f;
                if (x >= 0)
                {
                    corner1x = x - 0.5f;
                    corner2x = x - 0.5f;
                }
                else
                {
                    corner1x = x + 0.5f;
                    corner2x = x + 0.5f;
                }
            }
            else if ((x < 0 && z < 0) || (x > 0 && z > 0))
            {
                corner1x = x - 0.5f;
                corner1z = z + 0.5f;
                corner2x = x + 0.5f;
                corner2z = z - 0.5f;
            }
            else if ((x < 0 && z > 0) || (x > 0 && z < 0))
            {
                corner1x = x - 0.5f;
                corner1z = z - 0.5f;
                corner2x = x + 0.5f;
                corner2z = z + 0.5f;
            }
        }

        private void RaycastLine(FOWMap map, FOWMapPos pos, int centX, int centZ, float px, float pz, FOWFieldData field)
        {
            Vector2 dir = new Vector2(px, pz)*10;
            int x = centX + (int)dir.x;
            int y = centZ + (int)dir.y;

            SetInvisibleLine(map, pos.x, pos.y, x, y, centX, centZ, field.radiusSquare);
        }

    }
}