using UnityEngine;
using UnityFx.Outline;

public class AddOutline : MonoBehaviour
{
	protected OutlineEffect Outliner;

	public Interaction interaction;

	public int Layer = 2;

	public GameObject OutlineTarget;

	private bool removed;

	private void OnEnable()
	{
		if (Outliner == null)
		{
			Outliner = Camera.main.GetComponent<OutlineEffect>();
		}
		if (Outliner != null)
		{
			Outliner.OutlineLayers[Layer].Add((OutlineTarget == null) ? base.gameObject : OutlineTarget);
		}
		interaction.indicateHighlightEnd.AddListener(Highlighted);
		interaction.indicateHighlight.AddListener(HighlightedRemoved);
	}

	private void Highlighted()
	{
		Debug.Log("Highlighted");
		if (Outliner != null)
		{
			Outliner.OutlineLayers[Layer].Remove((OutlineTarget == null) ? base.gameObject : OutlineTarget);
		}
		Outliner.OutlineLayers[Layer].Remove(base.gameObject);
	}

	private void HighlightedRemoved()
	{
		Debug.Log("HighlightRemoved");
		Outliner.OutlineLayers[Layer].Remove((OutlineTarget == null) ? base.gameObject : OutlineTarget);
		Outliner.OutlineLayers[Layer].Remove(base.gameObject);
	}

	private void OnDisable()
	{
		if (Outliner != null)
		{
			Outliner.OutlineLayers[Layer].Remove((OutlineTarget == null) ? base.gameObject : OutlineTarget);
		}
		interaction.indicateHighlightEnd.RemoveListener(Highlighted);
		interaction.indicateHighlight.RemoveListener(HighlightedRemoved);
	}
}
