using System;
using Rewired;
using Unify;

public abstract class InputSource
{
	private UnifyManager unifyManager;

	protected Player _rewiredPlayer
	{
		get
		{
			if (ReInput.isReady && ReInput.players != null)
			{
				return ReInput.players.GetPlayer(0);
			}
			return null;
		}
	}

	protected InputSource()
	{
		unifyManager = UnifyManager.instance;
		UnifyManager obj = unifyManager;
		obj.OnUserControllerConnected = (UnifyManager.UserControllerConnected)Delegate.Combine(obj.OnUserControllerConnected, new UnifyManager.UserControllerConnected(OnUserControllerConnected));
		UserHelper.OnPlayerGamePadChanged = (UserHelper.PlayerGamePadChangedDelegate)Delegate.Combine(UserHelper.OnPlayerGamePadChanged, new UserHelper.PlayerGamePadChangedDelegate(HandlePlayerGamePadChanged));
		ReInput.ControllerConnectedEvent += OnControllerConnected;
	}

	private void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		_rewiredPlayer.controllers.AddController(ReInput.controllers.GetJoystick(args.controllerId), true);
	}

	~InputSource()
	{
		UnifyManager obj = unifyManager;
		obj.OnUserControllerConnected = (UnifyManager.UserControllerConnected)Delegate.Remove(obj.OnUserControllerConnected, new UnifyManager.UserControllerConnected(OnUserControllerConnected));
	}

	private void HandlePlayerGamePadChanged(int playerNo, User user)
	{
		UpdateRewiredPlayer();
	}

	public void OnUserControllerConnected(int playerNo, User user, bool connected)
	{
		UpdateRewiredPlayer();
	}

	public void OnPlayerUserChanged(int playerNo, User was, User now)
	{
		UpdateRewiredPlayer();
	}

	protected virtual void UpdateRewiredPlayer()
	{
		OnLastActiveControllerChanged();
		_rewiredPlayer.controllers.AddLastActiveControllerChangedDelegate(OnLastActiveControllerChanged);
	}

	protected virtual void OnLastActiveControllerChanged(Player player, Controller controller)
	{
	}

	protected virtual void OnLastActiveControllerChanged()
	{
	}

	protected bool GetButtonDown(int button)
	{
		if (_rewiredPlayer != null)
		{
			return _rewiredPlayer.GetButtonDown(button);
		}
		return false;
	}

	protected bool GetButtonHeld(int button)
	{
		if (_rewiredPlayer != null)
		{
			return _rewiredPlayer.GetButton(button);
		}
		return false;
	}

	protected bool GetButtonUp(int button)
	{
		if (_rewiredPlayer != null)
		{
			return _rewiredPlayer.GetButtonUp(button);
		}
		return false;
	}

	protected float GetAxis(int axis)
	{
		if (_rewiredPlayer != null)
		{
			return _rewiredPlayer.GetAxis(axis);
		}
		return 0f;
	}
}
