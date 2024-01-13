using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;
using EasyButtons;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GAS {
    public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
#endif
    public class ReadOnlyDrawer
#if UNITY_EDITOR
    : PropertyDrawer
#endif
    {
#if UNITY_EDITOR
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
#endif
    }

}