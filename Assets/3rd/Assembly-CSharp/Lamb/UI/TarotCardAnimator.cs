using System.Collections;
using Spine.Unity;
using UnityEngine;

namespace Lamb.UI
{
	public class TarotCardAnimator : BaseMonoBehaviour
	{
		private const string kStaticAnimation = "menu-static";

		private const string kStaticBackAnimation = "menu-static-back";

		private const string kMenuRevealAnimation = "menu-reveal";

		[SerializeField]
		private SkeletonGraphic _spine;

		[SerializeField]
		private ParticleSystem _particleSystem;

		[Header("Materials")]
		[SerializeField]
		private Material _normalMaterial;

		[SerializeField]
		private Material _rareMaterial;

		[SerializeField]
		private Material _superRareMaterial;

		public SkeletonGraphic Spine
		{
			get
			{
				return _spine;
			}
		}

		public RectTransform RectTransform { get; private set; }

		private void Awake()
		{
			RectTransform = GetComponent<RectTransform>();
			if (_particleSystem != null)
			{
				_particleSystem.Stop();
			}
		}

		public void Configure(TarotCards.TarotCard card)
		{
			if (_particleSystem != null)
			{
				_particleSystem.Stop();
				_particleSystem.Clear();
			}
			_spine.Skeleton.SetSkin(TarotCards.Skin(card.CardType));
			if (card.UpgradeIndex == 1)
			{
				Spine.material = _rareMaterial;
				if (_particleSystem != null)
				{
					_particleSystem.Play();
				}
			}
			else if (card.UpgradeIndex == 2)
			{
				Spine.material = _superRareMaterial;
				if (_particleSystem != null)
				{
					_particleSystem.Play();
				}
			}
			else
			{
				Spine.material = _normalMaterial;
			}
		}

		public void Configure(TarotCards.Card card)
		{
			Configure(new TarotCards.TarotCard(card, 1));
		}

		public void SetStaticBack()
		{
			_spine.AnimationState.SetAnimation(0, "menu-static-back", false);
		}

		public void SetStaticFront()
		{
			_spine.AnimationState.SetAnimation(0, "menu-static", false);
		}

		public IEnumerator YieldForReveal(float timeScale = 1f)
		{
			_spine.timeScale = timeScale;
			yield return _spine.YieldForAnimation("menu-reveal");
		}
	}
}
