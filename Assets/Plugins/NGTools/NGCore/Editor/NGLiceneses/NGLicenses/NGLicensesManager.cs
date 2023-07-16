using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Timers;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static UnityEditor.EditorApplication;

namespace NGLicenses
{
	internal static class NGLicensesManager
	{
		[CompilerGenerated]
		private sealed class Class7
		{
			public string string_0;

			internal bool method_0(License license_0)
			{
				return license_0.license == string_0;
			}
		}

		[CompilerGenerated]
		private sealed class Class8
		{
			public string string_0;

			internal bool method_0(License license_0)
			{
				return license_0.license == string_0;
			}
		}

		[CompilerGenerated]
		private sealed class Class9
		{
			public string[] string_0;

			public bool bool_0;

			public StringBuilder stringBuilder_0;

			internal void method_0(bool bool_1, string string_1)
			{
				//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d7: Expected O, but got Unknown
				//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e6: Expected O, but got Unknown
				//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0401: Expected O, but got Unknown
				//IL_0401: Unknown result type (might be due to invalid IL or missing references)
				//IL_040b: Expected O, but got Unknown
				//IL_043b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0440: Unknown result type (might be due to invalid IL or missing references)
				//IL_0446: Expected O, but got Unknown
				//IL_044b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0455: Expected O, but got Unknown
				//IL_0463: Unknown result type (might be due to invalid IL or missing references)
				//IL_046d: Expected O, but got Unknown
				//IL_046d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0477: Expected O, but got Unknown
				//IL_0492: Unknown result type (might be due to invalid IL or missing references)
				//IL_0497: Unknown result type (might be due to invalid IL or missing references)
				//IL_049d: Expected O, but got Unknown
				//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
				//IL_04ac: Expected O, but got Unknown
				Class10 CS_0024_003C_003E8__locals0 = new Class10
				{
					class9_0 = this,
					string_0 = string_1
				};
				if (!bool_1)
				{
					if (CS_0024_003C_003E8__locals0.string_0 == "[]")
					{
						lock (list_1)
						{
							int i = 0;
							for (int count = list_1.Count; i < count; i++)
							{
								int j = 0;
								for (int num = string_0.Length; j < num; j++)
								{
									if (list_1[i].license == string_0[j])
									{
										list_1[i].status = Status.Invalid;
									}
								}
							}
							CallbackFunction delayCall = EditorApplication.delayCall;
							object obj = Class13._003C_003E9__39_4;
							if (obj == null)
							{
								CallbackFunction val = delegate
								{
									smethod_1();
									InternalEditorUtility.RepaintAllViews();
								};
								obj = (object)val;
								Class13._003C_003E9__39_4 = val;
							}
							EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)obj);
							return;
						}
					}
					if (CS_0024_003C_003E8__locals0.string_0.StartsWith("["))
					{
						Class11 CS_0024_003C_003E8__locals1 = new Class11
						{
							class10_0 = CS_0024_003C_003E8__locals0
						};
						string[] array = CS_0024_003C_003E8__locals1.class10_0.string_0.Split('{');
						CS_0024_003C_003E8__locals1.list_0 = new List<string>(string_0);
						lock (list_1)
						{
							int k = 1;
							for (int num2 = array.Length; k < num2; k++)
							{
								Class12 CS_0024_003C_003E8__locals2 = new Class12();
								string[] array2 = array[k].Split(',');
								CS_0024_003C_003E8__locals2.invoice = array2[0].Split(':')[1];
								CS_0024_003C_003E8__locals2.invoice = CS_0024_003C_003E8__locals2.invoice.Substring(1, CS_0024_003C_003E8__locals2.invoice.Length - 2);
								if (bool_0)
								{
									bool_0 = false;
									int l = 0;
									for (int count2 = list_1.Count; l < count2; l++)
									{
										if (list_1[l].license == string_0[0])
										{
											string text = array2[1].Split(':')[1];
											string text2 = array2[2].Split(':')[1];
											Status status = (Status)int.Parse(text2.Substring(0, text2.Length - 2));
											if (text.Length > 2)
											{
												text = text.Substring(1, text.Length - 2);
												list_1[l].assetName = text;
											}
											list_1[l].status = status;
											CS_0024_003C_003E8__locals1.list_0.Remove(string_0[0]);
											break;
										}
									}
									continue;
								}
								int m = 0;
								for (int count3 = list_1.Count; m < count3; m++)
								{
									if (list_1[m].license.Contains(CS_0024_003C_003E8__locals2.invoice))
									{
										string text3 = array2[1].Split(':')[1];
										string text4 = array2[2].Split(':')[1];
										Status status2 = (Status)int.Parse(text4.Substring(0, text4.Length - 2));
										if (text3.Length > 2)
										{
											text3 = text3.Substring(1, text3.Length - 2);
											list_1[m].assetName = text3;
										}
										list_1[m].status = status2;
										CS_0024_003C_003E8__locals1.list_0.RemoveAll((string string_0) => string_0.Contains(CS_0024_003C_003E8__locals2.invoice));
										break;
									}
								}
							}
							smethod_1();
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							if (CS_0024_003C_003E8__locals1.list_0.Count > 0)
							{
								CS_0024_003C_003E8__locals1.class10_0.class9_0.stringBuilder_0 = Class36.smethod_0("Invoice(s)");
								int n = 0;
								for (int count4 = CS_0024_003C_003E8__locals1.list_0.Count; n < count4; n++)
								{
									CS_0024_003C_003E8__locals1.class10_0.class9_0.stringBuilder_0.Append(" \"");
									CS_0024_003C_003E8__locals1.class10_0.class9_0.stringBuilder_0.Append(CS_0024_003C_003E8__locals1.list_0[n]);
									CS_0024_003C_003E8__locals1.class10_0.class9_0.stringBuilder_0.Append('"');
								}
								CS_0024_003C_003E8__locals1.class10_0.class9_0.stringBuilder_0.Append(" might be invalid.\nPlease use real invoice and not voucher.");
								EditorUtility.DisplayDialog(Title, Class36.smethod_2(CS_0024_003C_003E8__locals1.class10_0.class9_0.stringBuilder_0), "OK");
							}
							InternalEditorUtility.RepaintAllViews();
						});
					}
					else if (CS_0024_003C_003E8__locals0.string_0 == "-2")
					{
						CallbackFunction delayCall2 = EditorApplication.delayCall;
						object obj2 = Class13._003C_003E9__39_1;
						if (obj2 == null)
						{
							CallbackFunction val2 = delegate
							{
								EditorUtility.DisplayDialog(Title, "Don't spam.", "OK");
							};
							obj2 = (object)val2;
							Class13._003C_003E9__39_1 = val2;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall2, (Delegate)obj2);
					}
					else
					{
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							EditorUtility.DisplayDialog(Title, "Request completed, but unexpected data." + CS_0024_003C_003E8__locals0.string_0, "OK");
						});
					}
					return;
				}
				CallbackFunction delayCall3 = EditorApplication.delayCall;
				object obj3 = Class13._003C_003E9__39_3;
				if (obj3 == null)
				{
					CallbackFunction val3 = delegate
					{
						EditorUtility.DisplayDialog(Title, "Request has failed. Please retry or contact the author.", "OK");
					};
					obj3 = (object)val3;
					Class13._003C_003E9__39_3 = val3;
				}
				EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall3, (Delegate)obj3);
			}
		}

		[CompilerGenerated]
		private sealed class Class10
		{
			public string string_0;

			public Class9 class9_0;

			internal void method_0()
			{
				EditorUtility.DisplayDialog(Title, "Request completed, but unexpected data." + string_0, "OK");
			}
		}

		[CompilerGenerated]
		private sealed class Class11
		{
			public List<string> list_0;

			public Class10 class10_0;

			internal void method_0()
			{
				if (list_0.Count > 0)
				{
					class10_0.class9_0.stringBuilder_0 = Class36.smethod_0("Invoice(s)");
					int i = 0;
					for (int count = list_0.Count; i < count; i++)
					{
						class10_0.class9_0.stringBuilder_0.Append(" \"");
						class10_0.class9_0.stringBuilder_0.Append(list_0[i]);
						class10_0.class9_0.stringBuilder_0.Append('"');
					}
					class10_0.class9_0.stringBuilder_0.Append(" might be invalid.\nPlease use real invoice and not voucher.");
					EditorUtility.DisplayDialog(Title, Class36.smethod_2(class10_0.class9_0.stringBuilder_0), "OK");
				}
				InternalEditorUtility.RepaintAllViews();
			}
		}

		[CompilerGenerated]
		private sealed class Class12
		{
			public string invoice;

			public Predicate<string> predicate_0;

			internal bool method_0(string string_0)
			{
				return string_0.Contains(invoice);
			}
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class Class13
		{
			public static readonly Class13 _003C_003E9 = new Class13();

			public static CallbackFunction _003C_003E9__39_4;

			public static CallbackFunction _003C_003E9__39_1;

			public static CallbackFunction _003C_003E9__39_3;

			public static Action<bool, string> _003C_003E9__40_0;

			public static CallbackFunction _003C_003E9__42_2;

			public static CallbackFunction _003C_003E9__44_1;

			public static CallbackFunction _003C_003E9__44_2;

			public static AsyncCallback _003C_003E9__46_1;

			internal void method_0()
			{
				smethod_1();
				InternalEditorUtility.RepaintAllViews();
			}

			internal void method_1()
			{
				EditorUtility.DisplayDialog(Title, "Don't spam.", "OK");
			}

			internal void method_2()
			{
				EditorUtility.DisplayDialog(Title, "Request has failed. Please retry or contact the author.", "OK");
			}

			internal void method_3(bool bool_0, string string_0)
			{
				//IL_030e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0318: Expected O, but got Unknown
				//IL_0318: Unknown result type (might be due to invalid IL or missing references)
				//IL_0322: Expected O, but got Unknown
				if (bool_0)
				{
					return;
				}
				Class14 CS_0024_003C_003E8__locals0 = new Class14();
				string[] array = string_0.Split('\n');
				CS_0024_003C_003E8__locals0.list_0 = new List<string>();
				lock (list_1)
				{
					int i = 0;
					for (int num = array.Length; i + 2 < num; i += 3)
					{
						int j = 0;
						for (int count = list_1.Count; j < count; j++)
						{
							if (!(list_1[j].license == array[i]))
							{
								continue;
							}
							if (array[i + 1] == "-1")
							{
								if (list_1[j].status != Status.Banned || list_1[j].active)
								{
									CS_0024_003C_003E8__locals0.list_0.Add("License \"" + list_1[j].license + "\" has been revoked and blocked." + ((!string.IsNullOrEmpty(array[i + 2])) ? (" (" + array[i + 2] + ")") : string.Empty));
									list_1[j].status = Status.Banned;
									list_1[j].active = false;
								}
							}
							else if (array[i + 1] == "0")
							{
								if (list_1[j].status != 0 || list_1[j].active)
								{
									CS_0024_003C_003E8__locals0.list_0.Add("License \"" + list_1[j].license + "\" has been revoked." + ((!string.IsNullOrEmpty(array[i + 2])) ? (" (" + array[i + 2] + ")") : string.Empty));
									list_1[j].status = Status.Valid;
									list_1[j].active = false;
								}
							}
							else if (array[i + 1] == "1" && (list_1[j].status != 0 || !list_1[j].active))
							{
								CS_0024_003C_003E8__locals0.list_0.Add("License \"" + list_1[j].license + "\" has been activated." + ((!string.IsNullOrEmpty(array[i + 2])) ? (" (" + array[i + 2] + ")") : string.Empty));
								list_1[j].status = Status.Valid;
								list_1[j].active = true;
							}
						}
					}
					if (CS_0024_003C_003E8__locals0.list_0.Count > 0)
					{
						smethod_1();
					}
				}
				if (CS_0024_003C_003E8__locals0.list_0.Count <= 0)
				{
					return;
				}
				EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
				{
					int k = 0;
					for (int count2 = CS_0024_003C_003E8__locals0.list_0.Count; k < count2; k++)
					{
                        UnityEngine.Debug.LogWarning((object)("[" + Title + "] " + CS_0024_003C_003E8__locals0.list_0[k]));
					}
					InternalEditorUtility.RepaintAllViews();
				});
			}

			internal void method_4()
			{
				EditorUtility.DisplayDialog(Title, "Request has failed.", "OK");
			}

			internal void method_5()
			{
				EditorUtility.DisplayDialog(Title, "Server has encountered an issue. Please contact the author.", "OK");
			}

			internal void method_6()
			{
				EditorUtility.DisplayDialog(Title, "Request for active seats has failed. Please retry or contact the author.", "OK");
			}

			internal void method_7(IAsyncResult iasyncResult_0)
			{
				((Action)iasyncResult_0.AsyncState).EndInvoke(iasyncResult_0);
			}
		}

		[CompilerGenerated]
		private sealed class Class14
		{
			public List<string> list_0;

			internal void method_0()
			{
				int i = 0;
				for (int count = list_0.Count; i < count; i++)
				{
                    UnityEngine.Debug.LogWarning((object)("[" + Title + "] " + list_0[i]));
				}
				InternalEditorUtility.RepaintAllViews();
			}
		}

		[CompilerGenerated]
		private sealed class Class15
		{
			public string string_0;

			public License license_0;

			public CallbackFunction callbackFunction_0;

			internal bool method_0(License license_1)
			{
				return license_1.license == string_0;
			}

			internal void method_1(bool bool_0, string string_1)
			{
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Expected O, but got Unknown
				//IL_0052: Expected O, but got Unknown
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Expected O, but got Unknown
				//IL_014e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Expected O, but got Unknown
				//IL_0158: Unknown result type (might be due to invalid IL or missing references)
				//IL_0162: Expected O, but got Unknown
				//IL_0170: Unknown result type (might be due to invalid IL or missing references)
				//IL_017a: Expected O, but got Unknown
				//IL_017a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0184: Expected O, but got Unknown
				Class16 CS_0024_003C_003E8__locals0 = new Class16
				{
					class15_0 = this,
					string_0 = string_1
				};
				if (!bool_0)
				{
					if (CS_0024_003C_003E8__locals0.string_0 == "1")
					{
						CallbackFunction delayCall = EditorApplication.delayCall;
						CallbackFunction obj = callbackFunction_0;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								lock (list_1)
								{
									license_0.active = true;
									if (NGLicensesManager.ActivationSucceeded != null)
									{
										NGLicensesManager.ActivationSucceeded(license_0.license);
									}
									smethod_1();
								}
								InternalEditorUtility.RepaintAllViews();
								EditorUtility.DisplayDialog(Title, "License activated.", "OK");
							};
							CallbackFunction val2 = val;
							callbackFunction_0 = val;
							obj = val2;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)(object)obj);
						return;
					}
					Class17 CS_0024_003C_003E8__locals1 = new Class17
					{
						class16_0 = CS_0024_003C_003E8__locals0
					};
					if (CS_0024_003C_003E8__locals1.class16_0.string_0 == "-2")
					{
						CS_0024_003C_003E8__locals1.string_0 = "You are not allowed to use this invoice. Please contact the author.";
						license_0.status = Status.Banned;
						license_0.active = false;
					}
					else if (CS_0024_003C_003E8__locals1.class16_0.string_0 == "-3")
					{
						CS_0024_003C_003E8__locals1.string_0 = "Invoice is invalid.";
						license_0.status = Status.Invalid;
						license_0.active = false;
					}
					else if (CS_0024_003C_003E8__locals1.class16_0.string_0 == "-4")
					{
						CS_0024_003C_003E8__locals1.string_0 = "No more activation is allowed, limitation per seat is already reached. Please first revoke your license on other computers before activating here.";
						license_0.status = Status.Valid;
						license_0.active = false;
					}
					else
					{
						CS_0024_003C_003E8__locals1.string_0 = "Server has encountered an issue. Please contact the author.";
					}
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
					{
						if (NGLicensesManager.ActivationFailed != null)
						{
							NGLicensesManager.ActivationFailed(CS_0024_003C_003E8__locals1.class16_0.class15_0.string_0, CS_0024_003C_003E8__locals1.string_0, CS_0024_003C_003E8__locals1.class16_0.string_0);
						}
					});
					return;
				}
				EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
				{
					if (NGLicensesManager.ActivationFailed != null)
					{
						NGLicensesManager.ActivationFailed(CS_0024_003C_003E8__locals0.class15_0.string_0, "Request has failed.", CS_0024_003C_003E8__locals0.string_0);
					}
				});
			}

			internal void method_2()
			{
				lock (list_1)
				{
					license_0.active = true;
					if (NGLicensesManager.ActivationSucceeded != null)
					{
						NGLicensesManager.ActivationSucceeded(license_0.license);
					}
					smethod_1();
				}
				InternalEditorUtility.RepaintAllViews();
				EditorUtility.DisplayDialog(Title, "License activated.", "OK");
			}
		}

		[CompilerGenerated]
		private sealed class Class16
		{
			public string string_0;

			public Class15 class15_0;

			internal void method_0()
			{
				if (NGLicensesManager.ActivationFailed != null)
				{
					NGLicensesManager.ActivationFailed(class15_0.string_0, "Request has failed.", string_0);
				}
			}
		}

		[CompilerGenerated]
		private sealed class Class17
		{
			public string string_0;

			public Class16 class16_0;

			internal void method_0()
			{
				if (NGLicensesManager.ActivationFailed != null)
				{
					NGLicensesManager.ActivationFailed(class16_0.class15_0.string_0, string_0, class16_0.string_0);
				}
			}
		}

		[CompilerGenerated]
		private sealed class Class18
		{
			public string string_0;

			public Action<string, string, string> action_0;

			public string string_1;

			public string string_2;

			public Predicate<License> predicate_0;

			public CallbackFunction callbackFunction_0;

			internal void method_0(bool bool_0, string string_3)
			{
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Expected O, but got Unknown
				//IL_0039: Expected O, but got Unknown
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Expected O, but got Unknown
				//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bf: Expected O, but got Unknown
				//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c9: Expected O, but got Unknown
				//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ef: Expected O, but got Unknown
				//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fe: Expected O, but got Unknown
				if (!bool_0)
				{
					if (string_3 == "1")
					{
						CallbackFunction delayCall = EditorApplication.delayCall;
						CallbackFunction obj = callbackFunction_0;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								lock (list_1)
								{
									License license = list_1.Find((License license_0) => license_0.license == string_0);
									if (license != null)
									{
										license.active = false;
										license.status = Status.Valid;
										smethod_1();
									}
								}
								action_0(string_0, string_1, string_2);
								InternalEditorUtility.RepaintAllViews();
								EditorUtility.DisplayDialog(Title, "Seat \"" + string_1 + " " + string_2 + "\" revoked.", "OK");
							};
							CallbackFunction val2 = val;
							callbackFunction_0 = val;
							obj = val2;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)(object)obj);
					}
					else
					{
						Class19 CS_0024_003C_003E8__locals0 = new Class19();
						if (string_3 == "0")
						{
							CS_0024_003C_003E8__locals0.string_0 = "The server could not revoke the seat \"" + string_1 + " " + string_2 + "\". Please contact the author.";
						}
						else
						{
							CS_0024_003C_003E8__locals0.string_0 = "Server has encountered an issue. Please contact the author.";
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							EditorUtility.DisplayDialog(Title, CS_0024_003C_003E8__locals0.string_0, "OK");
						});
					}
					return;
				}
				CallbackFunction delayCall2 = EditorApplication.delayCall;
				object obj2 = Class13._003C_003E9__42_2;
				if (obj2 == null)
				{
					CallbackFunction val3 = delegate
					{
						EditorUtility.DisplayDialog(Title, "Request has failed.", "OK");
					};
					obj2 = (object)val3;
					Class13._003C_003E9__42_2 = val3;
				}
				EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall2, (Delegate)obj2);
			}

			internal void method_1()
			{
				lock (list_1)
				{
					License license = list_1.Find((License license_0) => license_0.license == string_0);
					if (license != null)
					{
						license.active = false;
						license.status = Status.Valid;
						smethod_1();
					}
				}
				action_0(string_0, string_1, string_2);
				InternalEditorUtility.RepaintAllViews();
				EditorUtility.DisplayDialog(Title, "Seat \"" + string_1 + " " + string_2 + "\" revoked.", "OK");
			}

			internal bool method_2(License license_0)
			{
				return license_0.license == string_0;
			}
		}

		[CompilerGenerated]
		private sealed class Class19
		{
			public string string_0;

			internal void method_0()
			{
				EditorUtility.DisplayDialog(Title, string_0, "OK");
			}
		}

		[CompilerGenerated]
		private sealed class Class20
		{
			public string string_0;

			public License license_0;

			public CallbackFunction callbackFunction_0;

			internal bool method_0(License license_1)
			{
				return license_1.license == string_0;
			}

			internal void method_1(bool bool_0, string string_1)
			{
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Expected O, but got Unknown
				//IL_0052: Expected O, but got Unknown
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Expected O, but got Unknown
				//IL_010c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0116: Expected O, but got Unknown
				//IL_0116: Unknown result type (might be due to invalid IL or missing references)
				//IL_0120: Expected O, but got Unknown
				//IL_012e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0138: Expected O, but got Unknown
				//IL_0138: Unknown result type (might be due to invalid IL or missing references)
				//IL_0142: Expected O, but got Unknown
				Class21 CS_0024_003C_003E8__locals0 = new Class21
				{
					class20_0 = this,
					string_0 = string_1
				};
				if (!bool_0)
				{
					if (CS_0024_003C_003E8__locals0.string_0 == "1")
					{
						CallbackFunction delayCall = EditorApplication.delayCall;
						CallbackFunction obj = callbackFunction_0;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								lock (list_1)
								{
									license_0.status = Status.Valid;
									license_0.active = false;
									smethod_1();
								}
								InternalEditorUtility.RepaintAllViews();
								EditorUtility.DisplayDialog(Title, "License revoked.", "OK");
							};
							CallbackFunction val2 = val;
							callbackFunction_0 = val;
							obj = val2;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)(object)obj);
						return;
					}
					Class22 CS_0024_003C_003E8__locals1 = new Class22
					{
						class21_0 = CS_0024_003C_003E8__locals0
					};
					if (CS_0024_003C_003E8__locals1.class21_0.string_0 == "0")
					{
						CS_0024_003C_003E8__locals1.string_0 = "You are not using this invoice.";
						license_0.status = Status.Valid;
						license_0.active = false;
					}
					else if (CS_0024_003C_003E8__locals1.class21_0.string_0 == "-2")
					{
						CS_0024_003C_003E8__locals1.string_0 = "You are not allowed to revoke this invoice. Please contact the author.";
						license_0.status = Status.Banned;
						license_0.active = false;
					}
					else
					{
						CS_0024_003C_003E8__locals1.string_0 = "Server has encountered an issue. Please contact the author.";
					}
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
					{
						if (NGLicensesManager.RevokeFailed != null)
						{
							NGLicensesManager.RevokeFailed(CS_0024_003C_003E8__locals1.class21_0.class20_0.string_0, CS_0024_003C_003E8__locals1.string_0, CS_0024_003C_003E8__locals1.class21_0.string_0);
						}
					});
					return;
				}
				EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
				{
					if (NGLicensesManager.RevokeFailed != null)
					{
						NGLicensesManager.RevokeFailed(CS_0024_003C_003E8__locals0.class20_0.string_0, "Request has failed.", CS_0024_003C_003E8__locals0.string_0);
					}
				});
			}

			internal void method_2()
			{
				lock (list_1)
				{
					license_0.status = Status.Valid;
					license_0.active = false;
					smethod_1();
				}
				InternalEditorUtility.RepaintAllViews();
				EditorUtility.DisplayDialog(Title, "License revoked.", "OK");
			}
		}

		[CompilerGenerated]
		private sealed class Class21
		{
			public string string_0;

			public Class20 class20_0;

			internal void method_0()
			{
				if (NGLicensesManager.RevokeFailed != null)
				{
					NGLicensesManager.RevokeFailed(class20_0.string_0, "Request has failed.", string_0);
				}
			}
		}

		[CompilerGenerated]
		private sealed class Class22
		{
			public string string_0;

			public Class21 class21_0;

			internal void method_0()
			{
				if (NGLicensesManager.RevokeFailed != null)
				{
					NGLicensesManager.RevokeFailed(class21_0.class20_0.string_0, string_0, class21_0.string_0);
				}
			}
		}

		[CompilerGenerated]
		private sealed class Class23
		{
			public Action<string, string[]> action_0;

			public string string_0;

			internal void method_0(bool bool_0, string string_1)
			{
				//IL_00db: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e6: Expected O, but got Unknown
				//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f5: Expected O, but got Unknown
				//IL_0101: Unknown result type (might be due to invalid IL or missing references)
				//IL_010b: Expected O, but got Unknown
				//IL_010b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0115: Expected O, but got Unknown
				//IL_0130: Unknown result type (might be due to invalid IL or missing references)
				//IL_0135: Unknown result type (might be due to invalid IL or missing references)
				//IL_013b: Expected O, but got Unknown
				//IL_0140: Unknown result type (might be due to invalid IL or missing references)
				//IL_014a: Expected O, but got Unknown
				if (!bool_0)
				{
					if (string_1 == "[]")
					{
						action_0(string_0, new string[0]);
					}
					else if (string_1.StartsWith("["))
					{
						string[] array = string_1.Split(',');
						List<string> list = new List<string>();
						int i = 0;
						for (int num = array.Length; i < num; i++)
						{
							int num2 = array[i].IndexOf('"');
							list.Add(array[i].Substring(num2 + 1, array[i].IndexOf('"', num2 + 1) - num2 - 1));
						}
						action_0(string_0, list.ToArray());
					}
					else
					{
						CallbackFunction delayCall = EditorApplication.delayCall;
						object obj = Class13._003C_003E9__44_1;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								EditorUtility.DisplayDialog(Title, "Server has encountered an issue. Please contact the author.", "OK");
							};
							obj = (object)val;
							Class13._003C_003E9__44_1 = val;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)obj);
					}
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)new CallbackFunction(InternalEditorUtility.RepaintAllViews));
					return;
				}
				CallbackFunction delayCall2 = EditorApplication.delayCall;
				object obj2 = Class13._003C_003E9__44_2;
				if (obj2 == null)
				{
					CallbackFunction val2 = delegate
					{
						EditorUtility.DisplayDialog(Title, "Request for active seats has failed. Please retry or contact the author.", "OK");
					};
					obj2 = (object)val2;
					Class13._003C_003E9__44_2 = val2;
				}
				EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall2, (Delegate)obj2);
			}
		}

		[CompilerGenerated]
		private sealed class Class24
		{
			public string string_0;

			public Stopwatch stopwatch_0;

			public Action<bool, string> action_0;

			public string[] string_1;

			public string string_2;

			public CallbackFunction callbackFunction_0;

			internal void method_0()
			{
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Expected O, but got Unknown
				//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_01e6: Expected O, but got Unknown
				//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
				//IL_01f0: Expected O, but got Unknown
				//IL_0209: Unknown result type (might be due to invalid IL or missing references)
				//IL_0213: Expected O, but got Unknown
				//IL_0213: Unknown result type (might be due to invalid IL or missing references)
				//IL_021d: Expected O, but got Unknown
				//IL_0270: Unknown result type (might be due to invalid IL or missing references)
				//IL_0275: Unknown result type (might be due to invalid IL or missing references)
				//IL_0278: Expected O, but got Unknown
				//IL_027d: Expected O, but got Unknown
				//IL_0284: Unknown result type (might be due to invalid IL or missing references)
				//IL_028e: Expected O, but got Unknown
				//IL_038f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0399: Expected O, but got Unknown
				//IL_0399: Unknown result type (might be due to invalid IL or missing references)
				//IL_03a3: Expected O, but got Unknown
				//IL_0456: Unknown result type (might be due to invalid IL or missing references)
				//IL_0460: Expected O, but got Unknown
				//IL_0460: Unknown result type (might be due to invalid IL or missing references)
				//IL_046a: Expected O, but got Unknown
				Class25 class25_ = new Class25
				{
					class24_0 = this,
					timer_0 = new System.Timers.Timer(15000.0)
				};
				try
				{
					Class26 CS_0024_003C_003E8__locals0 = new Class26();
					CS_0024_003C_003E8__locals0.class25_0 = class25_;
					CS_0024_003C_003E8__locals0.class25_0.timer_0.Enabled = true;
					CS_0024_003C_003E8__locals0.httpWebRequest_0 = (HttpWebRequest)WebRequest.Create(string_0);
					CS_0024_003C_003E8__locals0.class25_0.timer_0.Elapsed += delegate
					{
						//IL_0089: Unknown result type (might be due to invalid IL or missing references)
						//IL_0093: Expected O, but got Unknown
						//IL_0093: Unknown result type (might be due to invalid IL or missing references)
						//IL_009d: Expected O, but got Unknown
						lock (CS_0024_003C_003E8__locals0.class25_0.class24_0.stopwatch_0)
						{
							Class27 CS_0024_003C_003E8__locals4 = new Class27();
							CS_0024_003C_003E8__locals0.class25_0.timer_0.Stop();
							if (CS_0024_003C_003E8__locals0.class25_0.class24_0.action_0 != null)
							{
								CS_0024_003C_003E8__locals0.httpWebRequest_0.Abort();
								CS_0024_003C_003E8__locals4.action_0 = CS_0024_003C_003E8__locals0.class25_0.class24_0.action_0;
								CS_0024_003C_003E8__locals0.class25_0.class24_0.action_0 = null;
								EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
								{
									CS_0024_003C_003E8__locals4.action_0(true, "Request expired. Please retry.");
								});
								int j = 0;
								for (int num3 = CS_0024_003C_003E8__locals0.class25_0.class24_0.string_1.Length; j < num3; j++)
								{
									int num4 = list_0.IndexOf(CS_0024_003C_003E8__locals0.class25_0.class24_0.string_1[j]);
									if (num4 != -1)
									{
										list_0.RemoveAt(num4);
									}
								}
							}
						}
					};
					CS_0024_003C_003E8__locals0.class25_0.timer_0.Start();
					bool_0 = true;
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)new CallbackFunction(InternalEditorUtility.RepaintAllViews));
					list_0.AddRange(string_1);
					CS_0024_003C_003E8__locals0.httpWebRequest_0.UserAgent = "Unity/" + string_2 + " " + NGLicensesManager.string_0 + "/NG Licenses/1.7";
					CS_0024_003C_003E8__locals0.httpWebRequest_0.Timeout = 5000;
					CS_0024_003C_003E8__locals0.httpWebRequest_0.ReadWriteTimeout = 15000;
					using (HttpWebResponse httpWebResponse = (HttpWebResponse)CS_0024_003C_003E8__locals0.httpWebRequest_0.GetResponse())
					{
						using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
						{
							lock (stopwatch_0)
							{
								Class28 CS_0024_003C_003E8__locals3 = new Class28();
								if (action_0 == null)
								{
									return;
								}
								CS_0024_003C_003E8__locals3.action_0 = action_0;
								action_0 = null;
								CS_0024_003C_003E8__locals0.class25_0.timer_0.Stop();
								CS_0024_003C_003E8__locals3.bool_0 = false;
								CS_0024_003C_003E8__locals3.string_0 = string.Empty;
								try
								{
									CS_0024_003C_003E8__locals3.string_0 = streamReader.ReadToEnd();
								}
								catch (Exception ex)
								{
									CS_0024_003C_003E8__locals3.bool_0 = true;
									CS_0024_003C_003E8__locals3.string_0 = ex.Message;
									bool_0 = false;
								}
								finally
								{
									EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
									{
										CS_0024_003C_003E8__locals3.action_0(CS_0024_003C_003E8__locals3.bool_0, CS_0024_003C_003E8__locals3.string_0);
									});
									bool bool_ = bool_0;
									bool_0 = true;
									EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)new CallbackFunction(InternalEditorUtility.RepaintAllViews));
									if (stopwatch_0.ElapsedMilliseconds < 500L)
									{
										Thread.Sleep(500 - (int)stopwatch_0.ElapsedMilliseconds);
									}
									if (!bool_)
									{
										bool_0 = false;
									}
									CallbackFunction delayCall = EditorApplication.delayCall;
									CallbackFunction obj = callbackFunction_0;
									if (obj == null)
									{
										CallbackFunction val = delegate
										{
											InternalEditorUtility.RepaintAllViews();
											int i = 0;
											for (int num = string_1.Length; i < num; i++)
											{
												int num2 = list_0.IndexOf(string_1[i]);
												if (num2 != -1)
												{
													list_0.RemoveAt(num2);
												}
											}
										};
										CallbackFunction val2 = val;
										callbackFunction_0 = val;
										obj = val2;
									}
									EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)(object)obj);
								}
							}
						}
					}
				}
				catch (WebException ex2)
				{
					Class29 @class = new Class29();
					WebException webException_ = ex2;
					@class.webException_0 = webException_;
					smethod_5(string_0, @class.webException_0);
					using (WebResponse webResponse = @class.webException_0.Response)
					{
						Class30 class30_ = new Class30
						{
							class29_0 = @class,
							httpWebResponse_0 = (HttpWebResponse)webResponse
						};
						using (Stream stream = webResponse.GetResponseStream())
						{
							using (StreamReader streamReader2 = new StreamReader(stream))
							{
								Class31 class31_ = new Class31
								{
									class30_0 = class30_,
									string_0 = streamReader2.ReadToEnd()
								};
								lock (stopwatch_0)
								{
									Class32 CS_0024_003C_003E8__locals1 = new Class32
									{
										class31_0 = class31_
									};
									if (action_0 != null)
									{
										CS_0024_003C_003E8__locals1.action_0 = action_0;
										action_0 = null;
										EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
										{
											CS_0024_003C_003E8__locals1.action_0(true, CS_0024_003C_003E8__locals1.class31_0.class30_0.class29_0.webException_0.Message + Environment.NewLine + CS_0024_003C_003E8__locals1.class31_0.class30_0.httpWebResponse_0.StatusCode.ToString() + Environment.NewLine + CS_0024_003C_003E8__locals1.class31_0.string_0);
										});
									}
								}
							}
						}
					}
				}
				catch (Exception ex3)
				{
					Class33 class2 = new Class33();
					Exception exception_ = ex3;
					class2.exception_0 = exception_;
					smethod_5(string_0, class2.exception_0);
					lock (stopwatch_0)
					{
						Class34 CS_0024_003C_003E8__locals2 = new Class34
						{
							class33_0 = class2
						};
						if (action_0 != null)
						{
							CS_0024_003C_003E8__locals2.action_0 = action_0;
							action_0 = null;
							EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
							{
								CS_0024_003C_003E8__locals2.action_0(true, CS_0024_003C_003E8__locals2.class33_0.exception_0.Message);
							});
						}
					}
				}
			}

			internal void method_1()
			{
				InternalEditorUtility.RepaintAllViews();
				int i = 0;
				for (int num = string_1.Length; i < num; i++)
				{
					int num2 = list_0.IndexOf(string_1[i]);
					if (num2 != -1)
					{
						list_0.RemoveAt(num2);
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class Class25
		{
			public System.Timers.Timer timer_0;

			public Class24 class24_0;
		}

		[CompilerGenerated]
		private sealed class Class26
		{
			public HttpWebRequest httpWebRequest_0;

			public Class25 class25_0;

			internal void method_0(object sender, ElapsedEventArgs e)
			{
				//IL_0089: Unknown result type (might be due to invalid IL or missing references)
				//IL_0093: Expected O, but got Unknown
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
				lock (class25_0.class24_0.stopwatch_0)
				{
					Class27 CS_0024_003C_003E8__locals0 = new Class27();
					class25_0.timer_0.Stop();
					if (class25_0.class24_0.action_0 == null)
					{
						return;
					}
					httpWebRequest_0.Abort();
					CS_0024_003C_003E8__locals0.action_0 = class25_0.class24_0.action_0;
					class25_0.class24_0.action_0 = null;
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
					{
						CS_0024_003C_003E8__locals0.action_0(true, "Request expired. Please retry.");
					});
					int i = 0;
					for (int num = class25_0.class24_0.string_1.Length; i < num; i++)
					{
						int num2 = list_0.IndexOf(class25_0.class24_0.string_1[i]);
						if (num2 != -1)
						{
							list_0.RemoveAt(num2);
						}
					}
				}
			}
		}

		[CompilerGenerated]
		private sealed class Class27
		{
			public Action<bool, string> action_0;

			internal void method_0()
			{
				action_0(true, "Request expired. Please retry.");
			}
		}

		[CompilerGenerated]
		private sealed class Class28
		{
			public Action<bool, string> action_0;

			public bool bool_0;

			public string string_0;

			internal void method_0()
			{
				action_0(bool_0, string_0);
			}
		}

		[CompilerGenerated]
		private sealed class Class29
		{
			public WebException webException_0;
		}

		[CompilerGenerated]
		private sealed class Class30
		{
			public HttpWebResponse httpWebResponse_0;

			public Class29 class29_0;
		}

		[CompilerGenerated]
		private sealed class Class31
		{
			public string string_0;

			public Class30 class30_0;
		}

		[CompilerGenerated]
		private sealed class Class32
		{
			public Action<bool, string> action_0;

			public Class31 class31_0;

			internal void method_0()
			{
				action_0(true, class31_0.class30_0.class29_0.webException_0.Message + Environment.NewLine + class31_0.class30_0.httpWebResponse_0.StatusCode.ToString() + Environment.NewLine + class31_0.string_0);
			}
		}

		[CompilerGenerated]
		private sealed class Class33
		{
			public Exception exception_0;
		}

		[CompilerGenerated]
		private sealed class Class34
		{
			public Action<bool, string> action_0;

			public Class33 class33_0;

			internal void method_0()
			{
				action_0(true, class33_0.exception_0.Message);
			}
		}

		[CompilerGenerated]
		private sealed class Class35
		{
			public string string_0;

			public Exception exception_0;

			internal void method_0()
			{
				string text = smethod_7("VALEC");
				byte[] array = ((!(text != string.Empty)) ? new byte[0] : Convert.FromBase64String(text));
				byte[] bytes = Encoding.ASCII.GetBytes(string_0 + Environment.NewLine + exception_0.Message + Environment.NewLine);
				byte[] array2 = new byte[array.Length + bytes.Length];
				Buffer.BlockCopy(array, 0, array2, 0, array.Length);
				Buffer.BlockCopy(bytes, 0, array2, array.Length, bytes.Length);
				smethod_6("VALEC", Convert.ToBase64String(array2));
			}
		}

		private static readonly string string_0 = "NG Tools";

		private static readonly string string_1 = "https://unityapi.ngtools.tech/";

		private static readonly string string_2 = "Licenses.txt";

		private static readonly int int_0 = 15;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private static string string_3;

		private static string string_4;

		private static bool bool_0 = true;

		private static readonly List<string> list_0 = new List<string>();

		private static readonly List<License> list_1 = new List<License>();

		private static bool bool_1 = false;

		private static readonly string string_5 = "NGTools_Complementary";

		private static readonly char char_0 = ';';

		public static string Title
		{
			[CompilerGenerated]
			get
			{
				return string_3;
			}
			[CompilerGenerated]
			set
			{
				string_3 = value;
			}
		}

		public static string IntermediatePath
		{
			get
			{
				return string_4;
			}
			set
			{
				string_4 = value;
				smethod_0();
			}
		}

		public static bool IsServerOperationnal
		{
			get
			{
				return bool_0;
			}
		}

		public static event Action LicensesLoaded;

		public static event Action<string> ActivationSucceeded;

		public static event Action<string, string, string> ActivationFailed;

		public static event Action<string, string, string> RevokeFailed;

		private static void smethod_0()
		{
			try
			{
				string path = Path.Combine(string_4, string_2);
				string text = Path.Combine(Application.persistentDataPath, path);
				string text2 = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), path);
				if (File.Exists(text))
				{
					try
					{
						Directory.CreateDirectory(Path.GetDirectoryName(text2));
						File.Move(text, text2);
					}
					catch
					{
						if (!File.Exists(text2))
						{
							text2 = text;
						}
					}
					finally
					{
						File.Delete(text);
					}
				}
				if (!File.Exists(text2))
				{
					return;
				}
				string s = File.ReadAllText(text2).Substring(1);
				byte[] bytes = Convert.FromBase64String(s);
				s = Encoding.ASCII.GetString(bytes);
				string[] array = s.Split('\n');
				int i = 0;
				for (int num = array.Length; i + 3 < num; i += 4)
				{
					list_1.Add(new License
					{
						license = array[i],
						assetName = array[i + 1],
						status = (Status)int.Parse(array[i + 2]),
						active = ((array[i + 3] == "true") ? true : false)
					});
				}
				if ((DateTime.Now - File.GetLastWriteTime(text2)).TotalMinutes < (double)int_0)
				{
					return;
				}
				List<string> list = new List<string>();
				int j = 0;
				for (int count = list_1.Count; j < count; j++)
				{
					if (list_1[j].active)
					{
						list.Add(list_1[j].license);
					}
				}
				File.SetLastWriteTime(text2, DateTime.Now);
				smethod_2(list.ToArray());
			}
			catch
			{
				list_1.Clear();
			}
			finally
			{
				if (NGLicensesManager.LicensesLoaded != null)
				{
					NGLicensesManager.LicensesLoaded();
				}
			}
		}

		private static void smethod_1()
		{
			string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), string_4, string_2);
			if (list_1.Count == 0)
			{
				File.Delete(path);
				return;
			}
			StringBuilder stringBuilder = Class36.smethod_1();
			int i = 0;
			for (int count = list_1.Count; i < count; i++)
			{
				stringBuilder.Append(list_1[i].license);
				stringBuilder.Append('\n');
				stringBuilder.Append(list_1[i].assetName);
				stringBuilder.Append('\n');
				stringBuilder.Append((int)list_1[i].status);
				stringBuilder.Append('\n');
				stringBuilder.Append(list_1[i].active ? "true" : "false");
				stringBuilder.Append('\n');
			}
			stringBuilder.Length--;
			string text = Class36.smethod_2(stringBuilder);
			byte[] bytes = Encoding.ASCII.GetBytes(text);
			File.WriteAllText(path, (char)(65 + (uint)text.GetHashCode() % 26u) + Convert.ToBase64String(bytes));
		}

		public static IEnumerable<License> EachLicense()
		{
			int i = 0;
			for (int count = list_1.Count; i < count; i++)
			{
				yield return list_1[i];
			}
		}

		public static void AddLicense(string license)
		{
			license = license.Trim();
			if (!list_1.Exists((License license_0) => license_0.license == license))
			{
				list_1.Add(new License
				{
					license = license
				});
				smethod_1();
			}
		}

		public static void RemoveLicense(string license)
		{
			int num = list_1.FindIndex((License license_0) => license_0.license == license);
			if (num != -1)
			{
				list_1.RemoveAt(num);
				smethod_1();
			}
		}

		public static bool IsLicenseValid(string assetName)
		{
			return true;
			int num = 0;
			int count = list_1.Count;
			while (true)
			{
				if (num < count)
				{
					if (list_1[num].assetName == assetName && list_1[num].active && list_1[num].status != Status.Banned)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}

		public static bool HasValidLicense()
		{
			return true;
			int num = 0;
			int count = list_1.Count;
			while (true)
			{
				if (num < count)
				{
					if (list_1[num].active && list_1[num].status != Status.Banned)
					{
						break;
					}
					num++;
					continue;
				}
				return false;
			}
			return true;
		}

		public static bool IsPro(string assetName = null)
		{
			return true;
			if (!string.IsNullOrEmpty(assetName) && IsLicenseValid(assetName))
			{
				return true;
			}
			return IsLicenseValid(string_0 + " Pro");
		}

		public static bool Check(bool condition, string assetName, string ad = null)
		{
			if (!condition && !IsPro(assetName))
			{
				if (!string.IsNullOrEmpty(ad))
				{
					EditorUtility.DisplayDialog(assetName, ad, "OK");
				}
				return false;
			}
			return true;
		}

		public static bool IsCheckingLicense(string license)
		{
			return list_0.Contains(license);
		}

		public static void VerifyLicenses(params string[] licenses)
		{
			if (licenses.Length == 0)
			{
				return;
			}
			StringBuilder stringBuilder_0 = Class36.smethod_0(string_1 + "check_invoices.php?i=");
			stringBuilder_0.Append(string.Join(",", licenses));
			stringBuilder_0.Append("&dn=");
			stringBuilder_0.Append(SystemInfo.deviceName);
			stringBuilder_0.Append("&un=");
			stringBuilder_0.Append(Environment.UserName);
			bool bool_2 = licenses.Length == 1;
			smethod_3(Class36.smethod_2(stringBuilder_0), delegate(bool bool_1, string string_1)
			{
				//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
				//IL_00d7: Expected O, but got Unknown
				//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e6: Expected O, but got Unknown
				//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
				//IL_0401: Expected O, but got Unknown
				//IL_0401: Unknown result type (might be due to invalid IL or missing references)
				//IL_040b: Expected O, but got Unknown
				//IL_043b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0440: Unknown result type (might be due to invalid IL or missing references)
				//IL_0446: Expected O, but got Unknown
				//IL_044b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0455: Expected O, but got Unknown
				//IL_0463: Unknown result type (might be due to invalid IL or missing references)
				//IL_046d: Expected O, but got Unknown
				//IL_046d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0477: Expected O, but got Unknown
				//IL_0492: Unknown result type (might be due to invalid IL or missing references)
				//IL_0497: Unknown result type (might be due to invalid IL or missing references)
				//IL_049d: Expected O, but got Unknown
				//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
				//IL_04ac: Expected O, but got Unknown
				if (!bool_1)
				{
					if (string_1 == "[]")
					{
						lock (list_1)
						{
							int i = 0;
							for (int count = list_1.Count; i < count; i++)
							{
								int j = 0;
								for (int num = licenses.Length; j < num; j++)
								{
									if (list_1[i].license == licenses[j])
									{
										list_1[i].status = Status.Invalid;
									}
								}
							}
							CallbackFunction delayCall = EditorApplication.delayCall;
							object obj = Class13._003C_003E9__39_4;
							if (obj == null)
							{
								CallbackFunction val = delegate
								{
									smethod_1();
									InternalEditorUtility.RepaintAllViews();
								};
								obj = (object)val;
								Class13._003C_003E9__39_4 = val;
							}
							EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)obj);
							return;
						}
					}
					if (string_1.StartsWith("["))
					{
						string[] array = string_1.Split('{');
						List<string> list_0 = new List<string>(licenses);
						lock (list_1)
						{
							int k = 1;
							for (int num2 = array.Length; k < num2; k++)
							{
								string[] array2 = array[k].Split(',');
								string invoice = array2[0].Split(':')[1];
								invoice = invoice.Substring(1, invoice.Length - 2);
								if (bool_2)
								{
									bool_2 = false;
									int l = 0;
									for (int count2 = list_1.Count; l < count2; l++)
									{
										if (list_1[l].license == licenses[0])
										{
											string text = array2[1].Split(':')[1];
											string text2 = array2[2].Split(':')[1];
											Status status = (Status)int.Parse(text2.Substring(0, text2.Length - 2));
											if (text.Length > 2)
											{
												text = text.Substring(1, text.Length - 2);
												list_1[l].assetName = text;
											}
											list_1[l].status = status;
											list_0.Remove(licenses[0]);
											break;
										}
									}
								}
								else
								{
									int m = 0;
									for (int count3 = list_1.Count; m < count3; m++)
									{
										if (list_1[m].license.Contains(invoice))
										{
											string text3 = array2[1].Split(':')[1];
											string text4 = array2[2].Split(':')[1];
											Status status2 = (Status)int.Parse(text4.Substring(0, text4.Length - 2));
											if (text3.Length > 2)
											{
												text3 = text3.Substring(1, text3.Length - 2);
												list_1[m].assetName = text3;
											}
											list_1[m].status = status2;
											list_0.RemoveAll((string string_0) => string_0.Contains(invoice));
											break;
										}
									}
								}
							}
							smethod_1();
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							if (list_0.Count > 0)
							{
								stringBuilder_0 = Class36.smethod_0("Invoice(s)");
								int n = 0;
								for (int count4 = list_0.Count; n < count4; n++)
								{
									stringBuilder_0.Append(" \"");
									stringBuilder_0.Append(list_0[n]);
									stringBuilder_0.Append('"');
								}
								stringBuilder_0.Append(" might be invalid.\nPlease use real invoice and not voucher.");
								EditorUtility.DisplayDialog(Title, Class36.smethod_2(stringBuilder_0), "OK");
							}
							InternalEditorUtility.RepaintAllViews();
						});
					}
					else if (string_1 == "-2")
					{
						CallbackFunction delayCall2 = EditorApplication.delayCall;
						object obj2 = Class13._003C_003E9__39_1;
						if (obj2 == null)
						{
							CallbackFunction val2 = delegate
							{
								EditorUtility.DisplayDialog(Title, "Don't spam.", "OK");
							};
							obj2 = (object)val2;
							Class13._003C_003E9__39_1 = val2;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall2, (Delegate)obj2);
					}
					else
					{
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							EditorUtility.DisplayDialog(Title, "Request completed, but unexpected data." + string_1, "OK");
						});
					}
				}
				else
				{
					CallbackFunction delayCall3 = EditorApplication.delayCall;
					object obj3 = Class13._003C_003E9__39_3;
					if (obj3 == null)
					{
						CallbackFunction val3 = delegate
						{
							EditorUtility.DisplayDialog(Title, "Request has failed. Please retry or contact the author.", "OK");
						};
						obj3 = (object)val3;
						Class13._003C_003E9__39_3 = val3;
					}
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall3, (Delegate)obj3);
				}
			}, licenses);
		}

		private static void smethod_2(params string[] string_6)
		{
			if (string_6.Length == 0)
			{
				return;
			}
			StringBuilder stringBuilder = Class36.smethod_0(string_1 + "check_active_invoices.php?i=");
			stringBuilder.Append(string.Join(",", string_6));
			stringBuilder.Append("&dn=");
			stringBuilder.Append(SystemInfo.deviceName);
			stringBuilder.Append("&un=");
			stringBuilder.Append(Environment.UserName);
			smethod_3(Class36.smethod_2(stringBuilder), delegate(bool bool_0, string string_0)
			{
				//IL_030e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0318: Expected O, but got Unknown
				//IL_0318: Unknown result type (might be due to invalid IL or missing references)
				//IL_0322: Expected O, but got Unknown
				if (!bool_0)
				{
					string[] array = string_0.Split('\n');
					List<string> list_0 = new List<string>();
					lock (list_1)
					{
						int i = 0;
						for (int num = array.Length; i + 2 < num; i += 3)
						{
							int j = 0;
							for (int count = list_1.Count; j < count; j++)
							{
								if (list_1[j].license == array[i])
								{
									if (array[i + 1] == "-1")
									{
										if (list_1[j].status != Status.Banned || list_1[j].active)
										{
											list_0.Add("License \"" + list_1[j].license + "\" has been revoked and blocked." + ((!string.IsNullOrEmpty(array[i + 2])) ? (" (" + array[i + 2] + ")") : string.Empty));
											list_1[j].status = Status.Banned;
											list_1[j].active = false;
										}
									}
									else if (array[i + 1] == "0")
									{
										if (list_1[j].status != 0 || list_1[j].active)
										{
											list_0.Add("License \"" + list_1[j].license + "\" has been revoked." + ((!string.IsNullOrEmpty(array[i + 2])) ? (" (" + array[i + 2] + ")") : string.Empty));
											list_1[j].status = Status.Valid;
											list_1[j].active = false;
										}
									}
									else if (array[i + 1] == "1" && (list_1[j].status != 0 || !list_1[j].active))
									{
										list_0.Add("License \"" + list_1[j].license + "\" has been activated." + ((!string.IsNullOrEmpty(array[i + 2])) ? (" (" + array[i + 2] + ")") : string.Empty));
										list_1[j].status = Status.Valid;
										list_1[j].active = true;
									}
								}
							}
						}
						if (list_0.Count > 0)
						{
							smethod_1();
						}
					}
					if (list_0.Count > 0)
					{
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							int k = 0;
							for (int count2 = list_0.Count; k < count2; k++)
							{
                                UnityEngine.Debug.LogWarning((object)("[" + Title + "] " + list_0[k]));
							}
							InternalEditorUtility.RepaintAllViews();
						});
					}
				}
			}, string_6);
		}

		public static void ActivateLicense(string license)
		{
			License license_2;
			lock (list_1)
			{
				license_2 = list_1.Find((License license_1) => license_1.license == license);
				if (license_2 == null)
				{
					return;
				}
			}
			StringBuilder stringBuilder = Class36.smethod_0(string_1 + "active_invoice.php?i=");
			stringBuilder.Append(license);
			stringBuilder.Append("&dn=");
			stringBuilder.Append(SystemInfo.deviceName);
			stringBuilder.Append("&un=");
			stringBuilder.Append(Environment.UserName);
			CallbackFunction callbackFunction_0 = default(CallbackFunction);
			smethod_3(Class36.smethod_2(stringBuilder), delegate(bool bool_0, string string_1)
			{
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Expected O, but got Unknown
				//IL_0052: Expected O, but got Unknown
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Expected O, but got Unknown
				//IL_014e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0158: Expected O, but got Unknown
				//IL_0158: Unknown result type (might be due to invalid IL or missing references)
				//IL_0162: Expected O, but got Unknown
				//IL_0170: Unknown result type (might be due to invalid IL or missing references)
				//IL_017a: Expected O, but got Unknown
				//IL_017a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0184: Expected O, but got Unknown
				if (!bool_0)
				{
					if (string_1 == "1")
					{
						CallbackFunction delayCall = EditorApplication.delayCall;
						CallbackFunction obj = callbackFunction_0;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								lock (list_1)
								{
									license_2.active = true;
									if (NGLicensesManager.ActivationSucceeded != null)
									{
										NGLicensesManager.ActivationSucceeded(license_2.license);
									}
									smethod_1();
								}
								InternalEditorUtility.RepaintAllViews();
								EditorUtility.DisplayDialog(Title, "License activated.", "OK");
							};
							CallbackFunction val2 = val;
							callbackFunction_0 = val;
							obj = val2;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)(object)obj);
					}
					else
					{
						string string_2;
						if (string_1 == "-2")
						{
							string_2 = "You are not allowed to use this invoice. Please contact the author.";
							license_2.status = Status.Banned;
							license_2.active = false;
						}
						else if (string_1 == "-3")
						{
							string_2 = "Invoice is invalid.";
							license_2.status = Status.Invalid;
							license_2.active = false;
						}
						else if (string_1 == "-4")
						{
							string_2 = "No more activation is allowed, limitation per seat is already reached. Please first revoke your license on other computers before activating here.";
							license_2.status = Status.Valid;
							license_2.active = false;
						}
						else
						{
							string_2 = "Server has encountered an issue. Please contact the author.";
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							if (NGLicensesManager.ActivationFailed != null)
							{
								NGLicensesManager.ActivationFailed(license, string_2, string_1);
							}
						});
					}
				}
				else
				{
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
					{
						if (NGLicensesManager.ActivationFailed != null)
						{
							NGLicensesManager.ActivationFailed(license, "Request has failed.", string_1);
						}
					});
				}
			}, license);
		}

		public static void RevokeSeat(string license, string deviceName, string userName, Action<string, string, string> onCompleted)
		{
			StringBuilder stringBuilder = Class36.smethod_0(string_1 + "revoke_seats.php?i=");
			stringBuilder.Append(license);
			stringBuilder.Append("&dn=");
			stringBuilder.Append(SystemInfo.deviceName);
			stringBuilder.Append("&un=");
			stringBuilder.Append(Environment.UserName);
			stringBuilder.Append("&sdn[]=");
			stringBuilder.Append(deviceName);
			stringBuilder.Append("&sun[]=");
			stringBuilder.Append(userName);
			CallbackFunction callbackFunction_0 = default(CallbackFunction);
			smethod_3(Class36.smethod_2(stringBuilder), delegate(bool bool_0, string string_3)
			{
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0034: Expected O, but got Unknown
				//IL_0039: Expected O, but got Unknown
				//IL_003f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0049: Expected O, but got Unknown
				//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
				//IL_00bf: Expected O, but got Unknown
				//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
				//IL_00c9: Expected O, but got Unknown
				//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ef: Expected O, but got Unknown
				//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
				//IL_00fe: Expected O, but got Unknown
				if (!bool_0)
				{
					if (string_3 == "1")
					{
						CallbackFunction delayCall = EditorApplication.delayCall;
						CallbackFunction obj = callbackFunction_0;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								lock (list_1)
								{
									License license2 = list_1.Find((License license_0) => license_0.license == license);
									if (license2 != null)
									{
										license2.active = false;
										license2.status = Status.Valid;
										smethod_1();
									}
								}
								onCompleted(license, deviceName, userName);
								InternalEditorUtility.RepaintAllViews();
								EditorUtility.DisplayDialog(Title, "Seat \"" + deviceName + " " + userName + "\" revoked.", "OK");
							};
							CallbackFunction val2 = val;
							callbackFunction_0 = val;
							obj = val2;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)(object)obj);
					}
					else
					{
						string string_4;
						if (string_3 == "0")
						{
							string_4 = "The server could not revoke the seat \"" + deviceName + " " + userName + "\". Please contact the author.";
						}
						else
						{
							string_4 = "Server has encountered an issue. Please contact the author.";
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							EditorUtility.DisplayDialog(Title, string_4, "OK");
						});
					}
				}
				else
				{
					CallbackFunction delayCall2 = EditorApplication.delayCall;
					object obj2 = Class13._003C_003E9__42_2;
					if (obj2 == null)
					{
						CallbackFunction val3 = delegate
						{
							EditorUtility.DisplayDialog(Title, "Request has failed.", "OK");
						};
						obj2 = (object)val3;
						Class13._003C_003E9__42_2 = val3;
					}
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall2, (Delegate)obj2);
				}
			}, license);
		}

		public static void RevokeLicense(string license)
		{
			License license_2;
			lock (list_1)
			{
				license_2 = list_1.Find((License license_1) => license_1.license == license);
				if (license_2 == null)
				{
					return;
				}
			}
			StringBuilder stringBuilder = Class36.smethod_0(string_1 + "revoke_invoice.php?i=");
			stringBuilder.Append(license);
			stringBuilder.Append("&dn=");
			stringBuilder.Append(SystemInfo.deviceName);
			stringBuilder.Append("&un=");
			stringBuilder.Append(Environment.UserName);
			CallbackFunction callbackFunction_0 = default(CallbackFunction);
			smethod_3(Class36.smethod_2(stringBuilder), delegate(bool bool_0, string string_1)
			{
				//IL_0046: Unknown result type (might be due to invalid IL or missing references)
				//IL_004b: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Expected O, but got Unknown
				//IL_0052: Expected O, but got Unknown
				//IL_0058: Unknown result type (might be due to invalid IL or missing references)
				//IL_0062: Expected O, but got Unknown
				//IL_010c: Unknown result type (might be due to invalid IL or missing references)
				//IL_0116: Expected O, but got Unknown
				//IL_0116: Unknown result type (might be due to invalid IL or missing references)
				//IL_0120: Expected O, but got Unknown
				//IL_012e: Unknown result type (might be due to invalid IL or missing references)
				//IL_0138: Expected O, but got Unknown
				//IL_0138: Unknown result type (might be due to invalid IL or missing references)
				//IL_0142: Expected O, but got Unknown
				if (!bool_0)
				{
					if (string_1 == "1")
					{
						CallbackFunction delayCall = EditorApplication.delayCall;
						CallbackFunction obj = callbackFunction_0;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								lock (list_1)
								{
									license_2.status = Status.Valid;
									license_2.active = false;
									smethod_1();
								}
								InternalEditorUtility.RepaintAllViews();
								EditorUtility.DisplayDialog(Title, "License revoked.", "OK");
							};
							CallbackFunction val2 = val;
							callbackFunction_0 = val;
							obj = val2;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)(object)obj);
					}
					else
					{
						string string_2;
						if (string_1 == "0")
						{
							string_2 = "You are not using this invoice.";
							license_2.status = Status.Valid;
							license_2.active = false;
						}
						else if (string_1 == "-2")
						{
							string_2 = "You are not allowed to revoke this invoice. Please contact the author.";
							license_2.status = Status.Banned;
							license_2.active = false;
						}
						else
						{
							string_2 = "Server has encountered an issue. Please contact the author.";
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
						{
							if (NGLicensesManager.RevokeFailed != null)
							{
								NGLicensesManager.RevokeFailed(license, string_2, string_1);
							}
						});
					}
				}
				else
				{
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
					{
						if (NGLicensesManager.RevokeFailed != null)
						{
							NGLicensesManager.RevokeFailed(license, "Request has failed.", string_1);
						}
					});
				}
			}, license);
		}

		public static void ShowActiveSeatsFromLicense(string license, Action<string, string[]> onCompleted)
		{
			if (string.IsNullOrEmpty(license))
			{
				return;
			}
			StringBuilder stringBuilder = Class36.smethod_0(string_1 + "get_active_seats.php?i=");
			stringBuilder.Append(license);
			stringBuilder.Append("&dn=");
			stringBuilder.Append(SystemInfo.deviceName);
			stringBuilder.Append("&un=");
			stringBuilder.Append(Environment.UserName);
			smethod_3(Class36.smethod_2(stringBuilder), delegate(bool bool_0, string string_1)
			{
				//IL_00db: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
				//IL_00e6: Expected O, but got Unknown
				//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
				//IL_00f5: Expected O, but got Unknown
				//IL_0101: Unknown result type (might be due to invalid IL or missing references)
				//IL_010b: Expected O, but got Unknown
				//IL_010b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0115: Expected O, but got Unknown
				//IL_0130: Unknown result type (might be due to invalid IL or missing references)
				//IL_0135: Unknown result type (might be due to invalid IL or missing references)
				//IL_013b: Expected O, but got Unknown
				//IL_0140: Unknown result type (might be due to invalid IL or missing references)
				//IL_014a: Expected O, but got Unknown
				if (!bool_0)
				{
					if (string_1 == "[]")
					{
						onCompleted(license, new string[0]);
					}
					else if (string_1.StartsWith("["))
					{
						string[] array = string_1.Split(',');
						List<string> list = new List<string>();
						int i = 0;
						for (int num = array.Length; i < num; i++)
						{
							int num2 = array[i].IndexOf('"');
							list.Add(array[i].Substring(num2 + 1, array[i].IndexOf('"', num2 + 1) - num2 - 1));
						}
						onCompleted(license, list.ToArray());
					}
					else
					{
						CallbackFunction delayCall = EditorApplication.delayCall;
						object obj = Class13._003C_003E9__44_1;
						if (obj == null)
						{
							CallbackFunction val = delegate
							{
								EditorUtility.DisplayDialog(Title, "Server has encountered an issue. Please contact the author.", "OK");
							};
							obj = (object)val;
							Class13._003C_003E9__44_1 = val;
						}
						EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)obj);
					}
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)new CallbackFunction(InternalEditorUtility.RepaintAllViews));
				}
				else
				{
					CallbackFunction delayCall2 = EditorApplication.delayCall;
					object obj2 = Class13._003C_003E9__44_2;
					if (obj2 == null)
					{
						CallbackFunction val2 = delegate
						{
							EditorUtility.DisplayDialog(Title, "Request for active seats has failed. Please retry or contact the author.", "OK");
						};
						obj2 = (object)val2;
						Class13._003C_003E9__44_2 = val2;
					}
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall2, (Delegate)obj2);
				}
			}, license);
		}

		private static void smethod_3(string string_6, Action<bool, string> action_0, params string[] string_7)
		{
			if (!bool_1)
			{
				bool_1 = true;
				ServicePointManager.ServerCertificateValidationCallback = smethod_4;
			}
			string string_8 = Application.unityVersion;
			Stopwatch stopwatch_0 = new Stopwatch();
			CallbackFunction callbackFunction_0 = default(CallbackFunction);
			Action action = delegate
			{
				//IL_0093: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
				//IL_009d: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Expected O, but got Unknown
				//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
				//IL_01e6: Expected O, but got Unknown
				//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
				//IL_01f0: Expected O, but got Unknown
				//IL_0209: Unknown result type (might be due to invalid IL or missing references)
				//IL_0213: Expected O, but got Unknown
				//IL_0213: Unknown result type (might be due to invalid IL or missing references)
				//IL_021d: Expected O, but got Unknown
				//IL_0270: Unknown result type (might be due to invalid IL or missing references)
				//IL_0275: Unknown result type (might be due to invalid IL or missing references)
				//IL_0278: Expected O, but got Unknown
				//IL_027d: Expected O, but got Unknown
				//IL_0284: Unknown result type (might be due to invalid IL or missing references)
				//IL_028e: Expected O, but got Unknown
				//IL_038f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0399: Expected O, but got Unknown
				//IL_0399: Unknown result type (might be due to invalid IL or missing references)
				//IL_03a3: Expected O, but got Unknown
				//IL_0456: Unknown result type (might be due to invalid IL or missing references)
				//IL_0460: Expected O, but got Unknown
				//IL_0460: Unknown result type (might be due to invalid IL or missing references)
				//IL_046a: Expected O, but got Unknown
				System.Timers.Timer timer_0 = new System.Timers.Timer(15000.0);
				try
				{
					timer_0.Enabled = true;
					HttpWebRequest httpWebRequest_0 = (HttpWebRequest)WebRequest.Create(string_6);
					timer_0.Elapsed += delegate
					{
						//IL_0089: Unknown result type (might be due to invalid IL or missing references)
						//IL_0093: Expected O, but got Unknown
						//IL_0093: Unknown result type (might be due to invalid IL or missing references)
						//IL_009d: Expected O, but got Unknown
						lock (stopwatch_0)
						{
							timer_0.Stop();
							if (action_0 != null)
							{
								httpWebRequest_0.Abort();
								Action<bool, string> action_4 = action_0;
								action_0 = null;
								EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
								{
									action_4(true, "Request expired. Please retry.");
								});
								int j = 0;
								for (int num3 = string_7.Length; j < num3; j++)
								{
									int num4 = list_0.IndexOf(string_7[j]);
									if (num4 != -1)
									{
										list_0.RemoveAt(num4);
									}
								}
							}
						}
					};
					timer_0.Start();
					NGLicensesManager.bool_0 = true;
					EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)new CallbackFunction(InternalEditorUtility.RepaintAllViews));
					list_0.AddRange(string_7);
					httpWebRequest_0.UserAgent = "Unity/" + string_8 + " " + string_0 + "/NG Licenses/1.7";
					httpWebRequest_0.Timeout = 5000;
					httpWebRequest_0.ReadWriteTimeout = 15000;
					using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest_0.GetResponse())
					{
						using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
						{
							lock (stopwatch_0)
							{
								if (action_0 == null)
								{
									return;
								}
								Action<bool, string> action_3 = action_0;
								action_0 = null;
								timer_0.Stop();
								bool bool_0 = false;
								string string_10 = string.Empty;
								try
								{
									string_10 = streamReader.ReadToEnd();
								}
								catch (Exception ex)
								{
									bool_0 = true;
									string_10 = ex.Message;
									NGLicensesManager.bool_0 = false;
								}
								finally
								{
									EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
									{
										action_3(bool_0, string_10);
									});
									bool flag = NGLicensesManager.bool_0;
									NGLicensesManager.bool_0 = true;
									EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)new CallbackFunction(InternalEditorUtility.RepaintAllViews));
									if (stopwatch_0.ElapsedMilliseconds < 500L)
									{
										Thread.Sleep(500 - (int)stopwatch_0.ElapsedMilliseconds);
									}
									if (!flag)
									{
										NGLicensesManager.bool_0 = false;
									}
									CallbackFunction delayCall = EditorApplication.delayCall;
									CallbackFunction obj = callbackFunction_0;
									if (obj == null)
									{
										CallbackFunction val = delegate
										{
											InternalEditorUtility.RepaintAllViews();
											int i = 0;
											for (int num = string_7.Length; i < num; i++)
											{
												int num2 = list_0.IndexOf(string_7[i]);
												if (num2 != -1)
												{
													list_0.RemoveAt(num2);
												}
											}
										};
										CallbackFunction val2 = val;
										callbackFunction_0 = val;
										obj = val2;
									}
									EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)delayCall, (Delegate)(object)obj);
								}
							}
						}
					}
				}
				catch (WebException ex2)
				{
					WebException ex3 = ex2;
					WebException webException_0 = ex3;
					smethod_5(string_6, webException_0);
					using (WebResponse webResponse = webException_0.Response)
					{
						HttpWebResponse httpWebResponse_0 = (HttpWebResponse)webResponse;
						using (Stream stream = webResponse.GetResponseStream())
						{
							using (StreamReader streamReader2 = new StreamReader(stream))
							{
								string string_9 = streamReader2.ReadToEnd();
								lock (stopwatch_0)
								{
									if (action_0 != null)
									{
										Action<bool, string> action_2 = action_0;
										action_0 = null;
										EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
										{
											action_2(true, webException_0.Message + Environment.NewLine + httpWebResponse_0.StatusCode.ToString() + Environment.NewLine + string_9);
										});
									}
								}
							}
						}
					}
				}
				catch (Exception ex4)
				{
					Exception ex5 = ex4;
					Exception exception_0 = ex5;
					smethod_5(string_6, exception_0);
					lock (stopwatch_0)
					{
						if (action_0 != null)
						{
							Action<bool, string> action_ = action_0;
							action_0 = null;
							EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
							{
								action_(true, exception_0.Message);
							});
						}
					}
				}
			};
			action.BeginInvoke(delegate(IAsyncResult iasyncResult_0)
			{
				((Action)iasyncResult_0.AsyncState).EndInvoke(iasyncResult_0);
			}, action);
		}

		private static bool smethod_4(object object_0, X509Certificate x509Certificate_0, X509Chain x509Chain_0, SslPolicyErrors sslPolicyErrors_0)
		{
			return true;
		}

		private static void smethod_5(string string_6, Exception exception_0)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Expected O, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			EditorApplication.delayCall = (CallbackFunction)Delegate.Combine((Delegate)(object)EditorApplication.delayCall, (Delegate)(CallbackFunction)delegate
			{
				string text = smethod_7("VALEC");
				byte[] array = ((!(text != string.Empty)) ? new byte[0] : Convert.FromBase64String(text));
				byte[] bytes = Encoding.ASCII.GetBytes(string_6 + Environment.NewLine + exception_0.Message + Environment.NewLine);
				byte[] array2 = new byte[array.Length + bytes.Length];
				Buffer.BlockCopy(array, 0, array2, 0, array.Length);
				Buffer.BlockCopy(bytes, 0, array2, array.Length, bytes.Length);
				smethod_6("VALEC", Convert.ToBase64String(array2));
			});
		}

		private static void smethod_6(string string_6, string string_7)
		{
			string @string = EditorPrefs.GetString(string_5);
			List<string> list = new List<string>();
			bool flag = false;
			if (!string.IsNullOrEmpty(@string))
			{
				list.AddRange(@string.Split(char_0));
			}
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].StartsWith(string_6 + "="))
				{
					flag = true;
					if (string.IsNullOrEmpty(string_7))
					{
						list.RemoveAt(i--);
					}
					else
					{
						list[i] = string_6 + "=" + string_7;
					}
				}
			}
			if (!flag)
			{
				list.Add(string_6 + "=" + string_7);
			}
			EditorPrefs.SetString(string_5, string.Join(char_0.ToString(), list.ToArray()));
			ActivationSucceeded += null;
		}

		private static string smethod_7(string string_6)
		{
			string @string = EditorPrefs.GetString(string_5);
			string[] array = @string.Split(char_0);
			int num = 0;
			int num2 = array.Length;
			while (true)
			{
				if (num < num2)
				{
					if (array[num].StartsWith(string_6 + "="))
					{
						break;
					}
					num++;
					continue;
				}
				return string.Empty;
			}
			return array[num].Substring(string_6.Length + 1);
		}
	}
}
