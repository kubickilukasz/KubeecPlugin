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
        const float minOutlineValue = 0;
        const float maxOutlineValue = 40f;

        static int colorShaderId = Shader.PropertyToID(colorShaderName);
        static int viewShaderId = Shader.PropertyToID(viewShaderName);
        static int outlineShaderId = Shader.PropertyToID(outlineShaderName);
        static int closeShaderId = Shader.PropertyToID(closeShaderName);

        [SerializeField] Color defaultColor = Color.black;
        [SerializeField] Renderer renderer;
        [SerializeField] float smoothFollow = 1f;
        [SerializeField] float smoothState = 10f;
        [SerializeField] float fadeIn = 0.6f;
        [SerializeField] float fadeOut = 0.4f;

        Material material;
        Transform camera;
        float currentView = 0f; 
        float currentOutline = 0f;

        void Start() {
            SetFade(0f);   
        }

        void Update() {
            UpdateTransform();
        }

        public void Set(float viewNormalized, float outlineNormalized) {
            currentView = Mathf.Lerp(currentView, viewNormalized, Time.deltaTime * smoothState);
            currentOutline = Mathf.Lerp(currentOutline, outlineNormalized, Time.deltaTime * smoothState);
            material.SetFloat(viewShaderId, Mathf.Lerp(minViewValue, maxViewValue, currentView));
            material.SetFloat(outlineShaderId, Mathf.Lerp(minOutlineValue, maxOutlineValue, currentOutline));
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
            if (isActiveAndEnabled) {
                StopAllCoroutines();
                StartCoroutine(StartFade(isReady, onCompleteFadeIn, onCompleteFadeOut));
            }
        }

        protected override void OnInit(VignetteData data) {
            material = renderer.sharedMaterial;
            material.SetColor(colorShaderId, defaultColor);
            if (data != null) {
                camera = data.camera;
            }
        }

        IEnumerator StartFade(Func<bool> isReady, Action onCompleteFadeIn, Action onCompleteFadeOut) {
            float fadeTime = 0f;
            SetFade(0f);
            while (fadeTime < fadeIn) {
                SetFade((fadeTime / fadeIn).SineIn());
                fadeTime += Time.unscaledDeltaTime;
                yield return null;
            }
            SetFade(1f);
            onCompleteFadeIn?.Invoke();
            do {
                yield return null;
            } while (!isReady());
            fadeTime = 0f;
            while (fadeTime < fadeOut) {
                SetFade((1f - fadeTime / fadeOut).SineIn());
                fadeTime += Time.unscaledDeltaTime;
                yield return null;
            }
            SetFade(0f);
            onCompleteFadeOut?.Invoke();
        }

        void SetFade(float value) {
            material.SetFloat(closeShaderId, Mathf.Lerp(0.7f, 1f, value));
        }


    }

    public class VignetteData {
        public Transform camera;
    }

}
