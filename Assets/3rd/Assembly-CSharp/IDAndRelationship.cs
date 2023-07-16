using System;

[Serializable]
public class IDAndRelationship
{
	public enum RelationshipState
	{
		Enemies,
		Strangers,
		Friends,
		Lovers
	}

	public int ID;

	public int Relationship;

	public RelationshipState CurrentRelationshipState = RelationshipState.Strangers;
}
