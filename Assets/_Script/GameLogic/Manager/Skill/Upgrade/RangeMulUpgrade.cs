using System;
using UnityEngine;

[Serializable]
public struct RangeMulUpgrade : IRangeUpgrade
{
    [SerializeField] private float _mul;
    public int priority => 10;

    public UpgradeType upgradeType => UpgradeType.RangeUpgrade;

    public UpgradeType[] upgradeTypes => null;

    public int UpgradeRange(int range)
    {
        return Mathf.FloorToInt(range * _mul);
    }
}
