using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestController : MonoBehaviour
{
    [System.Serializable]
    public class Outpost
    {
        public Transform transform;
        public float radius;
    }

    public Outpost[] outposts;

    public TestAI[] ais;

	void Start () {
	    foreach (var ai in ais)
	    {
	        if (ai != null)
	            ai.onRefreshTarget = GetRandomPosition;
	    }
	}

    private Vector3 GetRandomPosition()
    {
        int outpostIndex = Random.Range(0, outposts.Length);
        var op = outposts[outpostIndex];
        float ang = Random.Range(0, 360f)*Mathf.Deg2Rad;
        float rad = Random.Range(0, op.radius);

        float x = Mathf.Cos(ang) * rad;
        float z = Mathf.Sin(ang) * rad;
        return op.transform.position + new Vector3(x, 0, z);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        if (outposts != null && outposts.Length > 0)
        {
            for (int i = 0; i < outposts.Length; i++)
            {
                if (outposts[i] != null && outposts[i].transform != null)
                    Gizmos.DrawWireSphere(outposts[i].transform.position, outposts[i].radius);
            }
        }
    }
}
