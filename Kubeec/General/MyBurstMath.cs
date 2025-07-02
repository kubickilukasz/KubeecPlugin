using System.Collections.Generic;
using System;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public static class MyBurstMath{
    // extension method
    public static quaternion Average(this IEnumerable<quaternion> values) => AverageQuaternion(values);

    [BurstCompile]
    public static void Average(in this NativeArray<quaternion> values, out quaternion result) {
        var count = values.Length;
        switch (count) {
            case 0:
                result = quaternion.identity;
                return;
            case 1:
                result = values[0];
                return;
            case 2:
                result = math.slerp(values[0], values[1], 0.5f);
                return;
        }

        var cumulative = float4.zero;
        var first = values[0];

        for (var i = 0; i < count; i++) {
            AccumulateQuaternions(ref cumulative, values[i], first);
        }
        Normalize(cumulative / count, out result);
    }

    public static quaternion AverageQuaternion(IEnumerable<quaternion> values) {
        var cumulative = float4.zero;
        var first = quaternion.identity;
        var second = quaternion.identity;
        uint count = 0;

        foreach (var quat in values) {
            switch (++count) {
                case 1:
                    first = quat;
                    break;
                case 2:
                    second = quat;
                    break;
            }

            AccumulateQuaternions(ref cumulative, quat, first);
        }

        switch (count) {
            case 0:
                return quaternion.identity;
            case 1:
                return first;
            case 2:
                return math.slerp(first, second, 0.5f);
        }

        //note: if speed is an issue, you can skip the normalization step
        Normalize(cumulative / count, out var result);
        return result;
    }

    //Get an average (mean) from more then two quaternions (with two, slerp would be used).
    //Note: this only works if all the quaternions are relatively close together.
    //Usage:
    //-Cumulative is an external Vector4 which holds all the added x y z and w components.
    //-newRotation is the next rotation to be added to the average pool
    //-firstRotation is the first quaternion of the array to be averaged
    //-addAmount holds the total amount of quaternions which are currently added
    [BurstCompile]
    private static void AccumulateQuaternions(ref float4 cumulative, in quaternion newRotation,
        in quaternion firstRotation) {
        //Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
        //q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
        var rotation = newRotation;
        if (!AreQuaternionsClose(rotation, firstRotation)) {
            InverseSignQuaternion(rotation, out rotation);
        }
        cumulative += rotation.value;
    }

    [BurstCompile]
    private static void Normalize(in this float4 fullQuat, out float4 result) => result = math.normalize(fullQuat);

    [BurstCompile]
    private static void Normalize(in quaternion q, out quaternion result) => result = math.normalize(q);

    //Changes the sign of the quaternion components. This is not the same as the inverse.
    [BurstCompile]
    public static void InverseSignQuaternion(in quaternion q, out quaternion result) {
        result = float4.zero - q.value;
    }

    //Returns true if the two input quaternions are close to each other. This can
    //be used to check whether or not one of two quaternions which are supposed to
    //be very similar but has its component signs reversed (q has the same rotation as
    //-q)
    [BurstCompile]
    public static bool AreQuaternionsClose(in quaternion q1, in quaternion q2) {
        return math.dot(q1, q2) >= 0.0f;
    }
}
