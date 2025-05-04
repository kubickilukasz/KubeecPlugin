using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System;

public static class Extensions {

    public const string pathToResourcesData = "Data";

    public static Vector3 GetRandomPosition(this Bounds bounds, Vector3? position = null) {
        Vector3 center = (position ?? Vector3.zero) + bounds.center;
        Vector3 radius = bounds.size / 2f;
        return new Vector3(
        center.x + UnityEngine.Random.Range(-radius.x, radius.x),
        center.y + UnityEngine.Random.Range(-radius.y, radius.y),
        center.z + UnityEngine.Random.Range(-radius.z, radius.z)
        );
    }

    public static float SineIn(this float t) {
        return (1 - Mathf.Cos((t * Mathf.PI) / 2));
    }

    public static float SineOut(this float t) {
        return (Mathf.Sin((t * Mathf.PI) / 2));
    }

    public static float SineInOut(this float t) {
        return (-(Mathf.Cos(Mathf.PI * t) - 1) / 2);
    }

    public static T GetOrAddComponent<T>(this MonoBehaviour behaviour) where T : Component {
        return GetOrAddComponent<T>(behaviour.gameObject);
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component{
        if (gameObject.TryGetComponent(out T component)) {
            return component;
        }
        return gameObject.AddComponent<T>();
    }

    public static void TryMoveTo(this Rigidbody rigidbody, Vector3 position, Quaternion rotation, float weight = 0f) {
        rigidbody.TryMoveTo(position, rotation, Time.fixedDeltaTime, weight);
    }

    public static void TryMoveTo(this Rigidbody rigidbody, Vector3 position, Quaternion rotation, float deltaTime, float weight = 0f) {
        float mass = rigidbody.mass * (1 - weight);
        rigidbody.AddForce((((position - rigidbody.position) * (1 / deltaTime)) - rigidbody.linearVelocity) * mass, ForceMode.Impulse);
        rotation = ShortestRotation(rotation, rigidbody.rotation);
        rotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
        rotationAxis *= angleInDegrees;
        rotationAxis = (rotationAxis * Mathf.Deg2Rad) * (1 / deltaTime);
        rigidbody.inertiaTensorRotation = Quaternion.identity;
        //rigidbody.AddTorque(rotationAxis * rigidbody.mass, ForceMode.Impulse); //Wiggle wiggle wiggle
        rigidbody.angularVelocity = rotationAxis;
        rigidbody.maxAngularVelocity = 14;
    }

    public static Quaternion ShortestRotation(Quaternion a, Quaternion b) {
        if (Quaternion.Dot(a, b) < 0) {
            return a * Quaternion.Inverse(b.Multiply(-1));
        } else return a * Quaternion.Inverse(b);
    }

    public static Quaternion Multiply(this Quaternion input, float scalar) {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }

    public static void InvokeOnEndOfFrame(this MonoBehaviour monoBehaviour, Action invokedCall) {
        if (monoBehaviour == null || !monoBehaviour.isActiveAndEnabled || invokedCall == null) {
            return;
        }
        monoBehaviour.StartCoroutine(WaitForInvoke(invokedCall));
        IEnumerator WaitForInvoke(Action invokedCall) {
            yield return new WaitForEndOfFrame();
            invokedCall.Invoke();
        }
    }

    public static void InvokeDelay(this MonoBehaviour monoBehaviour, Action invokedCall, float delay) {
        if (monoBehaviour == null || !monoBehaviour.isActiveAndEnabled || invokedCall == null) {
            return;
        }
        monoBehaviour.StartCoroutine(WaitForInvoke(invokedCall, delay));
        IEnumerator WaitForInvoke(Action invokedCall, float delay) {
            yield return new WaitForSeconds(delay);
            invokedCall.Invoke();
        }
    }

    public static bool CheckHashedPassword(this string hashedPassword, string password) {
        byte[] hashBytes = Convert.FromBase64String(hashedPassword);
        byte[] salt = new byte[16];
        Array.Copy(hashBytes, 0, salt, 0, 16);
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000);
        byte[] hash = pbkdf2.GetBytes(20);
        for (int i = 0; i < 20; i++)
            if (hashBytes[i + 16] != hash[i])
                return false;
        return true;
    }

    public static string HashPassword(this string input) {
        byte[] salt;
        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        var pbkdf2 = new Rfc2898DeriveBytes(input, salt, 100000);
        byte[] hash = pbkdf2.GetBytes(20);
        byte[] hashBytes = new byte[36];
        Array.Copy(salt, 0, hashBytes, 0, 16);
        Array.Copy(hash, 0, hashBytes, 16, 20);
        return Convert.ToBase64String(hashBytes);
    }

    public static Vector3 GetAngle180(this Vector3 vector3) {
        if (vector3.x >= 180) {
            vector3.x -= 360;
        }
        if (vector3.y >= 180) {
            vector3.y -= 360;
        }
        if (vector3.z >= 180) {
            vector3.z -= 360;
        }
        return vector3;
    }

    public static Vector3 Multiply(this Vector3 vector3, Vector3 other) {
        return new Vector3(vector3.x * other.x, vector3.y * other.y, vector3.z * other.z);
    }

    public static int CountParents(this Transform transform) {
        int i = 0;
        Transform current = transform;
        while (current.parent != null) {
            i++;
            current = current.parent;
        }
        return i;
    }

    public static int[] GetChildIndices(this Transform transform) {
        int [] res = new int[transform.CountParents() + 1];
        Transform current = transform;
        for (int i = res.Length - 1; i >= 0; i--) {
            res[i] = current.GetSiblingIndex();
            current = current.parent;
        }
        return res;
    }

    public static string GetPathToResourcesData(this string suffix) {
        return Path.Combine(pathToResourcesData, suffix);
    }

    public static List<T> GetComponentsInChildrenInOrder<T>(this GameObject gameObject) where T : Component {
        return gameObject.transform.GetComponentsInChildrenInOrder<T>();
    }

    public static List<T> GetComponentsInChildrenInOrder<T>(this Transform transform) where T : Component {
        List<T> values = new List<T>();
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(transform);
        while (queue.TryDequeue(out Transform result)) {
            T[] components = result.GetComponents<T>();
            for (int i = 0; i < components.Length; i++) {
                if (!values.Contains(components[i])) {
                    values.Add(components[i]);
                }
            }
            foreach (Transform t in result) {
                queue.Enqueue(t);
            }
        }
        return values;
    }

}
