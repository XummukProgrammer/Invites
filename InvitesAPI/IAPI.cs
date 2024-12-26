using CounterStrikeSharp.API.Core;

namespace Invites.API
{
    public interface IAPI
    {
        void AddReward(string id, IRewardDelegate @delegate);

        string? GenerateInvite(string rewardId);

        void ApplyInvite(CCSPlayerController? controller, string inviteId);
    }
}
