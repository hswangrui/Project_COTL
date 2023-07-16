using System.IO;
using MMTools;
using TMPro;
using UnityEngine;

public class InputTextFocusOnStart : BaseMonoBehaviour
{
	public TMP_InputField TMP_InputField;

	private void Start()
	{
		TMP_InputField.ActivateInputField();
	}

	public void SaveEmail()
	{
		if (!MMTransition.IsPlaying)
		{
			string text = TMP_InputField.text;
			StreamWriter streamWriter = new StreamWriter(Application.persistentDataPath + "/emails.txt", true);
			streamWriter.Write(text + ", ");
			streamWriter.Close();
		}
	}
}
