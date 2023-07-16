public static class EnemyModifierExtension
{
	public static bool HasModifier(this EnemyModifier enemyModifier, EnemyModifier.ModifierType modifierType)
	{
		if (enemyModifier != null)
		{
			return enemyModifier.Modifier == modifierType;
		}
		return false;
	}
}
