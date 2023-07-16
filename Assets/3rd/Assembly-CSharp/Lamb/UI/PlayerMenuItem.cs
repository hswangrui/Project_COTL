namespace Lamb.UI
{
	public abstract class PlayerMenuItem<T> : BaseMonoBehaviour
	{
		public abstract void Configure(T item);
	}
}
