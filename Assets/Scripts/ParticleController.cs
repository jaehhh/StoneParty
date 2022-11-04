using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    // 참조
    private MemoryPool memoryPool;
    // 내 컴포넌트 참조
    private ParticleSystem[] particles;
    private AudioSource audioSource;

    // 수치값 설정
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
        // Setup()보다 OnEnable()이 먼저 실행되기 때문에 순차접근 셋업..
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
