using System;

[Serializable]
public abstract class ObjectivesDataFinalized
{
	public string GroupId;

	public int Index;

	public string UniqueGroupID;

	public abstract string GetText();
}
