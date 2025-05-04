using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class MyMath {

    public static Vector3 Min(this Vector3 ob1, float length) {
        float ob1Length = ob1.magnitude;
        return ob1.normalized * Mathf.Min(ob1Length, length);
    }

    public static Vector3 Min(this Vector3 ob1, Vector3 ob2) {
        return new Vector3(Mathf.Min(ob1.x, ob2.x), Mathf.Min(ob1.y, ob2.y), Mathf.Min(ob1.z, ob2.z));
    }

    public static Quaternion AngVelToDeriv(Quaternion Current, Vector3 AngVel) {
        var Spin = new Quaternion(AngVel.x, AngVel.y, AngVel.z, 0f);
        var Result = Spin * Current;
        return new Quaternion(0.5f * Result.x, 0.5f * Result.y, 0.5f * Result.z, 0.5f * Result.w);
    }

    public static Vector3 DerivToAngVel(Quaternion Current, Quaternion Deriv) {
        var Result = Deriv * Quaternion.Inverse(Current);
        return new Vector3(2f * Result.x, 2f * Result.y, 2f * Result.z);
    }

    public static Quaternion IntegrateRotation(Quaternion Rotation, Vector3 AngularVelocity, float DeltaTime) {
        if (DeltaTime < Mathf.Epsilon) return Rotation;
        var Deriv = AngVelToDeriv(Rotation, AngularVelocity);
        var Pred = new Vector4(
                Rotation.x + Deriv.x * DeltaTime,
                Rotation.y + Deriv.y * DeltaTime,
                Rotation.z + Deriv.z * DeltaTime,
                Rotation.w + Deriv.w * DeltaTime
        ).normalized;
        return new Quaternion(Pred.x, Pred.y, Pred.z, Pred.w);
    }

    public static Quaternion SmoothDamp(Quaternion rot, Quaternion target, ref Quaternion deriv, float time) {
        if (Time.deltaTime < Mathf.Epsilon) return rot;
        // account for double-cover
        var Dot = Quaternion.Dot(rot, target);
        var Multi = Dot > 0f ? 1f : -1f;
        target.x *= Multi;
        target.y *= Multi;
        target.z *= Multi;
        target.w *= Multi;
        // smooth damp (nlerp approx)
        var Result = new Vector4(
            Mathf.SmoothDamp(rot.x, target.x, ref deriv.x, time),
            Mathf.SmoothDamp(rot.y, target.y, ref deriv.y, time),
            Mathf.SmoothDamp(rot.z, target.z, ref deriv.z, time),
            Mathf.SmoothDamp(rot.w, target.w, ref deriv.w, time)
        ).normalized;

        // ensure deriv is tangent
        var derivError = Vector4.Project(new Vector4(deriv.x, deriv.y, deriv.z, deriv.w), Result);
        deriv.x -= derivError.x;
        deriv.y -= derivError.y;
        deriv.z -= derivError.z;
        deriv.w -= derivError.w;

        return new Quaternion(Result.x, Result.y, Result.z, Result.w);
    }
   
    public static bool Approximately(this Quaternion quatA, Quaternion value, float acceptableRange) {
        return 1 - Mathf.Abs(Quaternion.Dot(quatA, value)) < acceptableRange;
    }

    public static Vector3 ClampPoint(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd) {
        return ClampProjection(ProjectPoint(point, segmentStart, segmentEnd), segmentStart, segmentEnd);
    }

    public static Vector3 ProjectPoint(Vector3 point, Vector3 segmentStart, Vector3 segmentEnd) {
        return segmentStart + Vector3.Project(point - segmentStart, segmentEnd - segmentStart);
    }

    private static Vector3 ClampProjection(Vector3 point, Vector3 start, Vector3 end) {
        var toStart = (point - start).sqrMagnitude;
        var toEnd = (point - end).sqrMagnitude;
        var segment = (start - end).sqrMagnitude;
        if (toStart > segment || toEnd > segment) return toStart > toEnd ? end : start;
        return point;
    }

    public static Quaternion AverageQuaternion(Quaternion[] quats) {
        if (quats.Length == 0) {
            return Quaternion.identity;
        }

        Vector4 cumulative = new Vector4(0, 0, 0, 0);

        foreach (Quaternion quat in quats) {
            AverageQuaternion_Internal(ref cumulative, quat, quats[0]);
        }

        float addDet = 1f / (float)quats.Length;
        float x = cumulative.x * addDet;
        float y = cumulative.y * addDet;
        float z = cumulative.z * addDet;
        float w = cumulative.w * addDet;
        //note: if speed is an issue, you can skip the normalization step
        return NormalizeQuaternion(new Quaternion(x, y, z, w));
    }

    //Get an average (mean) from more then two quaternions (with two, slerp would be used).
    //Note: this only works if all the quaternions are relatively close together.
    //Usage:
    //-Cumulative is an external Vector4 which holds all the added x y z and w components.
    //-newRotation is the next rotation to be added to the average pool
    //-firstRotation is the first quaternion of the array to be averaged
    //-addAmount holds the total amount of quaternions which are currently added
    static void AverageQuaternion_Internal(ref Vector4 cumulative, Quaternion newRotation, Quaternion firstRotation) {
        //Before we add the new rotation to the average (mean), we have to check whether the quaternion has to be inverted. Because
        //q and -q are the same rotation, but cannot be averaged, we have to make sure they are all the same.
        if (!AreQuaternionsClose(newRotation, firstRotation)) {
            newRotation = InverseSignQuaternion(newRotation);
        }

        //Average the values
        cumulative.w += newRotation.w;
        cumulative.x += newRotation.x;
        cumulative.y += newRotation.y;
        cumulative.z += newRotation.z;
    }

    public static Quaternion NormalizeQuaternion(Quaternion quat) {
        float lengthD = 1.0f / Mathf.Sqrt(quat.w * quat.w + quat.x * quat.x + quat.y * quat.y + quat.z * quat.z);
        quat.x *= lengthD;
        quat.y *= lengthD;
        quat.z *= lengthD;
        quat.w *= lengthD;
        return quat;
    }

    //Changes the sign of the quaternion components. This is not the same as the inverse.
    public static Quaternion InverseSignQuaternion(Quaternion q) {
        return new Quaternion(-q.x, -q.y, -q.z, -q.w);
    }

    //Returns true if the two input quaternions are close to each other. This can
    //be used to check whether or not one of two quaternions which are supposed to
    //be very similar but has its component signs reversed (q has the same rotation as
    //-q)
    public static bool AreQuaternionsClose(Quaternion q1, Quaternion q2) {
        float dot = Quaternion.Dot(q1, q2);

        if (dot < 0.0f) {
            return false;
        } else {
            return true;
        }
    }
}
