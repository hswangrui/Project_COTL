using System;
using I2.Loc;
using Lamb.UI.Alerts;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Lamb.UI
{
	[ExecuteInEditMode]
	public class WorldMapIcon : BaseMonoBehaviour, ISelectHandler, IEventSystemHandler
	{
		public enum WorldMapRegion
		{
			Base,
			Ratau_Hut,
			Shore,
			Sozo,
			Midas_Cave,
			Plimbo_Shop
		}

		public Action<WorldMapIcon> OnLocationSelected;

		[SerializeField]
		private RectTransform _rectTransform;

		[SerializeField]
		private RectTransform _localPoint;

		[SerializeField]
		private FollowerLocation _location;

		[SerializeField]
		[TermsPopup("")]
		private string _locationTerm;

		[SerializeField]
		private WorldMapRegion _mapRegion;

		[SerializeField]
		private InspectorScene _scene;

		[SerializeField]
		private LocationAlert _alert;

		[SerializeField]
		private MMButton _button;

		[SerializeField]
		private ParallaxLayer _layer;

		[SerializeField]
		private Vector2 _parallaxPosition;

		[SerializeField]
		private GameObject _youAreHere;

		public RectTransform RectTransform
		{
			get
			{
				return _rectTransform;
			}
		}

		public FollowerLocation Location
		{
			get
			{
				return _location;
			}
		}

		public WorldMapRegion MapRegion
		{
			get
			{
				return _mapRegion;
			}
		}

		public string LocationTerm
		{
			get
			{
				return _locationTerm;
			}
		}

		public MMButton Button
		{
			get
			{
				return _button;
			}
		}

		public InspectorScene Scene
		{
			get
			{
				return _scene;
			}
		}

		public Vector2 ParallaxPosition
		{
			get
			{
				return _parallaxPosition;
			}
		}

		private void Start()
		{
			if (Application.isPlaying)
			{
				_alert.Configure(_location);
				_button.onClick.AddListener(OnButtonClicked);
				_youAreHere.SetActive(_location == DataManager.Instance.CurrentLocation);
			}
		}

		private void OnButtonClicked()
		{
			Action<WorldMapIcon> onLocationSelected = OnLocationSelected;
			if (onLocationSelected != null)
			{
				onLocationSelected(this);
			}
		}

		private void Update()
		{
			if (_localPoint != null)
			{
				_rectTransform.anchoredPosition = _rectTransform.parent.InverseTransformPoint(_localPoint.parent.TransformPoint(_localPoint.localPosition));
			}
		}

		public string RegionMaterialProperty()
		{
			return string.Format("_{0}", _mapRegion);
		}

		public string GetLocalisedLocation()
		{
			if (_location == FollowerLocation.Base)
			{
				if (string.IsNullOrEmpty(DataManager.Instance.CultName))
				{
					return LocalizationManager.GetTranslation("NAMES/Place/Cult");
				}
				return DataManager.Instance.CultName;
			}
			return LocalizationManager.GetTranslation(_locationTerm);
		}

		public void OnSelect(BaseEventData eventData)
		{
			_alert.TryRemoveAlert();
		}
	}
}
