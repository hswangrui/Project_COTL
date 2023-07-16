using I2.Loc;

namespace Lamb.UI
{
	public class UIMysticSellerNameMenuController : UICultNameMenuController
	{
		public override void Awake()
		{
			base.Awake();
			_nameInputField.text = LocalizationManager.GetTranslation("NAMES/MysticShopSellerDefault");
		}
	}
}
