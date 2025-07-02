using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;

namespace Kubeec.VR.Player {

    public class HandPointer : MonoBehaviour {

        [SerializeField] Image image;

        [Space]

        [SerializeField] ColorData defaultColor;
        [SerializeField] ColorData activeColor;
        [SerializeField] ColorData.Variant defaultVariantColor;
        [SerializeField] ColorData.Variant activeVariantColor;
        [SerializeField] float animDuration = 0.075f;
        [SerializeField] float angleRot = -45;

        RectTransform _rectTransform;

        public RectTransform rectTransform {
            get {
                if (_rectTransform == null) {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }


        bool isActive;

        public void Set(bool isActive, bool force = false) {
            if (this.isActive == isActive && !force) {
                return;
            }
            this.isActive = isActive;
            image.rectTransform.DOKill();
            image.DOKill();
            if (isActive) {
                image.DOColor(ColorData.GetWithAlpha(image.color.a, activeColor.V2), animDuration);
                image.rectTransform.DOLocalRotate(Vector3.forward * angleRot, animDuration, RotateMode.Fast);
            } else {
                image.DOColor(ColorData.GetWithAlpha(image.color.a, defaultColor.V2), animDuration);
                image.rectTransform.DOLocalRotate(Vector3.zero, animDuration, RotateMode.Fast);
            }
        }

    }

}
