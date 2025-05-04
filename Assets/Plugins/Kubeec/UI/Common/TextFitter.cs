using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

namespace UI {

    [ExecuteInEditMode]
    public class TextFitter : MonoBehaviour {
        [SerializeField] TMP_Text text;
        [SerializeField] bool resizeTextObject = true;
        [SerializeField] Vector2 padding;
        [SerializeField] Vector2 maxSize = new Vector2(1000, float.PositiveInfinity);
        [SerializeField] Vector2 minSize;
        [SerializeField] Mode controlAxes = Mode.Both;

        [Flags]
        public enum Mode {
            None = 0,
            Horizontal = 0x1,
            Vertical = 0x2,
            Both = Horizontal | Vertical
        }

        string _lastText;
        Mode _lastControlAxes = Mode.None;
        Vector2 _lastSize;
        bool _forceRefresh;
        bool _isTextNull = true;
        RectTransform _textRectTransform;
        RectTransform _selfRectTransform;

        protected virtual float MinX {
            get {
                if ((controlAxes & Mode.Horizontal) != 0) return minSize.x;
                return _selfRectTransform.rect.width - padding.x;
            }
        }
        protected virtual float MinY {
            get {
                if ((controlAxes & Mode.Vertical) != 0) return minSize.y;
                return _selfRectTransform.rect.height - padding.y;
            }
        }
        protected virtual float MaxX {
            get {
                if ((controlAxes & Mode.Horizontal) != 0) return maxSize.x;
                return _selfRectTransform.rect.width - padding.x;
            }
        }
        protected virtual float MaxY {
            get {
                if ((controlAxes & Mode.Vertical) != 0) return maxSize.y;
                return _selfRectTransform.rect.height - padding.y;
            }
        }

        void OnValidate() {
            Refresh();
        }

        protected virtual void Update() {
            if (!_isTextNull && (text.text != _lastText || _lastSize != _selfRectTransform.rect.size || _forceRefresh || controlAxes != _lastControlAxes)) {
                var preferredSize = text.GetPreferredValues(MaxX, MaxY);
                preferredSize.x = Mathf.Clamp(preferredSize.x, MinX, MaxX);
                preferredSize.y = Mathf.Clamp(preferredSize.y, MinY, MaxY);
                preferredSize += padding;

                if ((controlAxes & Mode.Horizontal) != 0) {
                    _selfRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                    if (resizeTextObject) {
                        _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
                    }
                }
                if ((controlAxes & Mode.Vertical) != 0) {
                    _selfRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                    if (resizeTextObject) {
                        _textRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
                    }
                }

                _lastText = text.text;
                _lastSize = _selfRectTransform.rect.size;
                _lastControlAxes = controlAxes;
                _forceRefresh = false;
            }
        }

        // Forces a size recalculation on next Update
        public virtual void Refresh() {
            _forceRefresh = true;

            _isTextNull = text == null;
            if (text) _textRectTransform = text.GetComponent<RectTransform>();
            _selfRectTransform = GetComponent<RectTransform>();
        }

    }
}

