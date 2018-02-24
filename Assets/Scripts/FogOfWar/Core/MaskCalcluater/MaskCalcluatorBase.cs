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
            if (map.mapData.isPregeneration)
            {
                UsePregenerationDataCalculate(field, map);
            }
            else
            {
                RealtimeCalculate(field, map);
            }
        }

        /// <summary>
        /// 实时计算
        /// </summary>
        /// <param name="field"></param>
        /// <param name="map"></param>
        protected abstract void RealtimeCalculate(FOWFieldData field, FOWMap map);

        /// <summary>
        /// TODO:使用预生成FOV数据计算(暂未实现)
        /// </summary>
        /// <param name="field"></param>
        /// <param name="map"></param>
        private void UsePregenerationDataCalculate(FOWFieldData field, FOWMap map)
        {
            
        }

        public abstract void Release();
    }
}