using FlowFields.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public struct GenerateRandomLifeJob:IJobParallelFor
    {
        public NativeArray<MapTile> tiles;
        [ReadOnly] public Random random;
        public void Execute(int index)
        {
            var tile  = tiles[index];
            tile.isOccupied = (byte) (random.NextBool() ? 1 : 0);
            tiles[index] = tile;
        }
    }
}