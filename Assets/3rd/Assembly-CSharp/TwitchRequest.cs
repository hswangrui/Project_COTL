using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using I2.Loc;
//using SocketIOClient;
using UnityEngine;
using UnityEngine.Networking;

public static class TwitchRequest
{
	//[Serializable]
	//public class ConnectionTestData
	//{
	//	public bool connected;

	//	public bool is_live;

	//	public long channel_id;

	//	public string channel_display_name;

	//	public ActiveData features;
	//}

	//[Serializable]
	//public class ActiveData
	//{
	//	public bool helpHinder;

	//	public bool followerRaffle;

	//	public bool totem;

	//	public bool voting;
	//}

	//[Serializable]
	//public class SendingData
	//{
	//	public string language;

	//	public string active_save_id;
	//}

	//public enum ResponseType
	//{
	//	None,
	//	Success,
	//	Failure
	//}

	//public enum RequestType
	//{
	//	GET,
	//	POST,
	//	PUT,
	//	PATCH
	//}

	//public delegate void RequestResponse(ResponseType response, string result);

	//public delegate void ConnectionResponse(ResponseType response, ConnectionTestData result);

	//public delegate void SocketResponse(string key, string data);

	//[StructLayout(LayoutKind.Auto)]
	//[CompilerGenerated]
	//private struct _003CConnectToSocket_003Ed__18 : IAsyncStateMachine
	//{
	//	public int _003C_003E1__state;

	//	public AsyncVoidMethodBuilder _003C_003Et__builder;

	//	private TaskAwaiter _003C_003Eu__1;

	//	private void MoveNext()
	//	{
	//		int num = _003C_003E1__state;
	//		try
	//		{
	//			TaskAwaiter awaiter;
	//			switch (num)
	//			{
	//			default:
	//				if (socket == null || !socket.Connected)
	//				{
	//					if (socket == null)
	//					{
	//						socket = new SocketIOUnity(uri);
	//						socket.OnConnected += delegate
	//						{
	//							socket.Emit("channel.join", TwitchManager.ChannelID);
	//						};
	//						OnAnyHandler handler = delegate(string key, SocketIOResponse response)
	//						{
	//							string text = response.ToString();
	//							text = text.Remove(0, 1);
	//							text = text.Remove(text.Length - 1, 1);
	//							UnityEngine.Debug.Log("EVENT: " + key);
	//							SocketResponse onSocketReceived = TwitchRequest.OnSocketReceived;
	//							if (onSocketReceived != null)
	//							{
	//								onSocketReceived(key, text);
	//							}
	//						};
	//						socket.OnAnyInUnityThread(handler);
	//					}
	//					awaiter = socket.ConnectAsync().GetAwaiter();
	//					if (!awaiter.IsCompleted)
	//					{
	//						num = (_003C_003E1__state = 0);
	//						_003C_003Eu__1 = awaiter;
	//						_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
	//						return;
	//					}
	//					goto IL_00f4;
	//				}
	//				goto end_IL_0007;
	//			case 0:
	//				awaiter = _003C_003Eu__1;
	//				_003C_003Eu__1 = default(TaskAwaiter);
	//				num = (_003C_003E1__state = -1);
	//				goto IL_00f4;
	//			case 1:
	//				awaiter = _003C_003Eu__1;
	//				_003C_003Eu__1 = default(TaskAwaiter);
	//				num = (_003C_003E1__state = -1);
	//				goto IL_016c;
	//			case 2:
	//				{
	//					awaiter = _003C_003Eu__1;
	//					_003C_003Eu__1 = default(TaskAwaiter);
	//					num = (_003C_003E1__state = -1);
	//					break;
	//				}
	//				IL_016c:
	//				awaiter.GetResult();
	//				awaiter = System.Threading.Tasks.Task.Delay(5000).GetAwaiter();
	//				if (!awaiter.IsCompleted)
	//				{
	//					num = (_003C_003E1__state = 2);
	//					_003C_003Eu__1 = awaiter;
	//					_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
	//					return;
	//				}
	//				break;
	//				IL_00f4:
	//				awaiter.GetResult();
	//				if (socket != null && !socket.Connected)
	//				{
	//					awaiter = socket.DisconnectAsync().GetAwaiter();
	//					if (!awaiter.IsCompleted)
	//					{
	//						num = (_003C_003E1__state = 1);
	//						_003C_003Eu__1 = awaiter;
	//						_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
	//						return;
	//					}
	//					goto IL_016c;
	//				}
	//				goto end_IL_0007;
	//			}
	//			awaiter.GetResult();
	//			ConnectToSocket();
	//			end_IL_0007:;
	//		}
	//		catch (Exception exception)
	//		{
	//			_003C_003E1__state = -2;
	//			_003C_003Et__builder.SetException(exception);
	//			return;
	//		}
	//		_003C_003E1__state = -2;
	//		_003C_003Et__builder.SetResult();
	//	}

	//	void IAsyncStateMachine.MoveNext()
	//	{
	//		//ILSpy generated this explicit interface implementation from .override directive in MoveNext
	//		this.MoveNext();
	//	}

	//	[DebuggerHidden]
	//	private void SetStateMachine(IAsyncStateMachine stateMachine)
	//	{
	//		_003C_003Et__builder.SetStateMachine(stateMachine);
	//	}

	//	void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
	//	{
	//		//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
	//		this.SetStateMachine(stateMachine);
	//	}
	//}

	//[StructLayout(LayoutKind.Auto)]
	//[CompilerGenerated]
	//private struct _003CAbort_003Ed__21 : IAsyncStateMachine
	//{
	//	public int _003C_003E1__state;

	//	public AsyncVoidMethodBuilder _003C_003Et__builder;

	//	private TaskAwaiter _003C_003Eu__1;

	//	private void MoveNext()
	//	{
	//		int num = _003C_003E1__state;
	//		try
	//		{
	//			TaskAwaiter awaiter;
	//			if (num == 0)
	//			{
	//				awaiter = _003C_003Eu__1;
	//				_003C_003Eu__1 = default(TaskAwaiter);
	//				num = (_003C_003E1__state = -1);
	//				goto IL_0066;
	//			}
	//			if (socket != null)
	//			{
	//				awaiter = socket.DisconnectAsync().GetAwaiter();
	//				if (!awaiter.IsCompleted)
	//				{
	//					num = (_003C_003E1__state = 0);
	//					_003C_003Eu__1 = awaiter;
	//					_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
	//					return;
	//				}
	//				goto IL_0066;
	//			}
	//			goto end_IL_0007;
	//			IL_0066:
	//			awaiter.GetResult();
	//			socket.Dispose();
	//			socket = null;
	//			end_IL_0007:;
	//		}
	//		catch (Exception exception)
	//		{
	//			_003C_003E1__state = -2;
	//			_003C_003Et__builder.SetException(exception);
	//			return;
	//		}
	//		_003C_003E1__state = -2;
	//		_003C_003Et__builder.SetResult();
	//	}

	//	void IAsyncStateMachine.MoveNext()
	//	{
	//		//ILSpy generated this explicit interface implementation from .override directive in MoveNext
	//		this.MoveNext();
	//	}

	//	[DebuggerHidden]
	//	private void SetStateMachine(IAsyncStateMachine stateMachine)
	//	{
	//		_003C_003Et__builder.SetStateMachine(stateMachine);
	//	}

	//	void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
	//	{
	//		//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
	//		this.SetStateMachine(stateMachine);
	//	}
	//}

	//private static SocketIOUnity socket;

	//public static string uri
	//{
	//	get
	//	{
	//		if (true)
	//		{
	//			return "https://ebs.cotl.devolver.digital/";
	//		}
	//		return "https://cotl-ebs.dev.streamingtoolsmith.com/";
	//	}
	//}

	//public static bool SocketConnected
	//{
	//	get
	//	{
	//		if (socket != null)
	//		{
	//			return socket.Connected;
	//		}
	//		return false;
	//	}
	//}

	//public static event SocketResponse OnSocketReceived;

	//public static void SendEBSData()
	//{
	//	string data = JsonUtility.ToJson(new SendingData
	//	{
	//		language = LocalizationManager.CurrentLanguageCode,
	//		active_save_id = DataManager.Instance.SaveUniqueID
	//	});
	//	Request(uri + "channel", null, RequestType.PATCH, data, new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//public static void ConnectionTest(ConnectionResponse callback)
	//{
	//	RequestResponse callback2 = delegate(ResponseType response, string result)
	//	{
	//		if (response == ResponseType.Success)
	//		{
	//			try
	//			{
	//				ConnectionTestData connectionTestData = JsonUtility.FromJson<ConnectionTestData>(result);
	//				TwitchManager.ChannelID = connectionTestData.channel_id.ToString();
	//				TwitchManager.ChannelName = connectionTestData.channel_display_name;
	//				if (connectionTestData.is_live)
	//				{
	//					ConnectToSocket();
	//				}
	//				if (connectionTestData.features != null)
	//				{
	//					TwitchTotem.Disabled = !connectionTestData.features.totem;
	//					TwitchFollowers.Deactivated = !connectionTestData.features.followerRaffle;
	//					TwitchHelpHinder.Deactivated = !connectionTestData.features.helpHinder;
	//				}
	//				TwitchVoting.Deactivated = !connectionTestData.is_live;
	//				TwitchMessages.Deactivated = !connectionTestData.is_live;
	//				TwitchTotem.Initialise();
	//				TwitchHelpHinder.Initialise();
	//				TwitchMessages.Initialise();
	//				ConnectionResponse connectionResponse = callback;
	//				if (connectionResponse != null)
	//				{
	//					connectionResponse(response, connectionTestData);
	//				}
	//				SendEBSData();
	//			}
	//			catch (Exception)
	//			{
	//			}
	//		}
	//	};
	//	Request(uri + "auth/connection", callback2, RequestType.GET, "", new KeyValuePair<string, string>("x-cotl-channel-secret", TwitchManager.SecretKey));
	//}

	//[AsyncStateMachine(typeof(_003CConnectToSocket_003Ed__18))]
	//private static void ConnectToSocket()
	//{
	//	_003CConnectToSocket_003Ed__18 stateMachine = default(_003CConnectToSocket_003Ed__18);
	//	stateMachine._003C_003Et__builder = AsyncVoidMethodBuilder.Create();
	//	stateMachine._003C_003E1__state = -1;
	//	AsyncVoidMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
	//	_003C_003Et__builder.Start(ref stateMachine);
	//}

	//public static void Request(string uri, RequestResponse callback, RequestType requestType = RequestType.GET, string data = "", params KeyValuePair<string, string>[] headers)
	//{
	//	MonoSingleton<TwitchManager>.Instance.StartCoroutine(RequestIE(uri, callback, requestType, data, headers));
	//}

	//public static IEnumerator RequestIE(string uri, RequestResponse callback, RequestType requestType = RequestType.GET, string data = "", params KeyValuePair<string, string>[] headers)
	//{
	//	UnityEngine.Debug.Log(("TWITCH REQUEST MADE: " + uri).Colour(Color.magenta));
	//	using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
	//	{
	//		webRequest.method = requestType.ToString();
	//		for (int i = 0; i < headers.Length; i++)
	//		{
	//			KeyValuePair<string, string> keyValuePair = headers[i];
	//			webRequest.SetRequestHeader(keyValuePair.Key, keyValuePair.Value);
	//		}
	//		if (!string.IsNullOrEmpty(data))
	//		{
	//			UploadHandlerRaw uploadHandlerRaw = new UploadHandlerRaw(Encoding.UTF8.GetBytes(data));
	//			uploadHandlerRaw.contentType = "application/json";
	//			webRequest.uploadHandler = uploadHandlerRaw;
	//		}
	//		UnityWebRequestAsyncOperation requestOperation = webRequest.SendWebRequest();
	//		while (!requestOperation.isDone)
	//		{
	//			yield return null;
	//		}
	//		if (webRequest.isNetworkError)
	//		{
	//			if (callback != null)
	//			{
	//				callback(ResponseType.Failure, webRequest.downloadHandler.text);
	//			}
	//		}
	//		else if (callback != null)
	//		{
	//			callback(ResponseType.Success, webRequest.downloadHandler.text);
	//		}
	//	}
	//}

	//[AsyncStateMachine(typeof(_003CAbort_003Ed__21))]
	//public static void Abort()
	//{
	//	_003CAbort_003Ed__21 stateMachine = default(_003CAbort_003Ed__21);
	//	stateMachine._003C_003Et__builder = AsyncVoidMethodBuilder.Create();
	//	stateMachine._003C_003E1__state = -1;
	//	AsyncVoidMethodBuilder _003C_003Et__builder = stateMachine._003C_003Et__builder;
	//	_003C_003Et__builder.Start(ref stateMachine);
	//}
}
