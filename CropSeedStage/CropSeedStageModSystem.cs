using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace CropSeedStage;

[HarmonyPatch]
public class CropSeedStageModSystem : ModSystem
{
    public static int MAX_SEED_MULTIPLIER = 4;
    public static string MOD_ID = "KoboldRanger.CropSeedStage";
    //public static ILogger LOGGER;
    public static Random RANDOM = new Random();
    public Harmony harmony;

    public override void StartServerSide(ICoreServerAPI api)
    {
        //LOGGER = api.Logger;
        harmony = new Harmony(MOD_ID);
        //LOGGER.Notification($"{MOD_ID}: Initializing");
        harmony.PatchAll();
        //LOGGER.Notification($"{MOD_ID}: Should be done initializing now");
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
        //LOGGER.Notification($"{MOD_ID}: Broken crop of stage {cropStage}. Extra seed stage is {extraSeedStage}");
        if (cropStage != extraSeedStage) {
            //LOGGER.Notification($"{MOD_ID}: Crop is not in extra seed stage, leaving early");
            // exit early if it's not the second to last growth stage
            return;
        }

        ItemStack seedStack = null;
        for (int i = 0; i < __result.Length; i++) {
            ItemStack itemStack = __result[i];
            //LOGGER.Notification($"{MOD_ID}: Post BlockCrop.GetDrops[{i}] item: {itemStack.Item} quantity: {itemStack.StackSize}");
            if (itemStack.Item != null && itemStack.Item is ItemPlantableSeed) {
                seedStack = itemStack;
            }
        }

        if (seedStack == null) {
            //LOGGER.Notification($"TODO {MOD_ID}: BlockCrop did not have an existing seed drop, adding one anyway");
            foreach (BlockDropItemStack stack in __instance.Drops)
            {
                ItemStack resolvedItemStack = stack.ResolvedItemstack;
                //LOGGER.Notification($"{MOD_ID}: Droppable stack: {resolvedItemStack.Item}");
                if (resolvedItemStack.Item != null && resolvedItemStack.Item is ItemPlantableSeed) {
                    //LOGGER.Notification($"{MOD_ID}: Crop could have dropped a seed, attempting to add one manually");
                    seedStack = resolvedItemStack.Clone();
                    seedStack.StackSize = 1;
                    //LOGGER.Notification($"{MOD_ID}: Attempting to create new array");
                    List<ItemStack> newList = new List<ItemStack>(__result);
                    newList.Add(seedStack);
                    //LOGGER.Notification($"{MOD_ID}: Hoping reassigning __result doesn't just lose our reference and actually changes it");
                    __result = newList.ToArray();
                }
            }
        }

        int currentSeedDrop = seedStack.StackSize;
        int maxSeedDrop = currentSeedDrop * MAX_SEED_MULTIPLIER;
        // random.Next lower bound is inclusive, upper down is exclusive
        int newSeedDrop = RANDOM.Next(currentSeedDrop, maxSeedDrop + 1);
        seedStack.StackSize = newSeedDrop;
        //LOGGER.Notification($"{MOD_ID}: Increasing stack size from {currentSeedDrop} to {newSeedDrop}");
    }
}
