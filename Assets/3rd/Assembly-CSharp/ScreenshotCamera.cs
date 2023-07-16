using System;
using System.Collections;
using System.IO;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.VideoioModule;
using UnityEngine;

public class ScreenshotCamera : MonoBehaviour
{
	private static ScreenshotCamera _Instance;

	public Camera ScreenshotCameraObject;

	private int screenWidth = 2560;

	private int screenHeight = 1440;

	private Coroutine screenshotRoutine;

	public static ScreenshotCamera Instance
	{
		get
		{
			return _Instance;
		}
		set
		{
			_Instance = value;
		}
	}

	private void Start()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Combine(TimeManager.OnNewPhaseStarted, new Action(OnNewPhase));
		ScreenshotCameraObject.gameObject.SetActive(false);
		Instance = this;
		OnNewPhase();
	}

	private void OnDestroy()
	{
		TimeManager.OnNewPhaseStarted = (Action)Delegate.Remove(TimeManager.OnNewPhaseStarted, new Action(OnNewPhase));
	}

	private void OnNewPhase()
	{
		if (!(PlayerFarming.Instance == null) && DataManager.Instance.DateLastScreenshot != TimeManager.CurrentDay && TimeManager.IsDay && screenshotRoutine == null)
		{
			screenshotRoutine = StartCoroutine(WaitToTakeScreenshot());
		}
	}

	private IEnumerator WaitToTakeScreenshot()
	{
		while (PlayerFarming.Location != FollowerLocation.Base)
		{
			yield return null;
		}
		screenshotRoutine = null;
		TakeScreenshot();
	}

	public void ExportVideoCV()
	{
		StartCoroutine(ExportVideoCVRoutine());
	}

	private IEnumerator ExportVideoCVRoutine()
	{
		OpenCVForUnity.UnityUtils.Utils.setDebugMode(true, true);
		string encodedVideoFilePath = Path.Combine(Application.persistentDataPath, DataManager.Instance.CultName + "_" + SaveAndLoad.SAVE_SLOT + ".mp4");
		Size frameSize = new Size(screenWidth, screenHeight);
		Mat frame = new Mat(screenHeight, screenWidth, CvType.CV_8UC4);
		VideoWriter outputVideo = new VideoWriter(encodedVideoFilePath, VideoWriter.fourcc('X', '2', '6', '4'), 5.0, frameSize);
		outputVideo.set(1900, 1.0);
		outputVideo.set(47, 8000.0);
		outputVideo.set(1, 50.0);
		if (!outputVideo.isOpened())
		{
			Debug.Log("Could not open the output video for write");
		}
		int i = 0;
		while (i < TimeManager.CurrentDay)
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
				Debug.Log("% Complete = " + (float)i / (float)TimeManager.CurrentDay);
				yield return new WaitForEndOfFrame();
			}
			else
			{
				Debug.Log("File doesnt exist: " + encodedVideoFilePath);
			}
			int num = i + 1;
			i = num;
		}
		outputVideo.release();
		outputVideo.Dispose();
		if (outputVideo.IsDisposed)
		{
			Debug.Log("Video Exported: " + encodedVideoFilePath);
		}
	}

	public static string ScreenShotName()
	{
		string text = Path.Combine(Application.persistentDataPath, "Screenshots");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return string.Format(Path.Combine(text, "day_{0}_{1}.jpeg"), TimeManager.CurrentDay.ToString(), SaveAndLoad.SAVE_SLOT);
	}

	public void TakeScreenshotCV()
	{
		DataManager.Instance.DateLastScreenshot = TimeManager.CurrentDay;
		ScreenshotCameraObject.gameObject.SetActive(true);
		RenderTexture renderTexture = new RenderTexture(screenWidth, screenHeight, 24);
		ScreenshotCameraObject.targetTexture = renderTexture;
		Texture2D texture2D = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
		ScreenshotCameraObject.Render();
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new UnityEngine.Rect(0f, 0f, screenWidth, screenHeight), 0, 0);
		ScreenshotCameraObject.targetTexture = null;
		RenderTexture.active = null;
		UnityEngine.Object.Destroy(renderTexture);
		Mat mat = new Mat(screenHeight, screenWidth, CvType.CV_8UC3);
		OpenCVForUnity.UnityUtils.Utils.fastTexture2DToMat(texture2D, mat);
		Imgproc.cvtColor(mat, mat, 2);
		MatOfInt matOfInt = new MatOfInt();
		matOfInt.push_back(new MatOfInt(16));
		matOfInt.push_back(new MatOfInt(9));
		matOfInt.push_back(new MatOfInt(17));
		matOfInt.push_back(new MatOfInt(3));
		matOfInt.push_back(new MatOfInt(18));
		matOfInt.push_back(new MatOfInt(default(int)));
		OpenCVForUnity.UnityUtils.Utils.setDebugMode(true, true);
		if (Imgcodecs.imwrite(ScreenShotName(), mat, matOfInt))
		{
			Debug.Log(string.Format("Took screenshot to: {0}", ScreenShotName()));
		}
		else
		{
			Debug.Log(string.Format("Failed to take screenshot to: {0}", ScreenShotName()));
		}
		ScreenshotCameraObject.gameObject.SetActive(false);
	}

	public void TakeScreenshot()
	{
		StartCoroutine(TakeScreenshotRoutine());
	}

	private IEnumerator TakeScreenshotRoutine()
	{
		Debug.Log("Taking Screenshit");
		DataManager.Instance.DateLastScreenshot = TimeManager.CurrentDay;
		ScreenshotCameraObject.gameObject.SetActive(true);
		RenderTexture renderTexture = new RenderTexture(screenWidth, screenHeight, 24);
		ScreenshotCameraObject.targetTexture = renderTexture;
		Texture2D screenshot = new Texture2D(screenWidth, screenHeight, TextureFormat.RGB24, false);
		ScreenshotCameraObject.Render();
		RenderTexture.active = renderTexture;
		screenshot.ReadPixels(new UnityEngine.Rect(0f, 0f, screenWidth, screenHeight), 0, 0);
		ScreenshotCameraObject.targetTexture = null;
		RenderTexture.active = null;
		UnityEngine.Object.Destroy(renderTexture);
		yield return null;
		byte[] bytes = screenshot.EncodeToJPG(75);
		string text = ScreenShotName();
		File.WriteAllBytes(text, bytes);
		Debug.Log(string.Format("Took screenshot to: {0}", text));
		yield return null;
		ScreenshotCameraObject.gameObject.SetActive(false);
		UnityEngine.Object.Destroy(screenshot);
	}
}
