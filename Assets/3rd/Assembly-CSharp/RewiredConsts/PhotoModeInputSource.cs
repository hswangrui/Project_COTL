namespace RewiredConsts
{

	public class PhotoModeInputSource : CategoryInputSource
	{
		protected override int Category => 2;

		public static int[] AllBindings => new int[13]
		{
		83, 77, 84, 76, 75, 90, 80, 85, 89, 91,
		88, 86, 87
		};

		public bool GetPlaceStickerButtonDown()
		{
			return GetButtonDown(85);
		}

		public bool GetPlaceStickerButtonUp()
		{
			return GetButtonUp(85);
		}

		public float GetStickerScaleAxis()
		{
			return GetAxis(87);
		}

		public float GetStickerRotateAxis()
		{
			return GetAxis(86);
		}

		public bool GetFlipStickerButtonDown()
		{
			return GetButtonDown(88);
		}

		public bool GetUndoButtonDown()
		{
			return GetButtonDown(89);
		}

		public bool GetSaveButtonDown()
		{
			return GetButtonDown(90);
		}

		public bool GetClearStickersButtonDown()
		{
			return GetButtonDown(91);
		}

		public bool GetTakePhotoButtonDown()
		{
			return GetButtonDown(76);
		}

		public bool GetDeletePhotoButtonDown()
		{
			return GetButtonDown(80);
		}

		public bool GetGalleryFolderButtonDown()
		{
			return GetButtonDown(75);
		}

		public float GetCameraHeightAxis()
		{
			return GetAxis(77);
		}

		public float GetFocusAxis()
		{
			return GetAxis(83);
		}

		public float GetCameraTiltAxis()
		{
			return GetAxis(84);
		}
	}
}