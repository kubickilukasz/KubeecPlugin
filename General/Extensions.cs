using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System;
using Kubeec.General;
using System.Security.Cryptography;

public static class Extensions {

    public const string pathToResourcesData = "Data";
    public const string copyName = "_Copy";

    static EmptyBehaviour _emptyBehaviour = null;
    public static EmptyBehaviour emptyBehaviour {
        get {
            if (_emptyBehaviour == null) {
                _emptyBehaviour = new GameObject(typeof(EmptyBehaviour).Name).AddComponent<EmptyBehaviour>();
                GameObject.DontDestroyOnLoad(_emptyBehaviour.gameObject);
            }
            return _emptyBehaviour;
        }
    }

    public class EmptyBehaviour : MonoBehaviour {

        public bool isQuitting { private set; get; } = false;   

        void OnApplicationQuit() {
            isQuitting = true;
        }
    }

    public static Color SetAlpha(this Color color, float alpha) {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static void CopyValues(this Transform target, Transform origin) {
        target.parent = origin.parent;
        target.localPosition = origin.localPosition;
        target.localRotation = origin.localRotation;
        target.localScale = origin.localScale;
    }

    public static Transform Copy(this Transform transform) {
        Transform copy = new GameObject($"{transform.name}{copyName}").transform;
        copy.CopyValues(transform);
        return copy;
    }

    public static string GetString(this float value, int precision) {
        float prec = Mathf.Max(precision * 10, 1);
        int val = Mathf.RoundToInt(value * prec);
        value = val / prec;
        return val.ToString();
    }

    public static Vector3 GetRandomOffset(this Vector3 center, Vector3 size) {
        size /= 2f;
        return new Vector3() {
            x = center.x + UnityEngine.Random.Range(-size.x, size.x),
            y = center.y + UnityEngine.Random.Range(-size.y, size.y),
            z = center.z + UnityEngine.Random.Range(-size.z, size.z)
        };
    }

    public static T GetOrAdd<T>(this MonoBehaviour behaviour) where T : Component {
        return behaviour.gameObject.GetOrAdd<T>();
    }

    public static T GetOrAdd<T>(this GameObject gameObject) where T : Component {
        if (gameObject.TryGetComponent(out T component)) {
            return component;
        } else {
            return gameObject.AddComponent<T>();
        }
    }

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

    public static void TryMoveTo(this Rigidbody rigidbody, Vector3 position, Quaternion rotation) {
        rigidbody.TryMoveTo(position, rotation, Time.fixedDeltaTime);
    }

    public static void TryMoveTo(this Rigidbody rigidbody, Vector3 position, Quaternion rotation, float deltaTime) {
        deltaTime = (1 / deltaTime);
        rigidbody.AddForce((((position - rigidbody.position) * deltaTime) - rigidbody.linearVelocity), ForceMode.VelocityChange);
        rigidbody.TryRotateTo(rotation, deltaTime);
    }

    public static void TryMoveToSmooth(this Rigidbody rigidbody, Vector3 position, Quaternion rotation, float smooth) {
        float deltaTime = Mathf.Lerp(1f / Time.fixedDeltaTime, Time.fixedDeltaTime, smooth);
        rigidbody.AddForce((((position - rigidbody.position) * deltaTime) - rigidbody.linearVelocity), ForceMode.VelocityChange);
        rigidbody.TryRotateTo(rotation, deltaTime);
    }

    public static void TryMoveToSmooth(this Rigidbody rigidbody, Vector3 position, Quaternion rotation, Vector3 center, float smooth) {
        rigidbody.TryMoveToSmooth(position, rotation, Time.fixedDeltaTime, center, smooth);
    }

    public static void TryMoveToSmooth(this Rigidbody rigidbody, Vector3 position, Quaternion rotation, float deltaTime, Vector3 center, float smooth) {
        deltaTime = Mathf.Lerp(1f / Time.fixedDeltaTime, Time.fixedDeltaTime, smooth);
        Vector3 force = ((position - rigidbody.position) * deltaTime) - rigidbody.linearVelocity;
        rigidbody.AddForceAtPosition(force, center, ForceMode.Impulse);
        rigidbody.TryRotateTo(rotation, deltaTime);
    }

    public static void TryRotateTo(this Rigidbody rigidbody,Quaternion rotation, float deltaTime) {
        rotation = ShortestRotation(rotation, rigidbody.rotation);
        rotation.ToAngleAxis(out float angleInDegrees, out Vector3 rotationAxis);
        rotationAxis *= angleInDegrees;
        rotationAxis = (rotationAxis * Mathf.Deg2Rad) * deltaTime;
        rigidbody.inertiaTensorRotation = Quaternion.identity;
        rigidbody.angularVelocity = rotationAxis;
        rigidbody.maxAngularVelocity = 14;
    }

    public static void TryRotateTo(this Rigidbody rigidbody, Quaternion rotation, float deltaTime, float smooth) {
        deltaTime = Mathf.Lerp(1f / Time.fixedDeltaTime, Time.fixedDeltaTime, smooth);
        rigidbody.TryRotateTo(rotation, deltaTime);
    }

    public static Quaternion ShortestRotation(Quaternion a, Quaternion b) {
        if (Quaternion.Dot(a, b) < 0) {
            return a * Quaternion.Inverse(b.Multiply(-1));
        } else return a * Quaternion.Inverse(b);
    }

    public static Quaternion Multiply(this Quaternion input, float scalar) {
        return new Quaternion(input.x * scalar, input.y * scalar, input.z * scalar, input.w * scalar);
    }

    public static Coroutine InvokeOnEndOfFrame(this MonoBehaviour monoBehaviour, Action invokedCall) {
        return monoBehaviour.StartCoroutine(WaitForInvoke(invokedCall));
        IEnumerator WaitForInvoke(Action invokedCall) {
            yield return new WaitForEndOfFrame();
            invokedCall.Invoke();
        }
    }

    public static Coroutine InvokeDelay(this MonoBehaviour monoBehaviour, Action invokedCall, float delay) {
        return monoBehaviour.StartCoroutine(WaitForInvoke(invokedCall, delay));
        IEnumerator WaitForInvoke(Action invokedCall, float delay) {
            yield return new WaitForSeconds(delay);
            invokedCall.Invoke();
        }
    }

    public static Coroutine InvokeNextFrame(this MonoBehaviour monoBehaviour, Action invokedCall) {
        return monoBehaviour.StartCoroutine(WaitForInvoke(invokedCall));
        IEnumerator WaitForInvoke(Action invokedCall) {
            yield return null;
            invokedCall.Invoke();
        }
    }

    public static void SafeInvokeNextFrame(this MonoBehaviour monoBehaviour, Action invokedCall) {
        if (emptyBehaviour.isQuitting || invokedCall == null) {
            return;
        }
        emptyBehaviour.InvokeNextFrame(() => {
            invokedCall.Invoke();
        });
    }

    /// <summary>
    /// Wait 2 frames to delete object
    /// </summary>
    /// <param name="monoBehaviour"></param>
    public static void SafeSelftDestroy(this GameObject gameObject) {
        if (gameObject == null) {
            return;
        }
        gameObject.SetActive(false);
        if (emptyBehaviour.isQuitting) {
            return;
        }
        emptyBehaviour.StartCoroutine(WaitForDestroy(gameObject));
        IEnumerator WaitForDestroy(GameObject gameObject) {
            yield return null;
            yield return null;
            if (gameObject != null) {
                GameObject.Destroy(gameObject);
            }
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
