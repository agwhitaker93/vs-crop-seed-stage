using Vintagestory.API.Client;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using HarmonyLib;
using System.Reflection;

namespace CropSeedStage;

[HarmonyPatch]
public class CropSeedStageModSystem : ModSystem
{
    public static string MOD_ID = "KoboldRanger.CropSeedStage";
    //public static ILogger LOGGER;
    public Harmony harmony;

    public override void StartServerSide(ICoreServerAPI api)
    {
        //LOGGER = api.Logger;
        harmony = new Harmony(MOD_ID);
        //LOGGER.Notification($"{MOD_ID}: Initializing");
        harmony.PatchAll();
        //LOGGER.Notification($"{MOD_ID}: Should be done initializing now");
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
        for (int i = 0; i < __result.Length; i++) {
            ItemStack itemStack = __result[i];
            //LOGGER.Notification($"{MOD_ID}: Post BlockCrop.GetDrops[{i}] item: {itemStack.Item} quantity: {itemStack.StackSize}");
            if (itemStack.Item != null && itemStack.Item is ItemPlantableSeed) {
                //LOGGER.Notification($"{MOD_ID}: Doubling stack size from {itemStack.StackSize} to {itemStack.StackSize * 2}");
                itemStack.StackSize = itemStack.StackSize * 2;
            }
        }
    }
}
