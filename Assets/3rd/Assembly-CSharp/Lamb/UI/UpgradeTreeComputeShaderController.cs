using System.Collections;
using System.Collections.Generic;
using Lamb.UI.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace Lamb.UI
{
	public class UpgradeTreeComputeShaderController : MonoBehaviour
	{
		[Header("UI Stuff")]
		[SerializeField]
		private ComputeShader _computeShaderTest;

		[SerializeField]
		private RectTransform _rootViewport;

		[SerializeField]
		private MMScrollRect _scrollRect;

		[SerializeField]
		private RawImage _rawImage;

		[SerializeField]
		private List<UpgradeTreeNode> _treeNodes;

		[SerializeField]
		private UpgradeTreeConfiguration _upgradeTreeConfiguration;

		[SerializeField]
		private List<TierLockIcon> _tierLocks;

		[SerializeField]
		private bool _playerUpgrades;

		[Header("Effects Properties")]
		[SerializeField]
		[Range(0f, 1000f)]
		private float _radius = 100f;

		[SerializeField]
		[Range(0f, 1f)]
		private float _smoothStepMin;

		[SerializeField]
		[Range(0f, 1f)]
		private float _smoothStepMax = 1f;

		[SerializeField]
		[Range(0f, 5f)]
		private float _multiply = 1f;

		[SerializeField]
		[Range(800f, 2500f)]
		private float _height = 1000f;

		[SerializeField]
		[Range(0.1f, 1000f)]
		private float _verticalFalloff = 100f;

		[SerializeField]
		[Range(0.1f, 1000f)]
		private float _tierFalloff = 20f;

		private const int kResolutionScale = 2;

		private Vector2Int _resolution;

		private NodeData[] _data;

		private ComputeBuffer _nodeBuffer;

		private RenderTexture _renderTexture;

		private Material _effectsMaterial;

		private int _offsetProperty;

		private int _horizontalFixProperty;

		private int _verticalFixProperty;

		private Vector2 _offset;

		private UpgradeTreeNode.TreeTier _highestTier;

		private float _tierPosition;

		private Dictionary<UpgradeTreeNode.TreeTier, float> _tierPositions = new Dictionary<UpgradeTreeNode.TreeTier, float>();

		private int _updateKernel;

		private float _targetTierPosition
		{
			get
			{
				UpgradeTreeNode.TreeTier treeTier = (_playerUpgrades ? DataManager.Instance.CurrentPlayerUpgradeTreeTier : DataManager.Instance.CurrentUpgradeTreeTier);
				float a;
				if (treeTier < _highestTier)
				{
					a = _tierPositions[treeTier];
					UpgradeTreeNode.TreeTier treeTier2 = treeTier + 1;
					float b = _tierPositions[treeTier2];
					a = Mathf.Lerp(a, b, _upgradeTreeConfiguration.NormalizedProgressToNextTier(treeTier2));
				}
				else
				{
					int num = 5;
					int num2 = _upgradeTreeConfiguration.NumRequiredNodesForTier(_highestTier) + num;
					int num3 = _upgradeTreeConfiguration.NumUnlockedUpgrades();
					float t = 1f - Mathf.Clamp((float)(num2 - num3) / (float)(_upgradeTreeConfiguration.GetConfigForTier(_highestTier).NumRequiredToUnlock + num), 0f, 1f);
					a = Mathf.Lerp(_tierPositions[_highestTier], _height, t);
				}
				return a - _tierFalloff * 0.5f;
			}
		}

		public List<UpgradeTreeNode> TreeNodes
		{
			get
			{
				return _treeNodes;
			}
		}

		public UpgradeTreeConfiguration UpgradeTreeConfiguration
		{
			get
			{
				return _upgradeTreeConfiguration;
			}
		}

		public List<TierLockIcon> TierLockIcons
		{
			get
			{
				return _tierLocks;
			}
		}

		private void OnEnable()
		{
			_data = new NodeData[_treeNodes.Count];
			_nodeBuffer = new ComputeBuffer(_data.Length, _data.Length * NodeData.Size());
		}

		private void OnDisable()
		{
			if (_nodeBuffer != null)
			{
				_nodeBuffer.Release();
				_nodeBuffer = null;
			}
		}

		private IEnumerator Start()
		{
			yield return null;
			_resolution = new Vector2Int(Screen.width / 2, Screen.height / 2);
			if (_renderTexture == null)
			{
				_renderTexture = new RenderTexture(_resolution.x, _resolution.y, 24);
				_renderTexture.enableRandomWrite = true;
				_renderTexture.filterMode = FilterMode.Bilinear;
				_renderTexture.Create();
				_rawImage.texture = _renderTexture;
				_updateKernel = _computeShaderTest.FindKernel("Update");
			}
			if (_effectsMaterial == null)
			{
				_effectsMaterial = new Material(_rawImage.material);
				_offsetProperty = Shader.PropertyToID("_ScrollOffset");
				_horizontalFixProperty = Shader.PropertyToID("_HorizontalFix");
				_verticalFixProperty = Shader.PropertyToID("_VerticalFix");
				if (_resolution.x > _resolution.y)
				{
					_effectsMaterial.SetFloat(_horizontalFixProperty, (float)_resolution.x / (float)_resolution.y);
					_effectsMaterial.SetFloat(_verticalFixProperty, 1f);
				}
				else
				{
					_effectsMaterial.SetFloat(_horizontalFixProperty, 1f);
					_effectsMaterial.SetFloat(_verticalFixProperty, (float)_resolution.y / (float)_resolution.x);
				}
				_rawImage.material = _effectsMaterial;
			}
			_highestTier = _upgradeTreeConfiguration.HighestTier();
			_tierPositions.Add(UpgradeTreeNode.TreeTier.Tier1, 0f - _height);
			foreach (TierLockIcon tierLock in _tierLocks)
			{
				_tierPositions.Add(tierLock.Tier, tierLock.RectTransform.anchoredPosition.y);
			}
			_tierPosition = _targetTierPosition;
		}

		private void Update()
		{
			if (!(_renderTexture == null))
			{
				float num = 2f / ((_rootViewport != null) ? _rootViewport.localScale.x : 1f);
				for (int i = 0; i < _treeNodes.Count; i++)
				{
					_data[i].Position = _treeNodes[i].RectTransform.position;
					_data[i].Position /= 2f;
					_data[i].Influence = _treeNodes[i].UnlockedWeight;
				}
				_nodeBuffer.SetData(_data);
				_computeShaderTest.SetBuffer(0, "TestBuffer", _nodeBuffer);
				_computeShaderTest.SetInt("BufferSize", _nodeBuffer.count);
				_computeShaderTest.SetTexture(_updateKernel, "Result", _renderTexture);
				_computeShaderTest.SetFloat("SmoothStepMin", _smoothStepMin);
				_computeShaderTest.SetFloat("SmoothStepMax", _smoothStepMax);
				_computeShaderTest.SetFloat("Time", Time.deltaTime);
				_computeShaderTest.SetFloat("Radius", _radius / num);
				_computeShaderTest.SetFloat("Multiply", _multiply);
				_computeShaderTest.SetVector("MousePosition", Input.mousePosition / num);
				_tierPosition -= (_tierPosition - _targetTierPosition) * Time.unscaledDeltaTime;
				_computeShaderTest.SetFloat("TierPosition", _scrollRect.content.TransformPoint(new Vector2(0f, _tierPosition)).y / 2f);
				_computeShaderTest.SetFloat("TierFalloff", _tierFalloff / num);
				float y = _scrollRect.content.TransformPoint(new Vector2(0f, 0f - _height)).y;
				float y2 = _scrollRect.content.TransformPoint(new Vector2(0f, _height)).y;
				_computeShaderTest.SetFloat("VerticalExtentMin", y / 2f);
				_computeShaderTest.SetFloat("VerticalExtentMax", y2 / 2f);
				_computeShaderTest.SetFloat("VerticalFalloff", _verticalFalloff / num);
				_computeShaderTest.Dispatch(_updateKernel, _renderTexture.width, _renderTexture.height, 1);
				_offset = _scrollRect.content.position;
				_offset.x /= _scrollRect.viewport.rect.width;
				_offset.y /= _scrollRect.viewport.rect.height;
				_effectsMaterial.SetVector(_offsetProperty, -_offset);
			}
		}

		private void OnDestroy()
		{
			if (_effectsMaterial != null)
			{
				Object.Destroy(_effectsMaterial);
			}
			if (_renderTexture != null)
			{
				_renderTexture.Release();
				Object.Destroy(_renderTexture);
				_renderTexture = null;
			}
		}
	}
}
