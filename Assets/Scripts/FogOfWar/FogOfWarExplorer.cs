using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 视野数据-由于计算fov是在子线程操作，通过将视野数据以object类型参数传入，使用简单数据类型或结构体会产生装箱操作，因此将视野封装成类
/// </summary>
public class FOWFieldData
{
    public float radiusSquare;
    public Vector3 position;
    public float radius;

    public FOWFieldData(Vector3 position, float radius)
    {
        this.position = position;
        this.radius = radius;
        this.radiusSquare = radius*radius;
    }
}

/// <summary>
/// 探索者
/// </summary>
public class FogOfWarExplorer : MonoBehaviour
{
    /// <summary>
    /// 视野半径
    /// </summary>
    public float radius;

    private Vector3 m_OriginPosition;

    private FOWMapPos m_FowMapPos;

    private FOWFieldData m_FieldData;

    private bool m_IsInitialized;

    void Start()
    {
        m_FieldData = new FOWFieldData(transform.position, radius);
    }
	
	void Update ()
	{
	    if (radius <= 0)
	        return;
	    if (m_OriginPosition != transform.position)
	    {
	        m_OriginPosition = transform.position;
	        var pos = FogOfWarEffect.WorldPositionToFOW(transform.position);
	        if (m_FowMapPos.x != pos.x || m_FowMapPos.y != pos.y || !m_IsInitialized)
	        {
                m_FowMapPos = pos;
	            m_IsInitialized = true;
	            m_FieldData.position = transform.position;
	            m_FieldData.radius = radius;
                //FogOfWarEffect.SetVisibleAtPosition(m_FieldData);
	            FogOfWarEffect.UpdateFOWFieldData(m_FieldData);
	        }
	    }
	}

    void OnDestroy()
    {
        if (m_FieldData != null)
            FogOfWarEffect.ReleaseFOWFieldData(m_FieldData);
        m_FieldData = null;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
