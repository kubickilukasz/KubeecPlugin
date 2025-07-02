using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {

    public class PanelController : EnableDisableInitableDisposable{

        public event Action<Panel> onShowPanel;

        [SerializeField] Panel defaultPanel;
        [SerializeField] bool showDefaultOnInit = false;

        [HideInInspector] public Panel currentActivePanel;

        Panel [] panels;

        protected override void OnInit(object data = null) {
            panels = GetComponentsInChildren<Panel>(true);
            foreach (Panel panel in panels) {
                panel.Init();
                panel.onShow += OnShowPanel;
                panel.gameObject.SetActive(true);
                panel.Hide();
            }
            if (showDefaultOnInit) {
                defaultPanel.Show();
            }
        }

        protected override void OnDispose() {
            foreach (Panel panel in panels) {
                panel.onShow -= OnShowPanel;
            }
            panels = new Panel[0];
        }

        public void Toggle() {
            if (currentActivePanel != null && currentActivePanel.IsShown) {
                Hide();
            } else {
                Show();
            }
        }

        public void Show() {
            if (currentActivePanel != null) {
                currentActivePanel.Show();
            } else if (defaultPanel != null) {
                defaultPanel.Show();
            }
        }

        public void Hide() {
            if (currentActivePanel != null) {
                currentActivePanel.Hide();
                currentActivePanel = null;
            }
        }

        public T GetPanel<T>() where T : Panel {
            foreach (Panel panel in panels) {
                if (panel is T p) {
                    return p;
                }
            }
            return null;
        }

        public IEnumerable<Panel> GetPanels() {
            foreach (Panel panel in panels) {
                yield return panel;

            }
        }

        void OnShowPanel() {
            foreach (Panel panel in panels) {
                if (panel.IsShown) {
                    onShowPanel?.Invoke(panel);
                }
            }
        }

    }

}
