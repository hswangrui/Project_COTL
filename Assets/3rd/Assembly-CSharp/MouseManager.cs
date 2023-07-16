using UnityEngine;

public class MouseManager : BaseMonoBehaviour
{
	public TargetMarker target;

	private static MouseManager Instance;

	private LineRenderer lineRenderer;

	private Transform lineStartPosition;

	public bool showLine;

	private bool dragCam;

	private Vector3 oldPos;

	private Vector3 panOrigin;

	private Vector3 panTargetPosition;

	private Vector3 camZoom;

	private float camZoomTarget;

	private void Start()
	{
		Instance = this;
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		lineRenderer.startWidth = 0.05f;
		lineRenderer.endWidth = 0.05f;
		lineRenderer.positionCount = 2;
		lineRenderer.enabled = false;
		panTargetPosition = Camera.main.transform.position;
		camZoom.z = 0f;
	}

	private void Update()
	{
		if (showLine)
		{
			if (lineStartPosition == null)
			{
				showLine = false;
			}
			else
			{
				lineRenderer.SetPosition(0, lineStartPosition.position);
				Vector3 mousePosition = Input.mousePosition;
				mousePosition.z = 0f - Camera.main.transform.position.z;
				Vector3 position = Camera.main.ScreenToWorldPoint(mousePosition);
				lineRenderer.SetPosition(1, position);
				Instance.target.reveal(position);
			}
		}
		if (Input.GetMouseButtonDown(1))
		{
			dragCam = true;
			oldPos = Camera.main.transform.transform.position - camZoom;
			panOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
		}
		if (Input.GetMouseButton(1))
		{
			Vector3 vector = Camera.main.ScreenToViewportPoint(Input.mousePosition) - panOrigin;
			panTargetPosition = oldPos + -vector * (0f - Camera.main.transform.position.z);
		}
		if (Input.GetMouseButtonUp(1))
		{
			dragCam = false;
		}
		if (Input.GetAxis("Mouse ScrollWheel") > 0f)
		{
			camZoomTarget += 1f;
		}
		if (Input.GetAxis("Mouse ScrollWheel") < 0f)
		{
			camZoomTarget -= 1f;
		}
		if (camZoomTarget < -4f)
		{
			camZoomTarget = -4f;
		}
		if (camZoomTarget > 2f)
		{
			camZoomTarget = 2f;
		}
		camZoom.z += (camZoomTarget - camZoom.z) / 5f;
		Vector3 position2 = Camera.main.transform.position;
		position2 += (panTargetPosition + camZoom - position2) / 3f;
		Camera.main.transform.position = position2;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	private static MouseManager getInstance()
	{
		return Instance;
	}

	public static void placeTarget(Vector3 position)
	{
		if (!(Instance == null))
		{
			Instance.target.reveal(position);
		}
	}

	public static void ShowLine(Transform position)
	{
		if (!(Instance == null))
		{
			Instance.lineRenderer.enabled = true;
			Instance.lineStartPosition = position;
			Instance.showLine = true;
		}
	}

	public static void HideLine()
	{
		if (!(Instance == null))
		{
			Instance.showLine = false;
			Instance.lineRenderer.enabled = false;
		}
	}
}
