using UnityEngine;

public class ComponentReference : MonoBehaviour{
    
    [SerializeField] GameObject source;

    public bool TryGetComponentFromSource<T>(out T component) {
        return source.TryGetComponent(out component);
    }

}

public static class ComponentReferenceExt {

    public static bool TryGetComponentFromSource<T>(this GameObject gameObject, out T component){
        if (gameObject.TryGetComponent<T>(out component)) {
            return true;
        } else if (gameObject.TryGetComponent<ComponentReference>(out ComponentReference reference)) {
            return reference.TryGetComponentFromSource<T>(out component);  
        }
        return false;
    }

    public static bool TryGetComponentFromSource<T>(this Component source, out T component) {
        return source.gameObject.TryGetComponentFromSource(out component);
    }


}
