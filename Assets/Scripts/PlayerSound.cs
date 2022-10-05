using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerSound : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private AudioSource mouseAudio;
    [SerializeField]
    private AudioSource stoneAudio;

    public AudioClip[] jumpClips;
    public AudioClip[] dashClips;
    public AudioClip[] hitClips;
    public AudioClip[] happyClips;
    public AudioClip[] sadClips;
    public AudioClip[] deathClips;

    private void Start()
    {
        mouseAudio.volume = UserData.instance.EffectVolume;
        stoneAudio.volume = UserData.instance.EffectVolume;
    }

    public void JumpSound()
    {
        if (mouseAudio.isPlaying) return;

        int i = Random.Range(0, jumpClips.Length);
        photonView.RPC("RPCJumpSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCJumpSound(int i)
    {
        mouseAudio.Stop();

        mouseAudio.clip = jumpClips[i];
        mouseAudio.Play();
    }

    public void DashSound()
    {
        int i = Random.Range(0, dashClips.Length);
        photonView.RPC("RPCDashSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCDashSound(int i)
    {
        mouseAudio.Stop();

        mouseAudio.clip = dashClips[i];
        mouseAudio.Play();
    }

    public void HitSound()
    {
        int i = Random.Range(0, hitClips.Length);
        photonView.RPC("RPCHitSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCHitSound(int i)
    {
        mouseAudio.Stop();

        mouseAudio.clip = hitClips[i];
        mouseAudio.Play();
    }

    public void HappySound()
    {
        int i = Random.Range(0, happyClips.Length);
        photonView.RPC("RPCHappySound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCHappySound(int i)
    {
        mouseAudio.Stop();

        mouseAudio.clip = happyClips[i];
        mouseAudio.Play();
    }

    public void SadSound()
    {
        int i = Random.Range(0, sadClips.Length);
        photonView.RPC("RPCSadSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCSadSound(int i)
    {
        mouseAudio.Stop();

        mouseAudio.clip = sadClips[i];
        mouseAudio.Play();
    }

    public void DeathSound()
    {
        int i = Random.Range(0, deathClips.Length);
        photonView.RPC("RPCDeathSound", RpcTarget.All, i);
    }
    [PunRPC]
    private void RPCDeathSound(int i)
    {
        mouseAudio.Stop();

        mouseAudio.clip = deathClips[i];
        mouseAudio.Play();
    }
}
