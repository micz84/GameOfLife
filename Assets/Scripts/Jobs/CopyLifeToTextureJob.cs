using FlowFields.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Jobs
{
    [BurstCompile]
    public struct CopyLifeToTextureJob:IJobParallelFor
    {
        [ReadOnly] public NativeArray<MapTile> Data;
        public NativeArray<Color> Pixels;
        public void Execute(int index)
        {
            var occupied = Data[index].isOccupied;
            Pixels[index] =  new Color(occupied,occupied,occupied,1);
        }
    }

    [BurstCompile]
    public struct CopyLifeToTextureSmallJob:IJobParallelFor
    {
        [ReadOnly] public NativeArray<MapTile> Data;
        public NativeArray<Color> Pixels;
        [ReadOnly] public int TextureWidth;
        [ReadOnly] public int MapWidth;
        [ReadOnly] public int2 Position;
        [ReadOnly] public float scaleFactor;
        public void Execute(int index)
        {
            var dataIndex = index % TextureWidth + Position.y * MapWidth + Position.x + index / TextureWidth * MapWidth;
            var occupied = Data[dataIndex].isOccupied;
            Pixels[index] =  new Color(occupied,occupied,occupied,1);
        }
    }
}