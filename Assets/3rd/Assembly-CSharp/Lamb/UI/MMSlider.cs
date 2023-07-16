using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class MMSlider : Slider
	{
		public enum ValueDisplayFormat
		{
			Percentage,
			RawValue,
			Custom
		}

		[SerializeField]
		private int _increment = 1;

		[SerializeField]
		private ValueDisplayFormat _valueDisplayFormat;

		[SerializeField]
		private TextMeshProUGUI _valueText;

		public Func<float, string> GetCustomDisplayFormat;

		public override float value
		{
			get
			{
				if (base.wholeNumbers)
				{
					return Mathf.Round(m_Value);
				}
				return m_Value;
			}
			set
			{
				Set(Mathf.Round(value / (float)_increment) * (float)_increment);
			}
		}

		protected override void Update()
		{
			base.Update();
			if (!(_valueText == null))
			{
				if (_valueDisplayFormat == ValueDisplayFormat.Percentage)
				{
					_valueText.text = string.Format("{0}%", value);
				}
				else if (_valueDisplayFormat == ValueDisplayFormat.RawValue)
				{
					_valueText.text = value.ToString();
				}
				else
				{
					_valueText.text = GetCustomDisplayFormat(value);
				}
			}
		}

		public void IncrementValue()
		{
			value = (float)Math.Round(value + (float)_increment, 2);
		}

		public void DecrementValue()
		{
			value = (float)Math.Round(value - (float)_increment, 2);
		}
	}
}
