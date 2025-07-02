using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kubeec.VR.Outline {

    public interface IOutlineable {

        public bool CanOutline();

        public IEnumerable<OutlineObject> GetOutlineObjects(Vector3 source);

    }

}
    
