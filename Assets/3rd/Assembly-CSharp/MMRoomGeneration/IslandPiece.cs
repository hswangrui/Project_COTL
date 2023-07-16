using System;
using System.Collections;
using System.Collections.Generic;
using MMBiomeGeneration;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;

namespace MMRoomGeneration
{
	public class IslandPiece : BaseMonoBehaviour
	{
		[Serializable]
		public class GameObjectAndProbability
		{
			public string GameObjectPath;

			[Range(0f, 100f)]
			public int Probability = 50;

			public bool NewGamePlus;

			public bool LayerOne;

			public bool LayerTwo;

			public bool LayerThree;

			public bool LayerFour;

			internal bool AvailableOnLayer()
			{
				if (GameManager.DungeonUseAllLayers)
				{
					return true;
				}
				switch (GameManager.CurrentDungeonLayer)
				{
				case 1:
					return LayerOne;
				case 2:
					return LayerTwo;
				case 3:
					return LayerThree;
				case 4:
					return LayerFour;
				default:
					return false;
				}
			}
		}

		[Serializable]
		public class ListOfGameObjectAndProbability
		{
			public List<GameObjectAndProbability> ObjectList = new List<GameObjectAndProbability>();

			public string GetRandomGameObject(double RandomSeed)
			{
				return GetRandomWeightedIndex(ObjectList, RandomSeed).GameObjectPath;
			}
		}

		public bool TestMe;

		public bool CanUseRandomDoors;

		public IslandConnector[] Connectors;

		public List<IslandConnector> NorthConnectors = new List<IslandConnector>();

		public List<IslandConnector> EastConnectors = new List<IslandConnector>();

		public List<IslandConnector> SouthConnectors = new List<IslandConnector>();

		public List<IslandConnector> WestConnectors = new List<IslandConnector>();

		public PolygonCollider2D _Collider;

		public bool CanSpawnEncounter = true;

		public bool IsDoor;

		private string CurrentFilePath;

		private MeshRenderer m;

		private bool checkedMesh;

		private GameObject SpriteShapeGO;

		public ListOfGameObjectAndProbability SpriteShapes = new ListOfGameObjectAndProbability();

		public ListOfGameObjectAndProbability SpriteShapes2 = new ListOfGameObjectAndProbability();

		public ListOfGameObjectAndProbability Encounters = new ListOfGameObjectAndProbability();

		public List<SpriteShapeController> EncounterSpriteShapes = new List<SpriteShapeController>();

		private List<IslandConnector> ReturnConnectors;

		public string NewPrefabPath = "Assets";

		public PolygonCollider2D Collider
		{
			get
			{
				if (_Collider == null)
				{
					_Collider = GetComponentInChildren<PolygonCollider2D>();
				}
				if (_Collider == null)
				{
					_Collider = GetComponent<PolygonCollider2D>();
				}
				return _Collider;
			}
		}

		public string CurrentEncounter { get; private set; }

		private void OnDrawGizmos()
		{
			CurrentFilePath = NewPrefabPath + "/Encounter" + Encounters.ObjectList.Count + ".prefab";
		}

		private void CreateEnemyEncounterPrefab()
		{
		}

		private void GetConnectors()
		{
			Connectors = GetComponentsInChildren<IslandConnector>();
			NorthConnectors.Clear();
			EastConnectors.Clear();
			SouthConnectors.Clear();
			WestConnectors.Clear();
			IslandConnector[] connectors = Connectors;
			foreach (IslandConnector islandConnector in connectors)
			{
				switch (islandConnector.MyDirection)
				{
				case IslandConnector.Direction.North:
					NorthConnectors.Add(islandConnector);
					break;
				case IslandConnector.Direction.East:
					EastConnectors.Add(islandConnector);
					break;
				case IslandConnector.Direction.South:
					SouthConnectors.Add(islandConnector);
					break;
				case IslandConnector.Direction.West:
					WestConnectors.Add(islandConnector);
					break;
				}
			}
		}

		private void OnEnable()
		{
			if (!checkedMesh)
			{
				m = GetComponent<MeshRenderer>();
				if (m != null)
				{
					m.enabled = false;
				}
				else
				{
					checkedMesh = true;
				}
			}
		}

		public void SetSpriteShape()
		{
			GenerateRoom generateRoom = UnityEngine.Object.FindObjectOfType<GenerateRoom>();
			if (SpriteShapeGO == null)
			{
				SpriteShapeGO = new GameObject();
				SpriteShapeGO.transform.parent = base.transform;
				SpriteShapeGO.transform.localPosition = Vector3.zero;
				SpriteShapeGO.name = "Sprite Shape";
			}
			SpriteShapeController spriteShapeController = SpriteShapeGO.AddComponent<SpriteShapeController>();
			if (generateRoom.DecorationList.SpriteShapeMaterial != null)
			{
				Material[] sharedMaterials = spriteShapeController.spriteShapeRenderer.sharedMaterials;
				for (int i = 0; i < sharedMaterials.Length; i++)
				{
					sharedMaterials[i] = generateRoom.DecorationList.SpriteShapeMaterial;
				}
				spriteShapeController.spriteShapeRenderer.sharedMaterials = sharedMaterials;
			}
			spriteShapeController.spriteShape = generateRoom.DecorationList.SpriteShape;
			spriteShapeController.spline.Clear();
			int num = -1;
			Vector2[] points = Collider.points;
			Array.Reverse((Array)points);
			while (++num < points.Length)
			{
				spriteShapeController.spline.InsertPointAt(num, points[num]);
			}
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				componentsInChildren[j].enabled = false;
			}
		}

		public static GameObjectAndProbability GetRandomWeightedIndex(List<GameObjectAndProbability> weights, double Random)
		{
			if (weights == null || weights.Count == 0)
			{
				return null;
			}
			int num = 0;
			for (int i = 0; i < weights.Count; i++)
			{
				if (weights[i].Probability >= 0 && weights[i].AvailableOnLayer())
				{
					num += weights[i].Probability;
				}
			}
			float num2 = 0f;
			for (int j = 0; j < weights.Count; j++)
			{
				if (weights[j].AvailableOnLayer() && !((float)weights[j].Probability <= 0f))
				{
					num2 += (float)weights[j].Probability / (float)num;
					if ((double)num2 >= Random)
					{
						return weights[j];
					}
				}
			}
			return null;
		}

		public IEnumerator InitIsland(System.Random Seed, SpriteShape SecondarySpriteShape, Action completeCallback = null)
		{
			bool waiting = true;
			HideSprites();
			ListOfGameObjectAndProbability listOfGameObjectAndProbability = (GameManager.Layer2 ? SpriteShapes2 : SpriteShapes);
			if (listOfGameObjectAndProbability.ObjectList.Count > 0)
			{
				string gameObjectPath = listOfGameObjectAndProbability.ObjectList[Seed.Next(0, listOfGameObjectAndProbability.ObjectList.Count)].GameObjectPath;
				if (gameObjectPath != null)
				{
					AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(gameObjectPath, new Vector3(0f, 0f, -0.005f), Quaternion.identity, base.transform);
					asyncOperationHandle.Completed += delegate(AsyncOperationHandle<GameObject> obj)
					{
						GameObject result = obj.Result;
						EncounterSpriteShapes = new List<SpriteShapeController>(result.GetComponentsInChildren<SpriteShapeController>());
						foreach (SpriteShapeController encounterSpriteShape in EncounterSpriteShapes)
						{
							encounterSpriteShape.spriteShape = SecondarySpriteShape;
							if (GenerateRoom.Instance.DecorationList.SpriteShapeMaterial != null)
							{
								Material[] sharedMaterials = encounterSpriteShape.spriteShapeRenderer.sharedMaterials;
								for (int i = 0; i < sharedMaterials.Length; i++)
								{
									sharedMaterials[i] = GenerateRoom.Instance.DecorationList.SpriteShapeMaterial;
								}
								encounterSpriteShape.spriteShapeRenderer.sharedMaterials = sharedMaterials;
							}
							encounterSpriteShape.spriteShapeRenderer.sortingLayerName = "Ground - Secondary Layer";
							encounterSpriteShape.spriteShapeRenderer.shadowCastingMode = ShadowCastingMode.Off;
							encounterSpriteShape.spriteShapeRenderer.receiveShadows = false;
						}
						waiting = false;
					};
					while (waiting)
					{
						yield return null;
					}
				}
			}
			List<GameObjectAndProbability> list = new List<GameObjectAndProbability>();
			foreach (GameObjectAndProbability @object in Encounters.ObjectList)
			{
				if (@object.AvailableOnLayer() && (BiomeGenerator.Instance == null || !BiomeGenerator.EncounterAlreadyUsed(@object.GameObjectPath)))
				{
					list.Add(@object);
				}
			}
			if (Encounters.ObjectList.Count > 0 && list.Count <= 0)
			{
				foreach (GameObjectAndProbability object2 in Encounters.ObjectList)
				{
					BiomeGenerator.RemoveEncounterAsUsed(object2.GameObjectPath);
				}
				foreach (GameObjectAndProbability object3 in Encounters.ObjectList)
				{
					if (object3.AvailableOnLayer() && (BiomeGenerator.Instance == null || !BiomeGenerator.EncounterAlreadyUsed(object3.GameObjectPath)))
					{
						list.Add(object3);
					}
				}
			}
			if (Encounters.ObjectList.Count > 0 && list.Count > 0)
			{
				string gameObjectPath2 = list[Seed.Next(0, list.Count)].GameObjectPath;
				if (gameObjectPath2 != null)
				{
					BiomeGenerator.SetEncounterAsUsed(gameObjectPath2);
					CurrentEncounter = gameObjectPath2;
					waiting = true;
					AsyncOperationHandle<GameObject> asyncOperationHandle = Addressables.InstantiateAsync(gameObjectPath2, base.transform);
					asyncOperationHandle.Completed += delegate
					{
						waiting = false;
					};
					while (waiting)
					{
						yield return null;
					}
				}
				else
				{
					Debug.Log("WARNING: Null encounter");
				}
			}
			if (completeCallback != null)
			{
				completeCallback();
			}
		}

		public void HideSprites()
		{
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = false;
			}
		}

		private void ShowSprites()
		{
			if (SpriteShapeGO != null)
			{
				if (Application.isEditor)
				{
					UnityEngine.Object.DestroyImmediate(SpriteShapeGO);
				}
				else
				{
					UnityEngine.Object.Destroy(SpriteShapeGO);
				}
			}
			SpriteShapeGO = null;
			SpriteRenderer[] componentsInChildren = GetComponentsInChildren<SpriteRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].enabled = true;
			}
		}

		private void RoundColliders()
		{
			List<Vector2> list = new List<Vector2>();
			int num = -1;
			while (++num < Collider.points.Length)
			{
				float x = Mathf.Round(Collider.points[num].x * 2f) / 2f;
				float y = Mathf.Round(Collider.points[num].y * 2f) / 2f;
				list.Add(new Vector2(x, y));
			}
			Collider.SetPath(0, list);
		}

		public List<IslandConnector> GetConnectorsDirection(IslandConnector.Direction Direction, bool AcceptOthers)
		{
			switch (Direction)
			{
			case IslandConnector.Direction.North:
				if (AcceptOthers && NorthConnectors.Count <= 0)
				{
					return GetRandomConnector();
				}
				return NorthConnectors;
			case IslandConnector.Direction.East:
				if (AcceptOthers && EastConnectors.Count <= 0)
				{
					return GetRandomConnector();
				}
				return EastConnectors;
			case IslandConnector.Direction.South:
				if (AcceptOthers && SouthConnectors.Count <= 0)
				{
					return GetRandomConnector();
				}
				return SouthConnectors;
			case IslandConnector.Direction.West:
				if (AcceptOthers && WestConnectors.Count <= 0)
				{
					return GetRandomConnector();
				}
				return WestConnectors;
			default:
				return null;
			}
		}

		public List<IslandConnector> GetRandomConnector()
		{
			ReturnConnectors = new List<IslandConnector>();
			IslandConnector[] connectors = Connectors;
			foreach (IslandConnector islandConnector in connectors)
			{
				if (!islandConnector.Active)
				{
					ReturnConnectors.Add(islandConnector);
				}
			}
			return ReturnConnectors;
		}
	}
}
