using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraNetwork : NetworkBehaviour
{
    public Transform player;
    public GameObject cameraObject;

    private void Start()
    {
        if(IsOwner)
        {
            cameraObject.SetActive(true);
        }
    }

    void Update()
    {
        transform.position = player.transform.position;
    }
}
