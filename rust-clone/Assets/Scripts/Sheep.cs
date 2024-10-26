using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : NetworkBehaviour
{
    [SerializeField] private GameObject destination;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject target;
    [SerializeField] private GameObject head;

    private enum State { Idle, Walking, Eating, Drinking, Resting, Running ,Dead }
    private NetworkVariable<State> currentState = new NetworkVariable<State>(State.Idle);

    [SerializeField] private bool isAngekommen;
    [SerializeField] private bool isUnterwegs;
    [SerializeField] private bool Setzen;

    public int maxHealth = 100;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>();

    public healthbarscript HealthBar;
    public GameObject healthBar;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            currentHealth.Value = maxHealth;
            StartCoroutine(ChooseNewTarget());
        }

        HealthBar.SetMaxHealth(maxHealth);
        currentHealth.OnValueChanged += (oldValue, newValue) => HealthBar.SetHealth(newValue);
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (DetectPlayerInFront())
        {
            StartCoroutine(Rennen());
        }

        if (currentState.Value == State.Walking && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                isAngekommen = true;
                currentState.Value = State.Idle;

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

        if (currentHealth.Value <= 0)
        {
            currentState.Value = State.Dead;

            agent.enabled = false;

            DisableHealthBarClientRpc();
        }

        SyncAnimation();
    }

    private bool DetectPlayerInFront()
    {
        if (currentState.Value == State.Running || currentState.Value == State.Dead) return false;

        int numRaysHorizontal = 10;
        int numRaysVertical = 10;
        float horizontalAngleStep = 90f / (numRaysHorizontal - 1);
        float verticalAngleStep = 90f / (numRaysVertical - 1);
        float rayLength = 10f;

        for (int i = 0; i < numRaysHorizontal; i++)
        {
            for (int j = 0; j < numRaysVertical; j++)
            {
                float horizontalAngle = -45f + i * horizontalAngleStep;
                float verticalAngle = -45f + j * verticalAngleStep;

                Vector3 direction = head.transform.rotation * Quaternion.Euler(verticalAngle, horizontalAngle, 0) * Vector3.forward;

                if (Physics.Raycast(head.transform.position, direction, out RaycastHit hit, rayLength))
                {
                    if (hit.collider.CompareTag("Player"))
                    {
                        Debug.DrawRay(head.transform.position, direction * rayLength, Color.red);
                        return true;
                    }
                    else
                    {
                        Debug.DrawRay(head.transform.position, direction * rayLength, Color.green);
                    }
                }
                else
                {
                    Debug.DrawRay(head.transform.position, direction * rayLength, Color.green);
                }
            }
        }

        return false;
    }

    [ClientRpc]
    private void DisableHealthBarClientRpc()
    {
        healthBar.SetActive(false);
    }

    private void SyncAnimation()
    {
        if (currentState.Value == State.Walking)
        {
            animator.Play("Walking");
        }
        else if (currentState.Value == State.Eating)
        {
            animator.Play("Eating");
        }
        else if (currentState.Value == State.Drinking)
        {
            animator.Play("Eating");
        }
        else if (currentState.Value == State.Resting)
        {
            animator.Play("Setzten");
        }
        else if (currentState.Value == State.Dead)
        {
            animator.Play("Tot");
        }
        else if (currentState.Value == State.Running)
        {
            animator.Play("Rennen");
        }
    }

    private void OnMouseDown()
    {
        if (currentHealth.Value > 0)
        {
            if (IsServer)
            {
                TakeDamage(10);
            }
            else
            {
                TakeDamageServerRpc(10); 
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void TakeDamageServerRpc(int damage)
    {
        TakeDamage(damage); 
    }

    private void TakeDamage(int damage)
    {
        currentHealth.Value -= damage;
    }

    private IEnumerator Eat()
    {
        currentState.Value = State.Eating;
        Debug.Log("Schaf isst.");
        yield return new WaitForSeconds(7);
        currentState.Value = State.Idle;
    }

    private IEnumerator Drink()
    {
        currentState.Value = State.Drinking;
        Debug.Log("Schaf trinkt.");
        yield return new WaitForSeconds(UnityEngine.Random.Range(4, 10));
        currentState.Value = State.Idle;
    }

    private IEnumerator Rest()
    {
        currentState.Value = State.Resting;
        Debug.Log("Schaf setzt sich aus.");
        yield return new WaitForSeconds(UnityEngine.Random.Range(4, 30));
        animator.Play("Aufstehen");
        yield return new WaitForSeconds(2);
        Setzen = false;
        currentState.Value = State.Idle;
    }

    private IEnumerator Rennen()
    {
        currentState.Value = State.Running;
        agent.ResetPath();
        agent.speed = 5f;

        animator.speed = 2f;

        Vector3 exactDistancePoint = GetRandomPointAround(transform.position, 20f);
        agent.SetDestination(exactDistancePoint);

        while (agent.remainingDistance > agent.stoppingDistance)
        {
            yield return null;
        }

        currentState.Value = State.Idle;
        agent.speed = 1.3f;
        animator.speed = 1;
    }

    public IEnumerator ChooseNewTarget()
    {
        while (true)
        {
            if (currentState.Value == State.Idle && currentState.Value != State.Dead && currentState.Value != State.Running)
            {
                GameObject[] grass = GameObject.FindGameObjectsWithTag("Grass");
                GameObject[] wasser = GameObject.FindGameObjectsWithTag("Wasser");

                isAngekommen = false;
                float randomZahl = UnityEngine.Random.Range(0, 30);

                if (randomZahl < 5)
                {
                    GameObject randomWasser = wasser[UnityEngine.Random.Range(0, wasser.Length)];
                    agent.SetDestination(randomWasser.transform.position);
                    target = randomWasser;
                    currentState.Value = State.Walking;
                    Debug.Log("Schaf geht zum Wasser.");
                }
                else if (randomZahl >= 5 && randomZahl <= 10)
                {
                    GameObject randomGrass = grass[UnityEngine.Random.Range(0, grass.Length)];
                    agent.SetDestination(randomGrass.transform.position);
                    target = randomGrass;
                    currentState.Value = State.Walking;
                    Debug.Log("Schaf geht zum Gras.");
                }
                else
                {
                    Vector3 randomPoint = GetRandomPointAround(transform.position, 20f);
                    agent.SetDestination(randomPoint);
                    currentState.Value = State.Walking;
                    Setzen = true;
                    Debug.Log("Schaf sucht sich einen Ruheplatz.");
                }
            }

            yield return new WaitForSeconds(1);
        }
    }

    private Vector3 GetRandomPointAround(Vector3 center, float range)
    {
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * range;
        randomDirection += center;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, range, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return center;
    }
}