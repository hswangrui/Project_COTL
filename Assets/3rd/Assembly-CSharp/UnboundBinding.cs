using System;
using Rewired;

[Serializable]
public struct UnboundBinding
{
	public int Action;

	public int Category;

	public Pole AxisContribution;

	public UnboundBinding(int action, int category, Pole axisContribution)
	{
		Action = action;
		Category = category;
		AxisContribution = axisContribution;
	}
}
