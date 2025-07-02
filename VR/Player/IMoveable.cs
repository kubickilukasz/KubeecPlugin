using Kubeec.VR.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Player {
    public interface IMoveable {

        public bool CanMove();

        public Rigidbody GetRigidbody();

        public bool PerformOnCloseAction(HandInteractor hand);

    }
}
