using System;
using UnityEngine;

namespace Lamb.UI.Alerts
{
	public abstract class AlertBadge<T> : AlertBadgeBase
	{
		[Serializable]
		public enum AlertMode
		{
			Single,
			Total
		}

		[SerializeField]
		protected AlertMode _alertMode;

		protected T _alert;

		private bool _configured;

		protected abstract AlertCategory<T> _source { get; }

		private void Start()
		{
			ConfigureSingle();
		}

		public void Configure(T alert)
		{
			if (!_configured && _alertMode == AlertMode.Single)
			{
				_alert = alert;
				if (HasAlertSingle())
				{
					base.gameObject.SetActive(true);
					AlertCategory<T> source = _source;
					source.OnAlertRemoved = (Action<T>)Delegate.Combine(source.OnAlertRemoved, new Action<T>(OnAlertRemoved));
				}
				else
				{
					base.gameObject.SetActive(false);
				}
				_configured = true;
			}
		}

		public void ConfigureSingle()
		{
			if (!_configured && _alertMode == AlertMode.Total)
			{
				if (HasAlertTotal())
				{
					base.gameObject.SetActive(true);
					AlertCategory<T> source = _source;
					source.OnAlertRemoved = (Action<T>)Delegate.Combine(source.OnAlertRemoved, new Action<T>(OnAlertRemoved));
				}
				else
				{
					base.gameObject.SetActive(false);
				}
				_configured = true;
			}
		}

		protected virtual bool HasAlertSingle()
		{
			return _source.HasAlert(_alert);
		}

		protected virtual bool HasAlertTotal()
		{
			return _source.Total > 0;
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		private void OnDestroy()
		{
			AlertCategory<T> source = _source;
			source.OnAlertRemoved = (Action<T>)Delegate.Remove(source.OnAlertRemoved, new Action<T>(OnAlertRemoved));
		}

		private void OnAlertRemoved(T alert)
		{
			if (_alertMode == AlertMode.Single)
			{
				if (alert.Equals(_alert))
				{
					if (base.gameObject != null)
					{
						base.gameObject.SetActive(false);
					}
					AlertCategory<T> source = _source;
					source.OnAlertRemoved = (Action<T>)Delegate.Remove(source.OnAlertRemoved, new Action<T>(OnAlertRemoved));
				}
			}
			else
			{
				base.gameObject.SetActive(_source.Total != 0);
			}
		}

		public bool TryRemoveAlert()
		{
			return _source.Remove(_alert);
		}

		public void ResetAlert()
		{
			_configured = false;
		}
	}
}
