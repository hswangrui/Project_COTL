using System;
using Rewired;
using Steamworks;
using UnityEngine;

public class GeneralInputSource : InputSource
{
	public Action<Controller> OnActiveControllerChanged;

	public static Action OnBindingsReset;

	private bool _mouseInputActive;

	public bool MouseInputActive
	{
		get
		{
			return _mouseInputActive;
		}
		set
		{
			_mouseInputActive = value;
		}
	}

	public bool InputIsController()
	{
		if (base._rewiredPlayer != null)
		{
			return InputIsController(GetLastActiveController());
		}
		return false;
	}

	public bool InputIsController(Controller controller)
	{
		if (controller != null)
		{
			return controller.type == ControllerType.Joystick;
		}
		return false;
	}

	public Controller GetController(ControllerType controllerType)
	{
		if (base._rewiredPlayer == null)
		{
			return null;
		}
		if (controllerType == ControllerType.Joystick)
		{
			foreach (Joystick joystick in base._rewiredPlayer.controllers.Joysticks)
			{
				if (joystick.isConnected && joystick.enabled)
				{
					return base._rewiredPlayer.controllers.GetController(controllerType, joystick.id);
				}
			}
		}
		return base._rewiredPlayer.controllers.GetController(controllerType, 0);
	}

	public Controller GetLastActiveController()
	{
		if (base._rewiredPlayer != null)
		{
			return base._rewiredPlayer.controllers.GetLastActiveController();
		}
		return null;
	}

	public ControllerMap GetMap(Controller controller)
	{
		return base._rewiredPlayer.controllers.maps.GetMap(controller, 0);
	}

	public ControllerMap GetControllerMapForCategory(int category, ControllerType controllerType)
	{
		return GetControllerMapForCategory(category, GetController(controllerType));
	}

	public ControllerMap GetControllerMapForCategory(int category, Controller controller)
	{
		switch (category)
		{
		case 0:
			return InputManager.Gameplay.GetControllerMap(controller);
		case 2:
			return InputManager.PhotoMode.GetControllerMap(controller);
		default:
			return InputManager.UI.GetControllerMap(controller);
		}
	}

	protected override void UpdateRewiredPlayer()
	{
		base.UpdateRewiredPlayer();
		base._rewiredPlayer.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
	}

	public void RemoveController(ControllerType controllerType)
	{
		base._rewiredPlayer.controllers.RemoveController(controllerType, 0);
	}

	public void AddController(ControllerType controllerType)
	{
		base._rewiredPlayer.controllers.AddController(controllerType, 0, false);
	}

	protected override void OnLastActiveControllerChanged(Player player, Controller controller)
	{
		if (controller != null && InputMapper.Default.status != InputMapper.Status.Listening)
		{
			Cursor.visible = controller.type == ControllerType.Keyboard || controller.type == ControllerType.Mouse;
			Action<Controller> onActiveControllerChanged = OnActiveControllerChanged;
			if (onActiveControllerChanged != null)
			{
				onActiveControllerChanged(controller);
			}
		}
	}

	public static ESteamInputType GetSteamInputType()
	{
		if (SteamInput.Init(false))
		{
			return SteamInput.GetInputTypeForHandle(SteamInput.GetControllerForGamepadIndex(0));
		}
		return ESteamInputType.k_ESteamInputType_Unknown;
	}

	public bool GetAnyButton()
	{
		if (base._rewiredPlayer != null)
		{
			Controller lastActiveController = GetLastActiveController();
			if (lastActiveController != null)
			{
				return lastActiveController.GetAnyButton();
			}
		}
		return false;
	}

	public Vector2 GetMousePosition()
	{
		return base._rewiredPlayer.controllers.Mouse.screenPosition;
	}

	public void ResetBindings()
	{
		base._rewiredPlayer.controllers.maps.LoadDefaultMaps(ControllerType.Keyboard);
		base._rewiredPlayer.controllers.maps.LoadDefaultMaps(ControllerType.Joystick);
		base._rewiredPlayer.controllers.maps.LoadDefaultMaps(ControllerType.Mouse);
		InputManager.Gameplay.ApplyAllBindings();
		InputManager.UI.ApplyAllBindings();
		InputManager.PhotoMode.ApplyAllBindings();
		Action onBindingsReset = OnBindingsReset;
		if (onBindingsReset != null)
		{
			onBindingsReset();
		}
	}
}
