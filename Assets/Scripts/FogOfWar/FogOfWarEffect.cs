using System.Collections;
using System.Collections.Generic;
using ASL.FogOfWar;
using UnityEngine;

public struct FOWMapPos
{
    public int x;
    public int y;

    public FOWMapPos(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

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

    /// <summary>
    /// 战争迷雾颜色(RGB迷雾颜色，Alpha已探索区域透明度)
    /// </summary>
    public Color fogColor { get { return m_FogColor; } }
    /// <summary>
    /// 迷雾区域宽度
    /// </summary>
    public float xSize { get { return m_XSize; } }
    /// <summary>
    /// 迷雾区域高度
    /// </summary>
    public float zSize { get { return m_ZSize; } }
    /// <summary>
    /// 迷雾贴图宽度
    /// </summary>
    public int texWidth { get { return m_TexWidth; } }
    /// <summary>
    /// 迷雾贴图高度
    /// </summary>
    public int texHeight { get { return m_TexHeight; } } 
    /// <summary>
    /// 迷雾区域中心坐标
    /// </summary>
    public Vector3 centerPosition { get { return m_CenterPosition; } }
    /// <summary>
    /// 迷雾模糊偏移量
    /// </summary>
    public float fogBlurOffset { get { return m_BlurOffset; } }

    [SerializeField]
    private Color m_FogColor = Color.black;
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

    /// <summary>
    /// 迷雾特效shader
    /// </summary>
    public Shader effectShader;

    /// <summary>
    /// 战争迷雾地图对象
    /// </summary>
    private FOWMap m_Map;
    /// <summary>
    /// 战争迷雾渲染器
    /// </summary>
    private FOWRenderer m_Renderer;

    private bool m_IsInitialized;

    private float m_MixTime = 0.0f;
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
        /*
        更新迷雾纹理
        */
        if (m_MixTime >= 1.0f)
        {
            if (m_RefreshTime >= 1.0f)
            {
                m_RefreshTime = 0.0f;
                if (m_Map.RefreshFOWTexture())
                {
                    
                    m_Renderer.SetFogFade(0);
                    m_MixTime = 0;
                    m_Renderer.SetFogTexture(m_Map.GetFOWTexture());
                }
            }
            else
            {
                m_RefreshTime += Time.deltaTime* kRefreshTextureSpeed;
            }
        }
        else
        {
            m_MixTime += Time.deltaTime* kDispearSpeed;
            m_Renderer.SetFogFade(m_MixTime);
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
        m_Renderer = new FOWRenderer(effectShader, m_CenterPosition, m_XSize, m_ZSize, m_FogColor, m_BlurOffset);
        m_Map = new FOWMap(m_BeginPos, m_XSize, m_ZSize, m_TexWidth, m_TexHeight, m_HeightRange);
        return true;
    }

    /// <summary>
    /// 世界坐标转战争迷雾坐标
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static FOWMapPos WorldPositionToFOW(Vector3 position)
    {
        if (!Instance)
            return default(FOWMapPos);
        if (!Instance.m_IsInitialized)
            return default(FOWMapPos);

        int x = Mathf.FloorToInt((position.x - Instance.m_BeginPos.x) / Instance.m_DeltaX);
        int z = Mathf.FloorToInt((position.z - Instance.m_BeginPos.z) / Instance.m_DeltaZ);

        return new FOWMapPos(x, z);
    }

    /// <summary>
    /// 将指定位置设置为可见
    /// </summary>
    /// <param name="fieldData">视野</param>
    public static void SetVisibleAtPosition(FOWFieldData fieldData)
    {
        if (!Instance)
            return;
        if (!Instance.m_IsInitialized)
            return;
        if (fieldData == null)
            return;

        Instance.m_Map.SetVisible(fieldData);

    }

    /// <summary>
    /// 是否在地图中可见
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
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
