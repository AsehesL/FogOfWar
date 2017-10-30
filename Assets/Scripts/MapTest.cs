using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTest : MonoBehaviour
{

    public float width;
    public float height;
    public int tileWidth;
    public int tileHeight;

    public Vector2 rayRange;

    public Material pjMat;

    private Texture2D m_Tex;
    
	void Start ()
	{
	    Projector pj = new GameObject("Pj").AddComponent<Projector>();
	    pj.transform.rotation = Quaternion.Euler(90, 0, 0);
	    pj.transform.position = transform.position;
	    pj.aspectRatio = width/height;
        pj.orthographic = true;
        pj.farClipPlane = 100;
	    pj.nearClipPlane = -100;
	    pj.orthographicSize = height/2;
	    pj.material = pjMat;

	    m_Tex = new Texture2D(tileWidth, tileHeight);

        float deltax = width / tileWidth;
        float deltay = height / tileHeight;
        Vector3 origin = transform.position - new Vector3(width / 2, 0, height / 2);
        for (int i = 0; i < tileWidth; i++)
	    {
	        for (int j = 0; j < tileHeight; j++)
	        {
	            float x = origin.x + i*deltax + deltax/2;
                float y = origin.y + j * deltay + deltay / 2;
	            Ray ray = new Ray(new Vector3(x, origin.y + rayRange.x, y), Vector3.down);
	            //if (Physics.SphereCast(ray, rayRange.x - rayRange.y))
                if(Physics.Raycast(ray, rayRange.x-rayRange.y))
	            {
	                m_Tex.SetPixel(i, j, Color.black);
	            }
	            else
	            {
                    m_Tex.SetPixel(i, j, Color.white);
                }
	        }
	    }
	    m_Tex.Apply();

	    pjMat.SetTexture("_ShadowTex", m_Tex);
	}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        if (tileWidth <= 0 || tileHeight <= 0 || width <= 0 || height <= 0)
            return;
        float deltax = width/tileWidth;
        float deltay = height/tileHeight;

        Vector3 origin = transform.position - new Vector3(width/2, 0, height/2);

        for (int i = 0; i <= tileWidth; i++)
        {
            Vector3 b = origin + new Vector3(i*deltax, 0, 0);
            Vector3 t = origin + new Vector3(i*deltax, 0, height);
            Gizmos.DrawLine(b, t);
        }
        for (int j = 0; j <= tileHeight; j++)
        {
            Vector3 b = origin + new Vector3(0, 0, j*deltay);
            Vector3 t = origin + new Vector3(width, 0, j*deltay);
            Gizmos.DrawLine(b, t);
        }
    }
}
