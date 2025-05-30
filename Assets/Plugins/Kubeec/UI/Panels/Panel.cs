using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using NaughtyAttributes;

namespace UI {

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Panel : RectMonoBehaviour {

        [SerializeField] float fadeInOutDuration = 0.2f;

        PanelController _controller;
        PanelController controller {
            get {
                if (_controller == null) {
                    _controller = GetComponentInParent<PanelController>();
                }
                return _controller;
            }
        }

        Panel currentActivePanel => controller != null ? controller.currentActivePanel : null;

        CanvasGroup _canvasGroup;
        CanvasGroup canvasGroup {
            get {
                if (_canvasGroup == null) {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        bool? isShown = null;
        public bool IsShown => !isShown.HasValue || isShown.Value;
        public bool IsHidden => !isShown.HasValue || !isShown.Value;

        protected virtual void OnDisable() {
            ResetAnimation();
        }

        [Button]
        public void Show() {
            if (IsHidden) {
                if (currentActivePanel != this && currentActivePanel != null) {
                    currentActivePanel.Hide();
                }
                controller.currentActivePanel = this;
                ResetAnimation();
                canvasGroup.DOFade(1f, fadeInOutDuration).OnComplete(() => canvasGroup.interactable = true);
                isShown = true;
                OnShow();
            }
        }

        [Button]
        public void Hide() {
            if (IsShown) {
                if (currentActivePanel == this) {
                    controller.currentActivePanel = null;
                }
                ResetAnimation();
                canvasGroup.DOFade(0f, fadeInOutDuration);
                canvasGroup.interactable = false;
                isShown = false;
                OnHide();
            }
        }

        public T Get<T>() where T : Panel {
            return controller.GetPanel<T>();
        }

        protected virtual void OnShow() { } 
        protected virtual void OnHide() { }

        void ResetAnimation() {
            canvasGroup.DOKill(gameObject);
        }

    }

}
