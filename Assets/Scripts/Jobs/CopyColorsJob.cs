using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Jobs
{
    public struct CopyColorsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> pixels;
        [WriteOnly] public NativeArray<Color> colors;
        public void Execute(int index)
        {
            var color = pixels[index];
            colors[index] =  new Color(color,color,color,1);
        }

    }
}