using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Unify.Utils
{
	public class Serialization
	{
		public static BinaryFormatter CreateBinaryFormatter()
		{
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			SurrogateSelector surrogateSelector = new SurrogateSelector();
			surrogateSelector.AddSurrogate(surrogate: new Vector3SerializationSurrogate(), type: typeof(Vector3), context: new StreamingContext(StreamingContextStates.All));
			surrogateSelector.AddSurrogate(surrogate: new Vector2SerializationSurrogate(), type: typeof(Vector2), context: new StreamingContext(StreamingContextStates.All));
			binaryFormatter.SurrogateSelector = surrogateSelector;
			return binaryFormatter;
		}
	}
}
