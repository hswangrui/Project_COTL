using System;
using System.Collections;
using System.IO;
using I2.Loc;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.VideoioModule;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI.VideoMenu
{
	public class UIVideoExportMenuController : UIMenuBase
	{
		[Header("Menu")]
		[SerializeField]
		private RawImage _imageHolder;

		[SerializeField]
		private CanvasGroup _progressCanvasGroup;

		[SerializeField]
		private CanvasGroup _buttonContainerCanvasGroup;

		[SerializeField]
		private UIMenuControlPrompts _controlIconsCanvasGroup;

		[SerializeField]
		private LoadingIcon _progressBar;

		[SerializeField]
		private TextMeshProUGUI _progressText;

		[SerializeField]
		private TextMeshProUGUI _completeText;

		[SerializeField]
		private TextMeshProUGUI _buttonText;

		[Header("Buttons")]
		[SerializeField]
		private MMButton _exportButton;

		private int screenWidth = 2560;

		private int screenHeight = 1440;

		private int amountOfScreenshots = 100;

		private bool _videoExported;

		private Texture2D tex;

		private Texture2D oldTex;

		private bool exportingVideo;

		private void Start()
		{
			StartCoroutine(ShowImages());
		}

		public void Show(int currentDay = 100, bool immediate = false)
		{
			amountOfScreenshots = currentDay;
			if (_canvas != null && !UIMenuBase.ActiveMenus.Contains(this))
			{
				UIMenuBase.ActiveMenus.Add(this);
				UpdateSortingOrder();
			}
			base.gameObject.SetActive(true);
			StopAllCoroutines();
			Show(immediate);
		}

		protected override void OnShowStarted()
		{
			_completeText.text = ScriptLocalization.UI_ExportVideo.Loading;
			_buttonText.text = ScriptLocalization.UI_ExportVideo.Export;
			_buttonContainerCanvasGroup.alpha = 0f;
			_progressCanvasGroup.alpha = 0f;
			_exportButton.onClick.AddListener(exportVideo);
		}

		private void exportVideo()
		{
			if (!exportingVideo)
			{
				if (_videoExported)
				{
					Hide();
					return;
				}
				_progressCanvasGroup.alpha = 1f;
				_buttonContainerCanvasGroup.alpha = 0f;
				_imageHolder.color = StaticColors.GreyColor;
				_controlIconsCanvasGroup.GetComponent<CanvasGroup>().alpha = 0f;
				StartCoroutine(exportVideoRoutine());
			}
		}

		private IEnumerator ShowImages()
		{
			while (!exportingVideo)
			{
				int i = 0;
				while (i < amountOfScreenshots)
				{
					string text = Path.Combine(Application.persistentDataPath, "Screenshots", "day_" + i + "_" + SaveAndLoad.SAVE_SLOT + ".jpeg");
					if (exportingVideo)
					{
						yield break;
					}
					if (File.Exists(text))
					{
						_buttonContainerCanvasGroup.alpha = 1f;
						_completeText.text = "";
						_imageHolder.color = Color.white;
						if (tex != null)
						{
							UnityEngine.Object.Destroy(tex);
						}
						if (oldTex != null)
						{
							UnityEngine.Object.Destroy(oldTex);
						}
						tex = null;
						oldTex = null;
						Debug.Log("File does exist: " + text);
						byte[] data = File.ReadAllBytes(text);
						tex = new Texture2D(2560, 1440, TextureFormat.RGBA32, false);
						oldTex = new Texture2D(2, 2);
						oldTex.LoadImage(data);
						tex.SetPixels(oldTex.GetPixels());
						tex.Apply();
						_imageHolder.texture = tex;
						yield return new WaitForSecondsRealtime(0.2f);
					}
					yield return null;
					int num = i + 1;
					i = num;
				}
				yield return null;
			}
		}

		private IEnumerator exportVideoRoutine()
		{
			exportingVideo = true;
			OpenCVForUnity.UnityUtils.Utils.setDebugMode(true, true);
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
			string encodedVideoFilePath = Path.Combine(folderPath, DataManager.Instance.CultName + "_" + SaveAndLoad.SAVE_SLOT + ".mp4");
			Size frameSize = new Size(screenWidth, screenHeight);
			Mat frame = new Mat(screenHeight, screenWidth, CvType.CV_8UC4);
			VideoWriter outputVideo = new VideoWriter(encodedVideoFilePath, VideoWriter.fourcc('X', '2', '6', '4'), 5.0, frameSize);
			outputVideo.set(1900, 1.0);
			if (!outputVideo.isOpened())
			{
				Debug.Log("Could not open the output video for write");
			}
			int i = 0;
			while (i < amountOfScreenshots)
			{
				string text = Path.Combine(Application.persistentDataPath, "Screenshots", "day_" + i + "_" + SaveAndLoad.SAVE_SLOT + ".jpeg");
				if (File.Exists(text))
				{
					Debug.Log("File does exist: " + text);
					byte[] data = File.ReadAllBytes(text);
					Texture2D texture2D = new Texture2D(screenHeight, screenWidth, TextureFormat.RGBA32, false);
					Texture2D texture2D2 = new Texture2D(2, 2);
					texture2D2.LoadImage(data);
					texture2D.SetPixels(texture2D2.GetPixels());
					texture2D.Apply();
					OpenCVForUnity.UnityUtils.Utils.fastTexture2DToMat(texture2D, frame);
					Imgproc.cvtColor(frame, frame, 2);
					Imgproc.putText(frame, i.ToString(), new Point(frame.cols() - 70, 30.0), 0, 1.0, new Scalar(255.0, 255.0, 255.0), 2, 16, false);
					Imgproc.putText(frame, DataManager.Instance.CultName, new Point(frame.cols() / 2, 30.0), 0, 1.0, new Scalar(255.0, 255.0, 255.0), 2, 16, false);
					outputVideo.write(frame);
					UnityEngine.Object.Destroy(texture2D);
					UnityEngine.Object.Destroy(texture2D2);
					_progressBar.UpdateProgress((float)i / (float)amountOfScreenshots);
					_progressText.text = "%  " + (float)i / (float)amountOfScreenshots * 100f;
					Debug.Log("% Complete = " + (float)i / (float)amountOfScreenshots);
					yield return new WaitForEndOfFrame();
				}
				else
				{
					Debug.Log("File doesnt exist: " + encodedVideoFilePath);
				}
				int num = i + 1;
				i = num;
			}
			_progressCanvasGroup.alpha = 0f;
			_buttonText.text = ScriptLocalization.Interactions.Done;
			_buttonContainerCanvasGroup.alpha = 1f;
			_controlIconsCanvasGroup.GetComponent<CanvasGroup>().alpha = 1f;
			outputVideo.release();
			outputVideo.Dispose();
			if (outputVideo.IsDisposed)
			{
				string text2 = string.Format(ScriptLocalization.UI_ExportVideo.Exported, encodedVideoFilePath);
				_completeText.text = text2;
			}
			else
			{
				_completeText.text = ScriptLocalization.UI_ExportVideo.Failed;
			}
			exportingVideo = false;
			_videoExported = true;
		}

		public override void OnCancelButtonInput()
		{
			if (!exportingVideo && _canvasGroup.interactable)
			{
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}
}
