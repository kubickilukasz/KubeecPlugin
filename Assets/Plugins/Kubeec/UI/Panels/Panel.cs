using System.Collections;
using System.Collections.Generic;
using Kubeec.General;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using NaughtyAttributes;
using System;

namespace UI {

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Panel : EnableDisableRectInitableDisposable {

        public event Action onShow;
        public event Action onHide;

        [SerializeField] AnimationController animationController;

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

        bool? isShown = null;
        public bool IsShown => !isShown.HasValue || isShown.Value;
        public bool IsHidden => !isShown.HasValue || !isShown.Value;

        protected override void OnInit(object data) {
            animationController.Init();
        }

        [Button]
        public void Show() {
            if (IsHidden) {
                if (currentActivePanel != this && currentActivePanel != null) {
                    currentActivePanel.Hide();
                }
                controller.currentActivePanel = this;
                isShown = true;
                animationController?.Stop();
                animationController?.Play(null, () => {
                    OnShow();
                    onShow?.Invoke();
                });
            }
        }

        [Button]
        public void Hide() {
            if (IsShown) {
                if (currentActivePanel == this) {
                    controller.currentActivePanel = null;
                }
                isShown = false;
                animationController?.Stop();
                animationController?.PlayBackwards(null, () => {
                    OnHide();
                    onHide?.Invoke();
                });
            }
        }

        public T Get<T>() where T : Panel {
            return controller.GetPanel<T>();
        }

        protected virtual void OnShow() { } 
        protected virtual void OnHide() { }

        
    }

}
