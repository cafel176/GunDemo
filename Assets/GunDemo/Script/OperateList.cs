using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Space
{
    Local,
    World
}

public enum OperateType
{
    Wait,
    Translate,
    Rotate,
    TranslateAndRotate
}

[System.Serializable]
public class Operate
{
    // 操作类型
    public OperateType Type;
    // 操作时间和操作对象
    public float Time=1.0f;
    public Transform Object=null;
    // 平移目标开始和结束位置
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    // 旋转点，轴和角度
    public Space pointSpace;
    public Vector3 Point;
    public Space axisSpace;
    public Vector3 Axis;
    public float Angle;
}

[CreateAssetMenu]
public class OperateList : ScriptableObject
{
    // 操作列表
    [HideInInspector]
    public List<Operate> list = new List<Operate>();
}
