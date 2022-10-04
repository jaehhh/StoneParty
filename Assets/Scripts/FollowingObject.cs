using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingObject : MonoBehaviour
{
    [SerializeField]
    private Transform targetTP;
    [SerializeField]
    private Vector3 addVector;

    private void Update()
    {
        this.transform.position = targetTP.position + addVector;
    }
}
