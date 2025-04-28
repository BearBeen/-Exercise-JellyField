using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

public class ConfigManager : Singleton<ConfigManager>
{
    public static readonly char[] ArraySeparator = { ',' };
    private Dictionary<Type, ScriptableObject> _configDatas = new Dictionary<Type, ScriptableObject>();
    private Dictionary<string, StringLzStructure> _stringDatas = new Dictionary<string, StringLzStructure>();

    public ConfigManager()
    {
        InitConfig();
    }

    public T GetConfig<T>() where T : ScriptableObject
    {
        Type type = typeof(T);
        if (!_configDatas.TryGetValue(type, out ScriptableObject config))
        {
            config = ResourceManager.Instance.GetAssetSync<T>(AssetConst.CreatPath(new string[] { AssetConst.ConfigRootPath, type.Name + AssetConst.ConfigExt }));
            if (config == null)
            {
                return null;
            }
            _configDatas.Add(type, config);
        }
        return Object.Instantiate(config) as T;
    }

    public void InitConfig()
    {
        _stringDatas.Clear();
        StringLz stringLz = GetConfig<StringLz>();
        for (int i = 0; i < stringLz.Data.Count; i++)
        {
            _stringDatas.Add(stringLz.Data[i].Key, stringLz.Data[i]);
        }
    }

    public string GetString(string key)
    {
        if (_stringDatas.TryGetValue(key, out StringLzStructure data))
        {
            return data.Value;
        }
        return string.Empty;
    }
}
