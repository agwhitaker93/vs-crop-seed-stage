using System;
using Vintagestory.API.Common;

namespace CropSeedStage;

public class CropSeedStageConfig
{
    public int MinimumSeeds = 2;
    public int MaximumSeedMultiplier = 4;
    public bool DebugLogging = false;

    public static CropSeedStageConfig TryLoadConfig(ILogger logger, ICoreAPI api)
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
        TrySaveConfig(logger, api, config);
        return config;
    }

    public static void TrySaveConfig(ILogger logger, ICoreAPI api, CropSeedStageConfig config)
    {
        try
        {
            api.StoreModConfig(config, "CropSeedStageConfig.json");
        }
        catch (Exception e)
        {
            logger.Error("Could not save config! Uh oh!");
            logger.Error(e);
        }
    }

}
