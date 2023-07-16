namespace Lamb.UI
{
	public abstract class ControlsScreenBase : UISubmenuBase
	{
		public abstract bool ValidInputType(InputType inputType);

		public abstract void Configure(InputType inputType);

		public abstract void Configure(SettingsData.ControlSettings controlSettings);

		public abstract bool ShowBindingPrompts();
	}
}
