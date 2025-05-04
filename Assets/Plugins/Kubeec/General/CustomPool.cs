using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Kubeec.General {

    public class CustomPool<T> : ObjectPool<T> where T : Component {

        const string nameDynamicHandler = "[Generated] {0}";
        const string nameReleasedObjects = "[Generated Pool] {0}";

        static Transform releasedObjects;
        static Transform ReleasedObjects {
            get {
                if (releasedObjects == null) {
                    releasedObjects = new GameObject(string.Format(nameReleasedObjects, typeof(T).Name)).transform;
                    UnityEngine.Object.DontDestroyOnLoad(releasedObjects);
                }
                return releasedObjects;
            }
        }
        static T prefab { get; set; }

        static string name;

        public CustomPool(int defaultCapacity = 5, int maxSize = 200) : base(OnCreate, OnGet, OnRelease, null, true, defaultCapacity, maxSize) {
            name = typeof(T).Name;
        }

        public CustomPool(T prefab) : this() {
            CustomPool<T>.prefab = prefab;
        }

        static T OnCreate() {
            T handler;
            if (prefab == null) {
                GameObject go = new GameObject(string.Format(nameDynamicHandler, name));
                handler = go.AddComponent<T>();
            } else {
                handler = GameObject.Instantiate(prefab);
                handler.name = string.Format(nameDynamicHandler, name);
            }
            return handler;
        }

        static void OnGet(T handler) {
            handler.gameObject.SetActive(true);
        }

        static void OnRelease(T handler) {
            handler.transform.SetParent(ReleasedObjects);
            handler.gameObject.SetActive(false);
        }
    }

}
