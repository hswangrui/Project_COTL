using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Lamb.UI;
using UnityEngine;
using WebSocketSharp;

namespace Data.ReadWrite.Conversion
{
	public class COTLDataConversion
	{
		public static IEnumerator ConvertFiles(Action andThen = null)
		{
			string path = Path.Combine(Application.persistentDataPath, "saves");
			MMJsonDataReadWriter<DataManager> saveFileReadWriterJSON = new MMJsonDataReadWriter<DataManager>();
			MMJsonDataReadWriter<MetaData> metaDataReadWriterJSON = new MMJsonDataReadWriter<MetaData>();
			MMJsonDataReadWriter<SettingsData> settingsDataReadWriterJSON = new MMJsonDataReadWriter<SettingsData>();
			MMXMLDataReadWriter<DataManager> saveFileReadWriterXML = new MMXMLDataReadWriter<DataManager>();
			MMXMLDataReadWriter<MetaData> metaDataReadWriterXML = new MMXMLDataReadWriter<MetaData>();
			MMXMLDataReadWriter<SettingsData> settingsDataReadWriterXML = new MMXMLDataReadWriter<SettingsData>();
			string[] files = Directory.GetFiles(path);
			foreach (string file in files)
			{
				if (!Path.GetExtension(file).Equals(".xml"))
				{
					continue;
				}
				bool canContinue = false;
				if (file.IndexOf("xml_") > -1)
				{
					string newFilename3 = file;
					newFilename3 = newFilename3.Replace("xml_", "slot_");
					newFilename3 = newFilename3.Replace(".xml", ".json");
					MMXMLDataReadWriter<DataManager> mMXMLDataReadWriter = saveFileReadWriterXML;
					mMXMLDataReadWriter.OnReadCompleted = (Action<DataManager>)Delegate.Combine(mMXMLDataReadWriter.OnReadCompleted, (Action<DataManager>)delegate(DataManager dataManager)
					{
						ConvertObjectiveIDs(dataManager);
						saveFileReadWriterJSON.Write(dataManager, newFilename3);
					});
					MMJsonDataReadWriter<DataManager> mMJsonDataReadWriter = saveFileReadWriterJSON;
					mMJsonDataReadWriter.OnWriteCompleted = (Action)Delegate.Combine(mMJsonDataReadWriter.OnWriteCompleted, (Action)delegate
					{
						saveFileReadWriterXML.Delete(file);
					});
					MMXMLDataReadWriter<DataManager> mMXMLDataReadWriter2 = saveFileReadWriterXML;
					mMXMLDataReadWriter2.OnDeletionComplete = (Action)Delegate.Combine(mMXMLDataReadWriter2.OnDeletionComplete, (Action)delegate
					{
						canContinue = true;
					});
					saveFileReadWriterXML.Read(file);
				}
				else if (file.IndexOf("meta_") > -1)
				{
					string newFilename2 = file;
					newFilename2 = newFilename2.Replace(".xml", ".json");
					MMXMLDataReadWriter<MetaData> mMXMLDataReadWriter3 = metaDataReadWriterXML;
					mMXMLDataReadWriter3.OnReadCompleted = (Action<MetaData>)Delegate.Combine(mMXMLDataReadWriter3.OnReadCompleted, (Action<MetaData>)delegate(MetaData metaData)
					{
						metaDataReadWriterJSON.Write(metaData, newFilename2);
					});
					MMJsonDataReadWriter<MetaData> mMJsonDataReadWriter2 = metaDataReadWriterJSON;
					mMJsonDataReadWriter2.OnWriteCompleted = (Action)Delegate.Combine(mMJsonDataReadWriter2.OnWriteCompleted, (Action)delegate
					{
						metaDataReadWriterXML.Delete(file);
					});
					MMXMLDataReadWriter<MetaData> mMXMLDataReadWriter4 = metaDataReadWriterXML;
					mMXMLDataReadWriter4.OnDeletionComplete = (Action)Delegate.Combine(mMXMLDataReadWriter4.OnDeletionComplete, (Action)delegate
					{
						canContinue = true;
					});
					metaDataReadWriterXML.Read(file);
				}
				else if (file.IndexOf("settings") > -1)
				{
					string newFilename = file;
					newFilename = newFilename.Replace(".xml", ".json");
					MMXMLDataReadWriter<SettingsData> mMXMLDataReadWriter5 = settingsDataReadWriterXML;
					mMXMLDataReadWriter5.OnReadCompleted = (Action<SettingsData>)Delegate.Combine(mMXMLDataReadWriter5.OnReadCompleted, (Action<SettingsData>)delegate(SettingsData settings)
					{
						settingsDataReadWriterJSON.Write(settings, newFilename);
					});
					MMJsonDataReadWriter<SettingsData> mMJsonDataReadWriter3 = settingsDataReadWriterJSON;
					mMJsonDataReadWriter3.OnWriteCompleted = (Action)Delegate.Combine(mMJsonDataReadWriter3.OnWriteCompleted, (Action)delegate
					{
						settingsDataReadWriterXML.Delete(file);
					});
					MMXMLDataReadWriter<SettingsData> mMXMLDataReadWriter6 = settingsDataReadWriterXML;
					mMXMLDataReadWriter6.OnDeletionComplete = (Action)Delegate.Combine(mMXMLDataReadWriter6.OnDeletionComplete, (Action)delegate
					{
						canContinue = true;
					});
					settingsDataReadWriterXML.Read(file);
				}
				while (!canContinue)
				{
					yield return null;
				}
				saveFileReadWriterJSON.OnWriteCompleted = null;
				saveFileReadWriterXML.OnReadCompleted = null;
				saveFileReadWriterXML.OnDeletionComplete = null;
				metaDataReadWriterJSON.OnWriteCompleted = null;
				metaDataReadWriterXML.OnReadCompleted = null;
				metaDataReadWriterXML.OnDeletionComplete = null;
				settingsDataReadWriterJSON.OnWriteCompleted = null;
				settingsDataReadWriterXML.OnReadCompleted = null;
				settingsDataReadWriterXML.OnDeletionComplete = null;
			}
			andThen?.Invoke();
		}

		public static void ConvertObjectiveIDs(DataManager dataManager)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			foreach (ObjectivesData objective in dataManager.Objectives)
			{
				if (objective.UniqueGroupID == null || objective.UniqueGroupID.IsNullOrEmpty())
				{
					if (dictionary.ContainsKey(objective.GroupId))
					{
						objective.UniqueGroupID = dictionary[objective.GroupId];
						continue;
					}
					objective.UniqueGroupID = objective.GroupId.GetStableHashCode().ToString();
					dictionary.Add(objective.GroupId, objective.UniqueGroupID);
				}
			}
			foreach (ObjectivesData completedObjective in dataManager.CompletedObjectives)
			{
				if (completedObjective.UniqueGroupID == null || completedObjective.UniqueGroupID.IsNullOrEmpty())
				{
					if (dictionary.ContainsKey(completedObjective.GroupId))
					{
						completedObjective.UniqueGroupID = dictionary[completedObjective.GroupId];
						continue;
					}
					completedObjective.UniqueGroupID = completedObjective.GroupId.GetStableHashCode().ToString();
					dictionary.Add(completedObjective.GroupId, completedObjective.UniqueGroupID);
				}
			}
			foreach (ObjectivesData failedObjective in dataManager.FailedObjectives)
			{
				if (failedObjective.UniqueGroupID == null || failedObjective.UniqueGroupID.IsNullOrEmpty())
				{
					if (dictionary.ContainsKey(failedObjective.GroupId))
					{
						failedObjective.UniqueGroupID = dictionary[failedObjective.GroupId];
						continue;
					}
					failedObjective.UniqueGroupID = failedObjective.GroupId.GetStableHashCode().ToString();
					dictionary.Add(failedObjective.GroupId, failedObjective.UniqueGroupID);
				}
			}
			foreach (ObjectivesDataFinalized item in dataManager.CompletedObjectivesHistory)
			{
				if (item.UniqueGroupID == null || item.UniqueGroupID.IsNullOrEmpty())
				{
					if (dictionary.ContainsKey(item.GroupId))
					{
						item.UniqueGroupID = dictionary[item.GroupId];
						continue;
					}
					item.UniqueGroupID = item.GroupId.GetStableHashCode().ToString();
					dictionary.Add(item.GroupId, item.UniqueGroupID);
				}
			}
			foreach (ObjectivesDataFinalized item2 in dataManager.FailedObjectivesHistory)
			{
				if (item2.UniqueGroupID == null || item2.UniqueGroupID.IsNullOrEmpty())
				{
					if (dictionary.ContainsKey(item2.GroupId))
					{
						item2.UniqueGroupID = dictionary[item2.GroupId];
						continue;
					}
					item2.UniqueGroupID = item2.GroupId.GetStableHashCode().ToString();
					dictionary.Add(item2.GroupId, item2.UniqueGroupID);
				}
			}
		}

		public static void UpgradeTierMismatchFix(DataManager dataManager)
		{
			if (dataManager.UnlockedUpgrades.Contains(UpgradeSystem.Type.Temple_III) && dataManager.UnlockedUpgrades.Contains(UpgradeSystem.Type.Temple_IV) && dataManager.CurrentUpgradeTreeTier == UpgradeTreeNode.TreeTier.Tier4)
			{
				dataManager.CurrentUpgradeTreeTier = UpgradeTreeNode.TreeTier.Tier5;
			}
			if (dataManager.UnlockedUpgrades.Contains(UpgradeSystem.Type.Economy_Refinery) && dataManager.CurrentUpgradeTreeTier < UpgradeTreeNode.TreeTier.Tier3)
			{
				dataManager.CurrentUpgradeTreeTier = UpgradeTreeNode.TreeTier.Tier3;
			}
		}
	}
}
