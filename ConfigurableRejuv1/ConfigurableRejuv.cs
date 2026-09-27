using DeadworksManaged.Api;
using System.Text.Json;

namespace ConfigurableRejuv;

public class ConfigurableRejuv : DeadworksPluginBase
{
    public class CreditsConfig
    {
        public int First { get; set; }
        public int Rest { get; set; }
    }

    public override string Name => "Configurable Rejuv";
    public string DataPath = "managed/plugin_data/configurable_rejuv.json"; 
    public CreditsConfig Credits = new CreditsConfig { First = 2, Rest = 2 };


    public override void OnLoad(bool isReload)
    {

        if (File.Exists(DataPath))
        {
            CreditsConfig? readData = JsonSerializer.Deserialize<CreditsConfig>(File.ReadAllText(DataPath));

            if (readData != null)
                Credits = readData;
        }
        else
        {
            string jsonString = JsonSerializer.Serialize(Credits, new JsonSerializerOptions { WriteIndented = true });
            Directory.CreateDirectory(Path.GetDirectoryName(DataPath)!);
            File.WriteAllText(DataPath, jsonString);
        }

        Console.WriteLine($"[{Name}] Loaded! (reload={isReload})");
        Console.WriteLine($"Rejuvinator Credits: {Credits.First} first, {Credits.Rest} rest");
    }

    [Command("rejuv_credits", Description = "get/set the amount of rejuvinator credits avaliable to get", ServerOnly = true)]
    public void RejuvCreditsCommand(CCitadelPlayerController? caller, params string[] commandParts)
    {
        if (commandParts.Length == 0 || commandParts.Length > 3)
        {
            Console.WriteLine($"[{Name}] Rejuvinator Credits: {Credits.First} first, {Credits.Rest} rest");
            return;
        }

        if (commandParts.Length > 3)
            throw new CommandException("Too many args");

        if (!int.TryParse(commandParts[0], out int first))
            throw new CommandException("1st arg must be an integer");

        int rest = first;
        bool shouldSave = false;

        if (commandParts.Length >= 2)
        {
            if (int.TryParse(commandParts[1], out int parsedRest))
                rest = parsedRest;

            else if (commandParts.Length == 2 && bool.TryParse(commandParts[1], out bool parsedBool))
                shouldSave = parsedBool;

            else
                throw new CommandException("2nd arg must be an integer or boolean");
        }

        if (commandParts.Length == 3 && !bool.TryParse(commandParts[2], out shouldSave))
            throw new CommandException("3rd arg must be a boolean (true/false)");

        Credits = new CreditsConfig { First = first, Rest = rest };

        bool? saved = false;
        if (shouldSave)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(Credits, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(DataPath, jsonString);
                saved = true;
            }
            catch {
                saved = null;
            }
        }

        Console.WriteLine($"[{Name}] Set Rejuvinator Credits to: {Credits.First} first, {Credits.Rest} rest (Saved: {saved?.ToString() ?? "Failed"})");
    }

    private static readonly SchemaAccessor<nint> _punchPickupModifier = new("CCitadelItemPickupRejuvVData"u8, "m_PunchPickupModifier"u8);
    private static readonly SchemaAccessor<int> _bossKill01 = new("CCitadel_Modifier_ItemPunchable_RejuvVData"u8, "m_iRejuvBossKill01"u8);
    private static readonly SchemaAccessor<int> _bossKill02 = new("CCitadel_Modifier_ItemPunchable_RejuvVData"u8, "m_iRejuvBossKill02"u8);

    public override HookResult OnAddModifier(AddModifierEvent args)
    {
        if (!args.Caster.IsValid || !args.Caster.DesignerName.StartsWith("citadel_item_pickup_rejuv"))
            return HookResult.Continue;

        var pickupVData = args.Caster.SubclassVData;
        nint vdata = args.ModifierVData.Handle;
        if (pickupVData is null)
            return HookResult.Continue;

        nint punchVData = System.Runtime.InteropServices.Marshal.ReadIntPtr(_punchPickupModifier.GetAddress(pickupVData.Handle), 8);
        if (vdata != punchVData)
            return HookResult.Continue;

        _bossKill01.Set(vdata, Credits.First);
        _bossKill02.Set(vdata, Credits.Rest);

        return HookResult.Continue;
    }
}