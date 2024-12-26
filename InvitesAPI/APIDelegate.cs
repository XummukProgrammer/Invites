using CounterStrikeSharp.API.Core;

namespace Invites.API
{
    public class APIDelegate
    {
        public BasePlugin? BasePlugin { get; private set; }

        public APIDelegate(BasePlugin basePlugin)
        {
            BasePlugin = basePlugin;
        }

        public virtual void OnRewardAdded(string rewardId)
        {
        }

        public virtual void OnRewardGived(string rewardId, CCSPlayerController controller)
        {
        }

        public virtual void OnInviteGenerated(string inviteId, string packId)
        {
        }

        public virtual void OnInviteApplied(string inviteId, CCSPlayerController controller)
        {
        }
    }
}
