using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerFollowerSelection : BaseMonoBehaviour
{
	public SpriteRenderer Image;

	public StateMachine State;

	public PlayerFarming playerFarming;

	public float Distance = 2f;

	public static bool IsPlaying;

	public static PlayerFollowerSelection Instance;

	public List<Follower> SelectedFollowers = new List<Follower>();

	private UICommandFollowerWheel UICommandFollowerWheel;

	private Coroutine cScaleCirlce;

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDisable()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}

	private void Start()
	{
		Image.transform.localScale = Vector3.zero;
		IsPlaying = false;
	}

	private IEnumerator IdleRoutine()
	{
		while (true)
		{
			if (Time.timeScale <= 0f)
			{
				yield return null;
				continue;
			}
			while (playerFarming.GoToAndStopping)
			{
				yield return null;
			}
			StateMachine.State cURRENT_STATE = State.CURRENT_STATE;
			if ((uint)cURRENT_STATE <= 1u && InputManager.UI.GetPageNavigateLeftDown())
			{
				break;
			}
			yield return null;
		}
		StartCoroutine(SelectRoutine());
	}

	private IEnumerator SelectRoutine()
	{
		IsPlaying = true;
		if (cScaleCirlce != null)
		{
			StopCoroutine(cScaleCirlce);
		}
		cScaleCirlce = StartCoroutine(ScaleCirlce(0f, Distance, 0.3f));
		yield return new WaitForSeconds(0.3f);
		while (InputManager.UI.GetPageNavigateLeftHeld() && !playerFarming.GoToAndStopping && !LetterBox.IsPlaying)
		{
			foreach (Follower follower in Follower.Followers)
			{
				if (!follower.Brain.FollowingPlayer && Vector3.Distance(base.transform.position, follower.transform.position) < Distance)
				{
					follower.Brain.FollowingPlayer = true;
					follower.Brain.CompleteCurrentTask();
					follower.PlayParticles();
					if (SelectedFollowers.Count <= 0)
					{
						CreateUI();
					}
					UICommandFollowerWheel.AddPortrait(follower.Brain.Info);
					SelectedFollowers.Add(follower);
				}
			}
			yield return null;
		}
		if (cScaleCirlce != null)
		{
			StopCoroutine(cScaleCirlce);
		}
		cScaleCirlce = StartCoroutine(ScaleCirlce(Image.transform.localScale.x, 0f, 0.3f));
		if (SelectedFollowers.Count <= 0)
		{
			IsPlaying = false;
			StartCoroutine(IdleRoutine());
		}
		else
		{
			State.CURRENT_STATE = StateMachine.State.Map;
			GameManager.GetInstance().CameraSetTargetZoom(15f);
		}
	}

	private void CreateUI()
	{
		GameObject gameObject = Object.Instantiate(Resources.Load("Prefabs/UI/UI Follower Command Wheel") as GameObject);
		float scaleFactor = ((Screen.width >= 0 && Screen.width <= 1080) ? 0.5f : ((Screen.width > 1080 && Screen.width < 2160) ? 1f : ((Screen.width < 2160) ? 1f : 2f)));
		gameObject.GetComponent<CanvasScaler>().scaleFactor = scaleFactor;
		UICommandFollowerWheel = gameObject.GetComponent<UICommandFollowerWheel>();
		UICommandFollowerWheel.CallbackClose = delegate(UICommandFollowerWheel.ActivityChoice.AvailableCommands activityChoice)
		{
			State.CURRENT_STATE = StateMachine.State.Idle;
			GameManager.GetInstance().CameraResetTargetZoom();
			foreach (Follower selectedFollower in SelectedFollowers)
			{
				selectedFollower.Brain.FollowingPlayer = false;
				selectedFollower.Brain.Stats.WorkerBeenGivenOrders = true;
				selectedFollower.PlayParticles();
				switch (activityChoice)
				{
				case UICommandFollowerWheel.ActivityChoice.AvailableCommands.ChopTrees:
					if (selectedFollower.Brain.CurrentTaskType != FollowerTaskType.ChopTrees)
					{
						selectedFollower.Brain.HardSwapToTask(new FollowerTask_ChopTrees());
					}
					break;
				case UICommandFollowerWheel.ActivityChoice.AvailableCommands.ClearRubble:
				{
					Rubble rubble = null;
					float num = float.MaxValue;
					foreach (Rubble rubble2 in Rubble.Rubbles)
					{
						if (Vector3.Distance(rubble2.transform.position, base.transform.position) < num)
						{
							rubble = rubble2;
						}
					}
					if (selectedFollower.Brain.CurrentTaskType != FollowerTaskType.ClearRubble)
					{
						selectedFollower.Brain.HardSwapToTask(new FollowerTask_ClearRubble(rubble));
					}
					break;
				}
				case UICommandFollowerWheel.ActivityChoice.AvailableCommands.ClearWeeds:
				{
					Interaction_Weed weed = null;
					float num = float.MaxValue;
					foreach (Interaction_Weed weed2 in Interaction_Weed.Weeds)
					{
						if (Vector3.Distance(weed2.transform.position, base.transform.position) < num)
						{
							weed = weed2;
						}
					}
					if (selectedFollower.Brain.CurrentTaskType != FollowerTaskType.ClearWeeds)
					{
						selectedFollower.Brain.HardSwapToTask(new FollowerTask_ClearWeeds(weed));
					}
					break;
				}
				default:
					selectedFollower.Brain.CompleteCurrentTask();
					break;
				}
			}
			IsPlaying = false;
			SelectedFollowers.Clear();
			StartCoroutine(IdleRoutine());
		};
	}

	private void Update()
	{
		Image.transform.Rotate(new Vector3(0f, 0f, 10f) * Time.deltaTime);
	}

	private IEnumerator ScaleCirlce(float Start, float Target, float Duration)
	{
		float Progress = 0f;
		while (true)
		{
			float num;
			Progress = (num = Progress + Time.deltaTime);
			if (!(num < Duration))
			{
				break;
			}
			Image.transform.localScale = Vector3.one * Mathf.SmoothStep(Start, Target, Progress / Duration);
			yield return null;
		}
		Image.transform.localScale = Vector3.one * Target;
	}
}
