using System;
using System.IO;
using Data.ReadWrite.Conversion;
using UnityEngine;

public class SaveAndLoad : Singleton<SaveAndLoad>
{
	public static int SAVE_SLOT = 5;

	public static bool Loaded = false;

	private const string kSaveFileName = "slot_{0}.json";

	private const string kMetaFileName = "meta_{0}.json";

	public static Action OnSaveCompleted;

	public static Action<MMReadWriteError> OnSaveError;

	public static Action OnLoadComplete;

	public static Action<MMReadWriteError> OnLoadError;

	public static Action<int> OnSaveSlotDeleted;

	private COTLDataReadWriter<DataManager> _saveFileReadWriter = new COTLDataReadWriter<DataManager>();

	private COTLDataReadWriter<MetaData> _metaReadWriter = new COTLDataReadWriter<MetaData>();

	public SaveAndLoad()
	{
		COTLDataReadWriter<DataManager> saveFileReadWriter = _saveFileReadWriter;
		saveFileReadWriter.OnReadCompleted = (Action<DataManager>)Delegate.Combine(saveFileReadWriter.OnReadCompleted, (Action<DataManager>)delegate(DataManager saveFile)
		{
			DataManager.Instance = saveFile;
			COTLDataConversion.ConvertObjectiveIDs(DataManager.Instance);
			COTLDataConversion.UpgradeTierMismatchFix(DataManager.Instance);
			LoadMetaData();
			Action onLoadComplete2 = OnLoadComplete;
			if (onLoadComplete2 != null)
			{
				onLoadComplete2();
			}
			Loaded = true;
		});
		COTLDataReadWriter<DataManager> saveFileReadWriter2 = _saveFileReadWriter;
		saveFileReadWriter2.OnCreateDefault = (Action)Delegate.Combine(saveFileReadWriter2.OnCreateDefault, (Action)delegate
		{
			DataManager.ResetData();
			LoadMetaData();
			Action onLoadComplete = OnLoadComplete;
			if (onLoadComplete != null)
			{
				onLoadComplete();
			}
			Loaded = true;
			Action onSaveCompleted2 = OnSaveCompleted;
			if (onSaveCompleted2 != null)
			{
				onSaveCompleted2();
			}
		});
		COTLDataReadWriter<DataManager> saveFileReadWriter3 = _saveFileReadWriter;
		saveFileReadWriter3.OnWriteCompleted = (Action)Delegate.Combine(saveFileReadWriter3.OnWriteCompleted, (Action)delegate
		{
			Action onSaveCompleted = OnSaveCompleted;
			if (onSaveCompleted != null)
			{
				onSaveCompleted();
			}
		});
		COTLDataReadWriter<DataManager> saveFileReadWriter4 = _saveFileReadWriter;
		saveFileReadWriter4.OnWriteError = (Action<MMReadWriteError>)Delegate.Combine(saveFileReadWriter4.OnWriteError, (Action<MMReadWriteError>)delegate(MMReadWriteError error)
		{
			Action<MMReadWriteError> onSaveError = OnSaveError;
			if (onSaveError != null)
			{
				onSaveError(error);
			}
		});
		COTLDataReadWriter<DataManager> saveFileReadWriter5 = _saveFileReadWriter;
		saveFileReadWriter5.OnReadError = (Action<MMReadWriteError>)Delegate.Combine(saveFileReadWriter5.OnReadError, (Action<MMReadWriteError>)delegate(MMReadWriteError error)
		{
			Action<MMReadWriteError> onLoadError = OnLoadError;
			if (onLoadError != null)
			{
				onLoadError(error);
			}
		});
		COTLDataReadWriter<MetaData> metaReadWriter = _metaReadWriter;
		metaReadWriter.OnReadCompleted = (Action<MetaData>)Delegate.Combine(metaReadWriter.OnReadCompleted, (Action<MetaData>)delegate(MetaData metaData)
		{
			DataManager.Instance.MetaData = metaData;
			DifficultyManager.ForceDifficulty(DataManager.Instance.MetaData.Difficulty);
		});
		COTLDataReadWriter<MetaData> metaReadWriter2 = _metaReadWriter;
		metaReadWriter2.OnCreateDefault = (Action)Delegate.Combine(metaReadWriter2.OnCreateDefault, (Action)delegate
		{
			DataManager.Instance.MetaData = MetaData.Default(DataManager.Instance);
			DifficultyManager.ForceDifficulty(DataManager.Instance.MetaData.Difficulty);
		});
	}

	public static void Save()
	{
		if (DataManager.Instance.AllowSaving && !CheatConsole.IN_DEMO)
		{
			HideSaveicon.instance.StartRoutineSave(Saving);
		}
	}

	private static void Saving()
	{
		Singleton<SaveAndLoad>.Instance._saveFileReadWriter.Write(DataManager.Instance, MakeSaveSlot(SAVE_SLOT));
		MetaData metaData = DataManager.Instance.MetaData;
		metaData.CultName = DataManager.Instance.CultName;
		metaData.FollowerCount = DataManager.Instance.Followers.Count;
		metaData.StructureCount = StructureManager.GetTotalHomesCount();
		metaData.DeathCount = DataManager.Instance.Followers_Dead.Count;
		metaData.Day = TimeManager.CurrentDay;
		metaData.PlayTime = DataManager.Instance.TimeInGame;
		metaData.GameBeaten = DataManager.Instance.DeathCatBeaten;
		metaData.SandboxBeaten = DataManager.Instance.CompletedSandbox;
		metaData.Dungeon1Completed = DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_1);
		metaData.Dungeon2Completed = DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_2);
		metaData.Dungeon3Completed = DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_3);
		metaData.Dungeon4Completed = DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_4);
		metaData.Dungeon1NGPCompleted = DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_1, true);
		metaData.Dungeon2NGPCompleted = DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_2, true);
		metaData.Dungeon3NGPCompleted = DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_3, true);
		metaData.Dungeon4NGPCompleted = DataManager.Instance.DungeonCompleted(FollowerLocation.Dungeon1_4, true);
		metaData.DeathCatRecruited = DataManager.Instance.HasDeathCatFollower();
		metaData.PercentageCompleted = 0;
		metaData.Permadeath = DataManager.Instance.PermadeDeathActive;
		metaData.Version = Application.version;
		DataManager.Instance.MetaData = metaData;
		SaveMetaData(metaData);
	}

	public static void Load(int saveSlot)
	{
		if (!CheatConsole.IN_DEMO)
		{
			SAVE_SLOT = saveSlot;
			Singleton<SaveAndLoad>.Instance._saveFileReadWriter.Read(MakeSaveSlot(SAVE_SLOT));
		}
	}

	public static bool SaveExist(int saveSlot)
	{
		if (Singleton<SaveAndLoad>.Instance._saveFileReadWriter.FileExists(MakeSaveSlot(saveSlot)))
		{
			return true;
		}
		return false;
	}

	public static void DeleteSaveSlot(int saveSlot)
	{
		Singleton<SaveAndLoad>.Instance._saveFileReadWriter.Delete(MakeSaveSlot(saveSlot));
		DeleteMetaData(saveSlot);
		Action<int> onSaveSlotDeleted = OnSaveSlotDeleted;
		if (onSaveSlotDeleted != null)
		{
			onSaveSlotDeleted(saveSlot);
		}
	}

	private static void DeleteScreenshotInfo(int slot)
	{
		COTLDataReadWriter<MetaData> cOTLDataReadWriter = new COTLDataReadWriter<MetaData>();
		cOTLDataReadWriter.Read(MakeMetaSlot(slot));
		cOTLDataReadWriter.OnReadCompleted = (Action<MetaData>)Delegate.Combine(cOTLDataReadWriter.OnReadCompleted, (Action<MetaData>)delegate(MetaData meta)
		{
			for (int i = 0; i <= meta.Day; i++)
			{
				string path = Path.Combine(Application.persistentDataPath, "Screenshots", string.Format("day_{0}_{1}.png", i, slot));
				if (File.Exists(path))
				{
					File.Delete(path);
				}
			}
		});
	}

	public static void ResetSave(int saveSlot, bool newGame)
	{
		SAVE_SLOT = saveSlot;
		DataManager.ResetData();
		if (!newGame)
		{
			Save();
		}
		Loaded = true;
	}

	public static void SaveMetaData(MetaData metaData)
	{
		Debug.Log("Save MetaData".Colour(Color.yellow));
		Singleton<SaveAndLoad>.Instance._metaReadWriter.Write(metaData, MakeMetaSlot(SAVE_SLOT));
	}

	private static void LoadMetaData()
	{
		Debug.Log("Load MetaData".Colour(Color.yellow));
		Singleton<SaveAndLoad>.Instance._metaReadWriter.Read(MakeMetaSlot(SAVE_SLOT));
	}

	private static void DeleteMetaData(int saveSlot)
	{
		Debug.Log("Delete MetaData".Colour(Color.yellow));
		Singleton<SaveAndLoad>.Instance._metaReadWriter.Delete(MakeMetaSlot(saveSlot));
	}

	public static string MakeSaveSlot(int slot)
	{
		return string.Format("slot_{0}.json", slot);
	}

	public static string MakeMetaSlot(int slot)
	{
		return string.Format("meta_{0}.json", slot);
	}
}
