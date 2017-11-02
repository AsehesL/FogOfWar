using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 探索者
/// </summary>
public class FogOfWarExplorer : MonoBehaviour
{
    public float radius;

    private Vector3 m_OriginPosition;

    private int m_OriginMapPosX;
    private int m_OriginMapPosZ;

    private bool m_IsInitialized;
	
	void Update () {
	    if (m_OriginPosition != transform.position)
	    {
	        m_OriginPosition = transform.position;
	        FogOfWarEffect.RefreshExplorerPosition(this);
	    }
	}

    public bool IsPosChange(int posX, int posZ)
    {
        if (!m_IsInitialized)
        {
            m_IsInitialized = true;
            return true;
        }
        return m_OriginMapPosX != posX || m_OriginMapPosZ != posZ;
    }
}
