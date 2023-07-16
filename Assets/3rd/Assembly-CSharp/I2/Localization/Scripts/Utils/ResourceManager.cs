﻿
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
namespace I2.Loc
{
	using System;
	using System.Collections.Generic;
	using UnityEngine;
	using Object = UnityEngine.Object;

	
	public interface IResourceManager_Bundles
	{
		Object LoadFromBundle(string path, Type assetType );
	}

	public class ResourceManager : MonoBehaviour 
	{
		#region Singleton
		public static ResourceManager pInstance
		{
			get {
				bool changed = mInstance==null;

				if (mInstance==null)
					mInstance = (ResourceManager)FindObjectOfType(typeof(ResourceManager));

				if (mInstance==null)
				{
					GameObject GO = new GameObject("I2ResourceManager", typeof(ResourceManager));
					GO.hideFlags = GO.hideFlags | HideFlags.HideAndDontSave;	// Only hide it if this manager was autocreated
					mInstance = GO.GetComponent<ResourceManager>();
					#if UNITY_5_4_OR_NEWER
					SceneManager.sceneLoaded += MyOnLevelWasLoaded;
					#endif
				}

				if (changed && Application.isPlaying)
					DontDestroyOnLoad(mInstance.gameObject);

				return mInstance;
			}
		}
		static ResourceManager mInstance;

		#endregion

		#region Management

		public List<IResourceManager_Bundles> mBundleManagers = new List<IResourceManager_Bundles>();

		#if UNITY_5_4_OR_NEWER
		public static void MyOnLevelWasLoaded(Scene scene, LoadSceneMode mode)
		#else
		public void OnLevelWasLoaded()
		#endif
		{
			pInstance.CleanResourceCache();
			LocalizationManager.UpdateSources();
		}

		#endregion

		#region Assets

		public Object[] Assets;

		// This function tries finding an asset in the Assets array, if not found it tries loading it from the Resources Folder
		public T GetAsset<T>( string Name ) where T : Object
		{
			T Obj = FindAsset( Name ) as T;
			if (Obj!=null)
				return Obj;

			return LoadFromResources<T>( Name );
		}

		Object FindAsset( string Name )
		{
			if (Assets!=null)
			{
				for (int i=0, imax=Assets.Length; i<imax; ++i)
					if (Assets[i]!=null && Assets[i].name == Name)
						return Assets[i];
			}
			return null;
		}

		public bool HasAsset( Object Obj )
		{
			if (Assets==null)
				return false;
			return Array.IndexOf (Assets, Obj) >= 0;
		}

		#endregion

		#region Resources Cache

		// This cache is kept for a few moments and then cleared
		// Its meant to avoid doing several Resource.Load for the same Asset while Localizing 
		// (e.g. Lot of labels could be trying to Load the same Font)
		readonly Dictionary<string, Object> mResourcesCache = new Dictionary<string, Object>(StringComparer.Ordinal); // This is used to avoid re-loading the same object from resources in the same frame
		//bool mCleaningScheduled = false;

		public T LoadFromResources<T>( string Path ) where T : Object
		{
			try
			{
				if (string.IsNullOrEmpty( Path ))
					return null;

				Object Obj;
				// Doing Resource.Load is very slow so we are catching the recently loaded objects
				if (mResourcesCache.TryGetValue( Path, out Obj ) && Obj!=null)
				{
					return Obj as T;
				}

				T obj = null;

                if (Path.EndsWith("]", StringComparison.OrdinalIgnoreCase))  // Handle sprites (Multiple) loaded from resources :   "SpritePath[SpriteName]"
                {
                    int idx = Path.LastIndexOf("[", StringComparison.OrdinalIgnoreCase);
                    int len = Path.Length - idx - 2;
                    string MultiSpriteName = Path.Substring(idx + 1, len);
                    Path = Path.Substring(0, idx);

                    T[] objs = Resources.LoadAll<T>(Path);
                    for (int j = 0, jmax = objs.Length; j < jmax; ++j)
                        if (objs[j].name.Equals(MultiSpriteName))
                        {
                            obj = objs[j];
                            break;
                        }
                }
                else
                {
                    obj = Resources.Load(Path, typeof(T)) as T;
                }

				if (obj == null)
					obj = LoadFromBundle<T>( Path );

				if (obj!=null)
					mResourcesCache[Path] = obj;

				/*if (!mCleaningScheduled)
				{
					Invoke("CleanResourceCache", 0.1f);
					mCleaningScheduled = true;
				}*/
				//if (obj==null)
					//Debug.LogWarningFormat( "Unable to load {0} '{1}'", typeof( T ), Path );

				return obj;
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat( "Unable to load {0} '{1}'\nERROR: {2}", typeof(T), Path, e.ToString() );
				return null;
			}
		}

		public T LoadFromBundle<T>(string path ) where T : Object
		{
			for (int i = 0, imax = mBundleManagers.Count; i < imax; ++i)
				if (mBundleManagers[i]!=null)
				{
					var obj = mBundleManagers[i].LoadFromBundle(path, typeof(T)) as T;
					if (obj != null)
						return obj;
				}
			return null;
		}

		public void CleanResourceCache(bool unloadResources = false)
		{
			mResourcesCache.Clear();
			if (unloadResources)
				Resources.UnloadUnusedAssets();

			CancelInvoke();
			//mCleaningScheduled = false;
		}

		public Dictionary<string, AsyncOperationHandle<UnityEngine.Object>> mAddressablesCache = new Dictionary<string, AsyncOperationHandle<UnityEngine.Object>>();

		public T LoadFromAddressables<T>(string Path) where T : UnityEngine.Object
		{
			try
			{
				if (string.IsNullOrEmpty(Path))
				{
					return null;
				}
				Path = ConvertPathToAddressable(Path);
				if (!AssetExists(Path))
				{
					return null;
				}
				if (mAddressablesCache.TryGetValue(Path, out var value) && value.IsValid())
				{
					return value.Result as T;
				}
				value = Addressables.LoadAssetAsync<UnityEngine.Object>(Path);
				T obj = value.WaitForCompletion() as T;
				if ((UnityEngine.Object)obj != (UnityEngine.Object)null)
				{
					mAddressablesCache.Add(Path, value);
				}
				return obj;
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Unable to load {0} '{1}'\nERROR: {2}", typeof(T), Path, ex.ToString());
				return null;
			}
		}

		public void UnloadAddressable(string Path)
		{
			try
			{
				if (!string.IsNullOrEmpty(Path))
				{
					Path = ConvertPathToAddressable(Path);
					if (mAddressablesCache.TryGetValue(Path, out var value) && value.IsValid())
					{
						Addressables.Release(value);
						mAddressablesCache.Remove(Path);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Unable to unload '{0}'\nERROR: {1}", Path, ex.ToString());
			}
		}
		public static bool AssetExists(object key)
		{
			if (Application.isPlaying)
			{
				foreach (IResourceLocator resourceLocator in Addressables.ResourceLocators)
				{
					if (resourceLocator.Locate(key, null, out var _))
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		public static string ConvertPathToAddressable(string path)
		{
			string result = "Assets/Resources_moved/" + path + ".asset";
			if (string.IsNullOrEmpty(path))
			{
				return path;
			}
			return result;
		}
		#endregion
	}
}