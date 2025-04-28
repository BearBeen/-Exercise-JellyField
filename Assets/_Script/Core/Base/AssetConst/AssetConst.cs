using UnityEngine;

public static partial class AssetConst
{
    #region FOLDER NAME
    public static readonly string AssetRoot = "Assets";
    public static readonly string BuiltInAssetRoot = "Resources";
    public static readonly string ResRoot = "GameResource";
    public static readonly string LuaRoot = "Lua";
    public static readonly string ModRoot = "Mod";
    public static readonly string ConfigRoot = "Config";
    public static readonly string ManagerConfigRoot = "ManagerConfig";
    public static readonly string AssetBundleRoot = "AssetBundle";
    public static readonly string AssetBundleTempRoot = "AssetBundleTemp";
    #endregion
    #region FILE EXTENSION
    public static readonly string LuaSciptExt = ".lua";
    public static readonly string ConfigExt = ".asset";
    public static readonly string ForwardSlash = "/";//unity prefer this slash
    public static readonly string BackSlash = "\\";//window's file system slash
    public static readonly string AssetBundleExtensionSeparator = "_";
    #endregion
    #region PATH
    public static readonly string ResRootPath = CreatPath(new string[] { AssetRoot, ResRoot });
    public static readonly string ModRootPath = CreatPath(new string[] { Application.persistentDataPath, ModRoot });
    public static readonly string ModLuaRootPath = CreatPath(new string[] { ModRootPath, LuaRoot });
    public static readonly string AssetBundleLuaRootPath = CreatPath(new string[] { AssetRoot, LuaRoot });
    public static readonly string ManagerConfigRootPath = CreatPath(new string[] { AssetRoot, BuiltInAssetRoot, ManagerConfigRoot });
    public static readonly string ConfigRootPath = CreatPath(new string[] { AssetRoot, ConfigRoot });

    public static readonly string AssetBundleRootPath
#if UNITY_EDITOR
    = EditorConfigManager.Instance.AssetBundlePath;
#else
    = Application.persistentDataPath;
#endif
    #endregion
}
