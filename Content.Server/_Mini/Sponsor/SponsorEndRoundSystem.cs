using System;
using Content.Shared.GameTicking;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Robust.Shared.Configuration;
using System.Threading.Tasks;

public sealed class MyRoundEndSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    public override void Initialize()
    {
        base.Initialize();
        
        //SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd);
    }
    private void OnRoundEnd(RoundEndMessageEvent ev)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await LoadSponsorsToStaticList();
            }
            catch (Exception e)
            {
                Log.Error($"Ошибка в Task: {e}");
            }
        });
    }

    public async Task LoadSponsorsToStaticList()
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = _cfg.GetCVar<string>("database.pg_host"),
            Port = _cfg.GetCVar<int>("database.pg_port"),
            Database = _cfg.GetCVar<string>("database.pg_database"),
            Username = _cfg.GetCVar<string>("database.pg_username"),
            Password = _cfg.GetCVar<string>("database.pg_password")
        };

        var tempList = new List<SponsorInfoComponent.SponsorInfo>();

        try
        {
            await using var dataSource = NpgsqlDataSource.Create(builder.ConnectionString);
            await using var cmd = dataSource.CreateCommand(@"
            SELECT da.user_id, ds.sponsor_level
            FROM discord_sponsor ds
            JOIN discord_auth da ON ds.discord_id = da.discord_id");

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var userIdStr = reader.GetGuid(0).ToString();
                var level = reader.GetInt32(1);

                tempList.Add(new SponsorInfoComponent.SponsorInfo(userIdStr, level));
            }
            SponsorInfoComponent.listOfSponsors = tempList;
        }
        catch (Exception ex)
        {
            Log.Info($"Ошибка подключения к бд {ex.Message}");
        }
    }
}
