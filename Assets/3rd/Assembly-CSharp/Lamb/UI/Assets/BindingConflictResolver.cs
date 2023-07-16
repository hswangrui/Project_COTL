using System;
using System.Collections.Generic;
using Rewired;
using RewiredConsts;
using UnityEngine;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "BindingConflictResolver", menuName = "Massive Monster/BindingConflictResolver", order = 1)]
	public class BindingConflictResolver : ScriptableObject
	{
		public abstract class BindingEntry
		{
			[ActionIdProperty(typeof(RewiredConsts.Action))]
			public int Binding;

			[Header("Locked?")]
			public bool LockedOnKeyboard;

			public bool LockedOnMouse;

			public bool LockedOnGamepad;

			[ActionIdProperty(typeof(RewiredConsts.Action))]
			public List<int> ConflictingBindings;

			public abstract int[] BindingSource { get; }

			public void ClearAll()
			{
				ConflictingBindings = new List<int>();
			}

			private void AddGameplay()
			{
				int[] allBindings = RewiredGameplayInputSource.AllBindings;
				foreach (int item in allBindings)
				{
					if (!ConflictingBindings.Contains(item))
					{
						ConflictingBindings.Add(item);
					}
				}
			}

			private void AddUI()
			{
				int[] allBindings = RewiredUIInputSource.AllBindings;
				foreach (int item in allBindings)
				{
					if (!ConflictingBindings.Contains(item))
					{
						ConflictingBindings.Add(item);
					}
				}
			}

			private void AddPhotoMode()
			{
				int[] allBindings = PhotoModeInputSource.AllBindings;
				foreach (int item in allBindings)
				{
					if (!ConflictingBindings.Contains(item))
					{
						ConflictingBindings.Add(item);
					}
				}
			}

			public void PopulateWithAll()
			{
				ConflictingBindings = new List<int>();
				for (int i = 0; i < BindingSource.Length; i++)
				{
					ConflictingBindings.Add(BindingSource[i]);
				}
			}
		}

		[Serializable]
		public class GameplayBindingEntry : BindingEntry
		{
			public override int[] BindingSource
			{
				get
				{
					return RewiredGameplayInputSource.AllBindings;
				}
			}
		}

		[Serializable]
		public class UIBindingEntry : BindingEntry
		{
			public override int[] BindingSource
			{
				get
				{
					return RewiredUIInputSource.AllBindings;
				}
			}
		}

		[Serializable]
		public class PhotoModeBindingEntry : BindingEntry
		{
			public override int[] BindingSource
			{
				get
				{
					return PhotoModeInputSource.AllBindings;
				}
			}
		}

		[SerializeField]
		private GameplayBindingEntry[] _gameplayBindings = Array.Empty<GameplayBindingEntry>();

		[SerializeField]
		private UIBindingEntry[] _uiBindings = Array.Empty<UIBindingEntry>();

		[SerializeField]
		private PhotoModeBindingEntry[] _photoModeBindings = Array.Empty<PhotoModeBindingEntry>();

		public BindingEntry GetEntry(KeybindItem keybindItem)
		{
			return GetEntry(keybindItem.Category, keybindItem.Action);
		}

		public BindingEntry GetEntry(int keybindCategory, int keybindAction)
		{
			switch (keybindCategory)
			{
			case 0:
			{
				GameplayBindingEntry[] gameplayBindings = _gameplayBindings;
				foreach (GameplayBindingEntry gameplayBindingEntry in gameplayBindings)
				{
					if (gameplayBindingEntry.Binding == keybindAction)
					{
						return gameplayBindingEntry;
					}
				}
				break;
			}
			case 1:
			{
				UIBindingEntry[] uiBindings = _uiBindings;
				foreach (UIBindingEntry uIBindingEntry in uiBindings)
				{
					if (uIBindingEntry.Binding == keybindAction)
					{
						return uIBindingEntry;
					}
				}
				break;
			}
			case 2:
			{
				PhotoModeBindingEntry[] photoModeBindings = _photoModeBindings;
				foreach (PhotoModeBindingEntry photoModeBindingEntry in photoModeBindings)
				{
					if (photoModeBindingEntry.Binding == keybindAction)
					{
						return photoModeBindingEntry;
					}
				}
				break;
			}
			default:
				return null;
			}
			return null;
		}

		private void PopulateAll()
		{
			_gameplayBindings = new GameplayBindingEntry[RewiredGameplayInputSource.AllBindings.Length];
			for (int i = 0; i < _gameplayBindings.Length; i++)
			{
				_gameplayBindings[i] = new GameplayBindingEntry
				{
					Binding = RewiredGameplayInputSource.AllBindings[i]
				};
			}
			_uiBindings = new UIBindingEntry[RewiredUIInputSource.AllBindings.Length];
			for (int j = 0; j < _uiBindings.Length; j++)
			{
				_uiBindings[j] = new UIBindingEntry
				{
					Binding = RewiredUIInputSource.AllBindings[j]
				};
			}
			_photoModeBindings = new PhotoModeBindingEntry[PhotoModeInputSource.AllBindings.Length];
			for (int k = 0; k < _photoModeBindings.Length; k++)
			{
				_photoModeBindings[k] = new PhotoModeBindingEntry
				{
					Binding = PhotoModeInputSource.AllBindings[k]
				};
			}
		}
	}
}
