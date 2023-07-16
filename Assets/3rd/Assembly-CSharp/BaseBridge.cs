using UnityEngine;

public class BaseBridge : BaseMonoBehaviour
{
	public GameObject BridgeFixed;

	public GameObject BridgeBroken;

	private void Start()
	{
		if (DataManager.Instance.BridgeFixed)
		{
			BridgeFixed.SetActive(true);
			BridgeBroken.SetActive(false);
		}
		else
		{
			BridgeFixed.SetActive(false);
			BridgeBroken.SetActive(true);
		}
	}

	public void FixBridge()
	{
		DataManager.Instance.BridgeFixed = true;
		BridgeFixed.SetActive(true);
		BridgeBroken.SetActive(false);
	}
}
