using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using Invites.API;

namespace CommandReward;

public class Constants
{
    public const string PluginName = "Command Reward [Module]";
    public const string PluginVersion = "1.0.0";
    public const string PluginAuthor = "Xummuk97";

    public const string RewardId = "Command";
}

public class RewardDelegate : IRewardDelegate
{
    public void OnGive(CCSPlayerController controller, string? @params)
    {
        var command = @params ?? "";

        command = command.Replace("{userid}", controller.UserId.ToString());
        command = command.Replace("{name}", controller.PlayerName);
        command = command.Replace("{steamid2}", controller.AuthorizedSteamID?.SteamId2);
        command = command.Replace("{steamid3}", controller.AuthorizedSteamID?.SteamId3);
        command = command.Replace("{steamid32}", controller.AuthorizedSteamID?.SteamId32.ToString());
        command = command.Replace("{steamid64}", controller.AuthorizedSteamID?.SteamId64.ToString());

        if (!string.IsNullOrEmpty(command))
        {
            Server.ExecuteCommand(command);
        }
    }
}

public class CommandRewardAPIDelegate : APIDelegate
{
    public CommandRewardAPIDelegate(BasePlugin basePlugin) : base(basePlugin)
    {
    }

    public override void OnCoreLoaded()
    {
        base.OnCoreLoaded();

        CommandReward.API?.AddReward(Constants.RewardId, new RewardDelegate());
    }
}

public class CommandReward : BasePlugin
{
    public override string ModuleName => Constants.PluginName;
    public override string ModuleVersion => Constants.PluginVersion;
    public override string ModuleAuthor => Constants.PluginAuthor;

    private PluginCapability<IAPI> _apiCapability = new(Invites.API.Constants.APICapability);
    private CommandRewardAPIDelegate? _apiDelegate;

    public static IAPI? API { get; private set; }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        base.OnAllPluginsLoaded(hotReload);

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
