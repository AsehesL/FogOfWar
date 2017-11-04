using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.FogOfWar
{
    /// <summary>
    /// 战争迷雾屏幕特效渲染器
    /// </summary>
    internal class FOWRenderer
    {

        private Material m_EffectMaterial;

        /// <summary>
        /// 世界空间到迷雾投影空间矩阵
        /// </summary>
        private Matrix4x4 m_WorldToProjector;

        public FOWRenderer(Shader effectShader, Vector3 position, float xSize, float zSize, Color fogColor, float blurOffset)
        {
            m_EffectMaterial = new Material(effectShader);
            m_EffectMaterial.SetFloat("_BlurOffset", blurOffset);
            m_EffectMaterial.SetColor("_FogColor", fogColor);
            Matrix4x4 worldToLocal = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
            Matrix4x4 proj = default(Matrix4x4);

            proj.m00 = 1/xSize;
            proj.m03 = -0.5f;
            proj.m12 = 1/zSize;
            proj.m13 = -0.5f;
            proj.m33 = 1.0f;

            m_WorldToProjector = proj*worldToLocal;
        }

        /// <summary>
        /// 渲染战争迷雾
        /// </summary>
        /// <param name="cameraToWorld"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void RenderFogOfWar(Matrix4x4 cameraToWorld, RenderTexture src, RenderTexture dst)
        {
            m_EffectMaterial.SetMatrix("internal_CameraToProjector", m_WorldToProjector*cameraToWorld);

            Graphics.Blit(src, dst, m_EffectMaterial);
        }

        /// <summary>
        /// 设置迷雾纹理
        /// </summary>
        /// <param name="texture"></param>
        public void SetFogTexture(Texture2D texture)
        {
            m_EffectMaterial.SetTexture("_FogTex", texture);
        }

        /// <summary>
        /// 设置当前迷雾和上一次更新的迷雾的插值
        /// </summary>
        /// <param name="fade"></param>
        public void SetFogFade(float fade)
        {
            m_EffectMaterial.SetFloat("_MixValue", fade);
        }

        public void Release()
        {
            if (m_EffectMaterial)
                Object.Destroy(m_EffectMaterial);
            m_EffectMaterial = null;
        }
    }
}