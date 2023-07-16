using UnityEngine;

public class MMPageIndicators : MonoBehaviour
{
	[SerializeField]
	private MMPageIndicator[] _indicators;

	public void SetNumPages(int pages)
	{
		for (int i = 0; i < _indicators.Length; i++)
		{
			_indicators[i].gameObject.SetActive(i < pages);
		}
	}

	public void SetPage(int page)
	{
		for (int i = 0; i < _indicators.Length; i++)
		{
			if (i == page)
			{
				_indicators[i].Activate();
			}
			else
			{
				_indicators[i].Deactivate();
			}
		}
	}
}
