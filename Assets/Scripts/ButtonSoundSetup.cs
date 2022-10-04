using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonSoundSetup : MonoBehaviour
{
    [SerializeField]
    private string soundName = "";

    private void Awake()
    {
        Button button = null;

        bool isButton = TryGetComponent<Button>(out button);

        if(isButton)
        {
            button.onClick.AddListener(() => SoundController.Instance.EffectSoundOn(soundName));
        }
        else
        {
            TMP_Dropdown dropdown = null;

            bool isDropdown = TryGetComponent<TMP_Dropdown>(out dropdown);

            if (isDropdown)
            {
                dropdown.onValueChanged.AddListener((i) => SoundController.Instance.EffectSoundOn(soundName));
            }
            else
            {
                TMP_InputField inputField = null;

                bool isInputField = TryGetComponent<TMP_InputField>(out inputField);

                if (isInputField)
                {
                    inputField.onSelect.AddListener((str) => SoundController.Instance.EffectSoundOn(soundName));
                }
            }
        }
    }
}
