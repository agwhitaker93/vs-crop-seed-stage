﻿using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using HarmonyLib;
using System.Reflection;
using System;

namespace CropSeedStage;

[HarmonyPatch]
public class CropSeedStageModSystem : ModSystem
{
    public static int MAX_SEEDS = 4;
    public static string MOD_ID = "KoboldRanger.CropSeedStage";
    public static ILogger LOGGER;
    public static Random RANDOM = new Random();
    public Harmony harmony;

    public override void StartServerSide(ICoreServerAPI api)
    {
        LOGGER = api.Logger;
        harmony = new Harmony(MOD_ID);
        LOGGER.Notification($"{MOD_ID}: Initializing");
        harmony.PatchAll();
        LOGGER.Notification($"{MOD_ID}: Should be done initializing now");
    }

    public override void Dispose()
    {
        harmony.UnpatchAll(MOD_ID);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(BlockCrop), "GetDrops")]
    public static void PatchBlockCropGetDrops(BlockCrop __instance, ref ItemStack[] __result)
    {
        int cropStage = __instance.CurrentStage();
        int extraSeedStage = __instance.CropProps.GrowthStages - 1;
        LOGGER.Notification($"{MOD_ID}: Broken crop of stage {cropStage}. Extra seed stage is {extraSeedStage}");
        if (cropStage != extraSeedStage) {
            LOGGER.Notification($"{MOD_ID}: Crop is not in extra seed stage, leaving early");
            // exit early if it's not the second to last growth stage
            return;
        }

        ItemStack seedStack = null;
        for (int i = 0; i < __result.Length; i++) {
            ItemStack itemStack = __result[i];
            LOGGER.Notification($"{MOD_ID}: Post BlockCrop.GetDrops[{i}] item: {itemStack.Item} quantity: {itemStack.StackSize}");
            if (itemStack.Item != null && itemStack.Item is ItemPlantableSeed) {
                seedStack = itemStack;
            }
        }

        if (seedStack == null) {
            LOGGER.Notification($"TODO {MOD_ID}: BlockCrop did not have an existing seed drop, adding one anyway");
            ItemStack expectedSeedDrop = null;
            foreach (BlockDropItemStack stack in __instance.Drops)
            {
                ItemStack resolvedItemstack = stack.ResolvedItemstack;
                LOGGER.Notification($"{MOD_ID}: Droppable stack: {resolvedItemStack.Item}");
                if (resolvedItemstack is ItemPlantableSeed) {
                    expectedSeedDrop = resolvedItemstack.Clone();
                }
            }

            if (expectedSeedDrop != null) {

            }
        }

        int currentSeedDrop = seedStack.StackSize;
        int maxSeedDrop = currentSeedDrop * MAX_SEEDS;
        // random.Next lower bound is inclusive, upper down is exclusive
        int newSeedDrop = RANDOM.Next(currentSeedDrop, maxSeedDrop+1);
        seedStack.StackSize = newSeedDrop;
        LOGGER.Notification($"{MOD_ID}: Doubling stack size from {currentSeedDrop} to {newSeedDrop}");
    }
}
