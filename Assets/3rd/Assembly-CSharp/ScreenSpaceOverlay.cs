using System;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
public class ScreenSpaceOverlay : BaseMonoBehaviour
{
	public static readonly int blendAmountID = Shader.PropertyToID("_BlendAmount");

	private Renderer _ren;

	private Material dummyMat;

	private Material[] overlayMaterials = new Material[2];

	public void Init()
	{
		_ren = GetComponent<Renderer>();
		_ren.sharedMaterials = overlayMaterials;
		if (dummyMat == null)
		{
			dummyMat = new Material(Shader.Find("Hidden/ScreenSpaceOverlayDummy"));
		}
		SetMaterials(null, null);
	}

	private void Awake()
	{
		Init();
	}

	private void OnEnable()
	{
		Init();
	}

	public void SetMaterials(Material matA, Material matB)
	{
		if (matA == null)
		{
			matA = dummyMat;
		}
		if (matB == null)
		{
			matB = dummyMat;
		}
		overlayMaterials[0] = matA;
		overlayMaterials[1] = matB;
		_ren.sharedMaterials = overlayMaterials;
	}

	public Renderer GetRenderer()
	{
		return _ren;
	}

	public void TransitionMaterial(float blendAmount)
	{
		_ren.sharedMaterials[0].SetFloat(blendAmountID, blendAmount);
		_ren.sharedMaterials[1].SetFloat(blendAmountID, 1f - blendAmount);
	}

	private void Start()
	{
		OnWillRenderObject();
	}

	private void OnWillRenderObject()
	{
		Camera main = Camera.main;
		if (!(main == null))
		{
			float num = main.farClipPlane - 0.1f;
			Vector3 position = main.transform.position;
			Vector3 vector = main.transform.forward * num;
			Vector3 vector2 = position + vector;
			base.transform.position = vector2;
			Vector3 vector3 = (base.transform.parent ? base.transform.parent.localScale : Vector3.one);
			float num2 = (main.orthographic ? (main.orthographicSize * 2f) : (Mathf.Tan(main.fieldOfView * ((float)Math.PI / 180f) * 0.5f) * num * 2f));
			base.transform.localScale = new Vector3(num2 * main.aspect / vector3.x, num2 / vector3.y, 0f);
			if (Camera.current == null || Camera.current == Camera.main)
			{
				base.transform.rotation = Quaternion.LookRotation(vector2 - position, main.transform.up);
			}
			else
			{
				base.transform.LookAt(position);
			}
		}
	}
}
