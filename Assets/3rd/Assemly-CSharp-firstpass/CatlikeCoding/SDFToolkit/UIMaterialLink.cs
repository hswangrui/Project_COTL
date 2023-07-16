using System;
using UnityEngine;

namespace CatlikeCoding.SDFToolkit
{
	[ExecuteInEditMode]
	[Obsolete("UIMaterialLink is no longer needed to make keyword materials work at run time. It will be removed in a future release.")]
	public class UIMaterialLink : MonoBehaviour
	{
		[SerializeField]
		private Material sourceMaterial;

		[NonSerialized]
		private string[] shaderKeywords;

		public Material SourceMaterial
		{
			get
			{
				return sourceMaterial;
			}
			set
			{
				if (value != sourceMaterial)
				{
					sourceMaterial = value;
					shaderKeywords = null;
				}
			}
		}

		public Material GetModifiedMaterial(Material baseMaterial)
		{
			if (shaderKeywords == null)
			{
				if (sourceMaterial == null)
				{
					Debug.LogWarning("UIMaterialLink needs a material reference!", this);
				}
				else
				{
					shaderKeywords = sourceMaterial.shaderKeywords;
				}
			}
			baseMaterial.shaderKeywords = shaderKeywords;
			return baseMaterial;
		}
	}
}
