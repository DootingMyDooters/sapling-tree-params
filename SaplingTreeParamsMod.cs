namespace SaplingTreeParams
{
    using System;
    using HarmonyLib;
    using Vintagestory.API.Common;
    using Vintagestory.API.Server;

    public class SaplingTreeParamsMod : ModSystem
    {
        ICoreAPI api;

        public string configFileName = "saplingtreeparam_config.json";

        public SaplingTreeParamConfig RCConfig
        {
            get
            {
                return (SaplingTreeParamConfig)this.api.ObjectCache[configFileName];
            }
            set
            {
                this.api.ObjectCache.Add(configFileName, value);
            }
        }

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true;
        }


        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            this.api = api;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            SaplingTreeParamConfig resinChanceConfig = null;
            try
            {
                resinChanceConfig = api.LoadModConfig<SaplingTreeParamConfig>(configFileName);
            }
            catch (System.Exception e)
            {

            }
            if (resinChanceConfig == null)
            {
                base.Mod.Logger.Warning("[" + Mod.Info.ModID + "]: config didn't load, generating default config.");
                api.StoreModConfig<SaplingTreeParamConfig>(new SaplingTreeParamConfig(true, 0.6f + (float)api.World.Rand.NextDouble() * 0.5f, 1, 1, 1 ), configFileName);
            }
            RCConfig = resinChanceConfig;

            // Apply patches with harmony
            var harmony = new Harmony(Mod.Info.ModID);
            harmony.PatchAll();
        }

    }
}