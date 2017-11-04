using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 潜行者
/// </summary>
public class FogOfWarStalker : MonoBehaviour
{

    private float m_RequestTime;

    private bool m_Visible = true;

    void FixedUpdate()
    {
        m_RequestTime += Time.deltaTime;
        if (m_RequestTime > 0.3f)
        {
            m_RequestTime = 0;
            bool visible = FogOfWarEffect.IsVisibleInMap(transform.position);
            SetVisible(visible);
        }
    }

    void SetVisible(bool visible)
    {
        if (m_Visible != visible)
        {
            m_Visible = visible;
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(visible);
                }
        }
    }
}
