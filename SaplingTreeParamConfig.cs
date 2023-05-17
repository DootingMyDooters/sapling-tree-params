namespace SaplingTreeParams
{
    public class SaplingTreeParamConfig
    {
        public bool skipForestFloor;
        public float size;
        public float otherBlockChance;
        public float vinesGrowthChance;
        public float mossGrowthChance;
        public SaplingTreeParamConfig()
        {
            skipForestFloor = true;
            size = 0.5f;
            otherBlockChance = 1;
            vinesGrowthChance = 1;
            mossGrowthChance = 1;
        }

        public SaplingTreeParamConfig(bool skipForestFloor, float size, float otherBlockChance, float vinesGrowthChance, float mossGrowthChance)
        {
            this.skipForestFloor = skipForestFloor;
            this.size = size;
            this.otherBlockChance = otherBlockChance;
            this.vinesGrowthChance = vinesGrowthChance;
            this.mossGrowthChance = mossGrowthChance;
        }
    }



}
