using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * 类、方法名、public变量、常量、枚举名和枚举值 所有单词开头字母大写(Pascal规则)
 * protected/private变量、方法参数 都是开头字母小写，第二个单词开始大写(camel规则)
 */

public enum CameraType
{
    None,  //不进行任何操作
    Normal, // 自由模式，没有选中可操作组件时
    Focus, // 聚焦模式，选中可操作组件时
    Auto  // 自动模式，当进行动画时
}

public class CameraControl : MonoBehaviour
{
    // 控制平移速度
    public float MoveSpeed = 2.0f;
    // 控制旋转速度
    public float RotateSpeed = 3.0f;
    // 聚焦中心的位置
    public Vector3 FocusPos = Vector3.zero;
    // 相机沿着弧线运动的变换时间
    public float translateTime = 0.5f;
    // 此参数控制相机沿着弧线运动的弧度，值越大弧度越小
    public float offset = 0.5f;

    // 当前相机的操作模式
    private CameraType nowType = CameraType.Normal;
    public CameraType NowType
    {
        get { return nowType; }
        set { nowType = value; }
    }

    // 当前操纵的相机
    private Camera mainCamera
    {
        get { return Camera.main; }
    }
    // 当前流程管理器
    private MyManager manager
    {
        get
        {
            var g = GameObject.FindGameObjectWithTag("Manager");
            return g.GetComponent<MyManager>();
        }
    }
    // 记录当前相机的位置和方向
    private Vector3 nowPos, nowRotation;
    // 相机沿着弧线运动所需参数
    private Vector3 center, startRelCenter, endRelCenter,startRotation,endRotation;
    // 相机沿着弧线运动的计时器
    private float timer=0;
    // 转换完成后的操作模式
    private CameraType targetType;

    void Start ()
    {
        // 记录当前相机位置和方向
        nowPos = mainCamera.transform.position;
        nowRotation = mainCamera.transform.rotation.eulerAngles;
    }

	void Update ()
    {
        // 空模式下，禁用一切操作
        if (NowType == CameraType.None)
            return;
        // 自动模式下，进行相机位移
        if (NowType == CameraType.Auto)
        {            
            timer += Time.deltaTime;
            mainCamera.transform.position = Vector3.Slerp(startRelCenter, endRelCenter, timer/translateTime)+ center;
            mainCamera.transform.rotation = Quaternion.Slerp(Quaternion.Euler(startRotation), Quaternion.Euler(endRotation), timer / translateTime);

            if (timer>=translateTime)
            {
                mainCamera.transform.position = endRelCenter + center;
                mainCamera.transform.rotation = Quaternion.Euler(endRotation);
                NowType = targetType;
                timer = 0;
            }

            return;
        }

        // 获取鼠标输入
        float _mouseX = Input.GetAxis("Mouse X");
        float _mouseY = Input.GetAxis("Mouse Y");

        // 鼠标点击选择物体
        MouseChoose();

        // 普通模式下控制视图
        if (NowType == CameraType.Normal)
        {
            CameraRotate(_mouseX, _mouseY);
            CameraMove(_mouseX, _mouseY);
            CameraScale();
        }
    }

    // 鼠标点选物体的函数
    private void MouseChoose()
    {
        // 点选物体
        if (Input.GetMouseButtonDown(0))
        {
            // 点击UI时不触发射线
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            var ray = mainCamera.ScreenPointToRay(Input.mousePosition);//鼠标的屏幕坐标转化为一条射线
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var g = hit.collider.gameObject;
                // 自由模式
                if (NowType == CameraType.Normal)
                {
                    // 点击到组件
                    var element = g.gameObject.GetComponent<Element>();
                    if (element)
                    {
                        // 切换当前操作组件
                        if (manager.NowElement != null)
                            manager.NowElement.BeChoosed(false);
                        element.BeChoosed(true);
                        manager.NowElement = element;

                        // 如果该组件当前可以操作
                        if (element.CanOperate)
                        {
                            // 聚焦相机角度
                            FocusOnObject(element.CameraPos, element.CameraRotation);
                            // 显示箭头
                            element.BeClicked();
                        }
                    }
                }
                else if (NowType == CameraType.Focus)
                {
                    // 当点击箭头时
                    if (g.tag == "Arrow")
                    {
                        var element = g.transform.parent.parent.gameObject.GetComponent<Element>();
                        if (element == manager.NowElement)
                        {
                            // 点击箭头开始操作
                            element.DoOperate();
                            manager.DoOperateList();
                            NowType = CameraType.None;
                        }
                    }
                }
            }
        }
    }

    // 相机旋转
    private void CameraRotate(float _mouseX, float _mouseY)
    {
        if (Input.GetMouseButton(0))
        {
            mainCamera.transform.RotateAround(FocusPos, Vector3.up, _mouseX * RotateSpeed);
            mainCamera.transform.RotateAround(FocusPos, mainCamera.transform.right, -_mouseY * RotateSpeed);
        }
    }

    // 相机平移
    private void CameraMove(float _mouseX, float _mouseY)
    {
        if (Input.GetMouseButton(1))
        {
            Vector3 moveDir = (_mouseX * -mainCamera.transform.right + _mouseY * -mainCamera.transform.up);
            mainCamera.transform.localPosition+=moveDir * MoveSpeed;
        }
    }

    // 滚轮缩放
    private void CameraScale()
    {
        //放大
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            mainCamera.transform.localPosition -= mainCamera.transform.forward;
        }
        //缩小
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            mainCamera.transform.localPosition += mainCamera.transform.forward;
        }
    }

    // 进行相机位置的弧形切换
    private void CameraTransform(Vector3 start,Vector3 end, Vector3 startR, Vector3 endR,CameraType target)
    {
        //弧线的中心
        center = (start+end) * 0.5f;
        Vector3 centorProject = Vector3.Project(center, start-end); // 中心点在两点之间的投影
        center = Vector3.MoveTowards(center, centorProject, offset); // 沿着投影方向移动移动距离（距离越大弧度越小）                
                                                                 //相对于中心在弧线上插值
        startRelCenter = start - center;
        endRelCenter = end - center;

        startRotation = startR;
        endRotation = endR;

        NowType = CameraType.Auto;
        targetType = target;
        timer = 0;
    }

    //=================================================公开函数==================================================
    // 设置聚焦物体
    public void FocusOnObject(Vector3 pos, Vector3 rotation)
    {
        nowPos = mainCamera.transform.position;
        nowRotation = mainCamera.transform.rotation.eulerAngles;

        CameraTransform(nowPos, pos, nowRotation, rotation, CameraType.Focus);
    }

    // 回到自由模式，是管理器操作结束后的回调函数
    public void BackToNormal()
    {
        CameraTransform(mainCamera.transform.position,nowPos,mainCamera.transform.rotation.eulerAngles, nowRotation, CameraType.Normal);
    }
}
