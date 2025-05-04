using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {

    [RequireComponent(typeof(RectTransform))]
    public class RectMonoBehaviour : MonoBehaviour {

        RectTransform _rectTransform;

        public RectTransform rectTransform {
            get {
                if (_rectTransform == null) {
                    _rectTransform = GetComponent<RectTransform>();
                }
                return _rectTransform;
            }
        }

    }

}
