using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Kubeec.General {

    public class CustomPool<T> : IObjectPool<T> where T : Component {

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

        ObjectPool<T> pool;
        T prefab { get; set; }

        public int CountInactive => throw new NotImplementedException();

        static string name;

        public CustomPool(int defaultCapacity = 5, int maxSize = 200) {
            name = typeof(T).Name;
            pool = new(OnCreate, OnGet, OnRelease, OnDestroy, true, defaultCapacity, maxSize);
        }

        public CustomPool(T prefab, int defaultCapacity = 5, int maxSize = 200) : this(defaultCapacity, maxSize) {
            this.prefab = prefab;
            name = prefab.name;
        }

        public T Get() => pool.Get();
        
        public PooledObject<T> Get(out T v) => pool.Get(out v);

        public void Release(T element) => pool.Release(element);

        public void Clear() => pool.Clear();

        T OnCreate() {
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

        void OnGet(T handler) {
            handler.gameObject.SetActive(true);
        }

        void OnRelease(T handler) {
            handler.transform.SetParent(ReleasedObjects);
            handler.gameObject.SetActive(false);
        }

        void OnDestroy(T handler) {
            if (handler != null) {
                GameObject.Destroy(handler.gameObject);
            }
        }

    }

}
