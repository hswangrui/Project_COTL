using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace src.Extensions
{
	public static class AddressableExtensions
	{
		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CLoadAssetFromPath_003Ed__0<T> : IAsyncStateMachine where T : UnityEngine.Object
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<T> _003C_003Et__builder;

			public string assetPath;

			public bool immediate;

			private TaskAwaiter<T> _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				T result;
				try
				{
					TaskAwaiter<T> awaiter;
					if (num == 0)
					{
						awaiter = _003C_003Eu__1;
						_003C_003Eu__1 = default(TaskAwaiter<T>);
						num = (_003C_003E1__state = -1);
						goto IL_0078;
					}
					AsyncOperationHandle<UnityEngine.Object> asyncOperationHandle = Addressables.LoadAssetAsync<UnityEngine.Object>(assetPath);
					if (!immediate)
					{
						awaiter = PerformAsyncLoadTask<T>(asyncOperationHandle).GetAwaiter();
						if (!awaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = awaiter;
							_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
							return;
						}
						goto IL_0078;
					}
					result = DoImmediate<T>(asyncOperationHandle);
					goto end_IL_0007;
					IL_0078:
					result = awaiter.GetResult();
					end_IL_0007:;
				}
				catch (Exception exception)
				{
					_003C_003E1__state = -2;
					_003C_003Et__builder.SetException(exception);
					return;
				}
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetResult(result);
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
				_003C_003Et__builder.SetStateMachine(stateMachine);
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CLoadAddressableAsync_003Ed__1<T> : IAsyncStateMachine where T : UnityEngine.Object
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<T> _003C_003Et__builder;

			public AssetReference assetReferenceGameObject;

			public bool immediate;

			private TaskAwaiter<T> _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				T result;
				try
				{
					TaskAwaiter<T> awaiter;
					if (num == 0)
					{
						awaiter = _003C_003Eu__1;
						_003C_003Eu__1 = default(TaskAwaiter<T>);
						num = (_003C_003E1__state = -1);
						goto IL_0078;
					}
					AsyncOperationHandle<UnityEngine.Object> asyncOperationHandle = Addressables.LoadAssetAsync<UnityEngine.Object>(assetReferenceGameObject);
					if (!immediate)
					{
						awaiter = PerformAsyncLoadTask<T>(asyncOperationHandle).GetAwaiter();
						if (!awaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = awaiter;
							_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
							return;
						}
						goto IL_0078;
					}
					result = DoImmediate<T>(asyncOperationHandle);
					goto end_IL_0007;
					IL_0078:
					result = awaiter.GetResult();
					end_IL_0007:;
				}
				catch (Exception exception)
				{
					_003C_003E1__state = -2;
					_003C_003Et__builder.SetException(exception);
					return;
				}
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetResult(result);
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
				_003C_003Et__builder.SetStateMachine(stateMachine);
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[StructLayout(LayoutKind.Auto)]
		[CompilerGenerated]
		private struct _003CPerformAsyncLoadTask_003Ed__2<T> : IAsyncStateMachine where T : UnityEngine.Object
		{
			public int _003C_003E1__state;

			public AsyncTaskMethodBuilder<T> _003C_003Et__builder;

			public AsyncOperationHandle<UnityEngine.Object> asyncOperationHandle;

			private TaskAwaiter<UnityEngine.Object> _003C_003Eu__1;

			private void MoveNext()
			{
				int num = _003C_003E1__state;
				T result;
				try
				{
					TaskAwaiter<UnityEngine.Object> awaiter;
					if (num != 0)
					{
						awaiter = asyncOperationHandle.Task.GetAwaiter();
						if (!awaiter.IsCompleted)
						{
							num = (_003C_003E1__state = 0);
							_003C_003Eu__1 = awaiter;
							_003C_003Et__builder.AwaitUnsafeOnCompleted(ref awaiter, ref this);
							return;
						}
					}
					else
					{
						awaiter = _003C_003Eu__1;
						_003C_003Eu__1 = default(TaskAwaiter<UnityEngine.Object>);
						num = (_003C_003E1__state = -1);
					}
					awaiter.GetResult();
					result = ParseResult<T>(asyncOperationHandle.Result);
				}
				catch (Exception exception)
				{
					_003C_003E1__state = -2;
					_003C_003Et__builder.SetException(exception);
					return;
				}
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetResult(result);
			}

			void IAsyncStateMachine.MoveNext()
			{
				//ILSpy generated this explicit interface implementation from .override directive in MoveNext
				this.MoveNext();
			}

			[DebuggerHidden]
			private void SetStateMachine(IAsyncStateMachine stateMachine)
			{
				_003C_003Et__builder.SetStateMachine(stateMachine);
			}

			void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
			{
				//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
				this.SetStateMachine(stateMachine);
			}
		}

		[AsyncStateMachine(typeof(_003CLoadAssetFromPath_003Ed__0<>))]
		public static Task<T> LoadAssetFromPath<T>(this string assetPath, bool immediate = false) where T : UnityEngine.Object
		{
			_003CLoadAssetFromPath_003Ed__0<T> stateMachine = default(_003CLoadAssetFromPath_003Ed__0<T>);
			stateMachine.assetPath = assetPath;
			stateMachine.immediate = immediate;
			stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder<T>.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncTaskMethodBuilder<T> _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
			return stateMachine._003C_003Et__builder.Task;
		}

		[AsyncStateMachine(typeof(_003CLoadAddressableAsync_003Ed__1<>))]
		public static Task<T> LoadAddressableAsync<T>(this AssetReference assetReferenceGameObject, bool immediate = false) where T : UnityEngine.Object
		{
			_003CLoadAddressableAsync_003Ed__1<T> stateMachine = default(_003CLoadAddressableAsync_003Ed__1<T>);
			stateMachine.assetReferenceGameObject = assetReferenceGameObject;
			stateMachine.immediate = immediate;
			stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder<T>.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncTaskMethodBuilder<T> _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
			return stateMachine._003C_003Et__builder.Task;
		}

		[AsyncStateMachine(typeof(_003CPerformAsyncLoadTask_003Ed__2<>))]
		private static Task<T> PerformAsyncLoadTask<T>(AsyncOperationHandle<UnityEngine.Object> asyncOperationHandle) where T : UnityEngine.Object
		{
			_003CPerformAsyncLoadTask_003Ed__2<T> stateMachine = default(_003CPerformAsyncLoadTask_003Ed__2<T>);
			stateMachine.asyncOperationHandle = asyncOperationHandle;
			stateMachine._003C_003Et__builder = AsyncTaskMethodBuilder<T>.Create();
			stateMachine._003C_003E1__state = -1;
			AsyncTaskMethodBuilder<T> _003C_003Et__builder = stateMachine._003C_003Et__builder;
			_003C_003Et__builder.Start(ref stateMachine);
			return stateMachine._003C_003Et__builder.Task;
		}

		private static T DoImmediate<T>(AsyncOperationHandle<UnityEngine.Object> asyncOperationHandle) where T : UnityEngine.Object
		{
			return ParseResult<T>(asyncOperationHandle.WaitForCompletion());
		}

		private static T ParseResult<T>(UnityEngine.Object result) where T : UnityEngine.Object
		{
			GameObject gameObject;
			if ((object)(gameObject = result as GameObject) != null)
			{
				T component = gameObject.GetComponent<T>();
				if ((UnityEngine.Object)component != (UnityEngine.Object)null)
				{
					return component;
				}
			}
			return result as T;
		}

		public static IEnumerator YieldUntilCompleted(this System.Threading.Tasks.Task task)
		{
			while (!task.IsCompleted)
			{
				yield return null;
			}
		}
	}
}
