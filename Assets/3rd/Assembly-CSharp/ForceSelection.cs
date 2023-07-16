using System.Collections;
using Unify.Input;
using UnityEngine;
using UnityEngine.EventSystems;

public class ForceSelection : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The default selected object")]
	private GameObject DefaultOption;

	[SerializeField]
	[Tooltip("The other possible selected object")]
	private GameObject OtherOption;

	private GameObject previouslySelected;

	[SerializeField]
	[Tooltip("Other Options that could be selected.")]
	private GameObject[] OtherOptions;

	private void OnEnable()
	{
		if (EventSystem.current.currentSelectedGameObject != null)
		{
			Debug.Log("previouslySelected = " + EventSystem.current.currentSelectedGameObject.name);
			previouslySelected = EventSystem.current.currentSelectedGameObject;
		}
		StartCoroutine(DelaySelection());
	}

	private void Update()
	{
		if (RewiredInputManager.MainPlayer != null && EventSystem.current.currentSelectedGameObject != DefaultOption && EventSystem.current.currentSelectedGameObject != OtherOption && !AnyOtherOptionSelected())
		{
			EventSystem.current.SetSelectedGameObject(DefaultOption);
		}
	}

	private void OnDisable()
	{
		if (previouslySelected != null)
		{
			Debug.Log("OnDisable SetSelected to " + previouslySelected.name);
			EventSystem.current.SetSelectedGameObject(previouslySelected);
		}
	}

	private bool AnyOtherOptionSelected()
	{
		GameObject[] otherOptions = OtherOptions;
		foreach (GameObject gameObject in otherOptions)
		{
			if (EventSystem.current.currentSelectedGameObject == gameObject)
			{
				return true;
			}
		}
		return false;
	}

	private IEnumerator DelaySelection()
	{
		yield return new WaitForSeconds(0.1f);
		EventSystem.current.SetSelectedGameObject(null);
		EventSystem.current.SetSelectedGameObject(DefaultOption);
		Debug.Log("Delay Set Selected to " + DefaultOption.name);
	}
}
