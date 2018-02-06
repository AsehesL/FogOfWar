using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ASL.FogOfWar
{
    /// <summary>
    /// 战争迷雾蒙版计算器基类-将蒙版计算抽象出来，方便替换算法
    /// </summary>
    internal abstract class MaskCalcluatorBase
    {

        public void Calculate(FOWFieldData field, FOWMap map)
        {
            if (map.mapData.IsPregeneration)
            {

            }
            else
            {
                RealtimeCalculate(field, map);
            }
        }

        protected abstract void RealtimeCalculate(FOWFieldData field, FOWMap map);

        public abstract void Release();
    }
}