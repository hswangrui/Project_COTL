using System;
using Rewired;
using src.Extensions;
using UnityEngine;

[Serializable]
public struct Binding
{
	public KeyCode KeyCode;

	public ControllerType ControllerType;

	public Pole AxisContribution;

	public AxisRange AxisRange;

	public AxisType AxisType;

	public int Category;

	public int Action;

	public int ElementIdentifierID;

	public ElementAssignment ToElementAssigment()
	{
		if (ControllerType == ControllerType.Keyboard)
		{
			ElementAssignment result = default(ElementAssignment);
			result.keyboardKey = KeyCode;
			result.actionId = Action;
			result.axisContribution = AxisContribution;
			result.axisRange = AxisRange;
			return result;
		}
		if (ControllerType == ControllerType.Mouse || ControllerType == ControllerType.Joystick)
		{
			ElementAssignment result = default(ElementAssignment);
			result.elementIdentifierId = ElementIdentifierID;
			result.actionId = Action;
			result.axisContribution = AxisContribution;
			result.axisRange = AxisRange;
			result.type = AxisType.ToElementAssignmentType();
			return result;
		}
		return default(ElementAssignment);
	}

	public ElementAssignment ToElementAssignment(ActionElementMap actionElementMap)
	{
		ElementAssignment result = ToElementAssigment();
		if (actionElementMap != null)
		{
			result.elementMapId = actionElementMap.id;
		}
		return result;
	}
}
