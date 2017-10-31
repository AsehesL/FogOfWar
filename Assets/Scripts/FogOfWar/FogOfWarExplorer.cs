using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarExplorer : MonoBehaviour
{
    public float radius;

    private Vector3 m_OriginPosition;

    private int m_OriginMapPosX;
    private int m_OriginMapPosZ;

	void Start () {
		
	}
	
	void Update () {
	    if (m_OriginPosition != transform.position)
	    {
	        m_OriginPosition = transform.position;
	        FogOfWarManager.RefreshExplorerPosition(this);
	    }
	}

    public bool IsPosChange(int posX, int posZ)
    {
        return m_OriginMapPosX != posX || m_OriginMapPosZ != posZ;
    }
}
