using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;

public class SoundController : MonoBehaviourPunCallbacks
{
    public static SoundController Instance;

    public AudioSource effectAudio;
    public AudioSource BGMAudio;

    [SerializeField]
    private AudioClip clickEffectClip;
    [SerializeField]
    private AudioClip switchEffectClip;

    [SerializeField]
    private AudioClip introBGMClip;
    [SerializeField]
    private AudioClip[] lobbyBGMClips;
    [SerializeField]
    private AudioClip[] battleBGMClips;

    private string previousSceneName = "none";

    // UserData UserData.instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        BGMSoundOn();

        SceneManager.sceneLoaded += LoadedSceneEvent;
    }

    private void LoadedSceneEvent(Scene scene, LoadSceneMode mode)
    {
        BGMSoundOn();

        if(scene.name == "Lobby")
        {
            //UserData.instance = Resources.Load("Table/UserData") as UserData;

            effectAudio.volume = UserData.instance.EffectVolume;
            BGMAudio.volume = UserData.instance.BGMVolume * 0.75f;
        }
    }

    public void EffectAudioSetting(Slider slider)
    {
        float value = slider.value;
        effectAudio.volume = value;
        UserData.instance.EffectVolume = value;
    }

    public void BGMAudioSetting(Slider slider)
    {
        float value = slider.value;
        BGMAudio.volume = value *0.75f;
        UserData.instance.BGMVolume = value;
    }

    public void EffectSoundOn(string str = "")
    {
        effectAudio.Stop();

        if (str.ToLower().Contains("switch"))
        {
            effectAudio.clip = switchEffectClip;
        }
        if (str == "" || str.ToLower().Contains("click"))
        {
            effectAudio.clip = clickEffectClip;
        }

        effectAudio.Play();
    }

    public void BGMSoundOn()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName.ToLower().Contains("title")) // 인트로 씬
        {
            if (UserData.instance != null)
            {
                BGMAudio.volume = UserData.instance.BGMVolume;
                effectAudio.volume = UserData.instance.EffectVolume;
            }
            else
            {
                BGMAudio.volume = 0.5f;
                effectAudio.volume = 0.5f;
            }

            BGMAudio.clip = introBGMClip;

            BGMAudio.Play();
        }
        else if (sceneName.ToLower().Contains("lobby")) // 로비 씬
        {
            if(previousSceneName.ToLower().Contains("room")) // 룸에서 로비면 브금 이어서 진행
            {
 
            }
            else
            {
                int index = Random.Range(0, lobbyBGMClips.Length);
                BGMAudio.clip = lobbyBGMClips[index];

                BGMAudio.Play();
            }           
        }
        else if (sceneName.ToLower().Contains("room")) // 대기방 씬
        {
            if (previousSceneName.ToLower().Contains("lobby")) // 룸에서 로비면 브금 이어서 진행
            {

            }
            else
            {
                int index = Random.Range(0, lobbyBGMClips.Length);
                BGMAudio.clip = lobbyBGMClips[index];

                BGMAudio.Play();
            }
        }
        else if(sceneName.ToLower().Contains("linesmash"))
        {
            if(PhotonNetwork.IsMasterClient)
            {
                int index = Random.Range(0, battleBGMClips.Length);

                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "BGMIndex", index } });

                MainGameBGM(index);
            }
            else
            {
                while (true)
                {
                    int temp = (int)PhotonNetwork.CurrentRoom.CustomProperties["BGMIndex"];

                    if(temp != -1)
                    {
                        MainGameBGM(temp);

                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("어떤한 씬의 이름도 적출되지 않았음");
        }
        previousSceneName = SceneManager.GetActiveScene().name;
    }

    private void MainGameBGM(int index)
    {
        BGMAudio.clip = battleBGMClips[index];

        BGMAudio.Play();
    }
}
