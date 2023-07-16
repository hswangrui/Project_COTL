using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Rewired;
using src.Extensions;
using UnityEngine;

public abstract class CategoryInputSource : InputSource
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CWaitAndRecord_003Ed__9 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncTaskMethodBuilder _003C_003Et__builder;

		public CategoryInputSource _003C_003E4__this;

		private TaskAwaiter _003C_003Eu__1;

		private void MoveNext()
		{
			int num = _003C_003E1__state;
			CategoryInputSource categoryInputSource = _003C_003E4__this;
			try
			{
				TaskAwaiter awaiter;
				if (num != 0)
				{
					awaiter = System.Threading.Tasks.Task.Delay(100).GetAwaiter();
					if (!awaiter.IsCompleted)
					{
						num = (_003C_003E1__state = 0);
						_003C_003Eu__1 = awaiter;
						_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
						return;
					}
				}
				else
				{
					awaiter = _003C_003Eu__1;
					_003C_003Eu__1 = default(TaskAwaiter);
					num = (_003C_003E1__state = -1);
				}
				awaiter.GetResult();
				categoryInputSource.RecordDefaultBindings(categoryInputSource._rewiredPlayer.controllers.Keyboard, categoryInputSource._defaultKeyboardBindings);
				categoryInputSource.RecordDefaultBindings(categoryInputSource._rewiredPlayer.controllers.Mouse, categoryInputSource._defaultMouseBindings);
				IEnumerator<Joystick> enumerator = categoryInputSource._rewiredPlayer.controllers.Joysticks.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						Joystick current = enumerator.Current;
						categoryInputSource.RecordDefaultBindings(current, categoryInputSource._defaultGamepadBindings);
					}
				}
				finally
				{
					if (num < 0 && enumerator != null)
					{
						enumerator.Dispose();
					}
				}
				categoryInputSource.ApplyAllBindings();
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

	private List<Binding> _defaultKeyboardBindings = new List<Binding>();

	private List<Binding> _defaultMouseBindings = new List<Binding>();

	private List<Binding> _defaultGamepadBindings = new List<Binding>();

	private ControllerMap _keyboardMap;

	private ControllerMap _mouseMap;

	private ControllerMap _gamepadMap;

	protected abstract int Category { get; }

	protected override void UpdateRewiredPlayer()
	{
		base.UpdateRewiredPlayer();
		WaitAndRecord();
	}

	[AsyncStateMachine(typeof(_003CWaitAndRecord_003Ed__9))]
	private System.Threading.Tasks.Task WaitAndRecord()
	{
		_003CWaitAndRecord_003Ed__9 stateMachine = default(_003CWaitAndRecord_003Ed__9);
		stateMachine._003C_003E4__this = this;
		stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder.Create();
		stateMachine._003C_003E1__state = -1;
		AsyncTaskMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
		_003C_003Et__builder.Start(ref stateMachine);
		return stateMachine._003C_003Et__builder.Task;
	}

	protected override void OnLastActiveControllerChanged(Player player, Controller controller)
	{
		if (controller != null && controller.type == ControllerType.Joystick)
		{
			LoadAndBind(controller);
		}
	}

	private void RecordDefaultBindings(Controller controller, List<Binding> target)
	{
		if (controller == null)
		{
			UnityEngine.Debug.Log("RecordDefaultBindings - Controller was null");
		}
		else
		{
			if (target.Count > 0)
			{
				return;
			}
			ControllerMap controllerMap = ((controller.type != ControllerType.Joystick) ? GetControllerMap(controller) : GetControllerMap(controller, 3));
			if (controllerMap != null)
			{
				foreach (ActionElementMap allMap in controllerMap.AllMaps)
				{
					target.Add(allMap.ToBinding());
				}
				return;
			}
			UnityEngine.Debug.Log("Controller map is null. Can't record binds");
		}
	}

	public List<Binding> GetDefaultBindingsForControllerType(ControllerType controllerType)
	{
		switch (controllerType)
		{
		case ControllerType.Keyboard:
			return _defaultKeyboardBindings;
		case ControllerType.Mouse:
			return _defaultMouseBindings;
		case ControllerType.Joystick:
			return _defaultGamepadBindings;
		default:
			return null;
		}
	}

	public ControllerMap GetControllerMap(Controller controller)
	{
		if (controller.type == ControllerType.Joystick)
		{
			try
			{
				return GetControllerMap(controller, SettingsManager.Settings.Control.GamepadLayout);
			}
			catch
			{
				GetControllerMap(controller, 0);
			}
		}
		return GetControllerMap(controller, 0);
	}

	public ControllerMap GetControllerMap(Controller controller, int mapIndex)
	{
		if (base._rewiredPlayer == null)
		{
			return null;
		}
		if (controller.type != 0)
		{
			ControllerType type = controller.type;
			int num = 1;
		}
		return base._rewiredPlayer.controllers.maps.GetMap(controller, Category, mapIndex);
	}

	public IList<ControllerMap> GetControllerMaps(Controller controller)
	{
		if (base._rewiredPlayer == null)
		{
			return null;
		}
		return base._rewiredPlayer.controllers.maps.GetMaps(controller);
	}

	public void ApplyBindings()
	{
		LoadAndBind(InputManager.General.GetLastActiveController());
	}

	public void ApplyAllBindings()
	{
		LoadAndBind(base._rewiredPlayer.controllers.Keyboard);
		LoadAndBind(base._rewiredPlayer.controllers.Mouse);
		foreach (Joystick joystick in base._rewiredPlayer.controllers.Joysticks)
		{
			LoadAndBind(joystick);
		}
	}

	private void LoadAndBind(Controller controller)
	{
		if (controller == null)
		{
			UnityEngine.Debug.Log("LoadAndBind - Controller was null, could not bind");
			return;
		}
		if (SettingsManager.Settings == null)
		{
			UnityEngine.Debug.LogWarning("LoadAndBind - SettingsManager.Settings is not available yet (null).");
			return;
		}
		if (controller.type == ControllerType.Keyboard)
		{
			RecordDefaultBindings(controller, _defaultKeyboardBindings);
			ControllerMap controllerMap = GetControllerMap(controller);
			ControlSettingsUtilities.ApplyBindings(controllerMap, SettingsManager.Settings.Control.KeyboardBindings);
			ControlSettingsUtilities.DeleteUnboundBindings(controllerMap, SettingsManager.Settings.Control.KeyboardBindingsUnbound);
		}
		if (controller.type == ControllerType.Joystick)
		{
			foreach (ControllerMap controllerMap4 in GetControllerMaps(controller))
			{
				if (controllerMap4.layoutId != SettingsManager.Settings.Control.GamepadLayout)
				{
					base._rewiredPlayer.controllers.maps.RemoveMap(ControllerType.Joystick, controller.id, Category, controllerMap4.layoutId);
					break;
				}
			}
			base._rewiredPlayer.controllers.maps.LoadMap(ControllerType.Joystick, controller.id, Category, SettingsManager.Settings.Control.GamepadLayout);
			base._rewiredPlayer.controllers.maps.SetMapsEnabled(true, ControllerType.Joystick, Category, SettingsManager.Settings.Control.GamepadLayout);
			if (SettingsManager.Settings.Control.GamepadLayout == 3)
			{
				RecordDefaultBindings(controller, _defaultGamepadBindings);
				ControllerMap controllerMap2 = GetControllerMap(controller);
				ControlSettingsUtilities.ApplyBindings(controllerMap2, SettingsManager.Settings.Control.GamepadBindings);
				ControlSettingsUtilities.DeleteUnboundBindings(controllerMap2, SettingsManager.Settings.Control.GamepadBindingsUnbound);
			}
		}
		if (controller.type == ControllerType.Mouse)
		{
			RecordDefaultBindings(controller, _defaultMouseBindings);
			ControllerMap controllerMap3 = GetControllerMap(controller);
			ControlSettingsUtilities.ApplyBindings(controllerMap3, SettingsManager.Settings.Control.MouseBindings);
			ControlSettingsUtilities.DeleteUnboundBindings(controllerMap3, SettingsManager.Settings.Control.MouseBindingsUnbound);
		}
	}
}
