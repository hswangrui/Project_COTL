public class RewiredUIInputSource : CategoryInputSource
{
	protected override int Category
	{
		get
		{
			return 1;
		}
	}

	public static int[] AllBindings
	{
		get
		{
			return new int[7] { 34, 35, 43, 44, 38, 39, 56 };
		}
	}

	public float GetHorizontalAxis()
	{
		return GetAxis(35);
	}

	public float GetVerticalAxis()
	{
		return GetAxis(34);
	}

	public bool GetAcceptButtonDown()
	{
		return GetButtonDown(38);
	}

	public bool GetAcceptButtonHeld()
	{
		return GetButtonHeld(38);
	}

	public bool GetAcceptButtonUp()
	{
		return GetButtonUp(38);
	}

	public bool GetCancelButtonDown()
	{
		return GetButtonDown(39);
	}

	public bool GetCancelButtonHeld()
	{
		return GetButtonHeld(39);
	}

	public bool GetCancelButtonUp()
	{
		return GetButtonUp(39);
	}

	public bool GetPageNavigateLeftDown()
	{
		return GetButtonDown(43);
	}

	public bool GetPageNavigateLeftHeld()
	{
		return GetButtonHeld(43);
	}

	public bool GetPageNavigateRightHeld()
	{
		return GetButtonHeld(44);
	}

	public bool GetPageNavigateRightDown()
	{
		return GetButtonDown(44);
	}

	public bool GetResetAllSettingsButtonDown()
	{
		return GetButtonDown(49);
	}

	public bool GetAccountPickerButtonDown()
	{
		return GetButtonDown(52);
	}

	public bool GetEditBuildingsButtonDown()
	{
		return GetButtonDown(51);
	}

	public bool GetCookButtonDown()
	{
		return GetButtonDown(56);
	}

	public bool GetCancelBindingButtonDown()
	{
		return GetButtonDown(61);
	}

	public bool GetResetBindingButtonDown()
	{
		return GetButtonDown(60);
	}

	public bool GetUnbindButtonDown()
	{
		return GetButtonDown(65);
	}
}
