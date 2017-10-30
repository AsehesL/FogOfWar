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

    public float heightRange;

    void Awake()
    {
        
    }
	
	void Update () {
		
	}

    void OnDrawGizmosSelected()
    {
        FOWUtils.DrawFogOfWarGizmos(transform.position, xSize, zSize, texWidth, texHeight, heightRange);
    }
}
