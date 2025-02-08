using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class AssetManager
{
    #region LOADER INTERFACE
    private interface IAssetLoader
    {
        UnityEngine.Object Asset { get; }

        int refCount { get; }

        string resPath { get; }

        void AddCallback(Action<UnityEngine.Object> completedCallback);

        void LoadImmedietly();

        bool TryLoad();

    }
    #endregion
    #region ABSTRACT LOADER
    private abstract class AbsAssetLoader : IAssetLoader
    {
        protected int _refCount = 0;
        protected string _resPath = null;
        protected UnityEngine.Object _asset = null;
        protected Action<UnityEngine.Object> _completedCallback = null;

        public AbsAssetLoader(string resPath)
        {
            _resPath = resPath;
        }

        public virtual UnityEngine.Object Asset
        {
            get
            {
                return _asset;
            }
        }

        public virtual int refCount
        {
            get
            {
                return _refCount;
            }
        }

        public virtual string resPath
        {
            get
            {
                return _resPath;
            }
        }

        public virtual void AddCallback(Action<UnityEngine.Object> completedCallback)
        {
            _refCount++;
            if (completedCallback != null)
            {
                _completedCallback += completedCallback;
            }
        }

        public abstract void LoadImmedietly();

        public abstract bool TryLoad();
    }
    #endregion
    #region BUILT-IN ASSET LOADER
    private class BuiltInAssetLoader : AbsAssetLoader
    {
        private string _builtInAssetPath = null;
        private ResourceRequest _builtInLoadRequest = null;

        public BuiltInAssetLoader(string resPath, string builtInAssetPath) :base(resPath)
        {
            _builtInAssetPath = builtInAssetPath;
        }

        public override void LoadImmedietly()
        {
            if (TryLoad())
            {
                //greate, it've just completed in this frame. no need to do anything
            }
            else
            {
                //load it now and ignore the async task. i don't think i can cancel it. let the garbage collector do it's job
                _asset = Resources.Load(_builtInAssetPath);
                _completedCallback?.Invoke(_asset);
            }
        }

        public override bool TryLoad()
        {
            if (_TryLoadBuiltInAsset())
            {
                _completedCallback?.Invoke(_asset);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool _TryLoadBuiltInAsset()
        {
            if (_builtInLoadRequest == null)
            {
                _builtInLoadRequest = Resources.LoadAsync(_builtInAssetPath);
            }
            if (_builtInLoadRequest.isDone)
            {
                _asset = _builtInLoadRequest.asset;
                return true;
            }
            return false;
        }
    }
    #endregion
    #region ASSET BUNDLE LOADER
    private class AssetBundleLoader : AbsAssetLoader
    {
        private string _assetBundlePath = null;
        private AssetBundle _bundle = null;
        private AssetBundleCreateRequest _loadBundleRequest = null;
        private AssetBundleRequest _loadAssetRequest = null;

        public AssetBundleLoader(string resPath, string assetBundlePath) : base(resPath)
        {
            _assetBundlePath = assetBundlePath;
        }

        public AssetBundle Bundle
        {
            get
            {
                return _bundle;
            }
        }

        public override void LoadImmedietly()
        {
            if (TryLoad())
            {
                //greate, it've just completed in this frame. no need to do anything
            }
            else
            {
                //load it now and ignore the async task. i don't think i can cancel it. let the garbage collector do it's job
                if (_bundle == null)
                {
                    _bundle = AssetBundle.LoadFromFile(_assetBundlePath);
                }
                _asset = _bundle.LoadAllAssets<UnityEngine.Object>()[0];
                _completedCallback?.Invoke(_asset);
            }
        }

        public override bool TryLoad()
        {
            if (_TryLoadBundle())
            {
                if (_TryLoadAsset())
                {
                    _completedCallback?.Invoke(_asset);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool _TryLoadBundle()
        {
            if (_loadBundleRequest == null)
            {
                _loadBundleRequest = AssetBundle.LoadFromFileAsync(_assetBundlePath);
            }
            if (_loadBundleRequest.isDone)
            {
                _bundle = _loadBundleRequest.assetBundle;
                return true;
            }
            return false;
        }

        private bool _TryLoadAsset()
        {
            if (_loadAssetRequest == null)
            {
                _loadAssetRequest = _bundle.LoadAllAssetsAsync<UnityEngine.Object>();
            }
            if (_loadAssetRequest.isDone)
            {
                _asset = _loadAssetRequest.allAssets[0];
                return true;
            }
            return false;
        }
    }
    #endregion
#if UNITY_EDITOR
    #region EDITOR ASSET LOADER
    private class EditorAssetLoader : AbsAssetLoader
    {
        public EditorAssetLoader(string resPath) : base(resPath)
        {
        }

        public override void LoadImmedietly()
        {
            if (TryLoad())
            {
                //greate, it've just completed in this frame. no need to do anything
            }
            else
            {
                //just fo reading pleasure :))). i know we dont have assets async loading with AssetDatabase yet, so the flow will never really go here
                _asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_resPath);
                _completedCallback?.Invoke(_asset);
            }
        }

        public override bool TryLoad()
        {
            //well. this version of Unity did not have async load in AssetDatabase yet. so i will just load it sync and call it a day
            _asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(_resPath);
            _completedCallback?.Invoke(_asset);
            return true;
        }
    }
    #endregion
#endif
}
