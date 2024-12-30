using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Dapper;
using Invites.API;
using Microsoft.Data.Sqlite;
using System.Text.Json.Serialization;

namespace InvitesCore;

public class Constants
{
    public const string PluginName = "Invites [Core]";
    public const string PluginVersion = "1.0.0";
    public const string PluginAuthor = "Xummuk97";
}

public class APIDelegatesManager
{
    private List<APIDelegate> _delegates = new();

    public void Add(APIDelegate @delegate)
    {
        _delegates.Add(@delegate);
    }

    public void Remove(APIDelegate @delegate)
    {
        _delegates.Remove(@delegate);
    }

    public void OnCoreLoaded()
    {
        foreach (var @delegate in _delegates)
        {
            @delegate.OnCoreLoaded();
        }
    }

    public void OnCorePostLoaded()
    {
        foreach (var @delegate in _delegates)
        {
            @delegate.OnCorePostLoaded();
        }
    }

    public void OnRewardAdded(string rewardId)
    {
        foreach (var @delegate in _delegates)
        {
            @delegate.OnRewardAdded(rewardId);
        }
    }

    public void OnRewardGived(string rewardId, CCSPlayerController controller)
    {
        foreach (var @delegate in _delegates)
        {
            @delegate.OnRewardGived(rewardId, controller);
        }
    }

    public void OnInviteGenerated(string inviteId, string packId)
    {
        foreach (var @delegate in _delegates)
        {
            @delegate.OnInviteGenerated(inviteId, packId);
        }
    }

    public void OnInviteApplied(string inviteId, CCSPlayerController controller)
    {
        foreach (var @delegate in _delegates)
        {
            @delegate.OnInviteApplied(inviteId, controller);
        }
    }
}

public class RewardsManager
{
    private Dictionary<string, IRewardDelegate> _delegates = new();

    public void AddDelegate(string id, IRewardDelegate @delegate)
    {
        _delegates.Add(id, @delegate);

        Managers.Delegates.OnRewardAdded(id);
    }

    public IRewardDelegate? GetDelegate(string id)
    {
        if (_delegates.TryGetValue(id, out var @delegate))
        {
            return @delegate;
        }
        return null;
    }

    public void OnGive(string id, string? @params, CCSPlayerController? controller)
    {
        if (controller == null)
        {
            return;
        }

        var @delegate = GetDelegate(id);
        @delegate?.OnGive(controller, @params);

        Managers.Delegates.OnRewardGived(id, controller);
    }
}

public class PacksManager
{
    private ConfigModel? _config;

    public void SetConfig(ConfigModel? config)
    {
        _config = config;
    }

    public void Give(string id, CCSPlayerController? controller)
    {
        if (controller == null)
        {
            return;
        }

        var pack = _config?.Packs?.Find(pack => pack.ID == id);
        if (pack != null)
        {
            var rewards = pack.Rewards;

            if (rewards != null)
            {
                foreach (var reward in rewards)
                {
                    Managers.Rewards.OnGive(reward.ID ?? "", reward.Params, controller);
                }
            }
        }
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

    public void Add(string id, string packId, bool db = true)
    {
        _invites.Add(id, packId);

        if (db)
        {
            Managers.Database.AddInvite(id, packId);
        }
    }

    public void Remove(string id)
    {
        _invites.Remove(id);

        Managers.Database.RemoveInvite(id);
    }

    public string? GetPackId(string id)
    {
        if (_invites.TryGetValue(id, out var rewardId))
        {
            return rewardId;
        }
        return null;
    }

    public string? Generate(string packId, int count = 20)
    {
        var id = GenerateId(count);
        if (id != null)
        {
            Add(id, packId);

            Managers.Delegates.OnInviteGenerated(id, packId);
        }
        return id;
    }

    public void Apply(CCSPlayerController? controller, string id)
    {
        if (controller == null)
        {
            return;
        }

        var packId = GetPackId(id);
        if (packId != null)
        {
            Managers.Packs.Give(packId, controller);
            Remove(id);

            Managers.Delegates.OnInviteApplied(id, controller);
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

public class DatabaseManager
{
    private SqliteConnection _connection = null!;

    public void Load(string moduleDirectory)
    {
        _connection = new SqliteConnection($"Data Source={Path.Join(moduleDirectory, "database.db")}");
        _connection.Open();

        Task.Run(async () =>
        {
            await _connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS `Invites` (
                    `Id` INTEGER PRIMARY KEY AUTOINCREMENT,
	                `Key` VARCHAR(64),
	                `Pack` VARCHAR(64));");

            var invites = await _connection.QueryAsync<InviteDBModel>(@"SELECT * FROM `Invites`;");
            foreach (var invite in invites)
            {
                Managers.Invites.Add(invite.Key, invite.Pack, false);
            }

            Managers.Delegates.OnCoreLoaded();
            Managers.Delegates.OnCorePostLoaded();
        });
    }

    public void AddInvite(string id, string packId)
    {
        Task.Run(async () =>
        {
            await _connection.ExecuteAsync(
                $@"INSERT INTO `Invites` (`Key`, `Pack`) VALUES ('{id}', '{packId}');"
            );
        });
    }

    public void RemoveInvite(string id)
    {
        Task.Run(async () =>
        {
            await _connection.ExecuteAsync($@"DELETE FROM `Invites` WHERE `Key` = '{id}';");
        });
    }
}

public static class Managers
{
    public static APIDelegatesManager Delegates { get; private set; } = new();
    public static RewardsManager Rewards { get; private set; } = new();
    public static PacksManager Packs { get; private set; } = new();
    public static InvitesManager Invites { get; private set; } = new();
    public static DatabaseManager Database {  get; private set; } = new();
}

public class API : IAPI
{
    public void AddAPIDelegate(APIDelegate @delegate)
    {
        Managers.Delegates.Add(@delegate);
    }

    public void RemoveAPIDelegate(APIDelegate @delegate)
    {
        Managers.Delegates.Remove(@delegate);
    }

    public void AddReward(string id, IRewardDelegate @delegate)
    {
        Managers.Rewards.AddDelegate(id, @delegate);
    }

    public void ApplyInvite(CCSPlayerController? controller, string inviteId)
    {
        Managers.Invites.Apply(controller, inviteId);
    }

    public string? GenerateInvite(string packId)
    {
        return Managers.Invites.Generate(packId);
    }
}

public class RewardModel
{
    [JsonPropertyName("ID")] public string? ID { get; set; }
    [JsonPropertyName("Params")] public string? Params { get; set; }
}

public class PackModel
{
    [JsonPropertyName("ID")] public string? ID { get; set; }
    [JsonPropertyName("Rewards")] public List<RewardModel>? Rewards { get; set; }
}

public class ConfigModel : BasePluginConfig
{
    [JsonPropertyName("Packs")] public List<PackModel>? Packs { get; set; } = 
        [
            new PackModel 
            { 
                ID = "TestPack", Rewards = 
                    [
                        new RewardModel
                        {
                            ID = "Command", Params = "params"
                        },
                        new RewardModel
                        {
                            ID = "Kick"
                        }
                    ] 
            }
        ];
}

public class InviteDBModel
{
    public ulong Id { get; set; }
    public string Key { get; set; }
    public string Pack { get; set; }
}

public class InvitesCore : BasePlugin, IPluginConfig<ConfigModel>
{
    public override string ModuleName => Constants.PluginName;
    public override string ModuleVersion => Constants.PluginVersion;
    public override string ModuleAuthor => Constants.PluginAuthor;

    private PluginCapability<IAPI> _apiCapability = new(Invites.API.Constants.APICapability);

    public static IAPI? API { get; private set; }
    public ConfigModel Config { get; set; }

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);

        Capabilities.RegisterPluginCapability(_apiCapability, () => new API());
        API = _apiCapability.Get();
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        base.OnAllPluginsLoaded(hotReload);

        Managers.Database.Load(ModuleDirectory);
    }

    public void OnConfigParsed(ConfigModel config)
    {
        Config = config;

        Managers.Packs.SetConfig(config);
    }

    [ConsoleCommand("css_invite_generate", "")]
    [CommandHelper(minArgs: 1, usage: "[packid]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    [RequiresPermissions("@css/root")]
    public void OnInviteGenerateCommandHandler(CCSPlayerController? controller, CommandInfo command)
    {
        var packId = command.ArgByIndex(1);

        var inviteId = API?.GenerateInvite(packId);

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
