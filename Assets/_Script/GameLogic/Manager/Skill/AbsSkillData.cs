using System.Collections.Generic;
using UnityEngine;

public abstract class AbsSkillData : ScriptableObject
{
    public abstract AbsSkillInstance CreateSkillInstance();
    public abstract void Upgrade(List<ISkillUpgrade> upgrades);
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

    public override void Upgrade(List<ISkillUpgrade> upgrades)
    {
        upgrades.Sort((a, b) =>
        {
            return a.priority - b.priority;
        });
        for (int i = 0; i < upgrades.Count; i++)
        {
            ISkillUpgrade upgrade = upgrades[i];
            switch (upgrade.upgradeType)
            {
                case SkillUpgradeType.ComposeUpgrade:
                    CompositeUpgrade(upgrade);
                    break;
                case SkillUpgradeType.RangeUpgrade:
                    UpgradeRange(upgrade as IRangeUpgrade);
                    break;
                case SkillUpgradeType.TargetJellyIndexUpgrade:
                    UpgradeTargetJellyIndex(upgrade as ITargetJellyIndexUpgrade);
                    break;
                default:
                    DebugManager.Instance.LogError("Unsupported upgrade type " + upgrade.upgradeType);
                    break;
            }
        }
    }

    protected virtual void UpgradeRange(IRangeUpgrade rangeUpgrade)
    {
        DebugManager.Instance.LogError($"{this.GetType()} not implement UpgradeRange. Wrongly config?");
    }

    protected virtual void UpgradeTargetJellyIndex(ITargetJellyIndexUpgrade targetJellyIndexUpgrade)
    {
        DebugManager.Instance.LogError($"{this.GetType()} not implement UpgradeTargetJellyIndex. Wrongly config?");
    }

    private void CompositeUpgrade(ISkillUpgrade upgrade)
    {
        for (int i = 0; upgrade.upgradeTypes != null && i < upgrade.upgradeTypes.Length; i++)
        {
            switch (upgrade.upgradeTypes[i])
            {
                case SkillUpgradeType.ComposeUpgrade:
                    DebugManager.Instance.LogError("Wrongly configured. Unknown intend as nested ComposeUpgrade will lead to infinite loop.");
                    break;
                case SkillUpgradeType.RangeUpgrade:
                    UpgradeRange(upgrade as IRangeUpgrade);
                    break;
                case SkillUpgradeType.TargetJellyIndexUpgrade:
                    UpgradeTargetJellyIndex(upgrade as ITargetJellyIndexUpgrade);
                    break;
                default:
                    DebugManager.Instance.LogError("Unsupported upgrade type " + upgrade.upgradeType);
                    break;
            }
        }
    }
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