using System;
using System.Collections;
using System.Collections.Generic;
using MMTools;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;

public class SimpleCutscene : BaseMonoBehaviour
{
	private class ObjectAndPositions
	{
		public GameObject g;

		public List<Vector3> Positions = new List<Vector3>();

		public Color color;

		public ObjectAndPositions(GameObject Object, Vector3 StartingPosition, Vector3 DestinationPosition)
		{
			g = Object;
			Positions.Add(StartingPosition);
			Positions.Add(DestinationPosition);
			color = Color.blue;
		}

		public void AddPosition(Vector3 NewPosition)
		{
			Positions.Add(Positions[Positions.Count - 1] + NewPosition);
		}
	}

	[Serializable]
	public class CutsceneScene
	{
		public string Label = "Scene";

		public List<CutsceneObject> Scene = new List<CutsceneObject>();

		public void AddStep()
		{
			Scene.Add(new CutsceneObject(CutsceneObject.TypeOfScene.Animate));
		}

		public CutsceneScene(string Label)
		{
			this.Label = Label;
		}
	}

	[Serializable]
	public class CutsceneObject
	{
		public enum TypeOfScene
		{
			Animate,
			Delay,
			Destroy,
			Move,
			BeginCutscene,
			MoveCutsceneCamera,
			EndCutscene,
			AddObjectToCamera,
			ScreenShake,
			SetScale,
			Callback,
			PlaySound,
			Conversation
		}

		public TypeOfScene Type;

		public SkeletonAnimation Spine;

		[SpineAnimation("", "Spine", true, false)]
		public string Animation;

		public bool Loop;

		public bool WaitForEndOfAnimation = true;

		public bool DestroyAfterAnimation;

		public float Delay = 0.5f;

		public GameObject ObjectToDestroy;

		public GameObject CameraFocus;

		public float Zoom = 6f;

		public GameObject AddToCamera;

		public float MinimumShake = 0.2f;

		public float MaximumShake = 0.3f;

		public float ShakeDuration = 0.5f;

		public GameObject MoveObject;

		public Vector3 MoveDestination;

		public float MoveSpeed = 5f;

		public bool WaitForEndOfMove = true;

		public bool isPlayer;

		public GameObject ObjectToScale;

		public Vector3 Scale = Vector3.one;

		public UnityEvent Callback;

		public List<ConversationEntry> Entries;

		public bool EndLetterBoxAfterConversation = true;

		public CutsceneObject(TypeOfScene Type)
		{
			this.Type = Type;
		}
	}

	public bool TriggerOnCollision;

	public Vector3 TriggerPosition = Vector3.zero;

	public float TriggerRadius = 1f;

	[Space]
	public List<CutsceneScene> Cutscene = new List<CutsceneScene>();

	private GameObject Player;

	public void AddScene()
	{
		Cutscene.Add(new CutsceneScene("Scene " + (Cutscene.Count + 1)));
	}

	public void Play()
	{
		Debug.Log("PLAY CUTSCENE!");
		StartCoroutine(PlayRoutine());
	}

	private void Update()
	{
		if (TriggerOnCollision && !((Player = GameObject.FindWithTag("Player")) == null) && Vector3.Distance(Player.transform.position, base.transform.position + TriggerPosition) < TriggerRadius)
		{
			TriggerOnCollision = false;
			Play();
		}
	}

	private IEnumerator PlayRoutine()
	{
		foreach (CutsceneScene item in Cutscene)
		{
			foreach (CutsceneObject item2 in item.Scene)
			{
				switch (item2.Type)
				{
				case CutsceneObject.TypeOfScene.Animate:
					yield return StartCoroutine(AnimateRoutine(item2));
					break;
				case CutsceneObject.TypeOfScene.Delay:
					yield return new WaitForSeconds(item2.Delay);
					break;
				case CutsceneObject.TypeOfScene.Destroy:
					UnityEngine.Object.Destroy(item2.ObjectToDestroy);
					break;
				case CutsceneObject.TypeOfScene.BeginCutscene:
					GameManager.GetInstance().OnConversationNew();
					break;
				case CutsceneObject.TypeOfScene.MoveCutsceneCamera:
					GameManager.GetInstance().OnConversationNext(item2.CameraFocus, item2.Zoom);
					break;
				case CutsceneObject.TypeOfScene.EndCutscene:
					GameManager.GetInstance().OnConversationEnd();
					break;
				case CutsceneObject.TypeOfScene.AddObjectToCamera:
					GameManager.GetInstance().AddToCamera(item2.AddToCamera);
					break;
				case CutsceneObject.TypeOfScene.ScreenShake:
					CameraManager.instance.ShakeCameraForDuration(item2.MinimumShake, item2.MaximumShake, item2.ShakeDuration);
					break;
				case CutsceneObject.TypeOfScene.Move:
					if (item2.WaitForEndOfMove)
					{
						yield return StartCoroutine(MoveRoutine(item2));
					}
					else
					{
						StartCoroutine(MoveRoutine(item2));
					}
					break;
				case CutsceneObject.TypeOfScene.SetScale:
					item2.ObjectToScale.transform.localScale = item2.Scale;
					break;
				case CutsceneObject.TypeOfScene.Callback:
					item2.Callback.Invoke();
					break;
				case CutsceneObject.TypeOfScene.Conversation:
					MMConversation.Play(new ConversationObject(item2.Entries, null, null), item2.EndLetterBoxAfterConversation);
					while (MMConversation.isPlaying)
					{
						yield return null;
					}
					break;
				}
				yield return null;
			}
			yield return null;
		}
	}

	private IEnumerator AnimateRoutine(CutsceneObject c)
	{
		c.Spine.AnimationState.SetAnimation(0, c.Animation, c.Loop);
		if (c.WaitForEndOfAnimation)
		{
			yield return new WaitForSeconds(c.Spine.AnimationState.GetCurrent(0).Animation.Duration);
		}
		if (c.DestroyAfterAnimation)
		{
			StartCoroutine(DestroyAfterDelay(c, c.WaitForEndOfAnimation ? 0f : c.Spine.AnimationState.GetCurrent(0).Animation.Duration));
		}
	}

	private IEnumerator DestroyAfterDelay(CutsceneObject c, float Duration)
	{
		yield return new WaitForSeconds(Duration);
		UnityEngine.Object.Destroy(c.Spine.gameObject);
	}

	private IEnumerator MoveRoutine(CutsceneObject c)
	{
		float Progress = 0f;
		Vector3 StartingPosition = c.MoveObject.transform.position;
		if (c.isPlayer && Player != null)
		{
			StartingPosition = Player.transform.position;
		}
		float Distance = Vector3.Distance(StartingPosition, StartingPosition + c.MoveDestination);
		while (Progress < 1f)
		{
			Progress += c.MoveSpeed / Distance * Time.deltaTime;
			if (Progress >= 1f)
			{
				Progress = 1f;
			}
			c.MoveObject.transform.position = Vector3.Lerp(StartingPosition, StartingPosition + c.MoveDestination, Progress);
			yield return null;
		}
	}

	private void OnDrawGizmos()
	{
		if (TriggerOnCollision)
		{
			Utils.DrawCircleXY(base.transform.position + TriggerPosition, TriggerRadius, Color.yellow);
		}
		foreach (CutsceneScene item in Cutscene)
		{
			List<ObjectAndPositions> list = new List<ObjectAndPositions>();
			int num = -1;
			while (++num < item.Scene.Count)
			{
				CutsceneObject cutsceneObject = item.Scene[num];
				if (cutsceneObject.Type != CutsceneObject.TypeOfScene.Move || !(cutsceneObject.MoveObject != null))
				{
					continue;
				}
				ObjectAndPositions objectAndPositions = null;
				foreach (ObjectAndPositions item2 in list)
				{
					if (item2.g == cutsceneObject.MoveObject)
					{
						objectAndPositions = item2;
						break;
					}
				}
				if (objectAndPositions == null)
				{
					list.Add(new ObjectAndPositions(cutsceneObject.MoveObject, cutsceneObject.MoveObject.transform.position, cutsceneObject.MoveObject.transform.position + cutsceneObject.MoveDestination));
				}
				else
				{
					objectAndPositions.AddPosition(cutsceneObject.MoveDestination);
				}
			}
			foreach (ObjectAndPositions item3 in list)
			{
				num = -1;
				while (++num < item3.Positions.Count)
				{
					if (num > 0)
					{
						Utils.DrawLine(item3.Positions[num - 1], item3.Positions[num], item3.color);
					}
					Utils.DrawCircleXY(item3.Positions[num], 0.3f, item3.color);
				}
			}
		}
	}
}
