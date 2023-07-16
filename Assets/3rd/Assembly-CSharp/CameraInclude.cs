using UnityEngine;

public class CameraInclude : BaseMonoBehaviour
{
	public Vector3 ActivateOffset = Vector3.zero;

	public float Distance = 4f;

	public float TargetZoom = -1f;

	public float Weight = 1f;

	public int MinZoom = -1;

	public int MaxZoom = -1;

	private GameObject _Player;

	private GameObject Player
	{
		get
		{
			if (_Player == null)
			{
				_Player = GameObject.FindWithTag("Player");
			}
			return _Player;
		}
		set
		{
			_Player = value;
		}
	}

	private bool OnCam
	{
		get
		{
			return GameManager.GetInstance().CameraContains(base.gameObject);
		}
	}

	private void Update()
	{
		if (!(Player != null))
		{
			return;
		}
		if (LetterBox.IsPlaying)
		{
			if (OnCam && !GameManager.GetInstance().CamFollowTarget.IsMoving)
			{
				if (TargetZoom != -1f)
				{
					GameManager.GetInstance().CameraResetTargetZoom();
				}
				GameManager.GetInstance().RemoveFromCamera(base.gameObject);
			}
			return;
		}
		if (!OnCam && Vector2.Distance(Player.transform.position, base.transform.position + ActivateOffset) <= Distance)
		{
			if (TargetZoom != -1f)
			{
				GameManager.GetInstance().CameraSetTargetZoom(TargetZoom);
			}
			GameManager.GetInstance().AddToCamera(base.gameObject, Weight);
			if (MinZoom != -1)
			{
				GameManager.GetInstance().CamFollowTarget.MinZoom = MinZoom;
			}
			if (MaxZoom != -1)
			{
				GameManager.GetInstance().CamFollowTarget.MaxZoom = MaxZoom;
			}
		}
		if (OnCam && Vector2.Distance(Player.transform.position, base.transform.position + ActivateOffset) > Distance)
		{
			if (TargetZoom != -1f)
			{
				GameManager.GetInstance().CameraResetTargetZoom();
			}
			GameManager.GetInstance().RemoveFromCamera(base.gameObject);
			if (MinZoom != -1)
			{
				GameManager.GetInstance().CamFollowTarget.MinZoom = 11f;
			}
			if (MaxZoom != -1)
			{
				GameManager.GetInstance().CamFollowTarget.MaxZoom = 13f;
			}
		}
	}

	private void OnDisable()
	{
		if ((bool)GameManager.GetInstance())
		{
			if (OnCam)
			{
				GameManager.GetInstance().CameraResetTargetZoom();
			}
			GameManager.GetInstance().RemoveFromCamera(base.gameObject);
		}
	}

	private void OnDrawGizmos()
	{
		Utils.DrawCircleXY(base.transform.position + ActivateOffset, Distance, Color.green);
	}
}
