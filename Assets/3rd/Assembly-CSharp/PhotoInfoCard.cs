using Lamb.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhotoInfoCard : UIInfoCardBase<PhotoModeManager.PhotoData>
{
	[SerializeField]
	private TMP_Text photoTitle;

	[SerializeField]
	private RawImage photoImage;

	public override void Configure(PhotoModeManager.PhotoData config)
	{
		if (config != null)
		{
			photoTitle.text = config.PhotoName;
			photoImage.texture = config.PhotoTexture;
		}
	}
}
