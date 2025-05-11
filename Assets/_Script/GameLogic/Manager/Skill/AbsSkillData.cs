using System;
using System.Collections.Generic;
using SerializeReferenceEditor;
using UnityEngine;

public abstract class AbsSkillData : ScriptableObject
{
    [SerializeField] private bool _isUnlocked = false;
    [SerializeReference, SR] protected List<ISkillUpgrade> _upgrades = new List<ISkillUpgrade>();
    public bool isUnlocked => _isUnlocked;
    public abstract int skillID { get; }
    public abstract AbsSkillInstance CreateSkillInstance();
    public abstract void Upgrade(ISkillUpgrade skillUpgrade);
    protected abstract void InternalUpgrade(SkillUpgradeType skillUpgradeType, ISkillUpgrade skillUpgrade);
    public abstract bool IsUpgradable(ISkillUpgrade skillUpgrade);
    protected abstract bool InternalIsUpgradable(SkillUpgradeType skillUpgradeType, ISkillUpgrade skillUpgrade);
    public abstract List<ISkillUpgrade> GetAllUpgradable(Predicate<ISkillUpgrade> predicate);
    public void Unlock()
    {
        _isUnlocked = true;
    }
}

public abstract class AbsSkillInstance
{
    protected abstract bool IsExcuteCompleted();

    public abstract void Init(Jelly caster, int targetJellyIndex);

    protected abstract void InitExcute(Jelly caster);

    protected abstract void Excute();

    protected abstract void ClearExcute();

    public void Cast(Jelly caster)
    {
        InitExcute(caster);
        SkillManager.Instance.ExcuteSKill(new SkillManager.SkillExcuteData
        {
            Excute = Excute,
            IsExcuteCompleted = IsExcuteCompleted,
            ClearExcute = ClearExcute,
        });
    }
}

public abstract class AbsSkillData<T1, T2> : AbsSkillData
    where T1 : AbsSkillData
    where T2 : AbsSkillInstance<T1, T2>, new()
{
    public override AbsSkillInstance CreateSkillInstance()
    {
        T2 result = new T2();
        result.SetData(this as T1);
        return result;
    }

    public override void Upgrade(ISkillUpgrade skillUpgrade)
    {
        if (skillUpgrade.upgradeType == SkillUpgradeType.ComposeUpgrade)
        {
            CompositeUpgrade(skillUpgrade);
        }
        else
        {
            InternalUpgrade(skillUpgrade.upgradeType, skillUpgrade);
        }
    }

    private void CompositeUpgrade(ISkillUpgrade skillUpgrade)
    {
        for (int i = 0; skillUpgrade.upgradeTypes != null && i < skillUpgrade.upgradeTypes.Length; i++)
        {
            if (skillUpgrade.upgradeTypes[i] == SkillUpgradeType.ComposeUpgrade)
            {
                DebugManager.Instance.LogError("Nested ComposeUpgrade type !!!");
                continue;
            } 
            InternalUpgrade(skillUpgrade.upgradeTypes[i], skillUpgrade);
        }
    }

    public override bool IsUpgradable(ISkillUpgrade skillUpgrade)
    {
        if (skillUpgrade.upgradeType == SkillUpgradeType.ComposeUpgrade)
        {
            return IsCompositeUpgradable(skillUpgrade);
        }
        else
        {
            return InternalIsUpgradable(skillUpgrade.upgradeType, skillUpgrade);
        }
    }

    private bool IsCompositeUpgradable(ISkillUpgrade skillUpgrade)
    {
        for (int i = 0; skillUpgrade.upgradeTypes != null && i < skillUpgrade.upgradeTypes.Length; i++)
        {
            if (skillUpgrade.upgradeTypes[i] == SkillUpgradeType.ComposeUpgrade)
            {
                DebugManager.Instance.LogError("Nested ComposeUpgrade type !!!");
                return false;
            } 
            if (!InternalIsUpgradable(skillUpgrade.upgradeTypes[i], skillUpgrade)) 
            {
                return false;
            }
        }
        return true;
    }

    public override List<ISkillUpgrade> GetAllUpgradable(Predicate<ISkillUpgrade> predicate)
    {
        List<ISkillUpgrade> result = new List<ISkillUpgrade>();
        if (!isUnlocked) return result;
        for(int i = 0; i < _upgrades.Count; i++)
        {
            ISkillUpgrade upgrade = _upgrades[i];
            if (IsUpgradable(upgrade) && predicate(upgrade))
            {
                result.Add(upgrade);
            }
        }
        return result;
    }

    #region Common function
    protected bool IsRangeUpgradable(float range, ISkillUpgrade upgrade) 
    => upgrade is IRangeUpgrade rangeUpgrade && rangeUpgrade.rangeMax > range;

    protected void UpgradeRange(IRangeUpgrade rangeUpgrade, ref float rangePer, ref float rangeAdd) 
    {
        rangePer += rangeUpgrade.rangePer;
        rangeAdd += rangeUpgrade.rangeAdd;
    }

    protected bool IsMissileCountUpgradable(float missileCount, ISkillUpgrade upgrade)
    => upgrade is IMissileCountUpgrade missileCountUpgrade && missileCountUpgrade.missileMax > missileCount;

    protected void UpgradeMissileCount(IMissileCountUpgrade missileCountUpgrade, ref float missilePer, ref float missileAdd)
    {
        missilePer += missileCountUpgrade.missilePer;
        missileAdd += missileCountUpgrade.missileAdd;
    }

    protected bool IsJellyIndexUpgradable(bool isKillAllColor, ISkillUpgrade upgrade)
    => upgrade is ITargetJellyIndexUpgrade && !isKillAllColor;

    protected void UpgradeTargetJellyIndex(ITargetJellyIndexUpgrade targetJellyIndexUpgrade, ref bool isKillAllColor)
    {
        isKillAllColor = true;
    }
    #endregion
}

public abstract class AbsSkillInstance<T1, T2> : AbsSkillInstance
    where T1 : AbsSkillData
    where T2 : AbsSkillInstance<T1, T2>, new()
{
    protected T1 _skillData;
    public void SetData(T1 skillData)
    {
        _skillData = skillData;
    }
}