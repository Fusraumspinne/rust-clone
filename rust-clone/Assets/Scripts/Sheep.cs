using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour
{
    [SerializeField] private GameObject destination;
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private bool isAngekommen;

    public void Start()
    {
        agent.SetDestination(destination.transform.position);

        StartCoroutine(Test());
    }

    private void Update()
    {
        if (!isAngekommen)
        {
            if (!agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                    {
                        isAngekommen = true;
                    }
                }
            }
        }
    }

    public IEnumerator Test()
    {
        while (true)
        {
            Debug.Log("Hallo");

            GameObject[] grass = GameObject.FindGameObjectsWithTag("Grass");

            foreach (GameObject grassObject in grass)
            {
                Debug.Log(grassObject.name);
            }

            yield return new WaitForSeconds(Random.Range(2f, 5f));
        }
    }

    bool IsReachable(Vector3 targetPosition)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);

        return path.status == NavMeshPathStatus.PathComplete;
    }
}
