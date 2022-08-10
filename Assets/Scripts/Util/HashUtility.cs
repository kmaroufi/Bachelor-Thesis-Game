using Unity.Mathematics;
using UnityEngine;

public struct GridHash
{
    public readonly static int3[] cellOffsets =
    {
            new int3(0, 0, 0),
            new int3(-1, 0, 0),
            new int3(0, -1, 0),
            new int3(0, 0, -1),
            new int3(1, 0, 0),
            new int3(0, 1, 0),
            new int3(0, 0, 1)
        };

    public readonly static int2[] cell2DOffsets =
    {
		new int2(-1, -1),
		new int2(0, -1),
		new int2(1, -1),
		new int2(-1, 0),
		new int2(0, 0),
		new int2(1, 0),
		new int2(-1, 1),
		new int2(0, 1),
		new int2(1, 1),
	};

    public static int Hash(float3 v, float cellSize)
    {
        return Quantize(v, cellSize).GetHashCode();
    }

    public static int3 Quantize(float3 v, float cellSize)
    {
        return new int3(math.floor(v / cellSize));
    }

    public static int Hash(float2 v, float cellSize)
    {
        return Quantize(v, cellSize).GetHashCode();
    }
	
	public static int Hash(int2 v)
    {
        return v.GetHashCode();
    }

    public static int2 Quantize(float2 v, float cellSize)
    {
        return new int2(math.floor(v / cellSize));
    }
}