using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using DG.Tweening;
using FMOD.Studio;
using Lamb.UI;
using MMBiomeGeneration;
using MMRoomGeneration;
using MMTools;
using src.UI.Overlays.TutorialOverlay;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlayerRelic : MonoBehaviour
{
	public delegate void RelicEvent(RelicData relic);

	private enum BonusType
	{
		GainStrength,
		GainBlueHearts,
		GainBlackHearts,
		TakeDamage,
		GainSpiritHearts,
		GainInvincibility,
		LoseHeart
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass47_0
	{
		public PlayerRelic _003C_003E4__this;

		public RelicType relicType;

		public Transform playerTransform;

		public Action _003C_003E9__21;

		internal void _003CUseRelic_003Eb__0()
		{
			AudioManager.Instance.PlayOneShot("event:/relics/demon_bubble", _003C_003E4__this.gameObject);
		}

		internal void _003CUseRelic_003Eb__16()
		{
			for (int num = Health.team2.Count - 1; num >= 0; num--)
			{
				if (!(Health.team2[num] == null) && Health.team2[num].gameObject.activeInHierarchy)
				{
					Health health = Health.team2[num];
					if (relicType == RelicType.PoisonAll)
					{
						if (!(health == null))
						{
							health.AddPoison(_003C_003E4__this.gameObject, 10f);
							AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", _003C_003E4__this.gameObject);
							BiomeConstants.Instance.EmitSmokeExplosionVFX(health.transform.position, Color.green);
						}
					}
					else if (relicType == RelicType.FreezeAll && !(health == null))
					{
						health.AddIce(20f);
						AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", _003C_003E4__this.gameObject);
						BiomeConstants.Instance.EmitSmokeExplosionVFX(health.transform.position, Color.cyan);
					}
				}
			}
			AudioManager.Instance.ToggleFilter("freeze", false);
		}

		internal void _003CUseRelic_003Eb__19()
		{
			_003C_003E4__this.relicIcon.gameObject.SetActive(false);
		}

		internal void _003CUseRelic_003Eb__20(VFXObject result, int slot)
		{
			AudioManager.Instance.PlayOneShot("event:/relics/follower_impact", result.gameObject);
			_003C_003E4__this.StartCoroutine(_003C_003E4__this.Delay(0.25f, true, delegate
			{
				_003C_003E4__this.FadeRedAway();
			}));
		}

		internal void _003CUseRelic_003Eb__21()
		{
			_003C_003E4__this.FadeRedAway();
		}

		internal void _003CUseRelic_003Eg__FreezeTimeEnemies_007C13()
		{
			TimeFrozen = true;
			_003C_003E4__this.enemies = new List<Health>();
			for (int num = Health.team2.Count - 1; num >= 0; num--)
			{
				if (Health.team2[num] != null && Health.team2[num].gameObject.activeInHierarchy)
				{
					_003C_003E4__this.enemies.Add(Health.team2[num]);
				}
			}
			for (int num2 = _003C_003E4__this.enemies.Count - 1; num2 >= 0; num2--)
			{
				_003C_003E4__this.enemies[num2].AddFreezeTime();
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass47_1
	{
		public VFXSequence freezePoisonAllSequence;

		public List<HUD_Heart> blueHeartsList;

		public List<GameObject> tempObjs;

		public VFXSequence sequence;

		public int count;

		public List<DeadBodySliding> otherBodies;

		public List<DeadBodySliding> targetBodies;

		public Tentacle t;

		public UnitObject enemy;

		public List<Transform> targets;

		public GameObject tempObj;

		public VFXSequence freezeSequence;

		public VFXSequence damageSequence;

		public EventInstance loopedSound;

		public _003C_003Ec__DisplayClass47_0 CS_0024_003C_003E8__locals1;

		public Action _003C_003E9__25;

		public Action _003C_003E9__23;

		public Action<AsyncOperationHandle<GameObject>> _003C_003E9__26;

		public Action _003C_003E9__31;

		internal void _003CUseRelic_003Eg__DoFreezePoisonAll_007C2(VFXObject vfxObject, int impactVFXIndex)
		{
			VFXSequence vFXSequence = freezePoisonAllSequence;
			vFXSequence.OnImpact = (Action<VFXObject, int>)Delegate.Remove(vFXSequence.OnImpact, new Action<VFXObject, int>(_003CUseRelic_003Eg__DoFreezePoisonAll_007C2));
			AudioManager.Instance.PlayOneShot((CS_0024_003C_003E8__locals1.relicType == RelicType.FreezeAll) ? "event:/relics/freeze_relic" : "event:/relics/relic_poison", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/goop_charge", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
			AudioManager.Instance.ToggleFilter("freeze", true);
			GameManager.GetInstance().WaitForSeconds(0.2f, delegate
			{
				for (int num = Health.team2.Count - 1; num >= 0; num--)
				{
					if (!(Health.team2[num] == null) && Health.team2[num].gameObject.activeInHierarchy)
					{
						Health health = Health.team2[num];
						if (CS_0024_003C_003E8__locals1.relicType == RelicType.PoisonAll)
						{
							if (!(health == null))
							{
								health.AddPoison(CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject, 10f);
								AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
								BiomeConstants.Instance.EmitSmokeExplosionVFX(health.transform.position, Color.green);
							}
						}
						else if (CS_0024_003C_003E8__locals1.relicType == RelicType.FreezeAll && !(health == null))
						{
							health.AddIce(20f);
							AudioManager.Instance.PlayOneShot("event:/enemy/spit_gross_projectile", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
							BiomeConstants.Instance.EmitSmokeExplosionVFX(health.transform.position, Color.cyan);
						}
					}
				}
				AudioManager.Instance.ToggleFilter("freeze", false);
			});
		}

		internal void _003CUseRelic_003Eb__4()
		{
			for (int num = tempObjs.Count - 1; num >= 0; num--)
			{
				UnityEngine.Object.Destroy(tempObjs[num]);
			}
			if (CS_0024_003C_003E8__locals1.relicType == RelicType.HeartConversion_Blessed && PlayerFarming.Instance.health.BlueHearts > 0f)
			{
				float blueHearts = PlayerFarming.Instance.health.BlueHearts;
				PlayerFarming.Instance.health.BlueHearts = 0f;
				PlayerFarming.Instance.health.TotalSpiritHearts += blueHearts;
				BiomeConstants.Instance.EmitHeartPickUpVFX(CS_0024_003C_003E8__locals1._003C_003E4__this.transform.position, 0f, "red", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", CS_0024_003C_003E8__locals1.playerTransform.position);
			}
			else if (CS_0024_003C_003E8__locals1.relicType == RelicType.HeartConversion_Dammed && PlayerFarming.Instance.health.BlueHearts > 0f)
			{
				float blueHearts2 = PlayerFarming.Instance.health.BlueHearts;
				PlayerFarming.Instance.health.BlueHearts = 0f;
				PlayerFarming.Instance.health.BlackHearts += blueHearts2 * 2f;
				BiomeConstants.Instance.EmitHeartPickUpVFX(CS_0024_003C_003E8__locals1._003C_003E4__this.transform.position, 0f, "black", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", CS_0024_003C_003E8__locals1.playerTransform.position);
			}
		}

		internal void _003CUseRelic_003Eg__ApplyWeapon_007C5()
		{
			DataManager.Instance.CurrentWeaponLevel++;
			DataManager.Instance.CurrentRunWeaponLevel = DataManager.Instance.CurrentWeaponLevel;
			VFXSequence vFXSequence = sequence;
			vFXSequence.OnComplete = (Action)Delegate.Remove(vFXSequence.OnComplete, new Action(_003CUseRelic_003Eg__ApplyWeapon_007C5));
			PlayerFarming.Instance.playerWeapon.SetWeapon(DataManager.Instance.GetRandomWeaponInPool(), DataManager.Instance.CurrentWeaponLevel);
			AudioManager.Instance.PlayOneShot("event:/player/weapon_equip", CS_0024_003C_003E8__locals1._003C_003E4__this.transform.position);
			AudioManager.Instance.PlayOneShot("event:/player/weapon_unlocked", CS_0024_003C_003E8__locals1._003C_003E4__this.transform.position);
			PlayerFarming.Instance.TimedAction(1.6f, null, PlayerFarming.Instance.playerWeapon.CurrentWeapon.WeaponData.PickupAnimationKey);
		}

		internal void _003CUseRelic_003Eg__ApplyCurse_007C6()
		{
			DataManager.Instance.CurrentCurseLevel++;
			DataManager.Instance.CurrentRunCurseLevel = DataManager.Instance.CurrentCurseLevel;
			VFXSequence vFXSequence = sequence;
			vFXSequence.OnComplete = (Action)Delegate.Remove(vFXSequence.OnComplete, new Action(_003CUseRelic_003Eg__ApplyCurse_007C6));
			PlayerFarming.Instance.playerSpells.SetSpell(DataManager.Instance.GetRandomCurseInPool(), DataManager.Instance.CurrentCurseLevel);
			PlayerFarming.Instance.TimedAction(1.3f, null, "Curses/curse-get");
			FaithAmmo.Reload();
			AudioManager.Instance.PlayOneShot("event:/player/absorb_curse", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
			CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.DOKill();
			if (CS_0024_003C_003E8__locals1._003C_003E4__this.animationSequence != null)
			{
				CS_0024_003C_003E8__locals1._003C_003E4__this.animationSequence.Complete();
			}
			CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.gameObject.SetActive(true);
			CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.transform.localPosition = new Vector3(-1.25f * (float)PlayerFarming.Instance.simpleSpineAnimator.Dir, 0f, -0.2f);
			CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.transform.localScale = Vector3.one * 0f;
			CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.transform.DOShakePosition(2f, 0.25f);
			CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.transform.DOShakeRotation(2f, new Vector3(0f, 0f, 15f));
			CS_0024_003C_003E8__locals1._003C_003E4__this.animationSequence = DOTween.Sequence();
			CS_0024_003C_003E8__locals1._003C_003E4__this.animationSequence.Append(CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.transform.DOScale(Vector3.one * 1.2f, 0.3f));
			CS_0024_003C_003E8__locals1._003C_003E4__this.animationSequence.Append(CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.transform.DOScale(Vector3.one * 0.8f, 0.3f));
			CS_0024_003C_003E8__locals1._003C_003E4__this.animationSequence.Append(CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.transform.DOScale(Vector3.one * 1.2f, 0.3f));
			CS_0024_003C_003E8__locals1._003C_003E4__this.animationSequence.Append(CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.transform.DOScale(Vector3.one * 0f, 0.3f));
			CS_0024_003C_003E8__locals1._003C_003E4__this.animationSequence.AppendCallback(delegate
			{
				CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.gameObject.SetActive(false);
			});
			CS_0024_003C_003E8__locals1._003C_003E4__this.relicIcon.sprite = EquipmentManager.GetCurseData(DataManager.Instance.CurrentCurse).WorldSprite;
		}

		internal void _003CUseRelic_003Eb__22(VFXObject result, int slot)
		{
			_003C_003Ec__DisplayClass47_3 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass47_3
			{
				CS_0024_003C_003E8__locals3 = this,
				slot = slot
			};
			AudioManager.Instance.PlayOneShot("event:/relics/follower_impact", result.gameObject);
			if (CS_0024_003C_003E8__locals0.slot == count - 1)
			{
				CS_0024_003C_003E8__locals1._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals1._003C_003E4__this.Delay(0.5f, true, delegate
				{
					CS_0024_003C_003E8__locals1._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals1._003C_003E4__this.Delay(0.5f, true, delegate
					{
						foreach (DeadBodySliding otherBody in otherBodies)
						{
							if (otherBody != null)
							{
								otherBody.OnDie();
								InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, UnityEngine.Random.Range(2, 5), otherBody.transform.position);
							}
						}
					}));
					CS_0024_003C_003E8__locals1._003C_003E4__this.FadeRedAway();
				}));
			}
			CS_0024_003C_003E8__locals1._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals1._003C_003E4__this.Delay(1f, true, delegate
			{
				if (CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals3.targetBodies[CS_0024_003C_003E8__locals0.slot] != null)
				{
					CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals3.targetBodies[CS_0024_003C_003E8__locals0.slot].OnDie();
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, UnityEngine.Random.Range(2, 5), CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals3.targetBodies[CS_0024_003C_003E8__locals0.slot].transform.position);
				}
			}));
		}

		internal void _003CUseRelic_003Eb__23()
		{
			CS_0024_003C_003E8__locals1._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals1._003C_003E4__this.Delay(0.5f, true, delegate
			{
				foreach (DeadBodySliding otherBody in otherBodies)
				{
					if (otherBody != null)
					{
						otherBody.OnDie();
						InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, UnityEngine.Random.Range(2, 5), otherBody.transform.position);
					}
				}
			}));
			CS_0024_003C_003E8__locals1._003C_003E4__this.FadeRedAway();
		}

		internal void _003CUseRelic_003Eb__25()
		{
			foreach (DeadBodySliding otherBody in otherBodies)
			{
				if (otherBody != null)
				{
					otherBody.OnDie();
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, UnityEngine.Random.Range(2, 5), otherBody.transform.position);
				}
			}
		}

		internal void _003CUseRelic_003Eb__8()
		{
			AudioManager.Instance.PlayOneShot("event:/material/stone_break", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
			AudioManager.Instance.PlayOneShot("event:/followers/break_free", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
			CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.2f);
			BiomeConstants.Instance.EmitSmokeExplosionVFX(t.transform.position);
			BiomeConstants.Instance.EmitParticleChunk(BiomeConstants.TypeOfParticle.stone, t.transform.position, Vector3.one, 10);
		}

		internal void _003CUseRelic_003Eb__10()
		{
			BiomeGenerator.Instance.CurrentRoom.generateRoom.TurnEnemyIntoCritter(enemy);
			AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/staff_magic", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
		}

		internal void _003CUseRelic_003Eb__26(AsyncOperationHandle<GameObject> o)
		{
			_003C_003Ec__DisplayClass47_4 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass47_4();
			CS_0024_003C_003E8__locals0.familiar = o.Result.GetComponent<Familiar>();
			CS_0024_003C_003E8__locals0.familiar.SetMaster(PlayerFarming.Instance.unitObject);
			CS_0024_003C_003E8__locals0.familiar.gameObject.SetActive(false);
			CS_0024_003C_003E8__locals0.familiar.enabled = false;
			CS_0024_003C_003E8__locals0.familiar.Container.transform.localScale = Vector3.zero;
			CS_0024_003C_003E8__locals0.familiar.SetDirection((targets.Count == 0) ? 1 : (-1));
			Vector3 position = o.Result.transform.position;
			GameManager.GetInstance().WaitForSeconds(0.6f, delegate
			{
				CS_0024_003C_003E8__locals0.familiar.gameObject.SetActive(true);
				CS_0024_003C_003E8__locals0.familiar.Container.transform.DOScale(0.25f, 0.75f).OnComplete(delegate
				{
					CS_0024_003C_003E8__locals0.familiar.GetComponentInChildren<DOTweenAnimation>().DOPlay();
				});
				GameManager.GetInstance().WaitForSeconds(1f, delegate
				{
					CS_0024_003C_003E8__locals0.familiar.enabled = true;
				});
			});
			targets.Add(CS_0024_003C_003E8__locals0.familiar.transform);
			if (targets.Count >= 2)
			{
				sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.playerTransform, targets.ToArray());
			}
		}

		internal void _003CUseRelic_003Eb__11(AsyncOperationHandle<GameObject> o)
		{
			_003C_003Ec__DisplayClass47_5 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass47_5();
			CS_0024_003C_003E8__locals0.familiar = o.Result.GetComponent<Familiar>();
			CS_0024_003C_003E8__locals0.familiar.gameObject.SetActive(false);
			CS_0024_003C_003E8__locals0.familiar.GetComponentInChildren<DOTweenAnimation>().DOPlay();
			sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { tempObj.transform });
			GameManager.GetInstance().WaitForSeconds(0.8f, delegate
			{
				CS_0024_003C_003E8__locals0.familiar.gameObject.SetActive(true);
			});
			VFXSequence vFXSequence = sequence;
			vFXSequence.OnComplete = (Action)Delegate.Combine(vFXSequence.OnComplete, (Action)delegate
			{
				UnityEngine.Object.Destroy(tempObj);
			});
		}

		internal void _003CUseRelic_003Eb__31()
		{
			UnityEngine.Object.Destroy(tempObj);
		}

		internal void _003CUseRelic_003Eg__StartFreezeTime_007C12(VFXObject vfxObject, int i)
		{
			_003C_003Ec__DisplayClass47_6 _003C_003Ec__DisplayClass47_ = new _003C_003Ec__DisplayClass47_6
			{
				CS_0024_003C_003E8__locals4 = this,
				vfxObject = vfxObject
			};
			VFXSequence vFXSequence = freezeSequence;
			vFXSequence.OnImpact = (Action<VFXObject, int>)Delegate.Remove(vFXSequence.OnImpact, new Action<VFXObject, int>(_003CUseRelic_003Eg__StartFreezeTime_007C12));
			VFXObject vfxObject2 = _003C_003Ec__DisplayClass47_.vfxObject;
			vfxObject2.OnStopped = (Action<VFXObject>)Delegate.Combine(vfxObject2.OnStopped, new Action<VFXObject>(_003C_003Ec__DisplayClass47_._003CUseRelic_003Eg__CompleteFreezeTime_007C32));
			AudioManager.Instance.PlayOneShot("event:/relics/freeze_time", CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
			AudioManager.Instance.ToggleFilter("freeze", true);
			CS_0024_003C_003E8__locals1._003CUseRelic_003Eg__FreezeTimeEnemies_007C13();
		}

		internal void _003CUseRelic_003Eg__UnFreezeTimeEnemies_007C14()
		{
			if (CS_0024_003C_003E8__locals1._003C_003E4__this.enemies == null)
			{
				return;
			}
			VFXSequence vFXSequence = freezeSequence;
			vFXSequence.OnComplete = (Action)Delegate.Remove(vFXSequence.OnComplete, new Action(_003CUseRelic_003Eg__UnFreezeTimeEnemies_007C14));
			for (int num = CS_0024_003C_003E8__locals1._003C_003E4__this.enemies.Count - 1; num >= 0; num--)
			{
				if (CS_0024_003C_003E8__locals1._003C_003E4__this.enemies[num] != null)
				{
					CS_0024_003C_003E8__locals1._003C_003E4__this.enemies[num].ClearFreezeTime();
				}
			}
			TimeFrozen = false;
		}

		internal void _003CUseRelic_003Eb__15()
		{
			VFXObject[] impactVFXObjects = damageSequence.ImpactVFXObjects;
			for (int i = 0; i < impactVFXObjects.Length; i++)
			{
				impactVFXObjects[i].StopVFX();
			}
			AudioManager.Instance.StopLoop(loopedSound);
			DataManager.Instance.PLAYER_RUN_DAMAGE_LEVEL -= 0.75f;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass47_3
	{
		public int slot;

		public _003C_003Ec__DisplayClass47_1 CS_0024_003C_003E8__locals3;

		internal void _003CUseRelic_003Eb__24()
		{
			if (CS_0024_003C_003E8__locals3.targetBodies[slot] != null)
			{
				CS_0024_003C_003E8__locals3.targetBodies[slot].OnDie();
				InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, UnityEngine.Random.Range(2, 5), CS_0024_003C_003E8__locals3.targetBodies[slot].transform.position);
			}
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass47_4
	{
		public Familiar familiar;

		public TweenCallback _003C_003E9__28;

		public Action _003C_003E9__29;

		internal void _003CUseRelic_003Eb__27()
		{
			familiar.gameObject.SetActive(true);
			familiar.Container.transform.DOScale(0.25f, 0.75f).OnComplete(delegate
			{
				familiar.GetComponentInChildren<DOTweenAnimation>().DOPlay();
			});
			GameManager.GetInstance().WaitForSeconds(1f, delegate
			{
				familiar.enabled = true;
			});
		}

		internal void _003CUseRelic_003Eb__28()
		{
			familiar.GetComponentInChildren<DOTweenAnimation>().DOPlay();
		}

		internal void _003CUseRelic_003Eb__29()
		{
			familiar.enabled = true;
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass47_5
	{
		public Familiar familiar;

		internal void _003CUseRelic_003Eb__30()
		{
			familiar.gameObject.SetActive(true);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass47_6
	{
		public VFXObject vfxObject;

		public _003C_003Ec__DisplayClass47_1 CS_0024_003C_003E8__locals4;

		internal void _003CUseRelic_003Eg__CompleteFreezeTime_007C32(VFXObject o)
		{
			VFXObject vFXObject = vfxObject;
			vFXObject.OnStopped = (Action<VFXObject>)Delegate.Remove(vFXObject.OnStopped, new Action<VFXObject>(_003CUseRelic_003Eg__CompleteFreezeTime_007C32));
			AudioManager.Instance.ToggleFilter("freeze", false);
			CS_0024_003C_003E8__locals4._003CUseRelic_003Eg__UnFreezeTimeEnemies_007C14();
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass59_0
	{
		public List<Health> targets;

		public float damage;

		public PlayerRelic _003C_003E4__this;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass59_1
	{
		public Transform[] targetTransforms;

		public VFXSequence sequence;

		public _003C_003Ec__DisplayClass59_0 CS_0024_003C_003E8__locals1;

		internal void _003CLightningStrike_003Eg__Impact_007C2(VFXObject vfxObject, int targetIndex)
		{
			if (CS_0024_003C_003E8__locals1.targets.Count > targetIndex && targetTransforms[targetIndex] != null)
			{
				Vector3 position = CS_0024_003C_003E8__locals1.targets[targetIndex].transform.position;
				vfxObject.transform.SetPositionAndRotation(position, Quaternion.identity);
				CS_0024_003C_003E8__locals1.targets[targetIndex].DealDamage(CS_0024_003C_003E8__locals1.damage, CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject, position, false, Health.AttackTypes.Projectile, false, Health.AttackFlags.DoesntChargeRelics | Health.AttackFlags.Electrified);
			}
		}

		internal void _003CLightningStrike_003Eg__Complete_007C3()
		{
			VFXSequence vFXSequence = sequence;
			vFXSequence.OnImpact = (Action<VFXObject, int>)Delegate.Remove(vFXSequence.OnImpact, new Action<VFXObject, int>(_003CLightningStrike_003Eg__Impact_007C2));
			VFXSequence vFXSequence2 = sequence;
			vFXSequence2.OnComplete = (Action)Delegate.Remove(vFXSequence2.OnComplete, new Action(_003CLightningStrike_003Eg__Complete_007C3));
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass64_0
	{
		public List<Health> targetEnemies;

		public float damage;

		public PlayerRelic _003C_003E4__this;

		public float chanceForBlueHearts;

		public float chanceForBlackHearts;

		public VFXSequence sequence;

		internal void _003CDestroyTarotsDealDamage_003Eg__Impact_007C0(VFXObject vfxObject, int targetIndex)
		{
			if (targetEnemies.Count > targetIndex && targetEnemies[targetIndex] != null)
			{
				Vector3 position = targetEnemies[targetIndex].transform.position;
				vfxObject.transform.SetPositionAndRotation(position, Quaternion.identity);
				targetEnemies[targetIndex].DealDamage(damage, _003C_003E4__this.gameObject, position, false, Health.AttackTypes.Projectile, false, Health.AttackFlags.DoesntChargeRelics);
				if (chanceForBlueHearts != 0f && UnityEngine.Random.value < chanceForBlueHearts)
				{
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLUE_HEART, 1, position);
				}
				if (chanceForBlackHearts != 0f && UnityEngine.Random.value < chanceForBlackHearts)
				{
					InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_HEART, 1, position).MagnetToPlayer = false;
				}
			}
		}

		internal void _003CDestroyTarotsDealDamage_003Eg__Complete_007C1()
		{
			VFXSequence vFXSequence = sequence;
			vFXSequence.OnImpact = (Action<VFXObject, int>)Delegate.Remove(vFXSequence.OnImpact, new Action<VFXObject, int>(_003CDestroyTarotsDealDamage_003Eg__Impact_007C0));
			VFXSequence vFXSequence2 = sequence;
			vFXSequence2.OnComplete = (Action)Delegate.Remove(vFXSequence2.OnComplete, new Action(_003CDestroyTarotsDealDamage_003Eg__Complete_007C1));
		}
	}

	[SerializeField]
	private SpriteRenderer relicIcon;

	[SerializeField]
	private ParticleSystem relicPuff;

	[SerializeField]
	private GameObject plane;

	[SerializeField]
	private AssetReferenceGameObject friendlyEnemy;

	[SerializeField]
	private string tarotProjectilePath;

	[SerializeField]
	private BiomeLightingSettings RedLightingSettings;

	[SerializeField]
	private OverrideLightingProperties overrideLightingProperties;

	public static bool TimeFrozen;

	public static bool InvincibleFromRelic;

	private Vector3 relicIconOriginalPosition = new Vector3(0f, 0.806f, -1.468f);

	private Sequence animationSequence;

	private Coroutine animationCoroutine;

	private List<Health> enemies = new List<Health>();

	public RelicData CurrentRelic { get; private set; }

	public float ChargedAmount
	{
		get
		{
			return DataManager.Instance.RelicChargeAmount;
		}
		set
		{
			DataManager.Instance.RelicChargeAmount = value;
			RelicEvent onRelicChargeModified = PlayerRelic.OnRelicChargeModified;
			if (onRelicChargeModified != null)
			{
				onRelicChargeModified(CurrentRelic);
			}
			if (IsFullyCharged)
			{
				DataManager.Instance.RelicChargeAmount = float.MaxValue;
			}
		}
	}

	public float RequiredChargeAmount
	{
		get
		{
			if (!(CurrentRelic != null))
			{
				return 0f;
			}
			return PlayerWeapon.GetDamage(CurrentRelic.DamageRequiredToCharge, DataManager.Instance.CurrentWeaponLevel);
		}
	}

	public bool IsFullyCharged
	{
		get
		{
			return ChargedAmount >= RequiredChargeAmount;
		}
	}

	public static event RelicEvent OnRelicEquipped;

	public static event RelicEvent OnRelicConsumed;

	public static event RelicEvent OnRelicChargeModified;

	public static event RelicEvent OnRelicUsed;

	public static event RelicEvent OnRelicCantUse;

	public static event RelicEvent OnSubRelicChanged;

	private void Start()
	{
		RoomLockController.OnRoomCleared += IncreaseChargedAmount;
		if (DataManager.Instance.CurrentRelic != 0)
		{
			EquipRelic(EquipmentManager.GetRelicData(DataManager.Instance.CurrentRelic), false);
		}
		relicIcon.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		RoomLockController.OnRoomCleared -= IncreaseChargedAmount;
		TimeFrozen = false;
		InvincibleFromRelic = false;
	}

	private void FadeRedIn()
	{
		LightingManager.Instance.inOverride = true;
		RedLightingSettings.overrideLightingProperties = overrideLightingProperties;
		LightingManager.Instance.overrideSettings = RedLightingSettings;
		LightingManager.Instance.transitionDurationMultiplier = 0f;
		LightingManager.Instance.UpdateLighting(true);
	}

	public void FadeRedAway()
	{
		LightingManager.Instance.inOverride = false;
		LightingManager.Instance.overrideSettings = null;
		LightingManager.Instance.transitionDurationMultiplier = 0.25f;
		LightingManager.Instance.lerpActive = false;
		LightingManager.Instance.UpdateLighting(true);
	}

	public void UseRelic(RelicType relicType, bool forceConsumableAnimation = false)
	{
		_003C_003Ec__DisplayClass47_0 _003C_003Ec__DisplayClass47_ = new _003C_003Ec__DisplayClass47_0();
		_003C_003Ec__DisplayClass47_._003C_003E4__this = this;
		_003C_003Ec__DisplayClass47_.relicType = relicType;
		bool flag = true;
		_003C_003Ec__DisplayClass47_.playerTransform = PlayerFarming.Instance.transform;
		switch (CurrentRelic.RelicSubType)
		{
		case RelicSubType.Blessed:
			AudioManager.Instance.PlayOneShot("event:/relics/relic_blessed");
			break;
		case RelicSubType.Dammed:
			AudioManager.Instance.PlayOneShot("event:/relics/relic_damned");
			break;
		case RelicSubType.Any:
			AudioManager.Instance.PlayOneShot("event:/relics/relic_standard");
			break;
		}
		if (CurrentRelic.RelicType != RelicType.UseRandomRelic && CurrentRelic.RelicType != RelicType.UseRandomRelic_Blessed && CurrentRelic.RelicType != RelicType.UseRandomRelic_Dammed)
		{
			EquipmentManager.NextRandomRelic = RelicType.None;
		}
		AudioManager.Instance.ToggleFilter("blessed", false);
		AudioManager.Instance.ToggleFilter("dammed", false);
		_003C_003Ec__DisplayClass47_1 CS_0024_003C_003E8__locals1 = new _003C_003Ec__DisplayClass47_1();
		CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1 = _003C_003Ec__DisplayClass47_;
		switch (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType)
		{
		case RelicType.LightningStrike:
		case RelicType.LightningStrike_Dammed:
		case RelicType.LightningStrike_Blessed:
			LightningStrike();
			break;
		case RelicType.SpawnDemon:
		case RelicType.SpawnDemon_Dammed:
		case RelicType.SpawnDemon_Blessed:
		{
			StartCoroutine(Delay(1f, true, delegate
			{
				AudioManager.Instance.PlayOneShot("event:/relics/demon_bubble", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
			}));
			int level = 1;
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.SpawnDemon_Dammed)
			{
				level = UnityEngine.Random.Range(2, 4);
			}
			else if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.SpawnDemon_Blessed)
			{
				SpawnDemon(1);
			}
			SpawnDemon(level);
			break;
		}
		case RelicType.DestroyTarotGainBuff:
		case RelicType.DestroyTarotGainBuff_Dammed:
		case RelicType.DestroyTarotGainBuff_Blessed:
		{
			BonusType bonusType = ((CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType != RelicType.DestroyTarotGainBuff) ? BonusType.GainBlueHearts : BonusType.GainStrength);
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.DestroyTarotGainBuff_Dammed)
			{
				bonusType = BonusType.GainBlackHearts;
			}
			StartCoroutine(DestroyTarotsGainStrength(bonusType));
			break;
		}
		case RelicType.DestroyTarotDealDamge:
		case RelicType.DestroyTarotDealDamge_Dammed:
		case RelicType.DestroyTarotDealDamge_Blessed:
			if (DataManager.Instance.PlayerRunTrinkets.Count <= 0)
			{
				RelicEvent onRelicCantUse3 = PlayerRelic.OnRelicCantUse;
				if (onRelicCantUse3 != null)
				{
					onRelicCantUse3(EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType));
				}
				return;
			}
			DestroyTarotsDealDamage(1f, int.MaxValue, (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.DestroyTarotDealDamge_Blessed) ? 0.1f : 0f, (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.DestroyTarotDealDamge_Dammed) ? 0.1f : 0f);
			break;
		case RelicType.FiftyFiftyGamble:
		case RelicType.FiftyFiftyGamble_Dammed:
		case RelicType.FiftyFiftyGamble_Blessed:
			PlayerFarming.Instance.health.untouchable = true;
			FiftyFiftyGamble(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType);
			break;
		case RelicType.SpawnBombs:
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { base.transform });
			GameManager.GetInstance().WaitForSeconds(0.5f, delegate
			{
				BiomeGenerator.SpawnBombsInRoom(UnityEngine.Random.Range(15, 25), false);
			});
			break;
		case RelicType.GungeonBlank:
		case RelicType.GungeonBlank_Dammed:
		case RelicType.GungeonBlank_Blessed:
		{
			float num2 = 8f;
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.GungeonBlank_Blessed)
			{
				num2 = 12f;
			}
			else if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.GungeonBlank_Dammed)
			{
				num2 = 6f;
				PlayerFarming.Instance.EnableDamageOnTouchCollider(num2 * 1.25f);
			}
			PlayerFarming.Instance.playerController.MakeUntouchable(num2 * 1.25f);
			InvincibleFromRelic = true;
			VFXSequence sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform });
			GameManager.GetInstance().StartCoroutine(VFXTimer(num2 * 1.25f, sequence));
			break;
		}
		case RelicType.FreezeAll:
		case RelicType.PoisonAll:
		{
			CS_0024_003C_003E8__locals1.freezePoisonAllSequence = CurrentRelic.VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform });
			VFXSequence freezePoisonAllSequence = CS_0024_003C_003E8__locals1.freezePoisonAllSequence;
			freezePoisonAllSequence.OnImpact = (Action<VFXObject, int>)Delegate.Combine(freezePoisonAllSequence.OnImpact, new Action<VFXObject, int>(CS_0024_003C_003E8__locals1._003CUseRelic_003Eg__DoFreezePoisonAll_007C2));
			break;
		}
		case RelicType.Shrink:
		case RelicType.Enlarge:
		{
			BiomeConstants.Instance.PsychedelicFadeIn(0.25f, 0f, true, delegate
			{
				BiomeConstants.Instance.PsychedelicFadeOut(0.5f, 1f, true, delegate
				{
					AudioManager.Instance.SetMusicPsychedelic(0f);
					PlayerFarming.Instance.playerController.ToggleGhost(false);
				});
			});
			AudioManager.Instance.SetMusicPsychedelic(1f);
			BiomeConstants.Instance.EmitSmokeExplosionVFX(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform.position, Color.cyan);
			CameraManager.instance.ShakeCameraForDuration(1f, 1.25f, 0.5f);
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { base.transform });
			DataManager.Instance.PlayerScaleModifier = 1;
			PlayerFarming.Instance.playerController.ToggleGhost(true);
			Vector3 localScale = CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform.localScale;
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.Shrink)
			{
				AudioManager.Instance.PlayOneShot("event:/relics/shrink", PlayerFarming.Instance.gameObject);
				DataManager.Instance.PlayerScaleModifier--;
			}
			else if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.Enlarge)
			{
				AudioManager.Instance.PlayOneShot("event:/relics/enlarge", PlayerFarming.Instance.gameObject);
				DataManager.Instance.PlayerScaleModifier++;
			}
			Vector3 endValue = Vector3.one * Mathf.Lerp(0.66f, 1.5f, (float)DataManager.Instance.PlayerScaleModifier / 2f);
			base.transform.DOKill();
			base.transform.DOScale(endValue, 1f).SetEase(Ease.OutBounce);
			break;
		}
		case RelicType.HeartConversion_Dammed:
		case RelicType.HeartConversion_Blessed:
		{
			if (PlayerFarming.Instance.health.BlueHearts == 0f)
			{
				RelicEvent onRelicCantUse = PlayerRelic.OnRelicCantUse;
				if (onRelicCantUse != null)
				{
					onRelicCantUse(EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType));
				}
				return;
			}
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HeartConversion_Blessed)
			{
				AudioManager.Instance.PlayOneShot("event:/relics/heart_convert_blessed", base.gameObject);
			}
			else
			{
				AudioManager.Instance.PlayOneShot("event:/relics/heart_convert_dammed", base.gameObject);
			}
			CS_0024_003C_003E8__locals1.blueHeartsList = new List<HUD_Heart>();
			foreach (HUD_Heart heartIcon in HUD_Hearts.Instance.HeartIcons)
			{
				if (heartIcon.MyHeartType == HUD_Heart.HeartType.Blue)
				{
					CS_0024_003C_003E8__locals1.blueHeartsList.Add(heartIcon);
				}
			}
			CS_0024_003C_003E8__locals1.tempObjs = new List<GameObject>();
			_003C_003Ec__DisplayClass47_1 _003C_003Ec__DisplayClass47_2 = CS_0024_003C_003E8__locals1;
			int i;
			for (i = 0; (float)i < PlayerFarming.Instance.health.BlueHearts; i += 2)
			{
				Camera main = Camera.main;
				Vector3 position = main.ScreenToWorldPoint(new Vector3(_003C_003Ec__DisplayClass47_2.CS_0024_003C_003E8__locals1.playerTransform.position.x, _003C_003Ec__DisplayClass47_2.CS_0024_003C_003E8__locals1.playerTransform.position.y, main.nearClipPlane));
				GameObject gameObject = new GameObject();
				_003C_003Ec__DisplayClass47_2.tempObjs.Add(gameObject);
				gameObject.transform.position = position;
				_003C_003Ec__DisplayClass47_2.sequence = EquipmentManager.GetRelicData(_003C_003Ec__DisplayClass47_2.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(gameObject.transform, new Transform[1] { base.transform });
				Vector3 position2 = _003C_003Ec__DisplayClass47_2.blueHeartsList[i / 2].rectTransform.position;
				position = main.ScreenToWorldPoint(new Vector3(position2.x, position2.y, 10f));
				Transform transform = _003C_003Ec__DisplayClass47_2.sequence.ImpactVFXObjects[0].gameObject.transform;
				transform.position = new Vector3(transform.position.x, transform.position.y, -1f);
				float num3 = (float)i / 2f * 0.5f;
				transform.DOMove(position, 1.5f - num3).SetEase(Ease.InOutSine).SetDelay(num3)
					.OnComplete(delegate
					{
						_003C_003Ec__DisplayClass47_2.blueHeartsList[i / 2].transform.DOKill();
						_003C_003Ec__DisplayClass47_2.blueHeartsList[i / 2].transform.DOPunchScale(Vector3.one, 0.5f);
					});
			}
			StartCoroutine(Delay(1.5f, true, delegate
			{
				for (int num15 = CS_0024_003C_003E8__locals1.tempObjs.Count - 1; num15 >= 0; num15--)
				{
					UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals1.tempObjs[num15]);
				}
				if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HeartConversion_Blessed && PlayerFarming.Instance.health.BlueHearts > 0f)
				{
					float blueHearts = PlayerFarming.Instance.health.BlueHearts;
					PlayerFarming.Instance.health.BlueHearts = 0f;
					PlayerFarming.Instance.health.TotalSpiritHearts += blueHearts;
					BiomeConstants.Instance.EmitHeartPickUpVFX(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.transform.position, 0f, "red", "burst_big");
					AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform.position);
				}
				else if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HeartConversion_Dammed && PlayerFarming.Instance.health.BlueHearts > 0f)
				{
					float blueHearts2 = PlayerFarming.Instance.health.BlueHearts;
					PlayerFarming.Instance.health.BlueHearts = 0f;
					PlayerFarming.Instance.health.BlackHearts += blueHearts2 * 2f;
					BiomeConstants.Instance.EmitHeartPickUpVFX(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.transform.position, 0f, "black", "burst_big");
					AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform.position);
				}
			}));
			break;
		}
		case RelicType.RerollWeapon:
		case RelicType.RerollCurse:
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { base.transform });
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.RerollWeapon)
			{
				VFXSequence sequence6 = CS_0024_003C_003E8__locals1.sequence;
				sequence6.OnComplete = (Action)Delegate.Combine(sequence6.OnComplete, new Action(CS_0024_003C_003E8__locals1._003CUseRelic_003Eg__ApplyWeapon_007C5));
			}
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.RerollCurse)
			{
				VFXSequence sequence7 = CS_0024_003C_003E8__locals1.sequence;
				sequence7.OnComplete = (Action)Delegate.Combine(sequence7.OnComplete, new Action(CS_0024_003C_003E8__locals1._003CUseRelic_003Eg__ApplyCurse_007C6));
			}
			break;
		case RelicType.DealDamagePerFollower:
		case RelicType.DealDamagePerFollower_Blessed:
		case RelicType.DealDamagePerFollower_Dammed:
		{
			int num5 = 1;
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.DealDamagePerFollower_Blessed)
			{
				num5 = 10;
			}
			else if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.DealDamagePerFollower_Dammed)
			{
				num5 = 25;
			}
			int num6 = 0;
			foreach (FollowerInfo follower in DataManager.Instance.Followers)
			{
				if ((CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.DealDamagePerFollower_Blessed && follower.CursedState == Thought.OldAge) || (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.DealDamagePerFollower_Dammed && follower.CursedState == Thought.Dissenter) || CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.DealDamagePerFollower)
				{
					num6 += num5;
				}
			}
			Health.DamageAllEnemies(num6, Health.DamageAllEnemiesType.DamagePerFollower);
			break;
		}
		case RelicType.HealPerFollower:
		case RelicType.HealPerFollower_Blessed:
		case RelicType.HealPerFollower_Dammed:
		{
			int num13 = 0;
			foreach (FollowerInfo follower2 in DataManager.Instance.Followers)
			{
				if ((CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HealPerFollower_Blessed && follower2.CursedState == Thought.OldAge) || (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HealPerFollower_Dammed && follower2.CursedState == Thought.Dissenter) || CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HealPerFollower)
				{
					num13++;
				}
			}
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HealPerFollower_Blessed)
			{
				PlayerFarming.Instance.health.BlueHearts += num13;
				BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform.position);
			}
			else if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HealPerFollower_Dammed)
			{
				PlayerFarming.Instance.health.BlackHearts += num13 * 2;
				BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform.position);
			}
			else if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.HealPerFollower)
			{
				PlayerFarming.Instance.health.TotalSpiritHearts += Mathf.FloorToInt((float)num13 / 5f);
				BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform.position);
			}
			break;
		}
		case RelicType.SpawnCombatFollower:
		{
			AudioManager.Instance.PlayOneShot("event:/relics/undead_impact", base.gameObject);
			FadeRedIn();
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform });
			VFXSequence sequence4 = CS_0024_003C_003E8__locals1.sequence;
			sequence4.OnImpact = (Action<VFXObject, int>)Delegate.Combine(sequence4.OnImpact, new Action<VFXObject, int>(SpawnFriendlyEnemy));
			VFXSequence sequence5 = CS_0024_003C_003E8__locals1.sequence;
			sequence5.OnImpact = (Action<VFXObject, int>)Delegate.Combine(sequence5.OnImpact, (Action<VFXObject, int>)delegate(VFXObject result, int slot)
			{
				AudioManager.Instance.PlayOneShot("event:/relics/follower_impact", result.gameObject);
				CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.Delay(0.25f, true, delegate
				{
					CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.FadeRedAway();
				}));
			});
			break;
		}
		case RelicType.UseRandomRelic:
		case RelicType.UseRandomRelic_Blessed:
		case RelicType.UseRandomRelic_Dammed:
		{
			RelicData currentRelic = CurrentRelic;
			CurrentRelic = EquipmentManager.GetRelicData(EquipmentManager.NextRandomRelic);
			UseRelic(CurrentRelic.RelicType);
			switch (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType)
			{
			case RelicType.UseRandomRelic:
				EquipmentManager.PickRandomRelicData(true);
				break;
			case RelicType.UseRandomRelic_Blessed:
				EquipmentManager.PickRandomRelicData(true, RelicSubType.Blessed);
				break;
			case RelicType.UseRandomRelic_Dammed:
				EquipmentManager.PickRandomRelicData(true, RelicSubType.Dammed);
				break;
			}
			CurrentRelic = currentRelic;
			if (DataManager.Instance.CurrentRelic == RelicType.None)
			{
				DataManager.Instance.CurrentRelic = currentRelic.RelicType;
			}
			RelicEvent onSubRelicChanged = PlayerRelic.OnSubRelicChanged;
			if (onSubRelicChanged != null)
			{
				onSubRelicChanged(CurrentRelic);
			}
			break;
		}
		case RelicType.SpawnCombatFollowerFromBodies:
		{
			int num4 = 5;
			List<Transform> list = new List<Transform>();
			CS_0024_003C_003E8__locals1.targetBodies = new List<DeadBodySliding>();
			CS_0024_003C_003E8__locals1.otherBodies = new List<DeadBodySliding>();
			foreach (DeadBodySliding deadBody in DeadBodySliding.DeadBodies)
			{
				if ((list.Count == 0 || UnityEngine.Random.value < 1f / (float)DeadBodySliding.DeadBodies.Count) && list.Count < num4)
				{
					list.Add(deadBody.transform);
					CS_0024_003C_003E8__locals1.targetBodies.Add(deadBody);
				}
				else
				{
					CS_0024_003C_003E8__locals1.otherBodies.Add(deadBody);
				}
			}
			if (list.Count == 0)
			{
				RelicEvent onRelicCantUse2 = PlayerRelic.OnRelicCantUse;
				if (onRelicCantUse2 != null)
				{
					onRelicCantUse2(EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType));
				}
				return;
			}
			AudioManager.Instance.PlayOneShot("event:/relics/undead_impact", base.gameObject);
			FadeRedIn();
			CS_0024_003C_003E8__locals1.count = list.Count;
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, list.ToArray());
			VFXSequence sequence2 = CS_0024_003C_003E8__locals1.sequence;
			sequence2.OnImpact = (Action<VFXObject, int>)Delegate.Combine(sequence2.OnImpact, new Action<VFXObject, int>(SpawnFriendlyEnemy));
			VFXSequence sequence3 = CS_0024_003C_003E8__locals1.sequence;
			sequence3.OnImpact = (Action<VFXObject, int>)Delegate.Combine(sequence3.OnImpact, (Action<VFXObject, int>)delegate(VFXObject result, int slot)
			{
				_003C_003Ec__DisplayClass47_1 _003C_003Ec__DisplayClass47_3 = CS_0024_003C_003E8__locals1;
				AudioManager.Instance.PlayOneShot("event:/relics/follower_impact", result.gameObject);
				if (slot == CS_0024_003C_003E8__locals1.count - 1)
				{
					CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.Delay(0.5f, true, delegate
					{
						CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.Delay(0.5f, true, delegate
						{
							foreach (DeadBodySliding otherBody in CS_0024_003C_003E8__locals1.otherBodies)
							{
								if (otherBody != null)
								{
									otherBody.OnDie();
									InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, UnityEngine.Random.Range(2, 5), otherBody.transform.position);
								}
							}
						}));
						CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.FadeRedAway();
					}));
				}
				CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.StartCoroutine(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.Delay(1f, true, delegate
				{
					if (_003C_003Ec__DisplayClass47_3.targetBodies[slot] != null)
					{
						_003C_003Ec__DisplayClass47_3.targetBodies[slot].OnDie();
						InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BONE, UnityEngine.Random.Range(2, 5), _003C_003Ec__DisplayClass47_3.targetBodies[slot].transform.position);
					}
				}));
			});
			break;
		}
		case RelicType.FillUpFervour:
		{
			Debug.Log("===========================  RelicType.FillUpFervour".Colour(Color.green));
			Vector3 vector = Vector3.back;
			if (vector == Vector3.back)
			{
				vector = HUD_Manager.Instance.FaithAmmoTransition.transform.localPosition;
			}
			if (FaithAmmo.Ammo >= FaithAmmo.Total)
			{
				HUD_Manager.Instance.FaithAmmoTransition.transform.DOKill();
				HUD_Manager.Instance.FaithAmmoTransition.transform.localPosition = vector;
				HUD_Manager.Instance.FaithAmmoTransition.transform.DOPunchPosition(new Vector3(1f, 0f, 0f), 0.5f);
				RelicEvent onRelicCantUse4 = PlayerRelic.OnRelicCantUse;
				if (onRelicCantUse4 != null)
				{
					onRelicCantUse4(EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType));
				}
				return;
			}
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(base.transform, new Transform[1] { base.transform });
			int num8 = 30;
			int num9 = -1;
			float num10 = 0.5f;
			while (++num9 < num8)
			{
				float num11 = 360f / (float)num8 * (float)num9;
				Vector3 vector2 = new Vector3(num10 * Mathf.Cos(num11 * ((float)Math.PI / 180f)), num10 * Mathf.Sin(num11 * ((float)Math.PI / 180f)));
				BlackSoul blackSoul = InventoryItem.SpawnBlackSoul(1, base.transform.position + vector2, false, true);
				if (blackSoul != null)
				{
					blackSoul.Delta = 0;
					blackSoul.Delay = 0.2f;
					blackSoul.SetAngle(num11, new Vector2(10f, 10f));
					blackSoul.magnetiseDistance = 100f;
				}
			}
			GameManager.GetInstance().WaitForSeconds(0.75f, delegate
			{
				FaithAmmo.Reload();
			});
			break;
		}
		case RelicType.DamageBoss:
		{
			for (int num7 = Health.team2.Count - 1; num7 >= 0; num7--)
			{
				if (Health.team2[num7] != null && Health.team2[num7].GetComponent<UnitObject>() != null && Health.team2[num7].GetComponent<UnitObject>().IsBoss)
				{
					Health.team2[num7].DealDamage(PlayerWeapon.GetDamage(20f, DataManager.Instance.CurrentWeaponLevel), base.gameObject, base.transform.position, false, Health.AttackTypes.Melee, true, Health.AttackFlags.DoesntChargeRelics);
				}
			}
			break;
		}
		case RelicType.SpawnTentacle:
		{
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(base.transform, new Transform[1] { base.transform });
			float num2 = 60f;
			CS_0024_003C_003E8__locals1.t = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(EquipmentType.TENTACLE_TAROT_REF).Prefab, (GenerateRoom.Instance != null) ? GenerateRoom.Instance.transform : base.transform.parent, true).GetComponent<Tentacle>();
			CS_0024_003C_003E8__locals1.t.ShootsProjectiles = true;
			CS_0024_003C_003E8__locals1.t.TimeBetweenProjectiles = 10f;
			CS_0024_003C_003E8__locals1.t.transform.position = base.transform.position;
			CS_0024_003C_003E8__locals1.t.AttackFlags = Health.AttackFlags.DoesntChargeRelics;
			CS_0024_003C_003E8__locals1.t.GetComponent<Health>().enabled = false;
			GameObject obj = UnityEngine.Object.Instantiate(EquipmentManager.GetCurseData(EquipmentType.TENTACLE_TAROT_REF).SecondaryPrefab, (GenerateRoom.Instance != null) ? GenerateRoom.Instance.transform : base.transform.parent);
			obj.transform.position = base.transform.position - Vector3.right;
			obj.GetComponent<FX_CrackController>().duration = num2 + 0.5f;
			float damage = EquipmentManager.GetCurseData(EquipmentType.TENTACLE_TAROT_REF).Damage;
			CS_0024_003C_003E8__locals1.t.Play(0f, num2, damage * PlayerSpells.GetCurseDamageMultiplier(), Health.Team.PlayerTeam, false, 0, true, true);
			CameraManager.instance.ShakeCameraForDuration(0.6f, 0.8f, 0.25f);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/tentacles", CS_0024_003C_003E8__locals1.t.gameObject);
			AudioManager.Instance.PlayOneShot("event:/material/stone_break", base.gameObject);
			AudioManager.Instance.PlayOneShot("event:/followers/break_free", base.gameObject);
			BiomeConstants.Instance.EmitParticleChunk(BiomeConstants.TypeOfParticle.stone, CS_0024_003C_003E8__locals1.t.transform.position, Vector3.one, 5);
			GameManager.GetInstance().WaitForSeconds(num2 + 0.5f, delegate
			{
				AudioManager.Instance.PlayOneShot("event:/material/stone_break", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
				AudioManager.Instance.PlayOneShot("event:/followers/break_free", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
				CameraManager.instance.ShakeCameraForDuration(0.3f, 0.5f, 0.2f);
				BiomeConstants.Instance.EmitSmokeExplosionVFX(CS_0024_003C_003E8__locals1.t.transform.position);
				BiomeConstants.Instance.EmitParticleChunk(BiomeConstants.TypeOfParticle.stone, CS_0024_003C_003E8__locals1.t.transform.position, Vector3.one, 10);
			});
			break;
		}
		case RelicType.ProjectileRing:
		{
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(base.transform, new Transform[1] { base.transform });
			int num14 = DataManager.Instance.PlayerRunTrinkets.Count * 3;
			if (num14 == 0)
			{
				num14 = 2;
			}
			Projectile.CreatePlayerProjectiles(num14, PlayerFarming.Instance.health, base.transform.position, tarotProjectilePath, 16f, PlayerWeapon.GetDamage(4f, DataManager.Instance.CurrentWeaponLevel), 0f, true, null, Health.AttackFlags.DoesntChargeRelics);
			CameraManager.instance.ShakeCameraForDuration(0.6f, 0.8f, 0.25f);
			AudioManager.Instance.PlayOneShot("event:/player/Curses/goop_shot", base.gameObject);
			break;
		}
		case RelicType.RandomEnemyIntoCritter:
		{
			List<Health> list2 = new List<Health>();
			for (int num12 = Health.team2.Count - 1; num12 >= 0; num12--)
			{
				if (Health.team2[num12] != null && Health.team2[num12].gameObject.activeSelf && Health.team2[num12].GetComponent<UnitObject>() != null && !Health.team2[num12].GetComponent<UnitObject>().IsBoss && Health.team2[num12].DamageModifier >= 1f)
				{
					list2.Add(Health.team2[num12]);
				}
			}
			if (list2.Count == 0)
			{
				RelicEvent onRelicCantUse5 = PlayerRelic.OnRelicCantUse;
				if (onRelicCantUse5 != null)
				{
					onRelicCantUse5(EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType));
				}
				return;
			}
			list2 = list2.OrderByDescending((Health x) => x.HP).ToList();
			CS_0024_003C_003E8__locals1.enemy = list2[0].GetComponent<UnitObject>();
			CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(base.transform, new Transform[1] { CS_0024_003C_003E8__locals1.enemy.transform });
			AudioManager.Instance.PlayOneShot("event:/relics/rainbow_bubble", CS_0024_003C_003E8__locals1.enemy.gameObject);
			GameManager.GetInstance().WaitForSeconds(0.1f, delegate
			{
				BiomeGenerator.Instance.CurrentRoom.generateRoom.TurnEnemyIntoCritter(CS_0024_003C_003E8__locals1.enemy);
				AudioManager.Instance.PlayOneShot("event:/boss/jellyfish/staff_magic", CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1._003C_003E4__this.gameObject);
			});
			break;
		}
		case RelicType.TeleportToBoss:
			StartCoroutine(TeleportToBossRoom());
			break;
		case RelicType.RandomTeleport:
			if (!BiomeGenerator.Instance.CurrentRoom.IsBoss && BiomeGenerator.Instance.CurrentRoom.y != 999 && DungeonLeaderMechanics.Instance == null)
			{
				StartCoroutine(TeleportToRandomRoom());
			}
			break;
		case RelicType.DamageOnTouch_Familiar:
		case RelicType.FreezeOnTouch_Familiar:
		case RelicType.PoisonOnTouch_Familiar:
		{
			AudioManager.Instance.PlayOneShot("event:/relics/relic_runedraw_long");
			string key = "Assets/Prefabs/Familiars/Damage Familiar.prefab";
			if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.FreezeOnTouch_Familiar)
			{
				key = "Assets/Prefabs/Familiars/Freeze Familiar.prefab";
			}
			else if (CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType == RelicType.PoisonOnTouch_Familiar)
			{
				key = "Assets/Prefabs/Familiars/Poison Familiar.prefab";
			}
			CS_0024_003C_003E8__locals1.targets = new List<Transform>();
			for (int j = 0; j < 2; j++)
			{
				AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(key, CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform.position, Quaternion.identity, base.transform.parent);
				asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> o)
				{
					Familiar familiar = o.Result.GetComponent<Familiar>();
					familiar.SetMaster(PlayerFarming.Instance.unitObject);
					familiar.gameObject.SetActive(false);
					familiar.enabled = false;
					familiar.Container.transform.localScale = Vector3.zero;
					familiar.SetDirection((CS_0024_003C_003E8__locals1.targets.Count == 0) ? 1 : (-1));
					Vector3 position3 = o.Result.transform.position;
					GameManager.GetInstance().WaitForSeconds(0.6f, delegate
					{
						familiar.gameObject.SetActive(true);
						familiar.Container.transform.DOScale(0.25f, 0.75f).OnComplete(delegate
						{
							familiar.GetComponentInChildren<DOTweenAnimation>().DOPlay();
						});
						GameManager.GetInstance().WaitForSeconds(1f, delegate
						{
							familiar.enabled = true;
						});
					});
					CS_0024_003C_003E8__locals1.targets.Add(familiar.transform);
					if (CS_0024_003C_003E8__locals1.targets.Count >= 2)
					{
						CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, CS_0024_003C_003E8__locals1.targets.ToArray());
					}
				};
			}
			break;
		}
		case RelicType.ShootCurses_Familiar:
		{
			CS_0024_003C_003E8__locals1.tempObj = new GameObject();
			CS_0024_003C_003E8__locals1.tempObj.transform.position = base.transform.position + ((UnityEngine.Random.value > 0.5f) ? Vector3.right : Vector3.left);
			AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync("Assets/Prefabs/Familiars/Curse Familiar.prefab", CS_0024_003C_003E8__locals1.tempObj.transform.position - Vector3.forward, Quaternion.identity, base.transform.parent);
			asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> o)
			{
				Familiar familiar2 = o.Result.GetComponent<Familiar>();
				familiar2.gameObject.SetActive(false);
				familiar2.GetComponentInChildren<DOTweenAnimation>().DOPlay();
				CS_0024_003C_003E8__locals1.sequence = EquipmentManager.GetRelicData(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.relicType).VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { CS_0024_003C_003E8__locals1.tempObj.transform });
				GameManager.GetInstance().WaitForSeconds(0.8f, delegate
				{
					familiar2.gameObject.SetActive(true);
				});
				VFXSequence sequence8 = CS_0024_003C_003E8__locals1.sequence;
				sequence8.OnComplete = (Action)Delegate.Combine(sequence8.OnComplete, (Action)delegate
				{
					UnityEngine.Object.Destroy(CS_0024_003C_003E8__locals1.tempObj);
				});
			};
			break;
		}
		case RelicType.InstantlyKillModifiedEnemies:
		{
			for (int num = Health.team2.Count - 1; num >= 0; num--)
			{
				if (Health.team2[num] != null && Health.team2[num].gameObject.activeSelf && Health.team2[num].GetComponent<UnitObject>() != null && Health.team2[num].GetComponent<UnitObject>().HasModifier)
				{
					Health.team2[num].DealDamage(Health.team2[num].HP, base.gameObject, Health.team2[num].transform.position, false, Health.AttackTypes.Melee, false, Health.AttackFlags.DoesntChargeRelics);
				}
			}
			break;
		}
		case RelicType.FreezeTime:
		{
			CS_0024_003C_003E8__locals1.freezeSequence = CurrentRelic.VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform });
			VFXSequence freezeSequence = CS_0024_003C_003E8__locals1.freezeSequence;
			freezeSequence.OnImpact = (Action<VFXObject, int>)Delegate.Combine(freezeSequence.OnImpact, new Action<VFXObject, int>(CS_0024_003C_003E8__locals1._003CUseRelic_003Eg__StartFreezeTime_007C12));
			break;
		}
		case RelicType.IncreaseDamageForDuration:
			CS_0024_003C_003E8__locals1.damageSequence = CurrentRelic.VFXData.PlayNewSequence(CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform, new Transform[1] { CS_0024_003C_003E8__locals1.CS_0024_003C_003E8__locals1.playerTransform });
			CS_0024_003C_003E8__locals1.loopedSound = AudioManager.Instance.CreateLoop("event:/relics/increase_damage_for_duration", base.gameObject, true);
			DataManager.Instance.PLAYER_RUN_DAMAGE_LEVEL += 0.75f;
			GameManager.GetInstance().WaitForSeconds(10f, delegate
			{
				VFXObject[] impactVFXObjects = CS_0024_003C_003E8__locals1.damageSequence.ImpactVFXObjects;
				for (int k = 0; k < impactVFXObjects.Length; k++)
				{
					impactVFXObjects[k].StopVFX();
				}
				AudioManager.Instance.StopLoop(CS_0024_003C_003E8__locals1.loopedSound);
				DataManager.Instance.PLAYER_RUN_DAMAGE_LEVEL -= 0.75f;
			});
			break;
		}
		if ((CurrentRelic.InteractionType == RelicInteractionType.Fragile || CurrentRelic.InteractionType == RelicInteractionType.Instant) && EquipmentManager.NextRandomRelic != CurrentRelic.RelicType)
		{
			CurrentRelic = null;
			DataManager.Instance.CurrentRelic = RelicType.None;
			RelicEvent onRelicEquipped = PlayerRelic.OnRelicEquipped;
			if (onRelicEquipped != null)
			{
				onRelicEquipped(null);
			}
		}
		ResetChargedAmount();
		RelicEvent onRelicChargeModified = PlayerRelic.OnRelicChargeModified;
		if (onRelicChargeModified != null)
		{
			onRelicChargeModified(CurrentRelic);
		}
		if (flag)
		{
			ConsumeRelic(_003C_003Ec__DisplayClass47_.relicType, forceConsumableAnimation);
		}
	}

	private IEnumerator VFXTimer(float duration, VFXSequence sequence)
	{
		yield return new WaitForSeconds(duration);
		if (sequence != null && sequence.ImpactVFXObjects != null && sequence.ImpactVFXObjects[0] != null)
		{
			sequence.ImpactVFXObjects[0].StopVFX();
		}
	}

	public void RemoveRelic()
	{
		ConsumeRelic(CurrentRelic.RelicType, true);
		CurrentRelic = null;
		DataManager.Instance.CurrentRelic = RelicType.None;
		RelicEvent onRelicEquipped = PlayerRelic.OnRelicEquipped;
		if (onRelicEquipped != null)
		{
			onRelicEquipped(null);
		}
	}

	public void ConsumeRelic(RelicType relicType, bool forceConsumableAnimation)
	{
		RelicEvent onRelicConsumed = PlayerRelic.OnRelicConsumed;
		if (onRelicConsumed != null)
		{
			onRelicConsumed(null);
		}
		if (animationSequence != null)
		{
			animationSequence.Complete();
		}
		if (animationCoroutine != null)
		{
			StopCoroutine(animationCoroutine);
		}
		relicIcon.DOKill();
		relicIcon.gameObject.SetActive(false);
		RelicData data = EquipmentManager.GetRelicData(relicType);
		if (!data.ShowAnimationAbovePlayer)
		{
			return;
		}
		GameManager.GetInstance().WaitForSeconds(0.25f, delegate
		{
			relicIcon.gameObject.SetActive(true);
			relicIcon.transform.localPosition = relicIconOriginalPosition;
			relicIcon.transform.localScale = Vector3.one * 0.25f;
			relicIcon.transform.DOPunchScale(Vector3.one * 0.125f, 0.15f);
			relicIcon.sprite = ((data.WorldSprite != null) ? data.WorldSprite : data.UISprite);
			if (data.InteractionType == RelicInteractionType.Fragile || forceConsumableAnimation)
			{
				AudioManager.Instance.PlayOneShot("event:/relics/relic_break", base.gameObject);
				animationCoroutine = StartCoroutine(Delay(1.5f, true, delegate
				{
					relicPuff.Play();
					AudioManager.Instance.PlayOneShot("event:/relics/puff_of_smoke", base.gameObject);
					relicIcon.gameObject.SetActive(false);
				}));
			}
			else if (data.InteractionType == RelicInteractionType.Charging)
			{
				animationSequence = DOTween.Sequence();
				animationSequence.AppendInterval(1.2f);
				animationSequence.Append(relicIcon.transform.DOLocalMove(Vector3.zero, 0.35f).SetEase(Ease.InBack));
				animationSequence.Join(relicIcon.transform.DOScale(Vector3.zero, 0.35f).SetEase(Ease.InBack));
			}
		});
	}

	public void EquipRelic(RelicData relicData, bool fullyCharge = true, bool initialEquip = false)
	{
		Action action = delegate
		{
			AudioManager.Instance.PlayOneShot("event:/relics/relic_get");
			CurrentRelic = relicData;
			DataManager.Instance.CurrentRelic = relicData.RelicType;
			if (fullyCharge)
			{
				ChargedAmount = Mathf.Clamp(float.MaxValue, 0f, RequiredChargeAmount);
			}
			if (initialEquip)
			{
				switch (relicData.RelicType)
				{
				case RelicType.UseRandomRelic:
					EquipmentManager.PickRandomRelicData(true);
					break;
				case RelicType.UseRandomRelic_Blessed:
					EquipmentManager.PickRandomRelicData(true, RelicSubType.Blessed);
					break;
				case RelicType.UseRandomRelic_Dammed:
					EquipmentManager.PickRandomRelicData(true, RelicSubType.Dammed);
					break;
				}
			}
			RelicEvent onSubRelicChanged = PlayerRelic.OnSubRelicChanged;
			if (onSubRelicChanged != null)
			{
				onSubRelicChanged(CurrentRelic);
			}
			RelicEvent onRelicEquipped = PlayerRelic.OnRelicEquipped;
			if (onRelicEquipped != null)
			{
				onRelicEquipped(relicData);
			}
			if (animationSequence != null)
			{
				animationSequence.Complete();
			}
			relicIcon.DOKill();
			relicIcon.gameObject.SetActive(true);
			relicIcon.transform.localPosition = relicIconOriginalPosition;
			relicIcon.transform.localScale = Vector3.one * 0.25f;
			relicIcon.transform.DOPunchScale(Vector3.one * 0.2f, 0.25f);
			relicIcon.sprite = ((CurrentRelic.WorldSprite != null) ? CurrentRelic.WorldSprite : CurrentRelic.UISprite);
			animationSequence = DOTween.Sequence();
			animationSequence.AppendInterval(1f);
			animationSequence.Append(relicIcon.transform.DOLocalMove(Vector3.zero, 0.5f).SetEase(Ease.InBack));
			animationSequence.Join(relicIcon.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack));
			if (CurrentRelic.InteractionType == RelicInteractionType.Instant)
			{
				UseRelic(CurrentRelic.RelicType, true);
			}
		};
		if (DataManager.Instance.TryRevealTutorialTopic(TutorialTopic.Relics))
		{
			UITutorialOverlayController uITutorialOverlayController = MonoSingleton<UIManager>.Instance.ShowTutorialOverlay(TutorialTopic.Relics);
			uITutorialOverlayController.OnHidden = (Action)Delegate.Combine(uITutorialOverlayController.OnHidden, action);
		}
		else
		{
			action();
		}
	}

	public void IncreaseChargedAmount()
	{
		if (CurrentRelic != null)
		{
			float num = 1f;
			num *= PlayerFleeceManager.GetRelicChargeMultiplier();
			num *= TrinketManager.GetRelicChargeMultiplier();
			ChargedAmount = Mathf.Clamp(ChargedAmount + num, 0f, RequiredChargeAmount);
			RelicEvent onRelicChargeModified = PlayerRelic.OnRelicChargeModified;
			if (onRelicChargeModified != null)
			{
				onRelicChargeModified(CurrentRelic);
			}
		}
	}

	public void IncreaseChargedAmount(float increase = 1f)
	{
		if (CurrentRelic != null)
		{
			increase *= 0.85f;
			increase *= PlayerFleeceManager.GetRelicChargeMultiplier();
			increase *= TrinketManager.GetRelicChargeMultiplier();
			ChargedAmount = Mathf.Clamp(ChargedAmount + increase, 0f, RequiredChargeAmount);
			RelicEvent onRelicChargeModified = PlayerRelic.OnRelicChargeModified;
			if (onRelicChargeModified != null)
			{
				onRelicChargeModified(CurrentRelic);
			}
		}
	}

	public void ResetChargedAmount()
	{
		if (CurrentRelic != null)
		{
			ChargedAmount = 0f;
			RelicEvent onRelicChargeModified = PlayerRelic.OnRelicChargeModified;
			if (onRelicChargeModified != null)
			{
				onRelicChargeModified(CurrentRelic);
			}
		}
	}

	public static void Reload()
	{
		if (PlayerFarming.Instance != null)
		{
			PlayerFarming.Instance.playerRelic.FullyCharge();
		}
	}

	public void FullyCharge()
	{
		if (CurrentRelic != null)
		{
			ChargedAmount = RequiredChargeAmount;
			RelicEvent onRelicChargeModified = PlayerRelic.OnRelicChargeModified;
			if (onRelicChargeModified != null)
			{
				onRelicChargeModified(CurrentRelic);
			}
		}
	}

	private void Update()
	{
		if (PlayerFarming.Instance == null || PlayerFarming.Instance.GoToAndStopping)
		{
			return;
		}
		StateMachine.State cURRENT_STATE = PlayerFarming.Instance.state.CURRENT_STATE;
		if (((uint)cURRENT_STATE <= 2u || cURRENT_STATE == StateMachine.State.Aiming) && CurrentRelic != null && InputManager.Gameplay.GetRelicButtonDown() && !(ChargedAmount < RequiredChargeAmount))
		{
			RelicEvent onRelicUsed = PlayerRelic.OnRelicUsed;
			if (onRelicUsed != null)
			{
				onRelicUsed(CurrentRelic);
			}
			UseRelic(CurrentRelic.RelicType);
		}
	}

	private IEnumerator Delay(float delay, bool useTimeScale, Action callback)
	{
		if (useTimeScale)
		{
			yield return new WaitForSeconds(delay);
		}
		else
		{
			yield return new WaitForSecondsRealtime(delay);
		}
		if (callback != null)
		{
			callback();
		}
	}

	private void LightningStrike()
	{
		_003C_003Ec__DisplayClass59_0 _003C_003Ec__DisplayClass59_ = new _003C_003Ec__DisplayClass59_0();
		_003C_003Ec__DisplayClass59_._003C_003E4__this = this;
		_003C_003Ec__DisplayClass59_.damage = 12f + PlayerWeapon.GetDamage(1f, DataManager.Instance.CurrentWeaponLevel) * 5f;
		float num = ((CurrentRelic.RelicType != RelicType.LightningStrike) ? 2.5f : 0f);
		int num2 = 5;
		_003C_003Ec__DisplayClass59_.targets = new List<Health>(Health.team2);
		new List<Action>();
		for (int num3 = _003C_003Ec__DisplayClass59_.targets.Count - 1; num3 >= 0; num3--)
		{
			if (_003C_003Ec__DisplayClass59_.targets[num3] == null)
			{
				_003C_003Ec__DisplayClass59_.targets.RemoveAt(num3);
			}
		}
		_003C_003Ec__DisplayClass59_.targets = _003C_003Ec__DisplayClass59_.targets.OrderBy((Health x) => x.HP).Reverse().ToList();
		_003C_003Ec__DisplayClass59_.targets.RemoveAll((Health t) => t.InanimateObject);
		int count = _003C_003Ec__DisplayClass59_.targets.Count;
		if (count > 0)
		{
			_003C_003Ec__DisplayClass59_1 _003C_003Ec__DisplayClass59_2 = new _003C_003Ec__DisplayClass59_1();
			_003C_003Ec__DisplayClass59_2.CS_0024_003C_003E8__locals1 = _003C_003Ec__DisplayClass59_;
			num2 = Mathf.Min(num2, count);
			if (num > 0f)
			{
				for (int i = 0; i < num2; i++)
				{
					DropMultipleLootOnDeath dropMultipleLootOnDeath = _003C_003Ec__DisplayClass59_2.CS_0024_003C_003E8__locals1.targets[i].gameObject.AddComponent<DropMultipleLootOnDeath>();
					dropMultipleLootOnDeath.chanceToDropLoot = num;
					dropMultipleLootOnDeath.RandomAmountToDrop = Vector2.one;
					dropMultipleLootOnDeath.LootToDrop = new List<DropMultipleLootOnDeath.ItemAndProbability>(1)
					{
						new DropMultipleLootOnDeath.ItemAndProbability((CurrentRelic.RelicType == RelicType.LightningStrike_Blessed) ? InventoryItem.ITEM_TYPE.BLUE_HEART : InventoryItem.ITEM_TYPE.BLACK_HEART, 100)
					};
				}
			}
			_003C_003Ec__DisplayClass59_2.targetTransforms = new Transform[num2];
			for (int j = 0; j < num2; j++)
			{
				_003C_003Ec__DisplayClass59_2.targetTransforms[j] = _003C_003Ec__DisplayClass59_2.CS_0024_003C_003E8__locals1.targets[j].transform;
			}
			_003C_003Ec__DisplayClass59_2.sequence = CurrentRelic.VFXData.PlayNewSequence(PlayerFarming.Instance.transform, _003C_003Ec__DisplayClass59_2.targetTransforms);
			VFXSequence sequence = _003C_003Ec__DisplayClass59_2.sequence;
			sequence.OnImpact = (Action<VFXObject, int>)Delegate.Combine(sequence.OnImpact, new Action<VFXObject, int>(_003C_003Ec__DisplayClass59_2._003CLightningStrike_003Eg__Impact_007C2));
			VFXSequence sequence2 = _003C_003Ec__DisplayClass59_2.sequence;
			sequence2.OnComplete = (Action)Delegate.Combine(sequence2.OnComplete, new Action(_003C_003Ec__DisplayClass59_2._003CLightningStrike_003Eg__Complete_007C3));
		}
		else
		{
			CurrentRelic.VFXData.TestSequence(num2, false);
		}
	}

	private void SpawnDemon(int level)
	{
		int type = UnityEngine.Random.Range(0, 5);
		int num = 0;
		while (++num < 30)
		{
			int num2 = UnityEngine.Random.Range(0, 5);
			if (!DataManager.Instance.Followers_Demons_Types.Contains(num2))
			{
				type = num2;
				break;
			}
		}
		RelicType relicType = CurrentRelic.RelicType;
		BiomeGenerator.Instance.SpawnDemon(type, -1, level, true, delegate(Demon demon)
		{
			StartCoroutine(DemonSpawned(demon, relicType));
		});
	}

	private IEnumerator DemonSpawned(Demon demon, RelicType relicType)
	{
		demon.gameObject.SetActive(false);
		EquipmentManager.GetRelicData(relicType).VFXData.PlayNewSequence(PlayerFarming.Instance.transform, new Transform[1] { demon.transform });
		yield return new WaitForSeconds(2f);
		AudioManager.Instance.PlayOneShot("event:/relics/demon_spawn", demon.gameObject);
		demon.gameObject.SetActive(true);
		demon.transform.localScale = Vector3.zero;
		demon.transform.DOScale(1f, 0.25f);
	}

	private IEnumerator DestroyTarotsGainStrength(BonusType bonusType)
	{
		for (int i = DataManager.Instance.PlayerRunTrinkets.Count - 1; i >= 0; i--)
		{
			TrinketManager.RemoveTrinket(DataManager.Instance.PlayerRunTrinkets[i].CardType);
			switch (bonusType)
			{
			case BonusType.GainStrength:
				DataManager.Instance.PLAYER_RUN_DAMAGE_LEVEL += 0.25f;
				BiomeConstants.Instance.EmitHeartPickUpVFX(PlayerFarming.Instance.CameraBone.transform.position, 0f, "strength", "strength");
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", PlayerFarming.Instance.transform.position);
				break;
			case BonusType.GainBlackHearts:
				PlayerFarming.Instance.health.BlackHearts++;
				BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "black", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", PlayerFarming.Instance.transform.position);
				break;
			case BonusType.GainBlueHearts:
				PlayerFarming.Instance.health.BlueHearts++;
				BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "blue", "burst_big");
				AudioManager.Instance.PlayOneShot("event:/followers/love_hearts", PlayerFarming.Instance.transform.position);
				break;
			}
			yield return new WaitForSeconds(0.25f);
		}
		DataManager.Instance.PlayerRunTrinkets.Clear();
	}

	private void DestroyTarotsDealDamage(float damageMultiplier, int maxEnemies, float chanceForBlueHearts, float chanceForBlackHearts)
	{
		_003C_003Ec__DisplayClass64_0 _003C_003Ec__DisplayClass64_ = new _003C_003Ec__DisplayClass64_0();
		_003C_003Ec__DisplayClass64_._003C_003E4__this = this;
		_003C_003Ec__DisplayClass64_.chanceForBlueHearts = chanceForBlueHearts;
		_003C_003Ec__DisplayClass64_.chanceForBlackHearts = chanceForBlackHearts;
		_003C_003Ec__DisplayClass64_.damage = PlayerWeapon.GetDamage(6f, DataManager.Instance.CurrentWeaponLevel) * (float)DataManager.Instance.PlayerRunTrinkets.Count * damageMultiplier;
		_003C_003Ec__DisplayClass64_.damage = Mathf.Clamp(PlayerWeapon.GetDamage(2f, DataManager.Instance.CurrentWeaponLevel) * (float)DataManager.Instance.PlayerRunTrinkets.Count, 3f, 20f);
		List<Transform> list = new List<Transform>();
		_003C_003Ec__DisplayClass64_.targetEnemies = new List<Health>();
		for (int num = Health.team2.Count - 1; num >= 0; num--)
		{
			if (Health.team2[num] != null && _003C_003Ec__DisplayClass64_.targetEnemies.Count < maxEnemies)
			{
				_003C_003Ec__DisplayClass64_.targetEnemies.Add(Health.team2[num]);
				list.Add(Health.team2[num].transform);
			}
		}
		_003C_003Ec__DisplayClass64_.sequence = CurrentRelic.VFXData.PlayNewSequence(PlayerFarming.Instance.transform, list.ToArray());
		VFXSequence sequence = _003C_003Ec__DisplayClass64_.sequence;
		sequence.OnImpact = (Action<VFXObject, int>)Delegate.Combine(sequence.OnImpact, new Action<VFXObject, int>(_003C_003Ec__DisplayClass64_._003CDestroyTarotsDealDamage_003Eg__Impact_007C0));
		VFXSequence sequence2 = _003C_003Ec__DisplayClass64_.sequence;
		sequence2.OnComplete = (Action)Delegate.Combine(sequence2.OnComplete, new Action(_003C_003Ec__DisplayClass64_._003CDestroyTarotsDealDamage_003Eg__Complete_007C1));
	}

	private void FiftyFiftyGamble(RelicType relicType)
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(base.gameObject, 5f);
		GameManager.GetInstance().CameraSetOffset(new Vector3(0f, 0.5f, -1f));
		VFXSequence vFXSequence = EquipmentManager.GetRelicData(relicType).VFXData.PlayNewSequence(PlayerFarming.Instance.transform, new Transform[1] { base.transform });
		AudioManager.Instance.PlayOneShot("event:/relics/fifty_fifty_dice", base.gameObject);
		PlayerFarming.Instance.SpineUseDeltaTime(false);
		PlayerFarming.Instance.Spine.AnimationState.AddAnimation(0, "cast-spell2-loop", true, 0f);
		VFX_Dice component = vFXSequence.ImpactVFXObjects[0].GetComponent<VFX_Dice>();
		if (component.OnDiceRolled != null)
		{
			Delegate[] invocationList = component.OnDiceRolled.GetInvocationList();
			foreach (Delegate @delegate in invocationList)
			{
				component.OnDiceRolled = (Action<bool>)Delegate.Remove(component.OnDiceRolled, (Action<bool>)@delegate);
			}
		}
		component.OnDiceRolled = (Action<bool>)Delegate.Combine(component.OnDiceRolled, (Action<bool>)delegate(bool won)
		{
			PlayerFarming.Instance.health.untouchable = false;
			GameManager.GetInstance().WaitForSeconds(0.25f, delegate
			{
				switch (relicType)
				{
				case RelicType.FiftyFiftyGamble:
					if (!won)
					{
						PlayerFarming.Instance.health.TotalSpiritHearts += 2f;
						AudioManager.Instance.PlayOneShot("event:/player/collect_heart", base.transform.position);
						BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big");
					}
					else
					{
						PlayerFarming.Instance.health.Heal(TrinketManager.GetHealthAmountMultiplier());
						AudioManager.Instance.PlayOneShot("event:/player/collect_heart", base.transform.position);
						BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "red", "burst_big");
					}
					break;
				case RelicType.FiftyFiftyGamble_Blessed:
					if (won)
					{
						PlayerFarming.Instance.playerController.MakeUntouchable(12.5f);
						InvincibleFromRelic = true;
						VFXSequence sequence = EquipmentManager.GetRelicData(RelicType.GungeonBlank).VFXData.PlayNewSequence(PlayerFarming.Instance.transform, new Transform[1] { PlayerFarming.Instance.transform });
						GameManager.GetInstance().StartCoroutine(VFXTimer(12.5f, sequence));
					}
					else
					{
						PlayerFarming.Instance.health.BlueHearts += 2f;
						AudioManager.Instance.PlayOneShot("event:/player/collect_heart", base.transform.position);
						BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "blue", "burst_big");
					}
					break;
				case RelicType.FiftyFiftyGamble_Dammed:
					if (!won)
					{
						Health.DamageAllEnemies(PlayerWeapon.GetDamage(5f, DataManager.Instance.CurrentWeaponLevel), Health.DamageAllEnemiesType.Manipulation);
					}
					else
					{
						PlayerFarming.Instance.health.BlackHearts += 2f;
						AudioManager.Instance.PlayOneShot("event:/player/collect_heart", base.transform.position);
						BiomeConstants.Instance.EmitHeartPickUpVFX(base.transform.position, 0f, "black", "burst_big");
					}
					break;
				}
			});
			PlayerFarming.Instance.SpineUseDeltaTime(true);
			GameManager.GetInstance().OnConversationEnd();
			GameManager.GetInstance().CameraSetOffset(Vector3.zero);
		});
	}

	private IEnumerator TeleportToBossRoom()
	{
		GameManager.GetInstance().OnConversationNew();
		GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
		PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
		PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "warp-out-down", false);
		yield return new WaitForSeconds(2.5f);
		MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", delegate
		{
			GameManager.GetInstance().OnConversationEnd();
			Vector2 vector = new Vector2(BiomeGenerator.Instance.LastRoom.x, BiomeGenerator.Instance.LastRoom.y);
			Vector2 vector2 = BiomeGenerator.Instance.GetBossRoom();
			if (vector2 != new Vector2Int(0, 0) && vector2 != new Vector2(0f, 999f))
			{
				vector = BiomeGenerator.Instance.GetBossRoom();
			}
			BiomeGenerator.ChangeRoom((int)vector.x, (int)vector.y);
		});
	}

	private IEnumerator TeleportToRandomRoom()
	{
		BiomeRoom room = null;
		int num = 0;
		while (num++ < 100)
		{
			room = BiomeGenerator.Instance.Rooms[UnityEngine.Random.Range(0, BiomeGenerator.Instance.Rooms.Count)];
			if (room != BiomeGenerator.Instance.CurrentRoom && room != BiomeGenerator.Instance.RespawnRoom && room != BiomeGenerator.Instance.DeathCatRoom)
			{
				break;
			}
		}
		if (room != null)
		{
			GameManager.GetInstance().OnConversationNew();
			GameManager.GetInstance().OnConversationNext(PlayerFarming.Instance.gameObject);
			PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
			PlayerFarming.Instance.Spine.AnimationState.SetAnimation(0, "warp-out-down", false);
			yield return new WaitForSeconds(2.5f);
			MMTransition.Play(MMTransition.TransitionType.ChangeRoom, MMTransition.Effect.BlackFade, MMTransition.NO_SCENE, 0.5f, "", delegate
			{
				GameManager.GetInstance().OnConversationEnd();
				BiomeGenerator.ChangeRoom(room.x, room.y);
			});
		}
	}

	private void SpawnFriendlyEnemy(VFXObject result, int slot)
	{
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(friendlyEnemy, base.transform.parent);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> friendlyEnemy)
		{
			Health friendlyEnemyHealth = friendlyEnemy.Result.GetComponent<Health>();
			friendlyEnemyHealth.enabled = false;
			friendlyEnemyHealth.team = Health.Team.PlayerTeam;
			Health health = friendlyEnemyHealth;
			float hP = (friendlyEnemyHealth.totalHP = 15f);
			health.HP = hP;
			friendlyEnemyHealth.ImmuneToPlayer = true;
			friendlyEnemyHealth.transform.position = result.transform.position;
			friendlyEnemyHealth.gameObject.SetActive(false);
			StartCoroutine(Delay(1f, true, delegate
			{
				friendlyEnemyHealth.enabled = true;
				EnemySwordsman component = friendlyEnemy.Result.GetComponent<EnemySwordsman>();
				component.SeperateObject = true;
				component.gameObject.SetActive(true);
				component.Damage = PlayerWeapon.GetDamage(2f, DataManager.Instance.CurrentWeaponLevel);
				component.health.team = Health.Team.PlayerTeam;
				component.FollowPlayer = true;
				component.enabled = true;
				component.VisionRange = int.MaxValue;
				Vector3 normalized = (component.transform.position - base.transform.position).normalized;
				float distance = Vector3.Distance(component.transform.position, base.transform.position);
				RaycastHit2D[] array = Physics2D.RaycastAll(base.transform.position, normalized, distance);
				for (int i = 0; i < array.Length; i++)
				{
					RaycastHit2D raycastHit2D = array[i];
					CompositeCollider2D component2 = raycastHit2D.collider.GetComponent<CompositeCollider2D>();
					RoomLockController component3 = raycastHit2D.collider.GetComponent<RoomLockController>();
					if (component2 != null || component3 != null)
					{
						Debug.LogWarning("Friendly Combatant spawned outside of island collider, moving to player location");
						component.transform.position = PlayerFarming.Instance.transform.position;
						break;
					}
				}
				CameraManager.instance.ShakeCameraForDuration(0.5f, 0.7f, 0.2f);
			}));
		};
	}

	public void AddFrozenEnemy(Health enemy)
	{
		if (enemy != null)
		{
			enemy.AddFreezeTime();
			enemies.Add(enemy);
		}
	}
}
