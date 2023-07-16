using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SetSpriteshapeMaterial : BaseMonoBehaviour
{
	public List<SpriteShapeMaterial> SpriteShapes = new List<SpriteShapeMaterial>();

	public Material fallBackMaterial;

	public Material tmpMaterial;

	public static SetSpriteshapeMaterial Instance;

	private void OnEnable()
	{
		Instance = this;
	}

	private void OnDestroy()
	{
		SpriteShapes.Clear();
		Instance = null;
	}

	public GroundType GetGroundType(SpriteShape spriteShape)
	{
		foreach (SpriteShapeMaterial spriteShape2 in SpriteShapes)
		{
			if (spriteShape2.ss == spriteShape)
			{
				return spriteShape2.GroundTag;
			}
		}
		return GroundType.Grass;
	}

	public void SetSpriteShapeMaterials()
	{
		SpriteShapeController[] array = Object.FindObjectsOfType(typeof(SpriteShapeController)) as SpriteShapeController[];
		Debug.Log("Found " + array.Length + " instances with this script attached");
		SpriteShapeController[] array2 = array;
		foreach (SpriteShapeController spriteShapeController in array2)
		{
			tmpMaterial = GetSpriteshapeMaterial(spriteShapeController.spriteShape);
			spriteShapeController.spriteShapeRenderer.sharedMaterial = tmpMaterial;
		}
	}

	public Material GetSpriteshapeMaterial(SpriteShape ss)
	{
		Debug.Log(ss);
		for (int i = 0; i < SpriteShapes.Count - 1; i++)
		{
			if (SpriteShapes[i].ss == ss)
			{
				if (SpriteShapes[i].m == null)
				{
					return fallBackMaterial;
				}
				Debug.Log(SpriteShapes[i].m);
				return SpriteShapes[i].m;
			}
		}
		Debug.Log("Couldn't Find Spriteshape");
		return fallBackMaterial;
	}

	public SpriteShapeRenderer GetSpriteshapeRenderer(SpriteShape ss)
	{
		Debug.Log(ss);
		for (int i = 0; i < SpriteShapes.Count - 1; i++)
		{
			if (SpriteShapes[i].ss == ss)
			{
				Debug.Log(SpriteShapes[i].m);
				return SpriteShapes[i].ssr;
			}
		}
		Debug.Log("Couldn't Find Spriteshape");
		return null;
	}
}
