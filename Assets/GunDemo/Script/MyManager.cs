using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 记录当前管理器update函数执行操作的状态
public enum NowStatus
{
    None, // 无操作
    Push, //执行操作改变
    Do //执行操作细节
}

public class MyManager : MonoBehaviour
{
    // 当前选中的组件
    private Element nowElement = null;
    public Element NowElement
    {
        get { return nowElement; }
        set { nowElement = value; }
    }
    // 操作执行速度
    private float speed = 1.0f;
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    // 当前的相机控制器
    private CameraControl cameraControl
    {
        get { return Camera.main.GetComponent<CameraControl>(); }
    }
    // 当前执行到哪一步
    private int nowOperate = -1;
    // 当前update操作类型
    private NowStatus nowStatus = NowStatus.None;
    // 计时器
    private float timer = 0;

    void Update ()
    {
		if(nowStatus == NowStatus.Push)
        {
            // 设定好当前要进行的操作并开始进行
            nowOperate++;
            if (nowOperate<NowElement.List.list.Count)
            {
                nowStatus = NowStatus.Do;
            }
            else
            {
                // 操作结束，回调
                nowStatus = NowStatus.None;
                nowOperate = -1;
                NowElement.Finish();
                cameraControl.BackToNormal();
            }
        }
        else if (nowStatus == NowStatus.Do)
        {
            DoSth(NowElement.List.list[nowOperate]);
        }
    }

    // 开始执行操作列表
    public void DoOperateList()
    {
        if(NowElement!=null && NowElement.CanOperate)
        {
            nowOperate = -1;
            nowStatus = NowStatus.Push;
        }
    }

    // 按照操作类型执行
    private void DoSth(Operate op)
    {
        Transform t = op.Object;
        if (t == null)
            t = nowElement.transform;

        switch (op.Type)
        {
            case OperateType.Translate:
                t.localPosition+=(op.EndPosition-op.StartPosition) / op.Time * Speed * Time.deltaTime;
                break;
            case OperateType.Rotate:    // 要把局部坐标和向量转化为世界坐标
                t.RotateAround(op.pointSpace==Space.Local?t.TransformPoint(op.Point): op.Point,
                    op.axisSpace == Space.Local ? t.TransformDirection(op.Axis): op.Axis,
                    (op.Angle) / op.Time * Speed * Time.deltaTime);
                break;
            case OperateType.TranslateAndRotate:
                t.localPosition += (op.EndPosition - op.StartPosition) / op.Time * Speed * Time.deltaTime;
                t.RotateAround(op.pointSpace == Space.Local ? t.TransformPoint(op.Point) : op.Point,
                    op.axisSpace == Space.Local ? t.TransformDirection(op.Axis) : op.Axis,
                    (op.Angle) / op.Time * Speed * Time.deltaTime);
                break;
            default: break;
        }

        timer += Time.deltaTime * Speed;
        // 操作结束
        if(timer >= op.Time)
        {
            timer = 0;
            nowStatus = NowStatus.Push;
        }
    }
}
