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

        public event Action<HitInfo> onHit;
        public event Action<float, HitReceiver, HitProvider> onDeath;

        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected bool immortal = false;
        [SerializeField] protected List<HitDefinition> hitDefinition = new List<HitDefinition>();

        protected float currentHealth;

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;

#if UNITY_EDITOR
        void OnDrawGizmos() {
            if (MyEditorSettings.TypeSetting.ShowCurrentHitPoints.GetToggle()) {
                UnityEditor.Handles.Label(transform.position, $"{name} {currentHealth}/{maxHealth}");
            }
        }
#endif

        public bool CollectDamage(HitInfo hitInfo) {
            if (!IsInitialized()) {
                return false;
            }
            HitDefinition definition = hitDefinition.FirstOrDefault(x => x.hitType.Equals(hitInfo.type));
            if (definition != null) {
                hitInfo.damage *= definition.multiplier;
            }
            MyDebug.Log(MyDebug.TypeLog.Hit, hitInfo.hitReceiver, hitInfo.hitProvider, hitInfo.type, hitInfo.damage);
            if (!immortal || currentHealth > hitInfo.damage) {
                currentHealth -= hitInfo.damage;
            }
            OnHit(hitInfo);
            onHit?.Invoke(hitInfo);
            onHitEvent?.Invoke();
            HandleCurrentHealth(hitInfo.damage, hitInfo.hitReceiver, hitInfo.hitProvider);
            return true;
        }

        protected override void OnInit(object data) {
            currentHealth = maxHealth;
        }

        protected override void OnDispose() {
        }

        protected virtual void OnHit(HitInfo hitInfo) {
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