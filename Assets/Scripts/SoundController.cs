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

        if(UserData.instance != null)
        {
            effectAudio.volume = UserData.instance.EffectVolume;
            BGMAudio.volume = UserData.instance.BGMVolume * 0.75f; // 실제 사운드는 조금 작게. 파일 소리가 큼
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
        BGMAudio.volume = value *0.75f; // 실제 사운드는 조금 작게. 파일 소리가 큼
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
                effectAudio.volume = UserData.instance.EffectVolume;
                BGMAudio.volume = UserData.instance.BGMVolume * 0.75f;
            }
            else
            {
                effectAudio.volume = 0.5f;
                BGMAudio.volume = 0.5f * 0.75f;
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
            Debug.LogWarning("SoundController 커스텀프로퍼티 메인브금 인덱스 설정 및 참고 시작");

            if (PhotonNetwork.IsMasterClient)
            {
                int index = Random.Range(0, battleBGMClips.Length);

                PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable() { { "BGMIndex", index } });

                MainGameBGM(index);
            }
            else
            {
                BGMFind();
            }
        }
        else
        {
            Debug.LogWarning("어떤한 씬의 이름도 적출되지 않았음");
        }
        previousSceneName = SceneManager.GetActiveScene().name;
    }

    private void BGMFind()
    {
        int temp = (int)PhotonNetwork.CurrentRoom.CustomProperties["BGMIndex"];

        if (temp == -1)
        {
            Debug.Log("커스텀프로퍼티 브금 인덱스 검출 실패. 사유 : 방장이 방 커스텀프로퍼티 세팅 늦게 함");

            Invoke("BGMFind", 0.5f);
        }
        else
        {
            MainGameBGM(temp);
        }  
    }

    private void MainGameBGM(int index)
    {
        BGMAudio.clip = battleBGMClips[index];

        BGMAudio.Play();
    }
}
