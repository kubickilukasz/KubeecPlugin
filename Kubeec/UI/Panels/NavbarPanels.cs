using System.Collections.Generic;
using UnityEngine;

namespace UI {

    public class NavbarPanels : EnableDisableInitableDisposable {

        [SerializeField] ToggleButton prefabButton;
        [SerializeField] RectTransform content;
        [SerializeField] PanelController panelController;

        Dictionary<Panel, ToggleButton> toggleButtons = new();

        public void RefreshButtons() {
            ClearButtons();
            foreach (Panel panel in panelController.GetPanels()) {
                ToggleButton current = Instantiate(prefabButton);
                current.transform.SetParent(content);
                if (panelController.currentActivePanel.Equals(panel)) {
                    current.SetValue(true, false);
                } else {
                    current.SetValue(false, false);
                }
                current.onActionBool += value => {
                    OnChangeClicked(panel, value);
                };
                toggleButtons.Add(panel, current);
            }
        }

        public void ClearButtons() {
            foreach (ToggleButton button in toggleButtons.Values) {
                Destroy(button.gameObject);
            }
            toggleButtons.Clear();
        }

        protected override void OnInit(object data) {
            RefreshButtons();
            panelController.onShowPanel += OnShowPanel;
        }

        protected override void OnDispose() {
            ClearButtons();
            if (panelController) {
                panelController.onShowPanel -= OnShowPanel;
            }
        }

        void OnChangeClicked(Panel panel, bool value) {
            if (value) {
                panel.Show();
            }
        }

        void OnShowPanel(Panel panel) {
            foreach (KeyValuePair<Panel, ToggleButton> value in toggleButtons) {
                value.Value.SetValue(value.Key.Equals(panel), false);
            }
        }
    }

}
