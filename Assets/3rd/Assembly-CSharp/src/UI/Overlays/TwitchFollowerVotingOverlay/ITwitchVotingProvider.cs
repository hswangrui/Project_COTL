using System.Collections.Generic;

namespace src.UI.Overlays.TwitchFollowerVotingOverlay
{
	public interface ITwitchVotingProvider
	{
		bool AllowsVoting { get; set; }

	//	TwitchVoting.VotingType VotingType { get; set; }

		List<FollowerInfo> ProvideInfo();

		void FinalizeVote(FollowerInfo followerInfo);
	}
}
