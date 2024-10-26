using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class AnimalSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject sheepPrefab;
    [SerializeField] private Transform spawnPoint;

    private void Update()
    {
        if (IsOwner && Input.GetKeyDown(KeyCode.E))
        {
            SpawnSheepServerRpc();
        }
    }

    [ServerRpc]
    private void SpawnSheepServerRpc()
    {
        if (sheepPrefab != null)
        {
            GameObject newSheep = Instantiate(sheepPrefab, spawnPoint.position, Quaternion.identity);

            newSheep.GetComponent<NetworkObject>().Spawn();
        }
    }
}
