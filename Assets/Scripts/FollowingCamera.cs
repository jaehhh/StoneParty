using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    // 쫒아갈 대상
    public GameObject target;

    private float zDistance = 10;
    private float yDistance = 1.5f;
    private float followingXSpeed = 7;
    private float followingYSpeed = 4;

    private Vector3 tp; // target position
    private Vector3 myp; // my(camera) position

    private void Awake()
    {
        myp = transform.position;
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        tp = target.transform.position;

        myp.x = Mathf.Lerp(myp.x, tp.x, followingXSpeed * Time.deltaTime);
        myp.y = Mathf.Lerp(myp.y, tp.y, followingYSpeed * Time.deltaTime);

        transform.position = new Vector3(myp.x, (myp.y + yDistance), (tp.z - zDistance));
    }
}
