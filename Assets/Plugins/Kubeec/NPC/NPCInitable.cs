using System.Collections;
using UnityEngine;

namespace Kubeec.NPC {
    public class NPCInitable : InitableDisposable<NonPlayerCharacter> {

        NonPlayerCharacter character;

        public NonPlayerCharacter Character => character;

        protected override void OnInit(NonPlayerCharacter data) {
            character = data;
        }

    }
}