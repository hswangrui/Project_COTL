using UnityEngine.UI;

namespace Lamb.UI
{
	public class SelectorGridLayout : GridLayoutGroup
	{
		public void OnValidate()
		{
			base.constraint = Constraint.FixedRowCount;
		}

		protected override void OnTransformChildrenChanged()
		{
			UpdateConstraints();
			base.OnTransformChildrenChanged();
		}

		public override void SetLayoutHorizontal()
		{
			UpdateConstraints();
			base.SetLayoutHorizontal();
		}

		public override void SetLayoutVertical()
		{
			UpdateConstraints();
			base.SetLayoutVertical();
		}

		private void UpdateConstraints()
		{
			int num = 0;
			for (int i = 0; i < base.transform.childCount; i++)
			{
				LayoutElement component;
				if (base.transform.GetChild(i).TryGetComponent<LayoutElement>(out component))
				{
					if (!component.ignoreLayout)
					{
						num++;
					}
				}
				else
				{
					num++;
				}
			}
			if (num <= 4)
			{
				base.constraintCount = 1;
			}
			else
			{
				base.constraintCount = 2;
			}
		}
	}
}
