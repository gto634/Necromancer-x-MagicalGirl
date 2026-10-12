using UnityEngine;
using UnityEngine.Serialization;

public class SelectionButtonFeedback : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Button _button;
    [SerializeField, FormerlySerializedAs("_menuNormal")] Sprite _normal;
    [SerializeField, FormerlySerializedAs("_menuHover")] Sprite _hover;
    [SerializeField, FormerlySerializedAs("_menuPressed")] Sprite _pressed;

    void OnEnable()
    {
        if (_button == null || _normal == null) return;

        _button.image.sprite = _normal;
        _button.spriteState = new UnityEngine.UI.SpriteState
        {
            highlightedSprite = _hover,
            selectedSprite = _hover,
            pressedSprite = _pressed,
            disabledSprite = _normal
        };
        _button.image.overrideSprite = null;
    }
}
