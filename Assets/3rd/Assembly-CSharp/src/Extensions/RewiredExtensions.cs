using Rewired;

namespace src.Extensions
{
	public static class RewiredExtensions
	{
		public static AxisRange ToAxisRange(this Pole pole)
		{
			if (pole == Pole.Positive)
			{
				return AxisRange.Positive;
			}
			return AxisRange.Negative;
		}

		public static ElementAssignmentType ToElementAssignmentType(this AxisType axisType)
		{
			if (axisType == AxisType.Split)
			{
				return ElementAssignmentType.SplitAxis;
			}
			return ElementAssignmentType.Button;
		}

		public static Binding ToBinding(this ActionElementMap actionElementMap)
		{
			Binding result = default(Binding);
			result.Action = actionElementMap.actionId;
			result.ControllerType = actionElementMap.controllerMap.controllerType;
			result.AxisContribution = actionElementMap.axisContribution;
			result.AxisRange = actionElementMap.axisRange;
			result.AxisType = actionElementMap.axisType;
			result.KeyCode = actionElementMap.keyCode;
			result.ElementIdentifierID = actionElementMap.elementIdentifierId;
			result.Category = actionElementMap.controllerMap.categoryId;
			return result;
		}

		public static UnboundBinding ToUnboundBinding(this ActionElementMap actionElementMap)
		{
			UnboundBinding result = default(UnboundBinding);
			result.Action = actionElementMap.actionId;
			result.AxisContribution = actionElementMap.axisContribution;
			result.Category = actionElementMap.controllerMap.categoryId;
			return result;
		}

		public static ActionElementMap GetActionElementMap(this ControllerMap controllerMap, int action, Pole axisContribution)
		{
			ActionElementMap[] elementMapsWithAction = controllerMap.GetElementMapsWithAction(action);
			foreach (ActionElementMap actionElementMap in elementMapsWithAction)
			{
				if (actionElementMap.axisContribution == axisContribution)
				{
					return actionElementMap;
				}
			}
			return null;
		}
	}
}
