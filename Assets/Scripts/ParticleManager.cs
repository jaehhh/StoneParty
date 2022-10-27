using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    // 프리팹
    [SerializeField] GameObject[] particleDeath;
    [SerializeField] GameObject particleBump;

    // 메모리
    private MemoryPool memoryPoolDeathParticleBlue;
    private MemoryPool memoryPoolDeathParticleOrange;
    private MemoryPool memoryPoolBumpParticle;

    private void Awake()
    {
        memoryPoolDeathParticleBlue = new MemoryPool(particleDeath[0]);
        memoryPoolDeathParticleOrange = new MemoryPool(particleDeath[1]);
        memoryPoolBumpParticle = new MemoryPool(particleBump);
    }

    public void ActiveDeathParticle(int num, Vector3 pos)
    {
        GameObject clone = null;

        switch (num)
        {
            case 0:
                clone = memoryPoolDeathParticleBlue.ActivePoolItem();
                clone.transform.position = pos;
                clone.GetComponent<ParticleController>().Setup(memoryPoolDeathParticleBlue);
                break;

            case 1:
                clone = memoryPoolDeathParticleOrange.ActivePoolItem();
                clone.transform.position = pos;
                clone.GetComponent<ParticleController>().Setup(memoryPoolDeathParticleOrange);
                break;
        }
    }

    public void ActiveBumpParticle(Vector3 pos)
    {
        GameObject clone;

        clone = memoryPoolBumpParticle.ActivePoolItem();
        clone.transform.position = pos;
        clone.GetComponent<ParticleController>().Setup(memoryPoolBumpParticle);
    }
}
