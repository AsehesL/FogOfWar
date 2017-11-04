using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestPlayer : MonoBehaviour
{
    public float distance;
    public float angle = 45;

    private Vector3 m_CameraPosition;

    private NavMeshAgent m_Agent;

	void Start ()
	{
	    m_Agent = gameObject.GetComponent<NavMeshAgent>();
	    Camera.main.transform.rotation = Quaternion.Euler(angle, 0, 0);
	    Camera.main.transform.position = transform.position - Camera.main.transform.forward*distance;
	}
	
	void Update () {
		m_CameraPosition = transform.position - Camera.main.transform.forward * distance;
	    Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, m_CameraPosition, Time.deltaTime*3f);

	    if (Input.GetMouseButtonDown(0))
	    {
	        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
	        RaycastHit hit;
	        if (Physics.Raycast(ray, out hit))
	        {
	            m_Agent.SetDestination(hit.point);

	        }
	    }
	}
}
