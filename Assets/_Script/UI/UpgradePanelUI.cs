using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradePanelUI : AbsPanel
{
    protected override void Init()
    {


        GameEventManager<UpgradeEventBinder, UpgradeEvent, UpgradeEventData>.AddEventListener(UpgradeEvent.ShowUpgradePanelUI, OnEventShowUpgradePanelUI);
    }

    protected override void Clear()
    {
        GameEventManager<UpgradeEventBinder, UpgradeEvent, UpgradeEventData>.RemoveEventListener(UpgradeEvent.ShowUpgradePanelUI, OnEventShowUpgradePanelUI);
    }

    private void OnEventShowUpgradePanelUI(UpgradeEventData upgradeEventData)
    {
        this.Show();
    }
}

public class UpgradeCardUI:MonoBehaviour
{
    [SerializeField] Image _cardIcon;
    [SerializeField] Image _theme;
    [SerializeField] TextMeshProUGUI _positiveEffect;
    [SerializeField] TextMeshProUGUI _negativeEffect;
    [SerializeField] TextMeshProUGUI _des;

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
