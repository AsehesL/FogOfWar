using System.Collections;
using System.Collections.Generic;
using ASL.FogOfWar;
using UnityEngine;

/// <summary>
/// 屏幕空间战争迷雾
/// </summary>
public class FogOfWarEffect : MonoBehaviour {

    public static FogOfWarEffect Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<FogOfWarEffect>();
            return instance;
        }
    }

    private static FogOfWarEffect instance;

    [SerializeField]
    private Color m_FogColor = Color.black;
    [SerializeField]
    private float m_FogExploredAlpha = 0.5f;
    [SerializeField]
    private float m_XSize;
    [SerializeField]
    private float m_ZSize;
    [SerializeField]
    private int m_TexWidth;
    [SerializeField]
    private int m_TexHeight;
    [SerializeField]
    private Vector3 m_CenterPosition;
    [SerializeField]
    private float m_HeightRange;
    [SerializeField]
    private float m_BlurOffset;

    public Shader effectShader;

    private FOWMap m_Map;
    private FOWRenderer m_Renderer;

    private bool m_IsInitialized;

    private float m_CurrentTime = 0.0f;
    private float m_RefreshTime = 0.0f;

    private float m_DeltaX;
    private float m_DeltaZ;

    private Camera m_Camera;

    private const float kDispearSpeed = 3f;
    private const float kRefreshTextureSpeed = 4.0f;

    private Vector3 m_BeginPos;

    void Awake()
    {
        m_IsInitialized = Init();
        
    }

    void OnDestroy()
    {
        if (m_Renderer != null)
            m_Renderer.Release();
        if (m_Map != null)
            m_Map.Release();
        m_Renderer = null;
        m_Map = null;
        instance = null;
    }

    void FixedUpdate()
    {
        if (m_CurrentTime >= 1.0f)
        {
            if (m_RefreshTime >= 1.0f)
            {
                m_RefreshTime = 0.0f;
                if (m_Map.RefreshTexture())
                {
                    
                    m_Renderer.SetFogFade(0);
                    m_CurrentTime = 0;
                    m_Renderer.SetFogTexture(m_Map.GetTexture());
                }
            }
            else
            {
                m_RefreshTime += Time.deltaTime* kRefreshTextureSpeed;
            }
        }
        else
        {
            m_CurrentTime += Time.deltaTime* kDispearSpeed;
            m_Renderer.SetFogFade(m_CurrentTime);
        }
    }

    private bool Init()
    {
        if (m_XSize <= 0 || m_ZSize <= 0 || m_TexWidth <= 0 || m_TexHeight <= 0)
            return false;
        if (effectShader == null || !effectShader.isSupported)
            return false;
        m_Camera = gameObject.GetComponent<Camera>();
        if (m_Camera == null)
            return false;
        m_DeltaX = m_XSize / m_TexWidth;
        m_DeltaZ = m_ZSize / m_TexHeight;
        m_BeginPos = m_CenterPosition - new Vector3(m_XSize / 2, 0, m_ZSize / 2);
        m_Renderer = new FOWRenderer(effectShader, m_CenterPosition, m_XSize, m_ZSize, m_FogColor, m_FogExploredAlpha, m_BlurOffset);
        m_Map = new FOWMap(m_BeginPos, m_XSize, m_ZSize, m_TexWidth, m_TexHeight, m_HeightRange);
        return true;
    }

    public static void RefreshExplorerPosition(FogOfWarExplorer explorer)
    {
        if (!Instance)
            return;
        if (!Instance.m_IsInitialized)
            return;
      
        int x = Mathf.FloorToInt((explorer.transform.position.x - Instance.m_BeginPos.x) / Instance.m_DeltaX);
        int z = Mathf.FloorToInt((explorer.transform.position.z - Instance.m_BeginPos.z) / Instance.m_DeltaZ);
       
        if (explorer.IsPosChange(x, z))
        {
            Instance.m_Map.OpenFOV(explorer.transform.position, explorer.radius);
        }
        
    }

    public static bool IsVisibleInMap(Vector3 position)
    {
        if (!Instance)
            return true;
        if (!Instance.m_IsInitialized)
            return true;
        int x = Mathf.FloorToInt((position.x - Instance.m_BeginPos.x)/ Instance.m_DeltaX);
        int z = Mathf.FloorToInt((position.z - Instance.m_BeginPos.z)/ Instance.m_DeltaZ);

        return Instance.m_Map.IsVisibleInMap(x, z);

    }


    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!m_IsInitialized)
            Graphics.Blit(src, dst);
        else
        {
            m_Renderer.RenderFogOfWar(m_Camera.cameraToWorldMatrix, src, dst);
        }
    }

    void OnDrawGizmosSelected()
    {
        FOWUtils.DrawFogOfWarGizmos(m_CenterPosition, m_XSize, m_ZSize, m_TexWidth, m_TexHeight, m_HeightRange);
    }
}
