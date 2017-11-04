using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class TestAI : MonoBehaviour
{
    public delegate Vector3 RefreshTargetDelegate();

    public RefreshTargetDelegate onRefreshTarget;

    private Vector3 m_TargetPoint;

    private NavMeshAgent m_Agent;

    private float m_CurrentTime;

    private float m_WaitingTime;


    void Start()
    {
        m_Agent = gameObject.GetComponent<NavMeshAgent>();

        m_WaitingTime = Random.Range(1.0f, 5.0f);
    }
    
    void FixedUpdate()
    {
        m_CurrentTime += Time.deltaTime;
        if (m_CurrentTime > m_WaitingTime)
        {
            m_CurrentTime = 0;
            m_WaitingTime = Random.Range(4f, 40f);
            m_TargetPoint = onRefreshTarget();
            m_Agent.SetDestination(m_TargetPoint);
        }
    }
}
