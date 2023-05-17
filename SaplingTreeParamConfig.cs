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
            size = 1;
            otherBlockChance = 1;
            vinesGrowthChance = 0.01f;
            mossGrowthChance = 0.02f;
        }
    }



}
