using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

namespace ASL.FogOfWar
{
    /// <summary>
    /// 战争迷雾地图类
    /// </summary>
    internal class FOWMap
    {
        /// <summary>
        /// 地图数据（1表示障碍物）
        /// </summary>
        public IFOWMapData mapData { get { return m_MapData; } }

        public FOWMaskTexture maskTexture { get { return m_MaskTexture; } }

        public Vector3 beginPosition { get { return m_BeginPosition; } }

        public float deltaX { get { return m_DeltaX; } }

        public float deltaZ { get { return m_DeltaZ; } }

        public float invDeltaX { get { return m_InvDeltaX; } }

        public float invDeltaZ { get { return m_InvDeltaZ; } }
        public int texWidth { get { return m_TexWidth; } }
        public int texHeight { get { return m_TexHdight; } }
        
        private IFOWMapData m_MapData;

        /// <summary>
        /// 迷雾纹理
        /// </summary>
        private FOWMaskTexture m_MaskTexture;

        //private float[] m_SortAngle = new float[4];

        /// <summary>
        /// 在线程池中计算FOV
        /// </summary>
        private WaitCallback m_FOVCalculator;

        private Vector3 m_BeginPosition;
        
        private float m_DeltaX;
        private float m_DeltaZ;
        private float m_InvDeltaX;
        private float m_InvDeltaZ;
        private int m_TexWidth;
        private int m_TexHdight;

        /// <summary>
        /// FOV计算器
        /// </summary>
        private MaskCalcluatorBase m_CalculaterBase;

        private object m_Lock;

        public FOWMap(FogOfWarEffect.FogMaskType fogMaskType, Vector3 begionPosition, float xSize, float zSize, int texWidth, int texHeight, float heightRange)
        {
            m_FOVCalculator = new WaitCallback(this.CalculateFOV);

            //m_MapData = new FOWMapData(texWidth, texHeight);//new byte[texWidth, texHeight];
            m_MaskTexture = new FOWMaskTexture(texWidth, texHeight);

            m_DeltaX = xSize / texWidth;
            m_DeltaZ = zSize / texHeight;
            m_InvDeltaX = 1.0f/m_DeltaX;
            m_InvDeltaZ = 1.0f/m_DeltaZ;
            m_BeginPosition = begionPosition;
            m_TexWidth = texWidth;
            m_TexHdight = texHeight;

            m_CalculaterBase = CreateCalculator(fogMaskType);

            //GenerateMapData(heightRange);

            m_Lock = new object();
        }

        public void SetMapData(IFOWMapData mapData)
        {
            Debug.Log(mapData.GetType());
            this.m_MapData = mapData;
        }

        /// <summary>
        /// 生成地图数据
        /// </summary>
        /// <param name="heightRange">高度范围</param>
        public void GenerateMapData(float heightRange)
        {
            m_MapData.GenerateMapData(beginPosition.x, beginPosition.y, m_DeltaX, m_DeltaZ, heightRange);
        }

        /// <summary>
        /// 刷新战争迷雾纹理，成功则返回true
        /// </summary>
        /// <returns></returns>
        public bool RefreshFOWTexture()
        {
            lock (m_Lock)
            {
                return m_MaskTexture.RefreshTexture();
            }
        }

        /// <summary>
        /// 获得战争迷雾纹理
        /// </summary>
        /// <returns></returns>
        public Texture2D GetFOWTexture()
        {
            return m_MaskTexture.texture;
        }

        /// <summary>
        /// 根据视野数据设置可见
        /// </summary>
        /// <param name="fieldData">视野数据</param>
        //public void SetVisible(FOWFieldData fieldData)
        public void SetVisible(List<FOWFieldData> fieldDatas)
        {
            //lock (m_Lock)
            {
                ThreadPool.QueueUserWorkItem(m_FOVCalculator, fieldDatas);
            }
        }

        /// <summary>
        /// 指定坐标是否在地图中可见
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool IsVisibleInMap(int x, int z)
        {
            return m_MaskTexture.IsVisible(x, z);
        }
        
        public void Release()
        {
            lock (m_Lock)
            {
                if (m_MaskTexture != null)
                    m_MaskTexture.Release();
                m_MaskTexture = null;
                m_MapData = null;
                m_CalculaterBase.Release();
                m_CalculaterBase = null;
                m_FOVCalculator = null;
            }
            m_Lock = null;
        }

        /// <summary>
        /// 在子线程计算视野
        /// </summary>
        /// <param name="state">参数（视野数据）</param>
        private void CalculateFOV(object state)
        {
            if (state == null)
                return;
            var dt = (List<FOWFieldData>)state;
            lock (m_Lock)
            {
                for (int i = 0; i < dt.Count; i++)
                {
                    if (dt[i] == null)
                        continue;
                    m_CalculaterBase.Calculate(dt[i], this);
                }
                m_MaskTexture.MarkAsUpdated();
            }
        }


        private MaskCalcluatorBase CreateCalculator(FogOfWarEffect.FogMaskType maskType)
        {
            switch (maskType)
            {
                case FogOfWarEffect.FogMaskType.AccurateFOV:
                    return new FOVAccurate();
                case FogOfWarEffect.FogMaskType.BasicFOV:
                    return new FOVSimple();
                case FogOfWarEffect.FogMaskType.Circular:
                    return new CircularMask();
                default:
                    return null;
            }
        }
    }

}