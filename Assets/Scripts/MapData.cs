using System;
using System.Runtime.CompilerServices;
using FlowFields.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace FlowFields.Data
{
    public struct MapData : IDisposable
    {
        public bool IsValid => tiles.IsCreated;
        public NativeArray<MapTile> tiles;
        public readonly int width;
        public readonly int height;

        public BlobAssetReference<MapDataBlob> mapDataBlob;

        public MapData(int width, int height)
        {
            this.width = width;
            this.height = height;
            tiles = new NativeArray<MapTile>(width * height, Allocator.Persistent);
            mapDataBlob = MapDataBlobFactory.Generate(width);
        }

        public MapTile this[int2 index] => tiles[index.x + index.y * width];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsInsideMap(int2 tile)
        {
            if (tile.x < 0 || tile.x >= width)
                return false;

            return tile.y >= 0 && tile.y < height;
        }
        public bool IsInsideMap(int index)
        {
            return index >= 0 && index < tiles.Length;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMapTile(int2 position, MapTile tile)
        {
            tiles[position.x + position.y * width] = tile;
        }

        public void Dispose()
        {
            tiles.Dispose();
            mapDataBlob.Dispose();
        }
    }
}