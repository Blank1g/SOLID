using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using JUTPS.InventorySystem;

namespace JUTPS.CustomEditors
{
    [CustomEditor(typeof(JUInventory))]
    public class InventoryUIManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();

            JUInventory inventory = ((JUInventory)target);
            if (GUILayout.Button("Setup Items"))
            {
                inventory.SetupItens();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
