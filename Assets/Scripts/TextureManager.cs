using System;
using FlowFields.Data;
using Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class TextureManager : MonoBehaviour
{
    [SerializeField] private RenderTexture _GameOfLifeTexture = null;
    [SerializeField] private int _MapSize = 10;
    [Range(-3,3)]
    [SerializeField] private int _Scale = 1;
    [FormerlySerializedAs("_Precalculated")] [SerializeField]
    private bool _Batched = false;
    [SerializeField] private int2 _Position = int2.zero;
    private NativeArray<float> _Pixels;
    private NativeArray<int> _MoveDirectionIndexOffset;
    private NativeArray<Color> _ColorsNative;
    private Color[] _Colors;
    private MapData _Map;
    private Texture2D _Tex = null;
    private JobHandle _Handle;

    private Unity.Mathematics.Random _Random;
    private int _TextureWidth;
    [SerializeField]
    [Range(1,20)]
    private int _Batch = 6;

    void Start()
    {
        if (_GameOfLifeTexture == null) return;

        _TextureWidth = _GameOfLifeTexture.width;
        _Pixels = new NativeArray<float>(_TextureWidth * _TextureWidth, Allocator.Persistent);
        _ColorsNative = new NativeArray<Color>(_TextureWidth * _TextureWidth, Allocator.Persistent);
        _Colors = new Color[_Pixels.Length];
        _Tex = new Texture2D(_GameOfLifeTexture.width, _GameOfLifeTexture.height, TextureFormat.RGBA32, false);
        for (int x = 0; x < _Pixels.Length; x++)
            _Pixels[x] = Random.Range(0, 1f);
        var size = (int) math.pow(2,_MapSize);
        _Random = new Unity.Mathematics.Random((uint)(DateTime.Now.Ticks));
        _Map = new MapData(size, size);
        _MoveDirectionIndexOffset = new NativeArray<int>(8, Allocator.Persistent);
        _MoveDirectionIndexOffset[0] = size;
        _MoveDirectionIndexOffset[1] = 1 + size;
        _MoveDirectionIndexOffset[2] = 1;
        _MoveDirectionIndexOffset[3] = 1 - size;
        _MoveDirectionIndexOffset[4] = -size;
        _MoveDirectionIndexOffset[5] = -1 - size;
        _MoveDirectionIndexOffset[6] = -1;
        _MoveDirectionIndexOffset[7] = -1 + size;
        var directionsJob = new GenerateDirectionsJob()
        {
            Map = _Map
        };
        var handle = directionsJob.Schedule(_Map.tiles.Length, 64);
        var generateJob = new GenerateRandomLifeJob()
        {
            tiles = _Map.tiles,
            random = _Random
        };
        handle = generateJob.Schedule(_Map.tiles.Length, 64, handle);
        handle.Complete();

    }
    private void Update()
    {
        var state = new NativeArray<byte>(_Map.tiles.Length, Allocator.TempJob);
        var scaleFactor = math.pow(2,_Scale);
        if (_Batched)
        {
            var lifeJob = new GameOfLifeBatchJob()
            {
                Tiles = _Map.tiles,
                MapBlob = _Map.mapDataBlob,
                State = state
            };

            _Handle = lifeJob.ScheduleBatch(_Map.tiles.Length, (int) math.pow(2,_Batch));
        }
        else
        {
            var lifeJob = new GameOfLifeJob()
            {
                Tiles = _Map.tiles,
                State = state,
                MoveDirectionIndexOffset = _MoveDirectionIndexOffset
            };

            _Handle = lifeJob.ScheduleBatch(_Map.tiles.Length, (int) math.pow(2,_Batch));
        }




        var copyStateJob = new CopyStateJob()
        {
            Tiles = _Map.tiles,
            State = state
        };
        _Handle = copyStateJob.Schedule(_Map.tiles.Length, 64, _Handle);
        if (_Map.width == _TextureWidth)
        {
            var copyJob = new CopyLifeToTextureJob()
            {
                Pixels = _ColorsNative,
                Data = _Map.tiles
            };
            _Handle = copyJob.Schedule(_Pixels.Length, 64, _Handle);
        } else if (_Map.width > _TextureWidth)
        {
            var copyJob = new CopyLifeToTextureSmallJob()
            {
                Pixels = _ColorsNative,
                Data = _Map.tiles,
                MapWidth = _Map.width,
                TextureWidth = _TextureWidth,
                Position = _Position,
                scaleFactor = 1
            };
            _Handle = copyJob.Schedule(_Pixels.Length, 64, _Handle);
        }else if (_Map.width < _TextureWidth)
        {

        }


    }

    private void LateUpdate()
    {
        _Handle.Complete();
        RenderTexture.active = _GameOfLifeTexture;
        UpdateTexture();
        _Tex.Apply();
        Graphics.Blit(_Tex, _GameOfLifeTexture);
    }

    private void UpdateTexture()
    {
        _ColorsNative.CopyTo(_Colors);
        _Tex.SetPixels(_Colors);
    }

    private void OnDestroy()
    {
        _Pixels.Dispose();
        _Map.Dispose();
        _ColorsNative.Dispose();
    }
}
