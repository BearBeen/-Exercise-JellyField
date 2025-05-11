using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UpgradeCardUI : MonoBehaviour
{
    [SerializeField] Image _cardIcon;
    [SerializeField] Image _theme;
    [SerializeField] TextMeshProUGUI _positiveEffect;
    [SerializeField] TextMeshProUGUI _negativeEffect;
    [SerializeField] TextMeshProUGUI _des;
    [SerializeField] Button _button;

    public void Clear()
    {
        _button.onClick.RemoveAllListeners();
    }

    public void SetupOnUpgrade(UnityAction onUpgradeSelect)
    {
        _button.onClick.AddListener(onUpgradeSelect);
    }

    public void SetupUpgrade(UnityAction upgrade)
    {
        _button.onClick.AddListener(upgrade);
    }

    public void SetupImage(Sprite cardIcon, Sprite theme)
    {
        _cardIcon.sprite = cardIcon ?? _cardIcon.sprite;
        _theme.sprite = theme ?? _theme.sprite;
    }

    public void SetupColor(Color? cardColor, Color? themeColor)
    {
        _cardIcon.color = cardColor ?? _cardIcon.color;
        _theme.color = themeColor ?? _theme.color;
    }

    public void SetupText(string pos, string neg, string des)
    {
        _positiveEffect.SetText(pos);
        _negativeEffect.SetText(neg);
        _des.SetText(des);
    }
}
