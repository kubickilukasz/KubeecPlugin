using UnityEngine;

public class ComponentReference : MonoBehaviour{
    
    [SerializeField] GameObject source;

    public bool TryGetComponentFromSource<T>(out T component) where T : Component {
        return source.TryGetComponent(out component);
    }

}

public static class ComponentReferenceExt {

    public static bool TryGetComponentFromSource<T>(this GameObject gameObject, out T component) where T : Component {
        if (gameObject.TryGetComponent(out component)) {
            return true;
        } else if (gameObject.TryGetComponent(out ComponentReference reference)) {
            return reference.TryGetComponentFromSource(out component);  
        }
        return false;
    }

    public static bool TryGetComponentFromSource<T>(this Component source, out T component) where T : Component {
        return source.gameObject.TryGetComponentFromSource(out component);
    }

}
