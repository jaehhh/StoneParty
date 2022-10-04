using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDeath : MonoBehaviour
{
    private float destroyTime = 5f;

    private void Awake()
    {
        StartCoroutine("AutoDestroy", destroyTime);
    }

    private IEnumerator AutoDestroy(float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(this.gameObject);
    }
}
