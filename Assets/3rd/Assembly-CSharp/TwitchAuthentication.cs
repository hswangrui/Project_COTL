using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Steamworks;
using UnityEngine;

public static class TwitchAuthentication
{
	//[Serializable]
	//public class LogInResultData
	//{
	//	public bool success;

	//	public data data;
	//}

	//[Serializable]
	//public class data
	//{
	//	public string secret;
	//}

	//public delegate void LogInResponse(TwitchRequest.ResponseType response, LogInResultData resultData);

	//public delegate void AuthenticationResponse();


	//public static string redirect
	//{
	//	get
	//	{
	//		if (true)
	//		{
	//			return "https://cotl.connect.streamingtoolsmith.com/redirect&state=";
	//		}
	//		return "https://cotl.connect.dev.streamingtoolsmith.com/redirect&state=";
	//	}
	//}

	//public static string request
	//{
	//	get
	//	{
	//		if (true)
	//		{
	//			return "https://cotl.connect.streamingtoolsmith.com/subscribe?state=";
	//		}
	//		return "https://cotl.connect.dev.streamingtoolsmith.com/subscribe?state=";
	//	}
	//}

	//public static bool IsAuthenticated { get; private set; }

	//public static bool IsLiveStreaming { get; private set; }

	//public static event AuthenticationResponse OnAuthenticated;

	//public static event AuthenticationResponse OnLoggedOut;

	//public static void TryAuthenticate(TwitchRequest.ConnectionResponse response)
	//{
	//	if (!string.IsNullOrEmpty(TwitchManager.SecretKey))
	//	{
	//		TwitchRequest.ConnectionTest(delegate(TwitchRequest.ResponseType res, TwitchRequest.ConnectionTestData result)
	//		{
	//			IsAuthenticated = res == TwitchRequest.ResponseType.Success;
	//			TwitchRequest.ConnectionResponse connectionResponse = response;
	//			if (connectionResponse != null)
	//			{
	//				connectionResponse(res, result);
	//			}
	//			if (result != null)
	//			{
	//				IsLiveStreaming = result.is_live;
	//			}
	//			if (res == TwitchRequest.ResponseType.Success)
	//			{
	//				AuthenticationResponse onAuthenticated = TwitchAuthentication.OnAuthenticated;
	//				if (onAuthenticated != null)
	//				{
	//					onAuthenticated();
	//				}
	//			}
	//		});
	//	}
	//	else
	//	{
	//		IsAuthenticated = false;
	//	}
	//}

 //   public static void RequestLogIn(Action<TwitchRequest.ResponseType> callback)
 //   {
 //       string text = "";
 //       text = SteamUser.GetSteamID().ToString();
 //       Application.OpenURL("https://id.twitch.tv/oauth2/authorize?response_type=code&client_id=wph0p912gucvcee0114kfoukn319db&redirect_uri=" + redirect + text);
 //       TwitchRequest.Request(request + text, delegate (TwitchRequest.ResponseType response, string result)
 //       {
 //           Debug.Log("Twitch authentication request finished... result: " + response);
 //           if (response == TwitchRequest.ResponseType.Success)
 //           {
 //               LogInResultData logInResultData = JsonUtility.FromJson<LogInResultData>(result);
 //               if (logInResultData != null && logInResultData.data != null)
 //               {
 //                   TwitchManager.SecretKey = logInResultData.data.secret;
 //                   GameManager.GetInstance().StartCoroutine(Delay());
 //               }
 //           }
 //           else
 //           {
 //               callback?.Invoke(TwitchRequest.ResponseType.Failure);
 //           }
 //       }, TwitchRequest.RequestType.GET, "");
 //       IEnumerator Delay()
 //       {
 //           yield return new WaitForEndOfFrame();
 //           TryAuthenticate(delegate
 //           {
 //               callback?.Invoke(TwitchRequest.ResponseType.Success);
 //           });
 //       }
 //   }

 //   public static void SignOut()
	//{
	//	TwitchManager.Abort();
	//	IsAuthenticated = false;
	//	TwitchManager.SecretKey = "";
	//	AuthenticationResponse onLoggedOut = TwitchAuthentication.OnLoggedOut;
	//	if (onLoggedOut != null)
	//	{
	//		onLoggedOut();
	//	}
	//}
}
