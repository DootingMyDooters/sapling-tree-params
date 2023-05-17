using System;
using System.Reflection;
using HarmonyLib;
using Vintagestory.GameContent;
using Vintagestory.API;
using Vintagestory.API.MathTools;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace SaplingTreeParams
{

    [HarmonyPatch(typeof(BlockEntitySapling), "CheckGrow")]
    internal class Sapling
    {

        

        static void Prefix(float dt, ref BlockEntitySapling __instance)
        {
            // Access private fields not normally available through reflection
            Type typ = typeof(BlockEntitySapling);

            // Retrieve private field totalHoursTillGrowth
            FieldInfo fieldTotalHoursTillGrowth = typ.GetField("totalHoursTillGrowth", BindingFlags.NonPublic | BindingFlags.Instance);
            double totalHoursTillGrowth = (double)fieldTotalHoursTillGrowth.GetValue(__instance);

            // Retrieve private field stage
            FieldInfo fieldStage = typ.GetField("stage", BindingFlags.NonPublic | BindingFlags.Instance);
            EnumTreeGrowthStage stage = (EnumTreeGrowthStage)fieldStage.GetValue(__instance);

            // Retrieve private field growListenerId
            FieldInfo fieldGrowListenerId = typ.GetField("growListenerId", BindingFlags.NonPublic | BindingFlags.Instance);
            long growListenerId = (long)fieldGrowListenerId.GetValue(__instance);

            // Retrieve property GrowthRateMod
            PropertyInfo propGrowthRateMod = typ.GetProperty("GrowthRateMod", BindingFlags.NonPublic | BindingFlags.Instance);
            float growthRateMod = (float)propGrowthRateMod.GetValue(__instance);

            // Retrieve property nextStageDaysRnd
            PropertyInfo propNextStageDaysRnd = typ.GetProperty("nextStageDaysRnd", BindingFlags.NonPublic | BindingFlags.Instance);
            NatFloat nextStageDaysRnd = (NatFloat)propNextStageDaysRnd.GetValue(__instance);


            // Original code
            if (__instance.Api.World.Calendar.TotalHours < totalHoursTillGrowth)
                return;

            ClimateCondition conds = __instance.Api.World.BlockAccessor.GetClimateAt(__instance.Pos, EnumGetClimateMode.NowValues);
            if (conds == null || conds.Temperature < 5)
            {
                return;
            }

            if (conds.Temperature < 0)
            {
                fieldTotalHoursTillGrowth.SetValue(__instance, __instance.Api.World.Calendar.TotalHours + (float)__instance.Api.World.Rand.NextDouble() * 72 * growthRateMod);
                return;
            }

            if (stage == EnumTreeGrowthStage.Seed)
            {
                fieldStage.SetValue(__instance, EnumTreeGrowthStage.Sapling);
                fieldTotalHoursTillGrowth.SetValue(__instance, __instance.Api.World.Calendar.TotalHours + nextStageDaysRnd.nextFloat(1, __instance.Api.World.Rand) * 24 * growthRateMod);
                __instance.MarkDirty(true);
                return;
            }

            int chunksize = __instance.Api.World.BlockAccessor.ChunkSize;
            foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
            {
                Vec3i dir = facing.Normali;
                int x = __instance.Pos.X + dir.X * chunksize;
                int z = __instance.Pos.Z + dir.Z * chunksize;

                // Not at world edge and chunk is not loaded? We must be at the edge of loaded chunks. Wait until more chunks are generated
                if (__instance.Api.World.BlockAccessor.IsValidPos(x, __instance.Pos.Y, z) && __instance.Api.World.BlockAccessor.GetChunkAtBlockPos(x, __instance.Pos.Y, z) == null)
                    return;
            }

            Block block = __instance.Api.World.BlockAccessor.GetBlock(__instance.Pos);
            string treeGenCode = block.Attributes?["treeGen"].AsString(null);

            if (treeGenCode == null)
            {
                __instance.Api.Event.UnregisterGameTickListener(growListenerId);
                return;
            }

            AssetLocation code = new AssetLocation(treeGenCode);
            ICoreServerAPI sapi = __instance.Api as ICoreServerAPI;

            ITreeGenerator gen;
            if (!sapi.World.TreeGenerators.TryGetValue(code, out gen))
            {
                __instance.Api.Event.UnregisterGameTickListener(growListenerId);
                return;
            }

            SaplingTreeParamConfig rcc = sapi.LoadModConfig<SaplingTreeParamConfig>("resinchance_config.json");

            __instance.Api.World.BlockAccessor.SetBlock(0, __instance.Pos);
            __instance.Api.World.BulkBlockAccessor.ReadFromStagedByDefault = true;
            float size = 0.6f + (float)__instance.Api.World.Rand.NextDouble() * 0.5f;

            TreeGenParams pa = new TreeGenParams()
            {
                skipForestFloor = rcc.skipForestFloor,
                size = 0.6f + rcc.size * 0.5f,
                otherBlockChance = rcc.otherBlockChance,
                vinesGrowthChance = rcc.vinesGrowthChance,
                mossGrowthChance = rcc.mossGrowthChance
            };

            sapi.World.TreeGenerators[code].GrowTree(__instance.Api.World.BulkBlockAccessor, __instance.Pos.DownCopy(), pa);
            __instance.Api.World.BulkBlockAccessor.Commit();

            return;
        }
    }

}
