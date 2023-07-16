using UnityEngine;

namespace Lamb.UI.Assets
{
	[CreateAssetMenu(fileName = "Tutorial Configuration", menuName = "Massive Monster/Tutorial Configuration", order = 1)]
	public class TutorialConfiguration : ScriptableObject
	{
		[SerializeField]
		private TutorialCategory[] _categories;

		public TutorialCategory[] Categories
		{
			get
			{
				return _categories;
			}
		}

		public TutorialCategory GetCategory(TutorialTopic topic)
		{
			TutorialCategory[] categories = _categories;
			foreach (TutorialCategory tutorialCategory in categories)
			{
				if (tutorialCategory.Topic == topic)
				{
					return tutorialCategory;
				}
			}
			return null;
		}
	}
}
