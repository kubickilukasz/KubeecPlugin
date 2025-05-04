using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Kubeec.Hittable {
    public class HitCollector : EnableInitableDisposable {

        public UnityEvent onHitEvent;
        public UnityEvent onDeathEvent;

        public event Action<float, HitReceiver, HitProvider> onHit;
        public event Action<float, HitReceiver, HitProvider> onDeath;

        [SerializeField] float maxHealth = 100f;
        [SerializeField] List<HitDefinition> hitDefinition = new List<HitDefinition>();

        float currentHealth;

#if UNITY_EDITOR
        void OnDrawGizmos() {
            if (MyEditorSettings.TypeSetting.ShowCurrentHitPoints.GetToggle()) {
                UnityEditor.Handles.Label(transform.position, $"{name} {currentHealth}/{maxHealth}");
            }
        }
#endif

        public bool CollectDamage(HitReceiver hitReceiver, HitProvider hitProvider, HitType hitType, float damage) {
            if (!IsInitialized()) {
                return false;
            }
            HitDefinition definition = hitDefinition.FirstOrDefault(x => x.hitType.Equals(hitType));
            if (definition != null) {
                damage *= definition.multiplier;
            }
            MyDebug.Log(MyDebug.TypeLog.Hit, hitReceiver, hitProvider, hitType, damage);
            currentHealth -= damage;
            onHit?.Invoke(damage, hitReceiver, hitProvider);
            onHitEvent?.Invoke();
            HandleCurrentHealth(damage, hitReceiver, hitProvider);
            return true;
        }

        protected override void OnInit(object data) {
            currentHealth = maxHealth;
        }

        protected override void OnDispose() {
        }

        void HandleCurrentHealth(float? damage = 0, HitReceiver hitReceiver = null, HitProvider hitProvider = null) {
            if (currentHealth <= 0) {
                onDeath?.Invoke(damage ?? 0, hitReceiver, hitProvider);
                onDeathEvent?.Invoke();
                Dispose();
            }
        }

        [ContextMenu("ForceDeath")]
        void ForceDeath() {
            Dispose();
        }

    }


    [Serializable]
    public class HitDefinition {
        public HitType hitType;
        public float multiplier;
    }

}