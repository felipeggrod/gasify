using UnityEngine;

namespace GAS {
    public static class AnimationCurveUtils {
        public static AnimationCurve QuadraticEaseOutCurve(float duration, float value) {
            Keyframe[] keys = new Keyframe[2];

            keys[0] = new Keyframe(0f, 0f, 0f, 2f); // The second control point handles the ease-out
            keys[1] = new Keyframe(duration, value);

            return new AnimationCurve(keys);
        }

        public static AnimationCurve CubicEaseOutCurve(float duration, float value) {
            Keyframe[] keys = new Keyframe[2];

            keys[0] = new Keyframe(0f, 0f, 0f, 3f); // The second control point handles the ease-out
            keys[1] = new Keyframe(duration, value);

            return new AnimationCurve(keys);
        }

        public static AnimationCurve QuadraticEaseInCurve(float duration, float value) {
            Keyframe[] keys = new Keyframe[2];

            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(duration, value, 2f, 0f); // The first control point handles the ease-in

            return new AnimationCurve(keys);
        }

        public static AnimationCurve CubicEaseInCurve(float duration, float value) {
            Keyframe[] keys = new Keyframe[2];

            keys[0] = new Keyframe(0f, 0f);
            keys[1] = new Keyframe(duration, value, 3f, 0f); // The first control point handles the ease-in

            return new AnimationCurve(keys);
        }

        public static AnimationCurve ExponentialCurve(float duration, float value, float exponent) {
            Keyframe[] keys = new Keyframe[2];

            keys[0] = new Keyframe(0f, 0f, Mathf.Pow(2f, exponent), 0f); // The first control point handles the ease-in
            keys[1] = new Keyframe(duration, value);

            return new AnimationCurve(keys);
        }

        public static AnimationCurve LogarithmicCurve(float duration, float value, float baseValue) {
            Keyframe[] keys = new Keyframe[2];

            keys[0] = new Keyframe(0f, 0f, 0f, Mathf.Log(baseValue)); // The second control point handles the ease-out
            keys[1] = new Keyframe(duration, value);

            return new AnimationCurve(keys);
        }

        public static AnimationCurve QuadraticCurve(float initialTime, float initialValue, float duration, float finalValue) {
            Keyframe[] keys = new Keyframe[2];

            keys[0] = new Keyframe(initialTime, initialValue, 2f, 0f); // The second control point handles the ease-out
            keys[1] = new Keyframe(duration, finalValue);

            return new AnimationCurve(keys);
        }
    }
}