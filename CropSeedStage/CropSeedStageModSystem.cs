using System;
using System.Collections.Generic;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace CropSeedStage;

[HarmonyPatch]
public class CropSeedStageModSystem : ModSystem
{
    public static readonly string MOD_ID = "KoboldRanger.CropSeedStage";
    private static readonly Random RANDOM = new();
    private static Harmony harmony;
    private static ILogger Logger;
    private static CropSeedStageConfig config;

    public override void StartServerSide(ICoreServerAPI api)
    {
        Logger = Mod.Logger;

        CropSeedStageConfig.Setup(Logger, api);
        config = CropSeedStageConfig.TryLoadConfig();

        Debug($"Loaded config. MinSeeds: {config.MinimumSeeds}, MaxMult: {config.MaximumSeeds}, Debug: {config.DebugLogging}");

        harmony = new Harmony(MOD_ID);
        Debug("Initializing");
        harmony.PatchAll();
        Debug("Should be done initializing now");
    }

    public override void Dispose()
    {
        harmony.UnpatchAll(MOD_ID);
    }

    private static void Debug(String toLog)
    {
        if (config.DebugLogging)
        {
            Logger.Notification(toLog);
        }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BlockCrop), "GetDrops")]
    public static void PatchBlockCropGetDrops(BlockCrop __instance, ref ItemStack[] __result)
    {
        int cropStage = __instance.CurrentStage();
        int extraSeedStage = __instance.CropProps.GrowthStages - 1;
        Debug(
            $"Broken crop of stage {cropStage}. Extra seed stage is {extraSeedStage}"
        );
        if (cropStage != extraSeedStage)
        {
            Debug("Crop is not in extra seed stage, leaving early");
            // exit early if it's not the second to last growth stage
            return;
        }

        ItemStack seedStack = null;
        for (int i = 0; i < __result.Length; i++)
        {
            ItemStack itemStack = __result[i];
            Debug(
                $"Post BlockCrop.GetDrops[{i}] item: {itemStack.Item} quantity: {itemStack.StackSize}"
            );
            if (itemStack.Item != null && itemStack.Item is ItemPlantableSeed)
            {
                seedStack = itemStack;
            }
        }

        if (seedStack == null)
        {
            Debug("BlockCrop did not have an existing seed drop, adding one anyway");
            foreach (BlockDropItemStack stack in __instance.Drops)
            {
                ItemStack resolvedItemStack = stack.ResolvedItemstack;
                Debug($"Droppable stack: {resolvedItemStack.Item}");
                if (resolvedItemStack.Item != null && resolvedItemStack.Item is ItemPlantableSeed)
                {
                    Debug(
                        "Crop could have dropped a seed, attempting to add one manually"
                    );
                    seedStack = resolvedItemStack.Clone();
                    seedStack.StackSize = 1;
                    Debug("Attempting to create new array");
                    List<ItemStack> newList = [.. __result, seedStack];
                    Debug(
                        "Hoping reassigning __result doesn't just lose our reference and actually changes it"
                    );
                    __result = newList.ToArray();
                }
            }
        }

        int currentSeedDrop = seedStack.StackSize;
        // random.Next lower bound is inclusive, upper bound is exclusive
        int newSeedDrop = RANDOM.Next(currentSeedDrop * config.MinimumSeeds, currentSeedDrop * config.MaximumSeeds + 1);
        seedStack.StackSize = newSeedDrop;
        Debug($"Increasing stack size from {currentSeedDrop} to {newSeedDrop}");
    }
}
