using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradePanelUI : AbsPanel
{
    [SerializeField] private UpgradeCardUI _cardTemplate;
    [SerializeField] private Transform _activatedTrs;
    [SerializeField] private Transform _reservedTrs;

    private List<UpgradeCardUI> _activatedCards = new List<UpgradeCardUI>();
    private List<UpgradeCardUI> _resevedCards = new List<UpgradeCardUI>();

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
        while(upgradeEventData.upgradeCardDatas.Count != _activatedCards.Count)
        {
            if (upgradeEventData.upgradeCardDatas.Count > _activatedCards.Count)
            {
                GetCardUI();
            }
            else
            {
                ReserveCardUI(_activatedCards[_activatedCards.Count - 1]);
            }
        }

        for(int i = 0; i < _activatedCards.Count; i++)
        {
            UpgradeCardUI upgradeCardUI = _activatedCards[i];
            upgradeCardUI.Clear();
            upgradeCardUI.SetupOnUpgrade(OnUpgradeSelect);
            upgradeEventData.upgradeCardDatas[i].DrawCardUI(_activatedCards[i]);
        }
        
        Show();
    }

    private void OnUpgradeSelect()
    {
        Hide();
    }

    private void GetCardUI()
    {
        UpgradeCardUI upgradeCardUI;
        if (_resevedCards.Count > 0)
        {
            upgradeCardUI = _resevedCards[_resevedCards.Count -1];
            _resevedCards.RemoveAt(_resevedCards.Count -1);
            upgradeCardUI.transform.SetParent(_activatedTrs, true);
        }
        else
        {
            upgradeCardUI = Instantiate(_cardTemplate, _activatedTrs);
        }
        _activatedCards.Add(upgradeCardUI);
    }

    private void ReserveCardUI(UpgradeCardUI upgradeCardUI)
    {
        _activatedCards.Remove(upgradeCardUI);
        upgradeCardUI.transform.SetParent(_reservedTrs, false);
        _resevedCards.Add(upgradeCardUI);
    }
}
