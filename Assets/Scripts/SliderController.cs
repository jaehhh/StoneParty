using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderController : MonoBehaviour
{
    [SerializeField]
    private string sliderName;

    private void Start()
    {
        AudioSource audioSoruce = null;
        Slider slider = GetComponent<Slider>();

        if (sliderName.ToLower().Contains("effectsound"))
        {
            SoundController.Instance.effectAudio.TryGetComponent<AudioSource>(out audioSoruce);
            slider.onValueChanged.AddListener((str) => SoundController.Instance.EffectAudioSetting(slider));
        }
        else if (sliderName.ToLower().Contains("bgmsound"))
        {
            SoundController.Instance.BGMAudio.TryGetComponent<AudioSource>(out audioSoruce);
            slider.onValueChanged.AddListener((str) => SoundController.Instance.BGMAudioSetting(slider));
        }

        // ����� �÷��̾� �ɼǼ������� ���� �����̴� ����� �¾�
        if (audioSoruce != null)
        {
             this.GetComponent<Slider>().value = audioSoruce.volume;
        } 
    }
}
