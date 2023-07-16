using UnityEngine;
using UnityEngine.UI;

namespace DOGames.Scripts
{
	[RequireComponent(typeof(Text))]
	public class BasicFPS : MonoBehaviour
	{
		private const double FpsMeasurePeriod = 0.5;

		private double m_FpsNextPeriod;

		private int m_FpsAccumulator;

		private int m_CurrentFps;

		private Text m_Text;

		private const string display = "{0:D2}";

		private void Start()
		{
			m_Text = GetComponent<Text>();
			if (!m_Text.gameObject.activeSelf || !m_Text.enabled)
			{
				m_Text = null;
			}
		}

		private void Update()
		{
			m_FpsAccumulator++;
			if ((double)Time.realtimeSinceStartup > m_FpsNextPeriod)
			{
				m_CurrentFps = (int)((double)m_FpsAccumulator / 0.5);
				m_FpsAccumulator = 0;
				m_FpsNextPeriod = (double)Time.realtimeSinceStartup + 0.5;
				if (m_Text != null)
				{
					m_Text.text = $"{m_CurrentFps:D2}";
				}
			}
		}
	}
}
