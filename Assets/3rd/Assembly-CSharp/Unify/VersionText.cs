using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Unify
{
	public class VersionText : MonoBehaviour
	{
		public static string versionString;

		private static string librarySemanticVersionString;

		public void Awake()
		{
			InitVersionStrings();
		}

		public void Start()
		{
			InitVersionStrings();
			Text component = GetComponent<Text>();
			if (component != null)
			{
				component.text = component.text.Replace("{version}", versionString);
				component.text = component.text.Replace("{librarySemanticVersion}", librarySemanticVersionString);
			}
			TextMeshProUGUI component2 = GetComponent<TextMeshProUGUI>();
			if (component2 != null)
			{
				component2.text = component2.text.Replace("{version}", versionString);
				component2.text = component2.text.Replace("{librarySemanticVersion}", librarySemanticVersionString);
			}
		}

		private void InitVersionStrings()
		{
			if (versionString == null)
			{
				TextAsset textAsset = (TextAsset)Resources.Load("version");
				if (textAsset != null)
				{
					string[] array = textAsset.text.Split('\n');
					string text = "(null)";
					string text2 = "";
					if (array.Length != 0)
					{
						text = array[0].Trim();
					}
					if (array.Length > 1)
					{
						text2 = " / " + array[1].Trim();
					}
					versionString = text + text2;
					Debug.Log("Game Version: " + versionString);
				}
			}
			if (librarySemanticVersionString == null)
			{
				if (UnifyManager.Instance != null)
				{
					librarySemanticVersionString = UnifyManager.Instance.GetLibrarySemanticVersion();
					Debug.Log("Unify Library Semantic Version: " + librarySemanticVersionString);
				}
				else
				{
					Debug.LogWarning("Unify VersionText.Awake(): UnifyManager not ready.");
				}
			}
		}
	}
}
