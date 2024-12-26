using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using Invites.API;
using Microsoft.Extensions.Logging;

namespace Logger;

public class Constants
{
    public const string PluginName = "Logger [Module]";
    public const string PluginVersion = "1.0.0";
    public const string PluginAuthor = "Xummuk97";
}

public class LoggerAPIDelegate : APIDelegate
{
    public LoggerAPIDelegate(BasePlugin basePlugin) : base(basePlugin)
    {
    }

    public override void OnRewardAdded(string rewardId)
    {
        BasePlugin?.Logger.LogInformation($"OnRewardAdded(rewardId:{rewardId})");
    }

    public override void OnRewardGived(string rewardId, CCSPlayerController controller)
    {
        BasePlugin?.Logger.LogInformation($"OnRewardGived(rewardId:{rewardId} playerName:{controller.PlayerName})");
    }

    public override void OnInviteGenerated(string inviteId, string rewardId)
    {
        BasePlugin?.Logger.LogInformation($"OnInviteGenerated(inviteId:{inviteId} rewardId:{rewardId})");
    }

    public override void OnInviteApplied(string inviteId, CCSPlayerController controller)
    {
        BasePlugin?.Logger.LogInformation($"OnInviteApplied(inviteId:{inviteId} playerName:{controller.PlayerName})");
    }
}

public class Logger : BasePlugin
{
    public override string ModuleName => Constants.PluginName;
    public override string ModuleVersion => Constants.PluginVersion;
    public override string ModuleAuthor => Constants.PluginAuthor;

    private PluginCapability<IAPI> _apiCapability = new(Invites.API.Constants.APICapability);
    private LoggerAPIDelegate? _apiDelegate;

    public static IAPI? API { get; private set; }

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        API = _apiCapability.Get();

        _apiDelegate = new(this);
        API?.AddAPIDelegate(_apiDelegate);
    }

    public override void Unload(bool hotReload)
    {
        base.Unload(hotReload);

        if (_apiDelegate != null)
        {
            API?.RemoveAPIDelegate(_apiDelegate);
            _apiDelegate = null;
        }
    }
}
