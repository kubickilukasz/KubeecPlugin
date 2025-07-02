using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UI {

    public class ScrollList : EnableDisableInitableDisposable {

        [SerializeField] List<RectTransform> elementsCanLeft = new List<RectTransform>();
        [SerializeField] List<RectTransform> elementsCanRight = new List<RectTransform>();
        [SerializeField] List<Element> elements = new List<Element>();
        [SerializeField] RectTransform content;

        [Space]

        [SerializeField] int currentElement = 0;
        [SerializeField] bool loop = false;
        [SerializeField] Vector2 moveDirection = new Vector2(1, 0);
        [SerializeField] float moveMultiplier = 2;
        [SerializeField] float duration = 0.5f;
        [SerializeField] Ease ease = Ease.InOutSine;

        Sequence sequence;
        bool isMoving = false;

        public int CurrentElement => currentElement;
        public bool IsMoving => isMoving;

        protected override void OnInit(object data) {
            RefreshElements();
            SetStartPositions();
        }

        protected override void OnDispose() {
            base.OnDispose();
            KillSequence();
        }

        public void GoLeft() {
            Go(-1);
        }

        public void GoRight() {
            Go(1);
        }

        public void Go(int dir) {
            KillSequence();
            Element elementToHide = elements[currentElement];
            int newIndex = GetIndex(dir);
            if (currentElement == newIndex) {
                return;
            }
            currentElement = newIndex;
            Element element = elements[newIndex];
            Vector2 newPos = content.anchoredPosition + moveDirection * moveMultiplier;
            element.rectTransform.anchoredPosition = newPos * dir;
            elementToHide.rectTransform.anchoredPosition = content.anchoredPosition;
            isMoving = true;
            sequence = DOTween.Sequence();
            sequence.Append(elementToHide.rectTransform.DOAnchorPos(newPos * -dir, duration, true));
            sequence.Insert(0f, element.rectTransform.DOAnchorPos(content.anchoredPosition, duration, true));
            sequence.SetEase(ease);
            sequence.OnComplete(() => isMoving = false);
            RefreshElements();
        }

        int GetIndex(int dir) {
            int newIndex = currentElement;
            newIndex += dir;
            if (newIndex < 0 || newIndex >= elements.Count) {
                if (loop) {
                    int mod = newIndex % elements.Count;
                    if (mod < 0) {
                        mod = elements.Count + mod;
                    }
                    newIndex = mod;
                } else {
                    newIndex = Mathf.Clamp(newIndex, 0, elements.Count - 1);
                }
            }
            return newIndex;
        }

        void SetStartPositions() {
            Vector2 hidePos = content.anchoredPosition + moveDirection * moveMultiplier;
            elements.ForEach(x => x.rectTransform.anchoredPosition = hidePos);
            elements[GetIndex(0)].rectTransform.anchoredPosition = content.anchoredPosition;
        }

        void RefreshElements() {
            bool canLeft = loop || currentElement > 0;
            bool canRight = loop || currentElement < elements.Count - 1;
            elementsCanLeft.ForEach(x => x.gameObject.SetActive(canLeft));
            elementsCanRight.ForEach(x => x.gameObject.SetActive(canRight));
        }

        void KillSequence() {
            isMoving = false;
            if (sequence != null) {
                sequence.Complete();
                sequence = null;
            }
        }

        [Serializable]
        public class Element {
            public RectTransform rectTransform;
        }

    }

}
