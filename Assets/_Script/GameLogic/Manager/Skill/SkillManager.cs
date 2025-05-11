using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : MonoSingleton<SkillManager>
{
    public struct SkillExcuteData
    {
        public Action Excute;
        public Action ClearExcute;
        public Func<bool> IsExcuteCompleted;
    }

    [SerializeField] private List<AbsSkillData> _originalSkillDatas = new List<AbsSkillData>();

    private Dictionary<int, AbsSkillData> _skillDataDic = new Dictionary<int, AbsSkillData>();
    private List<SkillExcuteData> _skillExcuteDatas = new List<SkillExcuteData>();

    public override void Init()
    {
        base.Init();
        _skillDataDic.Clear();
        for (int i = 0, length = _originalSkillDatas.Count; i < length; i++)
        {
            AbsSkillData clone = Instantiate(_originalSkillDatas[i]);
            _skillDataDic.Add(clone.skillID, clone);
        }
    }

    public AbsSkillInstance GetRandomSkillInstance()
    {
        AbsSkillData skillData = _skillDataDic.Values.Where((skillData) => skillData.isUnlocked).ToList().GetRandom();
        return skillData != null ? skillData.CreateSkillInstance() : null;
    }

    public string GetSkillName(int skillID)
    {
        return ConfigManager.Instance.GetString("SkillName_" + skillID);
    }

    public void UpgradeSkill(int skillID, ISkillUpgrade skillUpgrade)
    {
        if (_skillDataDic.TryGetValue(skillID, out AbsSkillData skillData) && skillData.isUnlocked)
        {
            skillData.Upgrade(skillUpgrade);
        }
    }

    public void UnlockSkill(int skillID)
    {
        if (_skillDataDic.TryGetValue(skillID, out AbsSkillData skillData))
        {
            skillData.Unlock();
        }
    }

    private void Update()
    {
        for (int i = 0; i < _skillExcuteDatas.Count; i++)
        {
            SkillExcuteData excuteData = _skillExcuteDatas[i];
            if (excuteData.IsExcuteCompleted())
            {
                //remove skill excution data
                _skillExcuteDatas[i] = _skillExcuteDatas[_skillExcuteDatas.Count - 1];
                _skillExcuteDatas.RemoveAt(_skillExcuteDatas.Count - 1);
                i--;
                excuteData.ClearExcute();
            }
            else
            {
                excuteData.Excute();
            }
        }
    }

    public void ExcuteSKill(SkillExcuteData excuteData)
    {
        _skillExcuteDatas.Add(excuteData);
    }
}