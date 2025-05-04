using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;

public static class MyDebug{

    const string separator = ", ";

    static bool wasInit = false;
    static Dictionary<TypeLog, TypeWhenLog> setup;
    static TypeWhenLog current;

    static void InitIfNeeded() {
        if (wasInit) {
            return;
        }
        current = Application.isEditor ? TypeWhenLog.Editor : TypeWhenLog.Build;
        setup = new() {
            { TypeLog.Network, TypeWhenLog.None },
            { TypeLog.Temporary, TypeWhenLog.None },
            { TypeLog.Interaction, TypeWhenLog.None },
            { TypeLog.OnlyBuild, TypeWhenLog.Build },
            { TypeLog.Database, TypeWhenLog.All },
            { TypeLog.Hit, TypeWhenLog.All }
        };
        wasInit = true;
    }

    public static void Log(this MonoBehaviour monoBehaviour, TypeLog typeLog, params object[] logs) {
        Log(PrepareString(typeLog, logs), monoBehaviour);
    }

    public static void Log(TypeLog typeLog, params object[] logs) {
        Log(PrepareString(typeLog, logs));
    }

    public static void Log<T>(TypeLog typeLog, IList<T> list) {
        Log(typeLog, string.Join(", ", list.Select(x => x.ToString())));
    }

    public static void LogError(this MonoBehaviour monoBehaviour, TypeLog typeLog, params object[] logs) {
        LogError(PrepareString(typeLog, logs), monoBehaviour);
    }

    public static void LogError(TypeLog typeLog, params object[] logs) {
        LogError(PrepareString(typeLog, logs));
    }

    public static void LogError<T>(TypeLog typeLog, List<T> list) {
        LogError(typeLog, string.Join(", ", list.Select(x => x.ToString())));
    }

    static string PrepareString(TypeLog typeLog, params object[] logs) {
        InitIfNeeded();
        if (((int)setup[typeLog] & (int)current) != 0) {
            StringBuilder sb = new StringBuilder($"[{typeLog.ToString()}]: ");
            for (int i = 0; i < logs.Length; i++) {
                sb.Append(logs[i]);
                sb.Append(separator);
            }
            return sb.ToString();
        }
        return null;
    }

    static void Log(string log, Object obj = null) {
        if (!string.IsNullOrEmpty(log)) {
            Debug.Log(log, obj);
        }
    }

    static void LogError(string log, Object obj = null) {
        if (!string.IsNullOrEmpty(log)) {
            Debug.LogError(log, obj);
        }
    }

    public enum TypeLog {
        Network, Temporary, Interaction, OnlyBuild, Database, Hit
    }

    enum TypeWhenLog {
        All = 3, Editor = 1, Build = 2, None = 0
    }

}
