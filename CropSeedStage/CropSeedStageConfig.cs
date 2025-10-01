using System;
using Vintagestory.API.Common;

namespace CropSeedStage;

public class CropSeedStageConfig
{
    public int MinimumSeeds = 2;
    public int MaximumSeeds = 4;
    public bool DebugLogging = false;

    private static ILogger logger;
    private static ICoreAPI api;

    public static void Setup(ILogger logger, ICoreAPI api)
    {
        CropSeedStageConfig.logger = logger;
        CropSeedStageConfig.api = api;
    }

    public static CropSeedStageConfig TryLoadConfig()
    {
        CropSeedStageConfig config;
        try
        {
            config = api.LoadModConfig<CropSeedStageConfig>("CropSeedStageConfig.json");
            config ??= new CropSeedStageConfig();
        }
        catch (Exception e)
        {
            logger.Error("Could not load config! Loading default settings instead.");
            logger.Error(e);
            config = new CropSeedStageConfig();
        }
        config.TrySave();
        return config;
    }

    public void TrySave()
    {
        try
        {
            api.StoreModConfig(this, "CropSeedStageConfig.json");
        }
        catch (Exception e)
        {
            logger.Error("Could not save config! Uh oh!");
            logger.Error(e);
        }
    }
}
