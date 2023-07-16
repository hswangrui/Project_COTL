using Spine.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

public class PlacementObject : BaseMonoBehaviour
{
	public static PlacementObject Instance;

	private Transform ChildObject;

	public Vector2Int Bounds = new Vector2Int(1, 1);

	public string ToBuildAsset;

	private Vector3 Scale = Vector3.one;

	private Vector3 ScaleSpeed = Vector3.zero;

	public Vector2 ScaleEasing = new Vector2(0.3f, 0.5f);

	private float Shake;

	private float ShakeSpeed;

	public Vector2 ShakeEasing = new Vector2(0.3f, 0.5f);

	public float ShakeIntensity = 0.2f;

	private Vector3 originalPosition = Vector3.zero;

	private int Direction;

	public Transform RotatedObject;

	public StructureBrain.TYPES StructureType { get; set; }

	private void Start()
	{
		if (string.IsNullOrEmpty(ToBuildAsset))
		{
			return;
		}
		ToBuildAsset = "Assets/" + ToBuildAsset + ".prefab";
		AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(ToBuildAsset, Vector3.zero, Quaternion.identity, base.transform);
		asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
		{
			GameObject result = obj.Result;
			ChildObject = (((object)result != null) ? result.transform : null);
			MonoBehaviour[] componentsInChildren = ChildObject.GetComponentsInChildren<MonoBehaviour>();
			foreach (MonoBehaviour monoBehaviour in componentsInChildren)
			{
				if (!(monoBehaviour is SkeletonRenderer) && !(monoBehaviour is SpriteShapeController))
				{
					monoBehaviour.enabled = false;
				}
			}
		};
	}

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

	private void Update()
	{
		if (!(ChildObject == null))
		{
			ScaleSpeed.x += (1f - Scale.x) * ScaleEasing.x / Time.unscaledDeltaTime;
			Scale.x += (ScaleSpeed.x *= ScaleEasing.y) * Time.unscaledDeltaTime;
			ScaleSpeed.y += (1f - Scale.y) * ScaleEasing.x / Time.unscaledDeltaTime;
			Scale.y += (ScaleSpeed.y *= ScaleEasing.y) * Time.unscaledDeltaTime;
			ScaleSpeed.z += (1f - Scale.z) * ScaleEasing.x / Time.unscaledDeltaTime;
			Scale.z += (ScaleSpeed.z *= ScaleEasing.y) * Time.unscaledDeltaTime;
			ChildObject.localScale = Scale;
			ShakeSpeed += (0f - Shake) * ShakeEasing.x / Time.unscaledDeltaTime;
			Shake += (ShakeSpeed *= ShakeEasing.y) * Time.unscaledDeltaTime;
			ChildObject.localPosition = originalPosition + new Vector3(Shake, 0f, 0f);
		}
	}

	public void SetScale(Vector3 Scale)
	{
		this.Scale = Scale;
	}

	public void DoShake()
	{
		ShakeSpeed = ShakeIntensity * (float)((++Direction % 2 != 0) ? 1 : (-1));
	}

	private void OnDrawGizmos()
	{
		int num = -1;
		while (++num < Bounds.x)
		{
			float num2 = -1f;
			while ((num2 += 1f) < (float)Bounds.y)
			{
				Gizmos.matrix = RotatedObject.localToWorldMatrix;
				Gizmos.DrawWireCube(new Vector3(num, num2), new Vector3(0.7f, 0.7f, 0f));
			}
		}
	}
}
