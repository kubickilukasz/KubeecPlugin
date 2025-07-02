using NaughtyAttributes;
using System;
using System.Collections;
using UnityEngine;

namespace Kubeec.VR.Player {

    public class Vignette : Initable<VignetteData> {

        const string colorShaderName = "_Color";
        const string viewShaderName = "_View";
        const string outlineShaderName = "_Outline";
        const string closeShaderName = "_Close";

        const float minViewValue = -6f;
        const float maxViewValue = 0f;
        const float minFadeValue = 0.7f;
        const float maxFadeValue = 1f;

        static int colorShaderId = Shader.PropertyToID(colorShaderName);
        static int viewShaderId = Shader.PropertyToID(viewShaderName);
        static int outlineShaderId = Shader.PropertyToID(outlineShaderName);
        static int closeShaderId = Shader.PropertyToID(closeShaderName);

        [SerializeField] Color defaultColor = Color.black;
        [SerializeField] Renderer renderer;
        [SerializeField] float minOutlineValue = 0f;
        [SerializeField] float maxOutlineValue = 120f;
        [SerializeField] float smoothFollow = 1f;
        [SerializeField] float smoothState = 10f;
        [SerializeField] float fadeIn = 0.6f;
        [SerializeField] float fadeOut = 0.4f;
        [SerializeField] float minTimeInFade = 0.2f;

        Material material;
        Transform camera;
        float currentView = 0f; 
        float currentOutline = 0f;
        float fadeValue;
        event Action onCompleteFadeIn;
        event Action onCompleteFadeOut;

        void Start() {
            material = renderer.sharedMaterial;
            material.SetColor(colorShaderId, defaultColor);
            SetFade(1f);   
        }

        void Update() {
            UpdateTransform();
        }

        protected override void OnInit(VignetteData data) {
            material = renderer.sharedMaterial;
            material.SetColor(colorShaderId, defaultColor);
            if (data != null) {
                camera = data.camera;
            }
        }

        public void Set(float viewNormalized, float outlineNormalized) {
            currentView = Mathf.Lerp(currentView, viewNormalized, Time.deltaTime * smoothState);
            currentOutline = Mathf.Lerp(currentOutline, outlineNormalized, Time.deltaTime * smoothState);
            if (fadeValue < 0.1f) {
                material.SetFloat(viewShaderId, Mathf.Lerp(minViewValue, maxViewValue, currentView));
                material.SetFloat(outlineShaderId, Mathf.Lerp(minOutlineValue, maxOutlineValue, currentOutline));
            }
        }

        public void UpdateTransform() {
            if (camera != null) {
                transform.position = camera.position;
                transform.rotation = Quaternion.Lerp(transform.rotation, camera.rotation, Time.deltaTime * smoothFollow);
            }
        }

        [Button]
        public void TestFade() {
            SetFadeInFadeOut(() => true);
        }

        public void SetFadeInFadeOut(Func<bool> isReady, Action onCompleteFadeIn = null, Action onCompleteFadeOut = null) {
            if (IsInitialized()) {
                StopAllCoroutines();
                this.onCompleteFadeIn += onCompleteFadeIn;
                this.onCompleteFadeOut += onCompleteFadeOut;
                StartCoroutine(StartFade(isReady));
            }
        }

        IEnumerator StartFade(Func<bool> isReady) {
            float fadeTime = fadeIn * (1f - fadeValue);
            while (fadeTime < fadeIn) {
                SetFade((fadeTime / fadeIn));
                fadeTime += Time.unscaledDeltaTime;
                yield return null;
            }
            SetFade(1f);
            onCompleteFadeIn?.Invoke();
            onCompleteFadeIn = null;
            fadeTime = 0f;
            bool _isReady = false;
            do {
                fadeTime += Time.unscaledDeltaTime;
                yield return null;
                _isReady = isReady();
            } while (!_isReady || (_isReady && fadeTime < minTimeInFade));
            fadeTime = 0f;
            while (fadeTime < fadeOut) {
                SetFade((1f - fadeTime / fadeOut));
                fadeTime += Time.unscaledDeltaTime;
                yield return null;
            }
            SetFade(0f);
            onCompleteFadeOut?.Invoke();
            onCompleteFadeOut = null;
        }

        void SetFade(float value) {
            fadeValue = value;
            material.SetFloat(closeShaderId, Mathf.Lerp(minFadeValue, maxFadeValue, fadeValue));
        }


    }

    public class VignetteData {
        public Transform camera;
    }

}
