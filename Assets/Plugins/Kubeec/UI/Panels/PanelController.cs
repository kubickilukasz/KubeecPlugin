using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI {

    public class PanelController : MonoBehaviour{

        [SerializeField] Panel defaultPanel;

        [HideInInspector] public Panel currentActivePanel;

        Panel [] panels;

        void Start() {
            panels = GetComponentsInChildren<Panel>(true);
            foreach (Panel panel in panels) {
                panel.gameObject.SetActive(true);
                panel.Hide();
            }
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
            } else if (defaultPanel != null) {
                defaultPanel.Hide();
            }
        }

        public T GetPanel<T>() where T : Panel {
            foreach (Panel panel in panels) {
                if (panel is T) {
                    return (T)panel;
                }
            }
            return null;
        }

    }

}
