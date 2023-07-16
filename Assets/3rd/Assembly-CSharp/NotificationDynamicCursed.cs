using UnityEngine;

public class NotificationDynamicCursed : NotificationDynamicGeneric
{
	public Gradient ColourGradient;

	protected override Color GetColor(float norm)
	{
		return ColourGradient.Evaluate(norm);
	}
}
