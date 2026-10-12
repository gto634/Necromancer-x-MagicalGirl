using ALTUSHKA.EasyNodeDialogSystem;
using UnityEngine;

public class DialogFeedback : MonoBehaviour
{
    [SerializeField] DialogRunner _dialogRunner;
    [SerializeField] CanvasGroup _dialogGroup;
    [SerializeField] UnityEngine.UI.Image _panel;
    [SerializeField] UnityEngine.UI.Image _nameplate;
    [SerializeField] UnityEngine.UI.Image _portraitFrame;
    [SerializeField] UnityEngine.UI.Image _portrait;

    [SerializeField] string _necromancerKey = "Necromancian";
    [SerializeField] Sprite _necromancerPanel;
    [SerializeField] Sprite _necromancerNameplate;
    [SerializeField] Sprite _necromancerFrame;
    [SerializeField] Sprite _necromancerPortrait;

    [SerializeField] string _magicalGirlKey = "MagicalGirl";
    [SerializeField] Sprite _magicalGirlPanel;
    [SerializeField] Sprite _magicalGirlNameplate;
    [SerializeField] Sprite _magicalGirlFrame;
    [SerializeField] Sprite _magicalGirlPortrait;

    [SerializeField] Sprite _defaultPanel;
    [SerializeField] Sprite _defaultNameplate;
    [SerializeField] Sprite _defaultFrame;

    void OnEnable()
    {
        if (_dialogGroup == null)
        {
            _dialogGroup = _panel.GetComponent<CanvasGroup>();
            if (_dialogGroup == null) _dialogGroup = _panel.gameObject.AddComponent<CanvasGroup>();
            _dialogGroup.alpha = 0f;
        }

        _dialogRunner.DialogStarted += OnDialogStarted;
        _dialogRunner.LineReady += OnLineReady;
    }

    void OnDisable()
    {
        _dialogRunner.DialogStarted -= OnDialogStarted;
        _dialogRunner.LineReady -= OnLineReady;
    }

    void OnDialogStarted(DialogSessionInfo session)
    {
        _dialogGroup.alpha = 0f;
        ApplyStyle(_defaultPanel, _defaultNameplate, _defaultFrame, null);
    }

    void OnLineReady(DialogLinePresentation line)
    {
        if (line.IsInstant && string.IsNullOrEmpty(line.Text)) return;

        if (line.SpeakerKey == _necromancerKey)
        {
            ApplyStyle(_necromancerPanel, _necromancerNameplate, _necromancerFrame, _necromancerPortrait);
        }
        else if (line.SpeakerKey == _magicalGirlKey)
        {
            ApplyStyle(_magicalGirlPanel, _magicalGirlNameplate, _magicalGirlFrame, _magicalGirlPortrait);
        }
        else
        {
            ApplyStyle(_defaultPanel, _defaultNameplate, _defaultFrame, null);
        }

        _dialogGroup.alpha = 1f;
    }

    void ApplyStyle(Sprite panel, Sprite nameplate, Sprite frame, Sprite portrait)
    {
        _panel.sprite = panel;
        _nameplate.sprite = nameplate;
        _portraitFrame.sprite = frame;
        _portrait.sprite = portrait;
        _portraitFrame.gameObject.SetActive(portrait != null);
    }
}
