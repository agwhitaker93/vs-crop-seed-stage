using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace CropSeedStage;

[HarmonyPatch]
public class CropSeedStageModSystem : ModSystem
{
    public static int MIN_SEEDS = 2;
    public static int MAX_SEED_MULTIPLIER = 4;
    public static string MOD_ID = "KoboldRanger.CropSeedStage";

    private static ILogger LOGGER;
    public static Random RANDOM = new Random();
    public Harmony harmony;

    // Called on server and client
    // Useful for registering block/entity classes on both sides
    // public override void Start(ICoreAPI api)
    // {
    //     Mod.Logger.Notification("Hello from template mod: " + api.Side);
    // }

    // public override void StartClientSide(ICoreClientAPI api)
    // {
    //     Mod.Logger.Notification(
    //         "Hello from template mod client side: " + Lang.Get("cropseedstage:hello")
    //     );
    // }

    private static void Notification(string toLog)
    {
        // LOGGER.Notification($"{MOD_ID}: {toLog}");
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        LOGGER = Mod.Logger;
        Notification("Hello from template mod server side: " + Lang.Get("cropseedstage:hello"));
        harmony = new Harmony(MOD_ID);
        Notification($"{MOD_ID}: Initializing");
        harmony.PatchAll();
        Notification($"{MOD_ID}: Should be done initializing now");
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
        Notification(
            $"{MOD_ID}: Broken crop of stage {cropStage}. Extra seed stage is {extraSeedStage}"
        );
        if (cropStage != extraSeedStage)
        {
            Notification($"{MOD_ID}: Crop is not in extra seed stage, leaving early");
            // exit early if it's not the second to last growth stage
            return;
        }

        ItemStack seedStack = null;
        for (int i = 0; i < __result.Length; i++)
        {
            ItemStack itemStack = __result[i];
            Notification(
                $"{MOD_ID}: Post BlockCrop.GetDrops[{i}] item: {itemStack.Item} quantity: {itemStack.StackSize}"
            );
            if (itemStack.Item != null && itemStack.Item is ItemPlantableSeed)
            {
                seedStack = itemStack;
            }
        }

        if (seedStack == null)
        {
            Notification(
                $"TODO {MOD_ID}: BlockCrop did not have an existing seed drop, adding one anyway"
            );
            foreach (BlockDropItemStack stack in __instance.Drops)
            {
                ItemStack resolvedItemStack = stack.ResolvedItemstack;
                Notification($"{MOD_ID}: Droppable stack: {resolvedItemStack.Item}");
                if (resolvedItemStack.Item != null && resolvedItemStack.Item is ItemPlantableSeed)
                {
                    Notification(
                        $"{MOD_ID}: Crop could have dropped a seed, attempting to add one manually"
                    );
                    seedStack = resolvedItemStack.Clone();
                    seedStack.StackSize = 1;
                    Notification($"{MOD_ID}: Attempting to create new array");
                    List<ItemStack> newList = new List<ItemStack>(__result);
                    newList.Add(seedStack);
                    Notification(
                        $"{MOD_ID}: Hoping reassigning __result doesn't just lose our reference and actually changes it"
                    );
                    __result = newList.ToArray();
                }
            }
        }

        int currentSeedDrop = seedStack.StackSize;
        int maxSeedDrop = currentSeedDrop * MAX_SEED_MULTIPLIER;
        // random.Next lower bound is inclusive, upper down is exclusive
        int newSeedDrop = RANDOM.Next(MIN_SEEDS, maxSeedDrop + 1);
        seedStack.StackSize = newSeedDrop;
        Notification($"{MOD_ID}: Increasing stack size from {currentSeedDrop} to {newSeedDrop}");
    }
}
