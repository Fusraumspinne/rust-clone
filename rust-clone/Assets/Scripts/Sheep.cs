using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour
{
    [SerializeField] private GameObject destination;
    [SerializeField] private NavMeshAgent agent;

    [SerializeField] private Animator animator;

    [SerializeField] private GameObject target;

    [SerializeField] private bool isAngekommen;
    [SerializeField] private bool isUnterwegs;

    public void Start()
    {
        agent.SetDestination(destination.transform.position);

        StartCoroutine(ChooseNewTarget());
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
                        isUnterwegs = false;

                        if (target != null)
                        {
                            Destroy(target);
                        }

                        animator.Play("Eating");
                    }
                }
            }
        }
    }

    public IEnumerator ChooseNewTarget()
    {
        while (true)
        {
            GameObject[] grass = GameObject.FindGameObjectsWithTag("Grass");
            GameObject[] wasser = GameObject.FindGameObjectsWithTag("Wasser");


            if (!isUnterwegs)
            {
                isUnterwegs = true;
                isAngekommen = false;
                
                GameObject randomGrass = grass[Random.Range(0, grass.Length)];
                GameObject randomWasser = wasser[Random.Range(0, wasser.Length)];

                float randomZahl = Random.Range(0, 10);

                if (randomZahl < 1)
                {
                    agent.SetDestination(randomWasser.transform.position);
                    animator.Play("Walking");

                    Debug.Log("Wasser");
                }

                if (randomZahl >= 1)
                {
                    agent.SetDestination(randomGrass.transform.position);
                    animator.Play("Walking");

                    target = randomGrass;

                    Debug.Log("Grass");
                }

            }

            yield return new WaitForSeconds(Random.Range(2f, 5f));
        }
    }

}


