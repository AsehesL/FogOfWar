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

        private Material m_BlurMaterial;

        /// <summary>
        /// 世界空间到迷雾投影空间矩阵
        /// </summary>
        private Matrix4x4 m_WorldToProjector;

        private int m_BlurInteration;


        public FOWRenderer(Shader effectShader, Shader blurShader, Vector3 position, float xSize, float zSize, Color fogColor, float blurOffset, int blurInteration)
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
            m_EffectMaterial.SetMatrix("internal_WorldToProjector", m_WorldToProjector);

            if (blurShader && blurInteration > 0 && blurOffset > 0)
            {
                m_BlurMaterial = new Material(blurShader);
                m_BlurMaterial.SetFloat("_Offset", blurOffset);
            }
            m_BlurInteration = blurInteration;
        }

        /// <summary>
        /// 渲染战争迷雾
        /// </summary>
        /// <param name="camera"></param>
        /// <param name="src"></param>
        /// <param name="dst"></param>
        public void RenderFogOfWar(Camera camera, Texture2D fogTexture, RenderTexture src, RenderTexture dst)
        {
            Matrix4x4 frustumCorners = Matrix4x4.identity;

            float fovWHalf = camera.fieldOfView * 0.5f;

            Vector3 toRight = camera.transform.right * camera.nearClipPlane * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camera.aspect;
            Vector3 toTop = camera.transform.up * camera.nearClipPlane * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

            Vector3 topLeft = (camera.transform.forward * camera.nearClipPlane - toRight + toTop);
            float camScale = topLeft.magnitude * camera.farClipPlane / camera.nearClipPlane;

            topLeft.Normalize();
            topLeft *= camScale;

            Vector3 topRight = (camera.transform.forward * camera.nearClipPlane + toRight + toTop);
            topRight.Normalize();
            topRight *= camScale;

            Vector3 bottomRight = (camera.transform.forward * camera.nearClipPlane + toRight - toTop);
            bottomRight.Normalize();
            bottomRight *= camScale;

            Vector3 bottomLeft = (camera.transform.forward * camera.nearClipPlane - toRight - toTop);
            bottomLeft.Normalize();
            bottomLeft *= camScale;

            frustumCorners.SetRow(0, topLeft);
            frustumCorners.SetRow(1, topRight);
            frustumCorners.SetRow(2, bottomRight);
            frustumCorners.SetRow(3, bottomLeft);
            m_EffectMaterial.SetMatrix("_FrustumCorners", frustumCorners);
            

            if (m_BlurMaterial && fogTexture)
            {
                RenderTexture rt = RenderTexture.GetTemporary(fogTexture.width, fogTexture.height, 0);
                Graphics.Blit(fogTexture, rt, m_BlurMaterial);
                for (int i = 0; i <= m_BlurInteration; i++)
                {
                    RenderTexture rt2 = RenderTexture.GetTemporary(fogTexture.width / 2, fogTexture.height / 2, 0);
                    Graphics.Blit(rt, rt2, m_BlurMaterial);
                    RenderTexture.ReleaseTemporary(rt);
                    rt = rt2;
                }
                m_EffectMaterial.SetTexture("_FogTex", rt);
                CustomGraphicsBlit(src, dst, m_EffectMaterial);
                RenderTexture.ReleaseTemporary(rt);
            }
            else
            {
                m_EffectMaterial.SetTexture("_FogTex", fogTexture);
                CustomGraphicsBlit(src, dst, m_EffectMaterial);
            }
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
            if (m_BlurMaterial)
                Object.Destroy(m_BlurMaterial);
            m_EffectMaterial = null;
            m_BlurMaterial = null;
        }

        private static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial)
        {
            //Graphics.Blit(source, dest, fxMaterial);
            //return;
            RenderTexture.active = dest;

            fxMaterial.SetTexture("_MainTex", source);

            GL.PushMatrix();
            GL.LoadOrtho();

            fxMaterial.SetPass(0);

            GL.Begin(GL.QUADS);

            GL.MultiTexCoord2(0, 0.0f, 0.0f);
            GL.Vertex3(0.0f, 0.0f, 3.0f); // BL

            GL.MultiTexCoord2(0, 1.0f, 0.0f);
            GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

            GL.MultiTexCoord2(0, 1.0f, 1.0f);
            GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

            GL.MultiTexCoord2(0, 0.0f, 1.0f);
            GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

            GL.End();
            GL.PopMatrix();
        }
    }
}