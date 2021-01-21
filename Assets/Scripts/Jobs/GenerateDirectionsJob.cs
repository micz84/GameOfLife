using FlowFields.Data;
using Unity.Burst;
using Unity.Jobs;

namespace Jobs
{
    [BurstCompile]
    public struct GenerateDirectionsJob : IJobParallelFor
    {
        public MapData Map;

        public void Execute(int index)
        {
            var l = 8;
            byte code = 0;
            byte one = 1;
            for (var i = 0; i < l; i++)
            {
                var moveDir = Map.mapDataBlob.Value.MoveDirectionIndexOffset[i];
                var newTile = index + moveDir;
                if (Map.IsInsideMap(newTile)) code += (byte) (one << i);
            }

            var tile = Map.tiles[index];
            tile.availableDirectionsCode = code;
            Map.tiles[index] = tile;
        }
    }
}