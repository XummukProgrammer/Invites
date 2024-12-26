using CounterStrikeSharp.API.Core;

namespace Invites.API
{
    public interface IAPI
    {
        public void AddAPIDelegate(APIDelegate @delegate);
        public void RemoveAPIDelegate(APIDelegate @delegate);

        public void AddReward(string id, IRewardDelegate @delegate);

        public string? GenerateInvite(string rewardId);

        public void ApplyInvite(CCSPlayerController? controller, string inviteId);
    }
}
