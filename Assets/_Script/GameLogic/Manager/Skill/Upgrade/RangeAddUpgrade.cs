using System;
using UnityEngine;

[Serializable]
public struct RangeAddUpgrade: IRangeUpgrade
{
    [SerializeField] private int _add;

    public int priority => 0;

    public UpgradeType upgradeType => UpgradeType.RangeUpgrade;

    public UpgradeType[] upgradeTypes => null;

    public int UpgradeRange(int range)
    {
        return range + _add;
    }
}
