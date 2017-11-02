using System.Collections;
using System.Collections.Generic;
using ASL.FogOfWar;
using UnityEngine;

public class FogOfWarManager : MonoBehaviour {

    public static FogOfWarManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<FogOfWarManager>();
            return instance;
        }
    }

    private static FogOfWarManager instance;

    public float xSize;
    public float zSize;
    public int texWidth;
    public int texHeight;

    public Vector3 centerPosition;

    public float heightRange;

    public Shader effectShader;

    public float blurOffset;

    private FOWMap m_Map;
    private FOWRenderer m_Renderer;

    private bool m_IsInitialized;

    private float m_CurrentTime = 1.0f;
    private float m_RefreshTime = 1.0f;

    private Camera m_Camera;

    private const float kDispearSpeed = 2f;
    private const float kRefreshTextureSpeed = 3.0f;

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
                if (m_Map.maskTexture.RefreshTexture())
                {
                    m_Renderer.SetFogFade(0);
                    m_CurrentTime = 0;
                    m_Renderer.SetFogTexture(m_Map.maskTexture.texture);
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
        if (xSize <= 0 || zSize <= 0 || texWidth <= 0 || texHeight <= 0)
            return false;
        if (effectShader == null || !effectShader.isSupported)
            return false;
        m_Camera = gameObject.GetComponent<Camera>();
        if (m_Camera == null)
            return false;
        m_Renderer = new FOWRenderer(effectShader, centerPosition, xSize, zSize, blurOffset);
        m_Map = new FOWMap(centerPosition, xSize, zSize, texWidth, texHeight, heightRange);
        return true;
    }

    public static void RefreshExplorerPosition(FogOfWarExplorer explorer)
    {
        if (!Instance)
            return;
        if (!Instance.m_IsInitialized)
            return;
        float deltaXSize = Instance.xSize / Instance.texWidth;
        float deltaZSize = Instance.zSize / Instance.texHeight;
        Vector3 originPos = Instance.centerPosition - new Vector3(Instance.xSize / 2, 0, Instance.zSize / 2);
        int x = Mathf.FloorToInt((explorer.transform.position.x - originPos.x) / deltaXSize);
        int z = Mathf.FloorToInt((explorer.transform.position.z - originPos.z) / deltaZSize);
        if (explorer.IsPosChange(x, z))
        {
            Instance.m_Map.OpenFOV(explorer.transform.position, Instance.centerPosition, Instance.xSize,
                Instance.zSize, Instance.texWidth, Instance.texHeight, explorer.radius);
        }
    }

    public static bool IsVisibleInMap(Vector3 position)
    {
        if (!Instance)
            return true;
        if (!Instance.m_IsInitialized)
            return true;
        float deltaXSize = Instance.xSize/Instance.texWidth;
        float deltaZSize = Instance.zSize/Instance.texHeight;
        Vector3 originPos = Instance.centerPosition - new Vector3(Instance.xSize/2, 0, Instance.zSize/2);
        int x = Mathf.FloorToInt((position.x - originPos.x)/deltaXSize);
        int z = Mathf.FloorToInt((position.z - originPos.z)/deltaZSize);

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
        FOWUtils.DrawFogOfWarGizmos(centerPosition, xSize, zSize, texWidth, texHeight, heightRange);
    }
}
