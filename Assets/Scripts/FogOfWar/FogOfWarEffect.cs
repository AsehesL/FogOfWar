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

    public enum FogMaskType
    {
        /// <summary>
        /// 精确计算的FOV
        /// </summary>
        AccurateFOV,
        /// <summary>
        /// 基础FOV
        /// </summary>
        BasicFOV,
        /// <summary>
        /// 简单圆形
        /// </summary>
        Circular,
    }

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
    /// 迷雾蒙版类型
    /// </summary>
    public FogMaskType fogMaskType { get { return m_FogMaskType; } }
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

    public float heightRange { get { return m_HeightRange; } }

    public Texture2D fowMaskTexture
    {
        get
        {
            if (m_Map != null)
                return m_Map.GetFOWTexture();
            return null;
        }
    }

    public RenderTexture minimapMask
    {
        get
        {
            if (!m_GenerateMinimapMask)
                return null;
            return m_Renderer.GetMimiMapMask();
        }
    }

    [SerializeField]
    private FogMaskType m_FogMaskType;
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
    /// <summary>
    /// 模糊偏移量
    /// </summary>
    [SerializeField]
    private float m_BlurOffset;
    /// <summary>
    /// 模糊迭代次数
    /// </summary>
    [SerializeField]
    private int m_BlurInteration;
    /// <summary>
    /// 是否生成小地图蒙版
    /// </summary>
    private bool m_GenerateMinimapMask;

    /// <summary>
    /// 迷雾特效shader
    /// </summary>
    public Shader effectShader;
    /// <summary>
    /// 模糊shader
    /// </summary>
    public Shader blurShader;
    /// <summary>
    /// 小地图蒙版渲染shader
    /// </summary>
    public Shader minimapRenderShader;



    /// <summary>
    /// 预生成的地图FOV数据（如果为空则使用实时计算FOV）
    /// </summary>
    //public FOWPregenerationFOVMapData pregenerationFOVMapData;

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
    private float m_InvDeltaX;
    private float m_InvDeltaZ;

    private Camera m_Camera;

    private const float kDispearSpeed = 3f;
    private const float kRefreshTextureSpeed = 4.0f;

    private Vector3 m_BeginPos;

    private List<FOWFieldData> m_FieldDatas;

    private bool m_IsFieldDatasUpdated;

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
        if (m_FieldDatas != null)
            m_FieldDatas.Clear();
        m_FieldDatas = null;
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
                    m_IsFieldDatasUpdated = false;
                    //m_Renderer.SetFogTexture(m_Map.GetFOWTexture());
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
        m_Camera.depthTextureMode |= DepthTextureMode.Depth;
        m_DeltaX = m_XSize / m_TexWidth;
        m_DeltaZ = m_ZSize / m_TexHeight;
        m_InvDeltaX = 1.0f/m_DeltaX;
        m_InvDeltaZ = 1.0f/m_DeltaZ;
        m_BeginPos = m_CenterPosition - new Vector3(m_XSize * 0.5f, 0, m_ZSize * 0.5f);
        m_Renderer = new FOWRenderer(effectShader, blurShader, minimapRenderShader, m_CenterPosition, m_XSize, m_ZSize, m_FogColor, m_BlurOffset, m_BlurInteration);
        m_Map = new FOWMap(m_FogMaskType, m_BeginPos, m_XSize, m_ZSize, m_TexWidth, m_TexHeight, m_HeightRange);
        IFOWMapData md = gameObject.GetComponent<IFOWMapData>();
        if (md != null)
            m_Map.SetMapData(md);
        else
        {
            m_Map.SetMapData(new FOWMapData(m_TexHeight, m_TexHeight));
            m_Map.GenerateMapData(m_HeightRange);
        }
        if (minimapRenderShader != null)
            m_GenerateMinimapMask = true;
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

        int x = Mathf.FloorToInt((position.x - Instance.m_BeginPos.x) * Instance.m_InvDeltaX);
        int z = Mathf.FloorToInt((position.z - Instance.m_BeginPos.z) * Instance.m_InvDeltaZ);

        return new FOWMapPos(x, z);
    }

    public static Vector2 WorldPositionTo2DLocal(Vector3 position)
    {
        if (!Instance)
            return default(Vector2);
        if (!Instance.m_IsInitialized)
            return default(Vector2);

        Vector2 pos = default(Vector2);
        pos.x = (position.x - Instance.m_BeginPos.x)/Instance.m_XSize;
        pos.y = (position.z - Instance.m_BeginPos.z)/Instance.m_ZSize;

        return pos;
    }

    ///// <summary>
    ///// 将指定位置设置为可见
    ///// </summary>
    ///// <param name="fieldData">视野</param>
    //public static void SetVisibleAtPosition(FOWFieldData fieldData)
    //{
    //    if (!Instance)
    //        return;
    //    if (!Instance.m_IsInitialized)
    //        return;
    //    if (fieldData == null)
    //        return;

    //    Instance.m_Map.SetVisible(fieldData);

    //}

    public static void UpdateFOWFieldData(FOWFieldData data)
    {
        if (!Instance)
            return;
        if (!Instance.m_IsInitialized)
            return;
        if (Instance.m_FieldDatas == null)
            Instance.m_FieldDatas = new List<FOWFieldData>();
        if (!Instance.m_FieldDatas.Contains(data))
        {
            Instance.m_FieldDatas.Add(data);
        }
        if (!Instance.m_IsFieldDatasUpdated)
        {
            //lock (Instance.m_FieldDatas)
            {
                Instance.m_Map.SetVisible(Instance.m_FieldDatas);
                Instance.m_IsFieldDatasUpdated = true;
            }
        }
    }

    public static void ReleaseFOWFieldData(FOWFieldData data)
    {
        if (!instance)
            return;
        if (!instance.m_IsInitialized)
            return;
        //lock (instance.m_FieldDatas)
        {
            if (instance.m_FieldDatas != null && instance.m_FieldDatas.Contains(data))
                instance.m_FieldDatas.Remove(data);
        }
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
        int x = Mathf.FloorToInt((position.x - Instance.m_BeginPos.x)* Instance.m_InvDeltaX);
        int z = Mathf.FloorToInt((position.z - Instance.m_BeginPos.z)* Instance.m_InvDeltaZ);

        return Instance.m_Map.IsVisibleInMap(x, z);

    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (!m_IsInitialized)
            Graphics.Blit(src, dst);
        else
        {
            m_Renderer.RenderFogOfWar(m_Camera, m_Map.GetFOWTexture(), src, dst);
        }
    }

    void OnDrawGizmosSelected()
    {
        FOWUtils.DrawFogOfWarGizmos(m_CenterPosition, m_XSize, m_ZSize, m_TexWidth, m_TexHeight, m_HeightRange);
    }
}
