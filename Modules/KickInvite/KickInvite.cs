using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CSSharpUtils.Extensions;
using Invites.API;

namespace KickInvite;

public class Constants
{
    public const string PluginName = "Kick Invite [Module]";
    public const string PluginVersion = "1.0.0";
    public const string PluginAuthor = "Xummuk97";

    public const string RewardId = "KickInvite";
}

public class KickRewardDelegate : IRewardDelegate
{
    public void OnGive(CCSPlayerController controller)
    {
        controller.Kick("You have received an invite kick! :)");
    }
}

public class KickInvite : BasePlugin
{
    public override string ModuleName => Constants.PluginName;
    public override string ModuleVersion => Constants.PluginVersion;
    public override string ModuleAuthor => Constants.PluginAuthor;

    private PluginCapability<IAPI> _apiCapability = new(Invites.API.Constants.APICapability);

    public static IAPI? API { get; private set; }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        base.OnAllPluginsLoaded(hotReload);

        API = _apiCapability.Get();
        API?.AddReward(Constants.RewardId, new KickRewardDelegate());
    }
}
