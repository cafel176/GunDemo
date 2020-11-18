using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OperateList))]
public class ListInspector : Editor
{
    Editor cacheEditor;
    int index = 0;

    public override void OnInspectorGUI()
    {
        // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
        serializedObject.Update();

        //显示operatelist的默认UI
        base.OnInspectorGUI();

        GUILayout.Space(20);
        var list = ((OperateList)target).list;
        GUILayout.Label("Edit operate list, total num: " + list.Count);

        GUILayout.Space(10);
        for (int i = 0; i < list.Count; i++)
        {
            GUILayout.Space(10);
            var e = list[i];
            if (e != null)
            {
                //创建operate的Editor
                GUILayout.Label("Operate " + i + ":");
                e.Type = (OperateType)EditorGUILayout.EnumPopup("Type", e.Type);
                e.Time = EditorGUILayout.FloatField("Time", e.Time);
                if (e.Type == OperateType.Translate)// Translate的参数全部填写局部坐标
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Object");
                    // 操作对象，不写则默认是自己
                    e.Object = (Transform)EditorGUILayout.ObjectField(e.Object, typeof(Transform));
                    EditorGUILayout.EndHorizontal();

                    e.StartPosition = EditorGUILayout.Vector3Field("StartPosition", e.StartPosition);
                    e.EndPosition = EditorGUILayout.Vector3Field("EndPosition", e.EndPosition);
                }
                else if (e.Type == OperateType.Rotate)// Rotate的参数可以选择坐标系
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Object");
                    e.Object = (Transform)EditorGUILayout.ObjectField(e.Object, typeof(Transform));
                    EditorGUILayout.EndHorizontal();

                    e.pointSpace = (Space)EditorGUILayout.EnumPopup("PointSpace", e.pointSpace);
                    e.Point = EditorGUILayout.Vector3Field("Point", e.Point);

                    e.axisSpace = (Space)EditorGUILayout.EnumPopup("AxisSpace", e.axisSpace);
                    e.Axis = EditorGUILayout.Vector3Field("Axis", e.Axis);

                    e.Angle = EditorGUILayout.FloatField("Angle", e.Angle);
                }
                else if (e.Type == OperateType.TranslateAndRotate)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Object");
                    e.Object = (Transform)EditorGUILayout.ObjectField(e.Object, typeof(Transform));
                    EditorGUILayout.EndHorizontal();

                    e.StartPosition = EditorGUILayout.Vector3Field("StartPosition", e.StartPosition);
                    e.EndPosition = EditorGUILayout.Vector3Field("EndPosition", e.EndPosition);

                    e.pointSpace = (Space)EditorGUILayout.EnumPopup("PointSpace", e.pointSpace);
                    e.Point = EditorGUILayout.Vector3Field("Point", e.Point);

                    e.axisSpace = (Space)EditorGUILayout.EnumPopup("AxisSpace", e.axisSpace);
                    e.Axis = EditorGUILayout.Vector3Field("Axis", e.Axis);

                    e.Angle = EditorGUILayout.FloatField("Angle", e.Angle);
                }
            }
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Add Operate"))
        {
            list.Add(new Operate());
        }
        EditorGUILayout.BeginHorizontal();
        index = EditorGUILayout.IntField("Delete N0.", index);
        if (GUILayout.Button("Delete"))
        {
            if (list.Count > 0)
                list.RemoveAt(index);
        }
        EditorGUILayout.EndHorizontal();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
