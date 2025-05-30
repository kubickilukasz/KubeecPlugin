using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Kubeec.General {

    public class AudioPool : CustomMultiplePools<AudioObject> {

        public static AudioObject Get(IAudioReference audioReference, Transform parent = null) {
            AudioObject output = Get(audioReference.audioObject, parent);
            output.Init(audioReference);
            return output;
        }

        public static void Release(IAudioReference audioReference, AudioObject output) {
            if (audioReference != null) {
                if (output != null && output.audioSource != null) {
                    output.audioSource.volume = audioReference.audioObject.audioSource.volume;
                    output.audioSource.pitch = audioReference.audioObject.audioSource.pitch;
                    output.audioSource.loop = audioReference.audioObject.audioSource.loop;
                }
                Release(audioReference.audioObject, output);
            }
        }

    }

}
