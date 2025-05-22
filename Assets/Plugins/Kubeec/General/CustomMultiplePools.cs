using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Kubeec.General {

    public abstract class CustomMultiplePools<Y> where Y : Component{

        static Dictionary<int, CustomPool<Y>> pools = new();

        public static Y Get(Y prefab, Transform parent = null) {
#if UNITY_EDITOR
            Assert.IsTrue(UnityEditor.PrefabUtility.GetPrefabAssetType(prefab) != UnityEditor.PrefabAssetType.NotAPrefab, "This is not a prefab!");
#endif
            Y output = null;
            CustomPool<Y> pool;
            if (pools.TryGetValue(prefab.GetHashCode(), out pool)) {
                output = pool.Get();
                if (output == null) {
                    pool.Clear();
                    output = pool.Get();
                }
            } else {
                CustomPool<Y> newPool = new CustomPool<Y>(prefab);
                pools.Add(prefab.GetHashCode(), newPool);
                output = newPool.Get();
            }
            if (parent != null) {
                output.transform.SetParent(parent.transform, false);
            }
            return output;
        }

        public static void Release(Y prefab, Y output) {
#if UNITY_EDITOR
            Assert.IsTrue(UnityEditor.PrefabUtility.GetPrefabAssetType(prefab) != UnityEditor.PrefabAssetType.NotAPrefab, "This is not a prefab!");
#endif
            if (output != null) {
                if (pools.TryGetValue(prefab.GetHashCode(), out CustomPool<Y> pool)) {
                    pool.Release(output);
                } else {
                    GameObject.Destroy(output.gameObject);
                }
            }
        }

        public static void Clear(Y prefab) {
            if (pools.TryGetValue(prefab.GetHashCode(), out CustomPool<Y> pool)) {
                pool.Clear();
            }
        }

    }

}
