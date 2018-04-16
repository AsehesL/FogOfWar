using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestMiniMap : MonoBehaviour
{

    public Image mask;
    public Image map;
    public RawImage maskImage;
    public Transform player;

    //private Material minimapMat;

    //private Texture2D maskTexture;
    
	void Start ()
	{
	    //if (map != null)
	    //{
	    //    minimapMat = new Material(Shader.Find("UI/FogOfWar/MinimapMask"));
	    //    map.material = minimapMat;
	    //    map.material.SetColor("_FogColor", FogOfWarEffect.Instance.fogColor);
	        
	    //}
	}
	
	void Update () {
	    if (player != null)
	    {
	        if (maskImage.texture == null)
	        {
	            maskImage.texture = FogOfWarEffect.Instance.minimapMask;

	        }
//	        //if (maskTexture == null)
//	        {
//	            maskTexture = FogOfWarEffect.Instance.fowMaskTexture;
//                map.material.SetTexture("_MiniMap", maskTexture);
//            }
	        Vector2 p = FogOfWarEffect.WorldPositionTo2DLocal(player.position);
	        p.x = -p.x*map.rectTransform.rect.width;
	        p.y = -p.y*map.rectTransform.rect.height;

            map.rectTransform.anchoredPosition = p;
	    }
	}

    void OnGUI()
    {
        GUI.color = Color.red;
        GUILayout.Label(map.rectTransform.anchoredPosition.ToString("f4"));
    }
}
