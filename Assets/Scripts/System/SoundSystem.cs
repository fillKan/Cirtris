using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSystem : MonoBehaviour
{
    [SerializeField] private bool _IsEnableSound;

    [Header("Sound Button Property")]
    [SerializeField] private Sprite _SoundOnSprite;
    [SerializeField] private Sprite _SoundOffSprite;
    [SerializeField] private Image _SoundButtonImage;

    public void SetActiveSound()
    {
        _IsEnableSound = !_IsEnableSound;

        ButtonSpriteUpdate();
    }
    public void SetActiveSound(bool isActive)
    {
        _IsEnableSound = isActive;

        ButtonSpriteUpdate();
    }
    private void ButtonSpriteUpdate()
    {
        _SoundButtonImage.sprite =
                _IsEnableSound ? _SoundOnSprite : _SoundOffSprite;
    }
}
