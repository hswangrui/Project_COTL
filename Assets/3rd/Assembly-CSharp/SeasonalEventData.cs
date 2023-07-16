using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using I2.Loc;
using Lamb.UI;
using src.UI.Overlays.EventOverlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(menuName = "Massive Monster/Seasonal Event Data")]
public class SeasonalEventData : ScriptableObject
{
	[Serializable]
	public class OnboardingEntry
	{
		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CLoad_003Ed__9 : IAsyncStateMachine
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder _003C_003Et__builder;

			public OnboardingEntry _003C_003E4__this;

			private TaskAwaiter<Sprite> _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				OnboardingEntry onboardingEntry = _003C_003E4__this;
				try
				{
					TaskAwaiter<Sprite> awaiter;
					if (num == 0)
					{
						awaiter = _003C_003Eu__1;
						_003C_003Eu__1 = default(TaskAwaiter<Sprite>);
						num = (_003C_003E1__state = -1);
						goto IL_0089;
					}
					if (onboardingEntry.Image == null)
					{
						onboardingEntry._spriteAsyncOperationHandle = onboardingEntry._imageReferenceSprite.LoadAssetAsync<Sprite>();
						awaiter = onboardingEntry._spriteAsyncOperationHandle.Task.GetAwaiter();
						if (!awaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = awaiter;
							_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
							return;
						}
						goto IL_0089;
					}
					goto end_IL_000e;
					IL_0089:
					awaiter.GetResult();
					onboardingEntry.Image = onboardingEntry._spriteAsyncOperationHandle.Result;
					end_IL_000e:;
				}
				catch (Exception exception)
				{
					_003C_003E1__state = -2;
					_003C_003Et__builder.SetException(exception);
					return;
				}
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetResult();
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
				_003C_003Et__builder.SetStateMachine(stateMachine);
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[SerializeField]
		[TermsPopup("")]
		private string _term;

		[SerializeField]
		private AssetReferenceSprite _imageReferenceSprite;

		private AsyncOperationHandle<Sprite> _spriteAsyncOperationHandle;

		public string Text
		{
			get
			{
				return LocalizationManager.GetTranslation(_term);
			}
		}

		public Sprite Image { get; private set; }

		[AsyncStateMachine(typeof(_003CLoad_003Ed__9))]
		public System.Threading.Tasks.Task Load()
		{
			_003CLoad_003Ed__9 stateMachine = default(_003CLoad_003Ed__9);
			stateMachine._003C_003E4__this = this;
			stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncTaskMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
			return stateMachine._003C_003Et__builder.Task;
		}

		public void Unload()
		{
			Image = null;
			if (_spriteAsyncOperationHandle.IsValid())
			{
				Addressables.Release(_spriteAsyncOperationHandle);
			}
		}
	}

	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CLoadUIAssets_003Ed__40 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder _003C_003Et__builder;

		public SeasonalEventData _003C_003E4__this;

		private TaskAwaiter _003C_003Eu__1;

		private TaskAwaiter<UIEventOverlay> _003C_003Eu__2;

		private void MoveNext()
		{
			int num = _003C_003E1__state;
			SeasonalEventData seasonalEventData = _003C_003E4__this;
			try
			{
				TaskAwaiter<UIEventOverlay> awaiter;
				TaskAwaiter awaiter2;
				if (num != 0)
				{
					if (num == 1)
					{
						awaiter = _003C_003Eu__2;
						_003C_003Eu__2 = default(TaskAwaiter<UIEventOverlay>);
						num = (_003C_003E1__state = -1);
						goto IL_00fb;
					}
					awaiter2 = System.Threading.Tasks.Task.WhenAll(seasonalEventData._entry1.Load(), seasonalEventData._entry2.Load(), seasonalEventData._entry3.Load()).GetAwaiter();
					if (!awaiter2.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = awaiter2;
						_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter2, ref this);
						return;
					}
				}
				else
				{
					awaiter2 = _003C_003Eu__1;
					_003C_003Eu__1 = default(TaskAwaiter);
					num = (_003C_003E1__state = -1);
				}
				awaiter2.GetResult();
				awaiter = UIManager.LoadAsset<UIEventOverlay>(seasonalEventData._eventOverlay).GetAwaiter();
				if (!awaiter.IsCompleted)
				{
					num = (_003C_003E1__state = 1);
					_003C_003Eu__2 = awaiter;
					_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
					return;
				}
				goto IL_00fb;
				IL_00fb:
				UIEventOverlay result = awaiter.GetResult();
				seasonalEventData.EventOverlay = result;
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult();
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	public SeasonalEventType EventType;

	public int StartDay;

	public int StartMonth;

	public int EndDay;

	public int EndMonth;

	public string[] Skins;

	public StructureBrain.TYPES[] Decorations;

	[SerializeField]
	private AssetReferenceGameObject _eventOverlay;

	[SerializeField]
	[TermsPopup("")]
	private string _titleTerm;

	[SerializeField]
	[TermsPopup("")]
	private string _descriptionTerm;

	[SerializeField]
	[TermsPopup("")]
	private string _durationTerm;

	[SerializeField]
	[TermsPopup("")]
	private string _onboardingTitle;

	[SerializeField]
	private OnboardingEntry _entry1;

	[SerializeField]
	private OnboardingEntry _entry2;

	[SerializeField]
	private OnboardingEntry _entry3;

	public UIEventOverlay EventOverlay { get; private set; }

	public string Title
	{
		get
		{
			return LocalizationManager.GetTranslation(_titleTerm);
		}
	}

	public string Description
	{
		get
		{
			return LocalizationManager.GetTranslation(_descriptionTerm);
		}
	}

	public string Duration
	{
		get
		{
			return string.Format(LocalizationManager.GetTranslation(_durationTerm), Title, GetStartDate().ToLongDateString(), GetEndDate().ToLongDateString());
		}
	}

	public string OnboardingTitle
	{
		get
		{
			return LocalizationManager.GetTranslation(_onboardingTitle);
		}
	}

	public OnboardingEntry Entry1
	{
		get
		{
			return _entry1;
		}
	}

	public OnboardingEntry Entry2
	{
		get
		{
			return _entry2;
		}
	}

	public OnboardingEntry Entry3
	{
		get
		{
			return _entry3;
		}
	}

	public DateTime GetStartDate()
	{
		return new DateTime(DateTime.UtcNow.Year, StartMonth, StartDay);
	}

	public DateTime GetEndDate()
	{
		return new DateTime((EndMonth < StartMonth) ? (DateTime.UtcNow.Year + 1) : DateTime.UtcNow.Year, EndMonth, EndDay);
	}

	public bool IsEventActive()
	{
		if (IsValid())
		{
			if (DateTime.UtcNow > GetStartDate())
			{
				return DateTime.UtcNow < GetEndDate();
			}
			return false;
		}
		return false;
	}

	private bool IsValid()
	{
		return DataManager.Instance.OnboardingFinished;
	}

	public List<string> GetUnlockableSkins()
	{
		List<string> list = new List<string>();
		string[] skins = Skins;
		foreach (string text in skins)
		{
			if (!DataManager.GetFollowerSkinUnlocked(text))
			{
				list.Add(text);
			}
		}
		return list;
	}

	public List<StructureBrain.TYPES> GetUnlockableDecorations()
	{
		List<StructureBrain.TYPES> list = new List<StructureBrain.TYPES>();
		StructureBrain.TYPES[] decorations = Decorations;
		foreach (StructureBrain.TYPES item in decorations)
		{
			if (!DataManager.Instance.UnlockedStructures.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	[AsyncStateMachine(typeof(_003CLoadUIAssets_003Ed__40))]
	public System.Threading.Tasks.Task LoadUIAssets()
	{
		_003CLoadUIAssets_003Ed__40 stateMachine = default(_003CLoadUIAssets_003Ed__40);
		stateMachine._003C_003E4__this = this;
		stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder.Create();
		stateMachine._003C_003E1__state = -1;
		AsyncTaskMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
		_003C_003Et__builder.Start(ref stateMachine);
		return stateMachine._003C_003Et__builder.Task;
	}

	public void UnloadUIAssets()
	{
		UIManager.UnloadAsset(EventOverlay);
		_entry1.Unload();
		_entry2.Unload();
		_entry3.Unload();
	}
}
