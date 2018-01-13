using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DLineTest : MonoBehaviour
{
    private Texture2D m_Tex;

    private bool m_WaitForEnd;

    private int m_Begin;
    private int m_End;
    
	void Start ()
	{
	    m_Tex = new Texture2D(Screen.width, Screen.height);
	    for (int i = 0; i < m_Tex.width; i++)
	    {
	        for (int j = 0; j < m_Tex.height; j++)
	        {
	            m_Tex.SetPixel(i, j, Color.white);
	        }
	    }
	    m_Tex.Apply();
	}
	
	void Update () {
	    if (Input.GetMouseButtonDown(0))
	    {
            int x = (int)Input.mousePosition.x;
            int y = (int)Input.mousePosition.y;
	        if (m_WaitForEnd)
	        {
	            DrawLine(m_Tex, m_Begin, m_End, x, y);
	        }
	        else
	        {
                m_Begin = x;
                m_End = y;
	        }
	        m_WaitForEnd = !m_WaitForEnd;
	    }
    }

    void OnGUI()
    {
        if (m_Tex)
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), m_Tex);
    }

    private void DrawLine(Texture2D tex, int beginx, int beginy, int endx, int endy)
    {

        int dx = Mathf.Abs(endx - beginx);
        int dy = Mathf.Abs(endy - beginy);
        //int x, y;
        int step = ((endy < beginy && endx >= beginx) || (endy >= beginy && endx < beginx)) ? -1 : 1;

        int p, twod, twodm;
        int pv1, pv2, to;
        if (dy < dx)
        {
            p = 2*dy - dx;
            twod = 2*dy;
            twodm = 2*(dy - dx);
            if (beginx > endx)
            {
                pv1 = endx;
                pv2 = endy;
                endx = beginx;
            }
            else
            {
                pv1 = beginx;
                pv2 = beginy;
            }
            to = endx;
        }
        else
        {
            p = 2*dx - dy;
            twod = 2*dx;
            twodm = 2*(dx - dy);
            if (beginy > endy)
            {
                pv2 = endx;
                pv1 = endy;
                endy = beginy;
            }
            else
            {
                pv2 = beginx;
                pv1 = beginy;
            }
            to = endy;
        }
        if (dy < dx)
            tex.SetPixel(pv1, pv2, Color.black);
        else
            tex.SetPixel(pv2, pv1, Color.black);
        while (pv1 < to)
        {
            pv1++;
            if (p < 0)
                p += twod;
            else
            {
                pv2 += step;
                p += twodm;
            }
            if (dy < dx)
                tex.SetPixel(pv1, pv2, Color.black);
            else
                tex.SetPixel(pv2, pv1, Color.black);
        }

        tex.Apply();
    }
}
