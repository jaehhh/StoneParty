using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSound : MonoBehaviourPunCallbacks
{
    private AudioSource myAudio;

    public AudioClip[] jumpClips;
    public AudioClip[] dashClips;
    public AudioClip[] hitClips;
    public AudioClip[] happyClips;
    public AudioClip[] sadClips;
    public AudioClip[] deathClips;

    private void Awake()
    {
        myAudio = GetComponent<AudioSource>();
    }

    private void Start()
    {
        myAudio.volume = UserData.instance.EffectVolume;
    }

    public void JumpSound()
    {
        if (myAudio.isPlaying) return;

        int i = Random.Range(0, jumpClips.Length);
        photonView.RPC("RPCJumpSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCJumpSound(int i)
    {
        myAudio.Stop();

        myAudio.clip = jumpClips[i];
        myAudio.Play();
    }

    public void DashSound()
    {
        int i = Random.Range(0, dashClips.Length);
        photonView.RPC("RPCDashSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCDashSound(int i)
    {
        myAudio.Stop();

        myAudio.clip = dashClips[i];
        myAudio.Play();
    }

    public void HitSound()
    {
        int i = Random.Range(0, hitClips.Length);
        photonView.RPC("RPCHitSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCHitSound(int i)
    {
        myAudio.Stop();

        myAudio.clip = hitClips[i];
        myAudio.Play();
    }

    public void HappySound()
    {
        int i = Random.Range(0, happyClips.Length);
        photonView.RPC("RPCHappySound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCHappySound(int i)
    {
        myAudio.Stop();

        myAudio.clip = happyClips[i];
        myAudio.Play();
    }

    public void SadSound()
    {
        int i = Random.Range(0, sadClips.Length);
        photonView.RPC("RPCSadSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCSadSound(int i)
    {
        myAudio.Stop();

        myAudio.clip = sadClips[i];
        myAudio.Play();
    }

    public void DeathSound()
    {
        int i = Random.Range(0, deathClips.Length);
        photonView.RPC("RPCDeathSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCDeathSound(int i)
    {
        myAudio.Stop();

        myAudio.clip = deathClips[i];
        myAudio.Play();
    }
}
