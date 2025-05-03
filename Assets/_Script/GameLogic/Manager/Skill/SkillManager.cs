using System;
using System.Collections.Generic;

using UnityEngine;

public class SkillManager : MonoSingleton<SkillManager>
{
    //jelly dead can trigger a skill:
    // - fire work to destroy other jelly with the same color ? (DONE. need work on config/generator)
    // - slash an array of jelly ? (WIP)
    // - bag bang boom bla bla ?
    // - double the coin gain in this round ?
    //how about rpg mode? use jelly to combat ?
    // die jelly deal dmg/heal/add attribute/reduce enemy attribute/add shield/remove ememy shield,....
    //so skill need to be flexible. we play the effect here and open out a callback to trigger anything we want?
    //nah. let the skill be seting-up-able. normal skill will set which jelly it want to destroy or with box/row/column..
    //rpg skill will set which effect and how strong these effect can be.
    public struct SkillExcuteData
    {
        public Action Excute;
        public Action ClearExcute;
        public Func<bool> IsExcuteCompleted;
    }

    [SerializeField] private List<AbsSkillData> _originalSkillDatas = new List<AbsSkillData>();
    private Dictionary<Type, AbsSkillData> _skillDataDic = new Dictionary<Type, AbsSkillData>();
    private List<AbsSkillData> _skillDatas = new List<AbsSkillData>();

    public override void Init()
    {
        base.Init();
        _skillDataDic.Clear();
        for (int i = 0, length = _originalSkillDatas.Count; i < length; i++)
        {
            AbsSkillData clone = Instantiate(_originalSkillDatas[i]);
            _skillDataDic.Add(_skillDatas[i].GetType(), clone);
            _skillDatas.Add(clone);
        }
    }

    public T2 GetSkillInstance<T1, T2>()
        where T1 : AbsSkillData
        where T2 : AbsSkillInstance<T1, T2>, new()
    {
        if (_skillDataDic.TryGetValue(typeof(T1), out AbsSkillData absSkillData))
        {
            return absSkillData.CreateSkillInstance() as T2;
        }
        else
        {
            DebugManager.Instance.LogError("Skill data type " + typeof(T1) + " is not supported.");
            return null;
        }
    }

    public AbsSkillInstance GetRandomSkillInstance()
    {
        return _skillDatas.GetRandom().CreateSkillInstance();
    }

    private List<SkillExcuteData> _skillExcuteDatas = new List<SkillExcuteData>();

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