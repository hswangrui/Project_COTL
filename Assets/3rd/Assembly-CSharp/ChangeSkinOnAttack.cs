using System;
using Spine.Unity;
using UnityEngine;

public class ChangeSkinOnAttack : MonoBehaviour
{
	[SerializeField]
	private EnemyBat EnemyBat;

	[SerializeField]
	private SkeletonAnimation Spine;

	[SerializeField]
	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	private string DefaultSkin;

	[SerializeField]
	[SpineSkin("", "", true, false, false, dataField = "Spine")]
	private string[] AttackSkins;

	private void OnEnable()
	{
		EnemyBat enemyBat = EnemyBat;
		enemyBat.OnAttack = (Action<int>)Delegate.Combine(enemyBat.OnAttack, new Action<int>(OnAttack));
		EnemyBat enemyBat2 = EnemyBat;
		enemyBat2.OnAttackComplete = (Action)Delegate.Combine(enemyBat2.OnAttackComplete, new Action(OnAttackComplete));
	}

	private void OnDisable()
	{
		EnemyBat enemyBat = EnemyBat;
		enemyBat.OnAttack = (Action<int>)Delegate.Remove(enemyBat.OnAttack, new Action<int>(OnAttack));
		EnemyBat enemyBat2 = EnemyBat;
		enemyBat2.OnAttackComplete = (Action)Delegate.Remove(enemyBat2.OnAttackComplete, new Action(OnAttackComplete));
	}

	private void OnAttack(int obj)
	{
		Spine.Skeleton.SetSkin(AttackSkins[obj]);
	}

	private void OnAttackComplete()
	{
		Spine.Skeleton.SetSkin(DefaultSkin);
	}
}
