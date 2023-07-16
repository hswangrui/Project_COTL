using System.Runtime.Serialization;
using UnityEngine;

namespace Unify.Utils
{
	internal sealed class Vector2SerializationSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			Vector2 vector = (Vector2)obj;
			info.AddValue("x", vector.x);
			info.AddValue("y", vector.y);
			Debug.Log(vector);
		}

		public object SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
		{
			Vector2 vector = (Vector2)obj;
			vector.x = (float)info.GetValue("x", typeof(float));
			vector.y = (float)info.GetValue("y", typeof(float));
			obj = vector;
			return obj;
		}
	}
}
