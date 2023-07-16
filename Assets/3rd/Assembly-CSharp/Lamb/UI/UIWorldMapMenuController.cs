using System;
using System.Collections;
using DG.Tweening;
using I2.Loc;
using MMTools;
using TMPro;
using UnityEngine;

namespace Lamb.UI
{
	public class UIWorldMapMenuController : UIMenuBase
	{
		private const float kLocationFocusTime = 0.5f;

		public static readonly FollowerLocation[] UnlockableMapLocations = new FollowerLocation[5]
		{
			FollowerLocation.Hub1_RatauOutside,
			FollowerLocation.Hub1_Sozo,
			FollowerLocation.HubShore,
			FollowerLocation.Dungeon_Decoration_Shop1,
			FollowerLocation.Dungeon_Location_4
		};

		[Header("World Map Menu")]
		[SerializeField]
		private WorldMapIcon[] _locations;

		[SerializeField]
		private WorldMapClouds[] _clouds;

		[SerializeField]
		private AnimationCurve _focusCurve;

		[SerializeField]
		private UIMenuControlPrompts _controlPrompts;

		[Header("Map Controls")]
		[SerializeField]
		private RectTransform _mapContainer;

		[SerializeField]
		private WorldMapParallax _parallax;

		[Header("Header")]
		[SerializeField]
		private TextMeshProUGUI _locationHeader;

		[SerializeField]
		private RectTransform _locationHeaderContainer;

		[SerializeField]
		private CanvasGroup _locationHeaderCanvasGroup;

		[Header("Animations")]
		[SerializeField]
		private GameObject _flames;

		private FollowerLocation _revealLocation = FollowerLocation.None;

		private bool _reReveal;

		private Vector2 _locationHeaderOrigin;

		private bool _didCancel;

		private Coroutine _focusCoroutine;

		private void Start()
		{
			_canvasGroup.alpha = 0f;
			_flames.SetActive(DataManager.Instance.Lighthouse_Lit);
			WorldMapIcon[] locations = _locations;
			foreach (WorldMapIcon location in locations)
			{
				WorldMapIcon worldMapIcon = location;
				worldMapIcon.OnLocationSelected = (Action<WorldMapIcon>)Delegate.Combine(worldMapIcon.OnLocationSelected, new Action<WorldMapIcon>(OnLocationSelected));
				MMButton button = location.Button;
				button.OnSelected = (Action)Delegate.Combine(button.OnSelected, (Action)delegate
				{
					OnLocationHighlighted(location);
				});
				if (!DataManager.Instance.DiscoveredLocations.Contains(location.Location) && _revealLocation != location.Location)
				{
					location.gameObject.SetActive(false);
				}
				if (_revealLocation != FollowerLocation.None)
				{
					location.gameObject.SetActive(false);
				}
			}
		}

		public void Show(FollowerLocation revealLocation, bool reReveal = false, bool instant = false)
		{
			_revealLocation = revealLocation;
			_reReveal = reReveal;
			_controlPrompts.HideAcceptButton();
			_controlPrompts.HideCancelButton();
			Show(instant);
		}

		protected override void OnShowStarted()
		{
			WorldMapClouds[] clouds = _clouds;
			foreach (WorldMapClouds worldMapClouds in clouds)
			{
				if (DataManager.Instance.DiscoveredLocations.Contains(worldMapClouds.Location) && _revealLocation != worldMapClouds.Location)
				{
					worldMapClouds.Hide();
				}
				if (DataManager.Instance.DiscoveredLocations.Contains(worldMapClouds.Location) && _revealLocation == worldMapClouds.Location && _reReveal)
				{
					worldMapClouds.Hide();
				}
			}
		}

		protected override void OnShowCompleted()
		{
			FollowerLocation followerLocation = ((_revealLocation != FollowerLocation.None) ? _revealLocation : DataManager.Instance.CurrentLocation);
			WorldMapIcon[] locations = _locations;
			foreach (WorldMapIcon worldMapIcon in locations)
			{
				if (worldMapIcon.Location == followerLocation)
				{
					OverrideDefault(worldMapIcon.Button);
					break;
				}
			}
			ActivateNavigation();
		}

		protected override IEnumerator DoShowAnimation()
		{
			if (_revealLocation != FollowerLocation.None)
			{
				_canvasGroup.interactable = false;
				_mapContainer.localScale = Vector3.one * 0.65f;
				_locationHeaderOrigin = _locationHeaderContainer.anchoredPosition;
				_locationHeaderCanvasGroup.alpha = 0f;
			}
			while (_canvasGroup.alpha < 1f)
			{
				_canvasGroup.alpha += Time.unscaledDeltaTime * 4f;
				yield return null;
			}
			if (_revealLocation == FollowerLocation.None)
			{
				yield break;
			}
			WorldMapIcon targetIcon = null;
			WorldMapIcon[] locations = _locations;
			foreach (WorldMapIcon worldMapIcon in locations)
			{
				if (worldMapIcon.Location == _revealLocation)
				{
					targetIcon = worldMapIcon;
					break;
				}
			}
			WorldMapClouds targetCloud = null;
			WorldMapClouds[] clouds = _clouds;
			foreach (WorldMapClouds worldMapClouds in clouds)
			{
				if (worldMapClouds.Location == _revealLocation)
				{
					targetCloud = worldMapClouds;
					break;
				}
			}
			yield return new WaitForSecondsRealtime(0.5f);
			_mapContainer.DOScale(Vector3.one, 1.25f).SetEase(Ease.InOutSine).SetUpdate(true);
			yield return DoFocusLocation(targetIcon, 1.5f);
			UIManager.PlayAudio("event:/ui/map_location_pan");
			yield return new WaitForSecondsRealtime(0.25f);
			if (!_reReveal)
			{
				yield return targetCloud.DoHide();
				yield return new WaitForSecondsRealtime(0.25f);
				UIManager.PlayAudio("event:/ui/map_location_appear");
				_locationHeaderContainer.anchoredPosition = Vector3.zero;
				_locationHeaderContainer.localScale = Vector3.one * 4f;
				_locationHeaderContainer.DOScale(Vector3.one * 2f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
				_locationHeaderCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
				_locationHeader.text = LocalizationManager.GetTranslation(targetIcon.LocationTerm);
				yield return new WaitForSecondsRealtime(1f);
				_locationHeaderContainer.DOScale(Vector3.one, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				_locationHeaderContainer.DOAnchorPos(_locationHeaderOrigin, 0.5f).SetEase(Ease.InBack).SetUpdate(true);
				yield return new WaitForSecondsRealtime(0.5f);
			}
			else
			{
				_locationHeaderCanvasGroup.DOFade(1f, 0.5f).SetUpdate(true);
				_locationHeader.text = LocalizationManager.GetTranslation(targetIcon.LocationTerm);
			}
			yield return new WaitForSecondsRealtime(1f);
			Hide();
		}

		private void OnLocationSelected(WorldMapIcon location)
		{
			if (location.Location != DataManager.Instance.CurrentLocation)
			{
				DataManager.Instance.CurrentLocation = location.Location;
				_canvasGroup.interactable = false;
				MMTransition.Play(MMTransition.TransitionType.ChangeRoomWaitToResume, MMTransition.Effect.BlackFade, location.Scene.SceneName, 1f, location.GetLocalisedLocation(), delegate
				{
					Hide(true);
					SaveAndLoad.Save();
				});
				if (!DataManager.Instance.VisitedLocations.Contains(location.Location))
				{
					DataManager.Instance.VisitedLocations.Add(location.Location);
				}
			}
		}

		private void OnLocationHighlighted(WorldMapIcon location)
		{
			_locationHeader.text = location.GetLocalisedLocation();
			if (!InputManager.General.MouseInputActive)
			{
				FocusLocation(location, 0.5f);
			}
		}

		private void FocusLocation(WorldMapIcon location, float time)
		{
			float num = Vector2.Distance(Vector2.zero, location.ParallaxPosition) / 1080f;
			Vector2 targetPosition = location.ParallaxPosition.normalized * num;
			targetPosition *= 150f;
			if (_focusCoroutine != null)
			{
				StopCoroutine(_focusCoroutine);
			}
			_focusCoroutine = StartCoroutine(DoFocusPosition(targetPosition, time));
		}

		private IEnumerator DoFocusLocation(WorldMapIcon location, float time)
		{
			Vector2 parallaxPosition = location.ParallaxPosition;
			parallaxPosition.x *= 1f / (_parallax.HorizontalIntensity * _parallax.GlobalIntensity);
			parallaxPosition.y *= 1f / (_parallax.VerticalIntensity * _parallax.GlobalIntensity);
			yield return DoFocusPosition(parallaxPosition, time);
		}

		private IEnumerator DoFocusPosition(Vector2 targetPosition, float time)
		{
			Vector2 currentPosition = _parallax.RectTransform.anchoredPosition;
			float t = 0f;
			while (t <= time)
			{
				t += Time.unscaledDeltaTime;
				_parallax.RectTransform.anchoredPosition = Vector2.Lerp(currentPosition, targetPosition, _focusCurve.Evaluate(t / time));
				yield return null;
			}
		}

		protected override IEnumerator DoHideAnimation()
		{
			while (_canvasGroup.alpha > 0f)
			{
				_canvasGroup.alpha -= Time.unscaledDeltaTime * 4f;
				yield return null;
			}
		}

		public override void OnCancelButtonInput()
		{
			if (_canvasGroup.interactable)
			{
				_didCancel = true;
				Hide();
			}
		}

		protected override void OnHideCompleted()
		{
			UnityEngine.Object.Destroy(base.gameObject);
			if (_didCancel)
			{
				Action onCancel = OnCancel;
				if (onCancel != null)
				{
					onCancel();
				}
			}
		}
	}
}
