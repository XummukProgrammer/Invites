using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Commands;
using Invites.API;

namespace InvitesCore;

public class Constants
{
    public const string PluginName = "Invites [Core]";
    public const string PluginVersion = "1.0.0";
    public const string PluginAuthor = "Xummuk97";
}

public class RewardsManager
{
    private Dictionary<string, IRewardDelegate> _delegates = new();

    public void AddDelegate(string id, IRewardDelegate @delegate)
    {
        _delegates.Add(id, @delegate);
    }

    public IRewardDelegate? GetDelegate(string id)
    {
        if (_delegates.TryGetValue(id, out var @delegate))
        {
            return @delegate;
        }
        return null;
    }
}

public class InvitesManager
{
    private Dictionary<string, string> _invites = new();
    private string _idPattern = "";

    public InvitesManager()
    {
        for (char i = 'a'; i <= 'z'; i++)
        {
            _idPattern += i;
        }

        for (char i = 'A'; i <= 'Z'; i++)
        {
            _idPattern += i;
        }

        for (char i = '0'; i <= '9'; i++)
        {
            _idPattern += i;
        }
    }

    public void Add(string id, string rewardId)
    {
        _invites.Add(id, rewardId);
    }

    public void Remove(string id)
    {
        _invites.Remove(id);
    }

    public string? GetRewardId(string id)
    {
        if (_invites.TryGetValue(id, out var rewardId))
        {
            return rewardId;
        }
        return null;
    }

    public string? Generate(string rewardId, int count = 20)
    {
        var id = GenerateId(count);
        if (id != null)
        {
            Add(id, rewardId);
        }
        return id;
    }

    public void Apply(CCSPlayerController? controller, string id)
    {
        if (controller == null)
        {
            return;
        }

        var rewardId = GetRewardId(id);
        if (rewardId != null)
        {
            var @delegate = Managers.Rewards.GetDelegate(rewardId);
            @delegate?.OnGive(controller);

            Remove(id);
        }
    }

    private string GenerateId(int count)
    {
        string id = "";
        var random = new Random();

        for (int i = 0; i < count; i++)
        {
            id += _idPattern[random.Next(0, _idPattern.Length)];
        }

        return id;
    }
}

public static class Managers
{
    public static RewardsManager Rewards { get; private set; } = new();
    public static InvitesManager Invites { get; private set; } = new();
}

public class API : IAPI
{
    public void AddReward(string id, IRewardDelegate @delegate)
    {
        Managers.Rewards.AddDelegate(id, @delegate);
    }

    public void ApplyInvite(CCSPlayerController? controller, string inviteId)
    {
        Managers.Invites.Apply(controller, inviteId);
    }

    public string? GenerateInvite(string rewardId)
    {
        return Managers.Invites.Generate(rewardId);
    }
}

public class InvitesCore : BasePlugin
{
    public override string ModuleName => Constants.PluginName;
    public override string ModuleVersion => Constants.PluginVersion;
    public override string ModuleAuthor => Constants.PluginAuthor;

    private PluginCapability<IAPI> _apiCapability = new(Invites.API.Constants.APICapability);

    public static IAPI? API { get; private set; }

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        Capabilities.RegisterPluginCapability(_apiCapability, () => new API());
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        base.OnAllPluginsLoaded(hotReload);

        API = _apiCapability.Get();
    }

    [ConsoleCommand("css_invite_generate", "")]
    [CommandHelper(minArgs: 1, usage: "[rewardid]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void OnInviteGenerateCommandHandler(CCSPlayerController? controller, CommandInfo command)
    {
        var rewardId = command.ArgByIndex(1);

        var inviteId = API?.GenerateInvite(rewardId);

        command.ReplyToCommand($"A new invite has been generated: {inviteId}.");
    }

    [ConsoleCommand("css_invite_apply", "")]
    [CommandHelper(minArgs: 1, usage: "[id]", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void OnInviteApplyCommandHandler(CCSPlayerController? controller, CommandInfo command)
    {
        var id = command.ArgByIndex(1);

        API?.ApplyInvite(controller, id);
    }
}
