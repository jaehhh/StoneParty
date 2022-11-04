using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    // ����
    private MemoryPool memoryPool;
    // �� ������Ʈ ����
    private ParticleSystem[] particles;
    private AudioSource audioSource;

    // ��ġ�� ����
    [SerializeField]
    private float deactiveTime = 5f;
    [SerializeField]
    private AudioClip[] clips;
    private float volume;
    [SerializeField]
    private float volumeMultiply = 1f;

    public void Setup(MemoryPool memoryPool)
    {
        this.memoryPool = memoryPool;
        
    }

    private void OnEnable()
    {
        // Setup()���� OnEnable()�� ���� ����Ǳ� ������ �������� �¾�..
        if(particles == null)
        {
            particles = GetComponentsInChildren<ParticleSystem>();

            audioSource = GetComponent<AudioSource>();
            volume = UserData.instance.EffectVolume * volumeMultiply;
        } 

        for (int i = 0; i < particles.Length; ++i)
        {
            particles[i].Play();
        }

        audioSource.volume = volume;
        audioSource.clip = clips[Random.Range(0, clips.Length)];
        audioSource.Play();

        StartCoroutine("AutoDeactive");
    }

    private IEnumerator AutoDeactive()
    {
        yield return new WaitForSeconds(deactiveTime);

        for(int i =0; i< particles.Length; ++i)
        {
            particles[i].Stop();
        }

        memoryPool.DeactivePoolItem(this.gameObject);
    }
}
