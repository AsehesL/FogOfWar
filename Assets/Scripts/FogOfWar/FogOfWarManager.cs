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

    public Material pjMat;

    public float heightRange;

    private FOWMap m_Map;

    private bool m_IsInitialized;

    private float m_CurrentTime;

    void Awake()
    {
        m_IsInitialized = Init();


        Projector pj = new GameObject("Pj").AddComponent<Projector>();
        pj.transform.rotation = Quaternion.Euler(90, 0, 0);
        pj.transform.position = transform.position;
        pj.aspectRatio = xSize / zSize;
        pj.orthographic = true;
        pj.farClipPlane = 100;
        pj.nearClipPlane = -100;
        pj.orthographicSize = zSize / 2;
        pj.material = pjMat;

        pjMat.SetTexture("_ShadowTex", m_Map.maskTexture.GetMaskTexture(false));
    }

    void FixedUpdate()
    {
        m_CurrentTime += Time.deltaTime;
        if (m_CurrentTime > 1.0f)
        {
            m_CurrentTime = 0;
            pjMat.SetTexture("_ShadowTex", m_Map.maskTexture.GetMaskTexture(true));
        }
    }

    private bool Init()
    {
        if (xSize <= 0 || zSize <= 0 || texWidth <= 0 || texHeight <= 0)
            return false;
        m_Map = new FOWMap(transform.position, xSize, zSize, texWidth, texHeight, heightRange);
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
        Vector3 originPos = Instance.transform.position - new Vector3(Instance.xSize / 2, 0, Instance.zSize / 2);
        int x = Mathf.FloorToInt((explorer.transform.position.x - originPos.x) / deltaXSize);
        int z = Mathf.FloorToInt((explorer.transform.position.z - originPos.z) / deltaZSize);
        if (explorer.IsPosChange(x, z))
        {
            Instance.m_Map.OpenFOV(explorer.transform.position, Instance.transform.position, Instance.xSize,
                Instance.zSize, Instance.texWidth, Instance.texHeight, explorer.radius);
        }
    }

    void OnDrawGizmosSelected()
    {
        FOWUtils.DrawFogOfWarGizmos(transform.position, xSize, zSize, texWidth, texHeight, heightRange);
    }
}
