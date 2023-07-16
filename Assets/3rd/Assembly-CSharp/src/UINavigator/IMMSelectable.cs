using UnityEngine;
using UnityEngine.UI;

namespace src.UINavigator
{
	public interface IMMSelectable
	{
		Selectable Selectable { get; }

		bool Interactable { get; set; }

		bool TryPerformConfirmAction();

		IMMSelectable TryNavigateLeft();

		IMMSelectable TryNavigateRight();

		IMMSelectable TryNavigateUp();

		IMMSelectable TryNavigateDown();

		IMMSelectable FindSelectableFromDirection(Vector3 direction);

		void SetNormalTransitionState();

		void SetInteractionState(bool state);
	}
}
