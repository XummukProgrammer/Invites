using CounterStrikeSharp.API.Core;

namespace Invites.API
{
    public interface IRewardDelegate
    {
        void OnGive(CCSPlayerController controller);
    }
}
