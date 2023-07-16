public class GoopBomb : PoisonBomb
{
	protected override void BombLanded()
	{
		AudioManager.Instance.PlayOneShot("event:/player/Curses/goop_impact", base.gameObject);
		base.BombLanded();
	}
}
