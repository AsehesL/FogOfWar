using UnityEngine;
using System.Collections;
using System;

namespace ASL.FogOfWar
{
    [System.Serializable]
    public class FOWPregenerationFOVMapData : IFOWMapData
    {
        public bool IsPregeneration
        {
            get { return true; }
        }

        public bool this[int i, int j]
        {
            get { return true; }
        }

        public void GenerateMapData(float beginx, float beginy, float deltax, float deltay, float heightRange)
        {

        }
    }
}