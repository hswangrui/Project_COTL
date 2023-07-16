public struct AnimatedSizeChange
{
	public float Size { get; private set; }

	public float Difference { get; private set; }

	public AnimatedSizeChange(float Size, float CurrentSize)
	{
		this.Size = Size;
		Difference = Size - CurrentSize;
	}
}
