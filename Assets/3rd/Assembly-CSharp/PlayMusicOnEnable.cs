using FMODUnity;
using WebSocketSharp;

public class PlayMusicOnEnable : BaseMonoBehaviour
{
	[EventRef]
	public string musicPath;

	public string parameter;

	public int index;

	private void OnEnable()
	{
		if (!musicPath.IsNullOrEmpty())
		{
			AudioManager.Instance.PlayMusic(musicPath);
		}
		if (!parameter.IsNullOrEmpty())
		{
			AudioManager.Instance.SetMusicRoomID(index, parameter);
		}
	}

	public void SetID()
	{
		if (!parameter.IsNullOrEmpty())
		{
			AudioManager.Instance.SetMusicRoomID(index, parameter);
		}
	}
}
