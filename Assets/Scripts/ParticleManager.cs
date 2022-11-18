using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    // 프리팹
    [SerializeField] GameObject[] particleDeath;
    [SerializeField] GameObject particleBump;
    [SerializeField] GameObject particleOccupy;
    [SerializeField] GameObject particleCanOcc;

    // 메모리
    private MemoryPool memoryPoolDeathParticleBlue;
    private MemoryPool memoryPoolDeathParticleOrange;
    private MemoryPool memoryPoolBumpParticle;
    private MemoryPool memoryPoolOccupyParticle;
    private MemoryPool memoryPoolCanOcc;

    private Vector3 temp;

    private void Awake()
    {
        memoryPoolDeathParticleBlue = new MemoryPool(particleDeath[0]);
        memoryPoolDeathParticleOrange = new MemoryPool(particleDeath[1]);
        memoryPoolBumpParticle = new MemoryPool(particleBump);
        memoryPoolOccupyParticle = new MemoryPool(particleOccupy);
        memoryPoolCanOcc = new MemoryPool(particleCanOcc);
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

    public void ActiveOccupyParticle(Vector3 pos)
    {
        GameObject clone;

        clone = memoryPoolOccupyParticle.ActivePoolItem();
        clone.transform.position = pos;
        clone.GetComponent<ParticleController>().Setup(memoryPoolOccupyParticle);
    }

    public ParticleController ActiveCanOccParticle(Vector3 pos)
    {
        GameObject clone;

        clone = memoryPoolCanOcc.ActivePoolItem();
        clone.transform.position = pos;
        ParticleController value = clone.GetComponent<ParticleController>();
        value.Setup(memoryPoolCanOcc);

        return value;
    }
    public void DeactiveCanOccParticle(ParticleController target)
    {
        memoryPoolCanOcc.DeactivePoolItem(target.gameObject);
    }
}
