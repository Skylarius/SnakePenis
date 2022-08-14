using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DataPersistenceManager))]
public class DataPersistenceManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DataPersistenceManager script = (DataPersistenceManager)target;
        if (GUILayout.Button("Open File Location"))
        {
            script.OpenDataFileLocation();
        }
    }

}
