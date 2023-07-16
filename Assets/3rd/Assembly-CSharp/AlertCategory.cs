using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
public abstract class AlertCategory<T>
{
	[NonSerialized]
	[XmlIgnore]
	public Action<T> OnAlertAdded;

	[NonSerialized]
	[XmlIgnore]
	public Action<T> OnAlertRemoved;

	public List<T> _alerts = new List<T>();

	public List<T> _singleAlerts = new List<T>();

	public virtual int Total
	{
		get
		{
			return _alerts.Count;
		}
	}

	protected virtual bool IsValidAlert(T alert)
	{
		return true;
	}

	public bool Add(T alert)
	{
		if (IsValidAlert(alert) && !_alerts.Contains(alert) && !_singleAlerts.Contains(alert))
		{
			_alerts.Add(alert);
			Action<T> onAlertAdded = OnAlertAdded;
			if (onAlertAdded != null)
			{
				onAlertAdded(alert);
			}
			return true;
		}
		return false;
	}

	public bool AddOnce(T alert)
	{
		if (Add(alert))
		{
			_singleAlerts.Add(alert);
			return true;
		}
		return false;
	}

	public bool Remove(T alert)
	{
		if (_alerts.Contains(alert))
		{
			while (_alerts.Contains(alert))
			{
				_alerts.Remove(alert);
			}
			Action<T> onAlertRemoved = OnAlertRemoved;
			if (onAlertRemoved != null)
			{
				onAlertRemoved(alert);
			}
			return true;
		}
		return false;
	}

	public virtual bool HasAlert(T alert)
	{
		return _alerts.Contains(alert);
	}

	public void Clear()
	{
		_alerts.Clear();
	}

	public void ClearAll()
	{
		_alerts.Clear();
		_singleAlerts.Clear();
	}
}
