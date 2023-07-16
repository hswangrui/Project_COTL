using MMTools;

public class Interaction_GoToScene : Interaction
{
	public string SceneToLoad;

	public float FadeDuration;

	public override void OnInteract(StateMachine state)
	{
		base.OnInteract(state);
		MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, SceneToLoad, FadeDuration, "", null);
	}
}
