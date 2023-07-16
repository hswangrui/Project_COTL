using I2.Loc;

public class FollowerRoleInfo
{
	public static string GetLocalizedName(FollowerRole FollowerRole)
	{
		return LocalizationManager.GetTranslation(string.Format("Traits/{0}", FollowerRole));
	}

	public static string GetLocalizedDescription(FollowerRole FollowerRole)
	{
		return LocalizationManager.GetTranslation(string.Format("Traits/{0}/Description", FollowerRole));
	}
}
