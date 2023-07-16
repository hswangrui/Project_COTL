using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class Buttons
{
	public GameObject Button;

	public bool isSetting;

	public int Index;

	public bool selected;

	public buttons buttonTypes;

	public Button nextButton;

	public Button prevButton;

	public Slider slider;

	public Button SwitchButton;

	public TMP_InputField inputField;

	public bool canUse = true;
}
public enum buttons
{
	Null,
	HorizontalSelector,
	Slider,
	Toggle,
	InputField
}
