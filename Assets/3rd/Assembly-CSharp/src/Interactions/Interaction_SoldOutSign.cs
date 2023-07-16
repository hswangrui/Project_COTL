using I2.Loc;

namespace src.Interactions
{
	public class Interaction_SoldOutSign : Interaction
	{
		private string sSoldOut;

		private void Start()
		{
			UpdateLocalisation();
		}

		public override void GetLabel()
		{
			base.Label = sSoldOut;
		}

		public override void UpdateLocalisation()
		{
			base.UpdateLocalisation();
			sSoldOut = ScriptLocalization.Interactions.SoldOut;
		}

		public override void OnBecomeCurrent()
		{
			base.OnBecomeCurrent();
			Interactable = false;
		}
	}
}
