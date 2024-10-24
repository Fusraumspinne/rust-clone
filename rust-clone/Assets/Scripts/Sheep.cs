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

    private enum State { Idle, Walking, Eating, Drinking, Resting }
    private State currentState = State.Idle;

    [SerializeField] private bool isAngekommen;
    [SerializeField] private bool isUnterwegs;
    [SerializeField] private bool Setzen;

    public void Start()
    {
        StartCoroutine(ChooseNewTarget());
    }

    private void Update()
    {
        if (currentState == State.Walking && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                isAngekommen = true;
                currentState = State.Idle;

                if (target != null)
                {
                    if (target.CompareTag("Grass"))
                    {
                        StartCoroutine(Eat());
                    }
                    else if (target.CompareTag("Wasser"))
                    {
                        StartCoroutine(Drink());
                    }
                }
                else if (Setzen)
                {
                    StartCoroutine(Rest());
                }
            }
        }
    }

    private IEnumerator Eat()
    {
        currentState = State.Eating;
        animator.Play("Eating");
        Debug.Log("Schaf isst.");
        yield return new WaitForSeconds(7); 
        Destroy(target); 
        currentState = State.Idle;
    }

    private IEnumerator Drink()
    {
        currentState = State.Drinking;
        animator.Play("Eating");
        Debug.Log("Schaf trinkt.");
        yield return new WaitForSeconds(Random.Range(4, 10)); // Simuliere Trinkdauer
        currentState = State.Idle;
    }

    private IEnumerator Rest()
    {
        currentState = State.Resting;
        animator.Play("setzen");
        Debug.Log("Schaf setzt sich aus.");
        yield return new WaitForSeconds(Random.Range(4, 30)); // Simuliere Ruhezeit ///////////////////////////
        animator.Play("aufstehen");
        yield return new WaitForSeconds(2);
        Setzen = false;
        currentState = State.Idle;
    }

    public IEnumerator ChooseNewTarget()
    {
        while (true)
        {
            if (currentState == State.Idle)
            {
                GameObject[] grass = GameObject.FindGameObjectsWithTag("Grass");
                GameObject[] wasser = GameObject.FindGameObjectsWithTag("Wasser");

                isAngekommen = false;
                float randomZahl = Random.Range(0, 30);

                if (randomZahl < 5)
                {
                    GameObject randomWasser = wasser[Random.Range(0, wasser.Length)];
                    agent.SetDestination(randomWasser.transform.position);
                    animator.Play("Walking");
                    target = randomWasser;
                    currentState = State.Walking;
                    Debug.Log("Schaf geht zum Wasser.");
                }
                else if (randomZahl >= 5 && randomZahl <= 10)
                {
                    GameObject randomGrass = grass[Random.Range(0, grass.Length)];
                    agent.SetDestination(randomGrass.transform.position);
                    animator.Play("Walking");
                    target = randomGrass;
                    currentState = State.Walking;
                    Debug.Log("Schaf geht zum Gras.");
                }
                else
                {
                    Vector3 randomPoint = GetRandomPointAround(transform.position, 20f);
                    agent.SetDestination(randomPoint);
                    animator.Play("Walking");
                    currentState = State.Walking;
                    Setzen = true;
                    Debug.Log("Schaf sucht sich einen Ruheplatz.");
                }
            }

            yield return new WaitForSeconds(1);
        }
    }

    private Vector3 GetRandomPointAround(Vector3 center, float range)
    {
        Vector3 randomDirection = Random.insideUnitSphere * range;
        randomDirection += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center;
    }
}

