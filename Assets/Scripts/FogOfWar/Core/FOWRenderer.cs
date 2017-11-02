using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL.FogOfWar
{
    internal class FOWRenderer
    {

        private Material m_EffectMaterial;

        private Matrix4x4 m_WorldToProjector;

        public FOWRenderer(Shader effectShader, Vector3 position, float xSize, float zSize, float blurOffset)
        {
            m_EffectMaterial = new Material(effectShader);
            m_EffectMaterial.SetFloat("_BlurOffset", blurOffset);
            Matrix4x4 worldToLocal = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one);
            Matrix4x4 proj = default(Matrix4x4);

            proj.m00 = 1/xSize;
            proj.m03 = -0.5f;
            proj.m12 = 1/zSize;
            proj.m13 = -0.5f;
            proj.m33 = 1.0f;

            m_WorldToProjector = proj*worldToLocal;
        }

        public void RenderFogOfWar(Matrix4x4 cameraToWorld, RenderTexture src, RenderTexture dst)
        {
            m_EffectMaterial.SetMatrix("internal_CameraToProjector", m_WorldToProjector*cameraToWorld);

            Graphics.Blit(src, dst, m_EffectMaterial);
        }

        public void SetFogTexture(Texture2D texture)
        {
            m_EffectMaterial.SetTexture("_FogTex", texture);
        }

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