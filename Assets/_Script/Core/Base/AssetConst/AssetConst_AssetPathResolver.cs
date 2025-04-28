using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

public static partial class AssetConst
{
    private static readonly string _normalizedRsFolder = NormalizeToUnityPath(AssetConst.ForwardSlash + AssetConst.BuiltInAssetRoot + AssetConst.ForwardSlash);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string CreatPath(params string[] subpaths)
    {
        return string.Join(ForwardSlash, subpaths);
    }

    private struct PathCache
    {
        private string _assetBundlePath;
        private string _builtInAssetPath;
        public PathCache(string resPath, string assetBundlePath, string builtInAssetPath)
        {
            _assetBundlePath = assetBundlePath;
            _builtInAssetPath = builtInAssetPath;
        }
        public string AssetBundlePath { get { return _assetBundlePath; } }
        public string BuiltInAssetPath { get { return _builtInAssetPath; } }
    }

#if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts(0)]
    private static void OnScriptsReloaded()
    {
        ClearPathCache();
    }
#endif

    private static Dictionary<string, PathCache> _pathCache = new Dictionary<string, PathCache>();

    public static void ClearPathCache()
    {
        _pathCache.Clear();
    }

    public static void ResolveResPath(ref string resPath, out string assetBundlePath, out string builtInAssetPath)
    {
        resPath = NormalizeToUnityPath(resPath);

        if (_pathCache.TryGetValue(resPath, out PathCache pathCache))
        {
            assetBundlePath = pathCache.AssetBundlePath;
            builtInAssetPath = pathCache.BuiltInAssetPath;
        }
        else
        {
            int indexOfRsFolder = resPath.LastIndexOf(_normalizedRsFolder);
            if (indexOfRsFolder >= 0)
            {
                builtInAssetPath = resPath.Substring(indexOfRsFolder + _normalizedRsFolder.Length);
                int extensionSeparatorIndex = builtInAssetPath.LastIndexOf('.');
                if (extensionSeparatorIndex > 0)
                {
                    builtInAssetPath = builtInAssetPath.Substring(0, extensionSeparatorIndex);
                }
                else
                {
                    builtInAssetPath = null;
                }
            }
            else
            {
                builtInAssetPath = null;
            }
#if UNITY_EDITOR
            if (EditorConfigManager.Instance.IsUseAssetBundle)
            {
                assetBundlePath = CreatPath(new string[] { AssetBundleRootPath, resPath });
                if (!File.Exists(assetBundlePath))
                {
                    assetBundlePath = null;
                }
            }
            else
            {
                assetBundlePath = null;
            }
#else
            assetBundlePath = assetBundlePath = CreatPath(new string[] { AssetBundleRootPath, resPath });
            if (!File.Exists(assetBundlePath))
            {
                assetBundlePath = null;
            }
#endif
            _pathCache.Add(resPath, new PathCache(resPath, assetBundlePath, builtInAssetPath));
        }
    }

    public static string NormalizeToUnityPath(string resPath)
    {
        return resPath.Replace(BackSlash, ForwardSlash).ToLower();
    }
}
