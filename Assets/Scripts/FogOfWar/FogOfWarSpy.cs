using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarSpy : MonoBehaviour
{

    private float m_RequestTime;

    private Renderer[] m_Renderers;

    private bool m_Visible = true;

    void Start()
    {
        m_Renderers = gameObject.GetComponentsInChildren<Renderer>();
    }

    void Update()
    {
        m_RequestTime += Time.deltaTime;
        if (m_RequestTime > 1.0f)
        {
            m_RequestTime = 0;
            bool visible = FogOfWarManager.IsVisibleInMap(transform.position);
            SetVisible(visible);
        }
    }

    void SetVisible(bool visible)
    {
        if (m_Visible != visible)
        {
            m_Visible = visible;
            for (int i = 0; i < m_Renderers.Length; i++)
            {
                m_Renderers[i].enabled = visible;
            }
        }
    }
}
