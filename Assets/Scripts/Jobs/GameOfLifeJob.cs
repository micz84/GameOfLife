using FlowFields.Components;
using FlowFields.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Jobs
{
    [BurstCompile]
    public struct GameOfLifeJob:IJobParallelForBatch
    {

        [ReadOnly] public NativeArray<MapTile> Tiles;
        [ReadOnly] public NativeArray<int> MoveDirectionIndexOffset;
        [WriteOnly] public NativeArray<byte> State;
        public unsafe void Execute(int startIndex, int count)
        {
            const byte one = 1;
            int shiftLeft = startIndex == 0 ? 0 : count;
            int shiftRight = startIndex >= Tiles.Length - count - 1 ? 0 : count;

            var tilesCacheSize = count + shiftLeft + shiftRight;
            MapTile* tiles = stackalloc MapTile[tilesCacheSize];
            UnsafeUtility.MemCpy(tiles , ((MapTile*) Tiles.GetUnsafeReadOnlyPtr()) - shiftLeft + startIndex, MapTile.ByteSize*tilesCacheSize);
            byte* stateCache = stackalloc byte[count];

            for (var j = 0; j < count; j++)
            {
                var index = shiftLeft + j;
                ref var tile = ref tiles[index];
                //var index = startIndex + j;
                //var tile = Tiles[index];
                int code = (tile.availableDirectionsCode);

                byte aliveNeighbours = 0;
                for (var i = 0; i < 8; i++)
                {
                    var move = code >> i;
                    var valid = (byte) (move & one);
                    var direction = valid * MoveDirectionIndexOffset[i];
                    var newTile = tiles[index + direction];
                    //var newTile = Tiles[index + direction];
                    aliveNeighbours += (byte)(newTile.isOccupied * valid);
                }
                stateCache[j] = (byte)math.select(0, 1, (tile.isOccupied == 1 && aliveNeighbours == 2) || aliveNeighbours == 3);
            }
            UnsafeUtility.MemCpy(((byte*)State.GetUnsafePtr()) + startIndex, stateCache, count);


        }

    }
    [BurstCompile]
    public struct GameOfLifeBatchJob:IJobParallelForBatch
    {

        [ReadOnly] public NativeArray<MapTile> Tiles;
        [ReadOnly] public BlobAssetReference<MapDataBlob> MapBlob;
        [WriteOnly] public NativeArray<byte> State;
        public unsafe void Execute(int startIndex, int count)
        {
            byte* stateCache = stackalloc byte[count];

            for (int j = 0; j < count; j++)
            {
                var tile = Tiles[startIndex+j];
                var code = (byte) (tile.availableDirectionsCode ^ 255);
                var directionsStartEnd = MapBlob.Value.CodeStartEndIndex[code];
                byte aliveNeighbours = 0;
                for (int i = directionsStartEnd.startIndex; i < directionsStartEnd.endIndex; i++)
                {
                    var direction = MapBlob.Value.DirectionData[i];
                    var newTile = Tiles[startIndex + j + direction.IndexOffset];
                    aliveNeighbours += newTile.isOccupied;
                }
                stateCache[j] = (byte)math.select(0, 1, (tile.isOccupied == 1 && aliveNeighbours == 2) || aliveNeighbours == 3);
            }
            UnsafeUtility.MemCpy((byte*)State.GetUnsafePtr() + startIndex, stateCache, count);


        }

    }
    [BurstCompile]
    public struct CopyStateJob:IJobParallelFor
    {

        public NativeArray<MapTile> Tiles;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<byte> State;
        public void Execute(int index)
        {
            var tile = Tiles[index];
            tile.isOccupied = State[index];
            Tiles[index] = tile;
        }
    }
}