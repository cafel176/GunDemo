using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Element : MonoBehaviour
{
    // 操作该组件时的提示箭头
    public GameObject Hint;
    // 用于记录被选中时相机看向该组件的状态
    public Vector3 CameraPos;
    public Vector3 CameraRotation;
    // 该组件要执行的操作
    public OperateList List;
    // 组件是否可以操作
    private bool canOperate = false;
    public bool CanOperate
    {
        get { return canOperate; }
        set { canOperate = value; }
    }

    void Start ()
    {
        if(Hint != null)
        {
            Hint.SetActive(false);
            // 通过层级结构来判断可操作组件
            if (CheckCanOperate(transform))
            {
                CanOperate = true;
            }
        }
    }
	
    // 当组件本身被点击时
    public void BeClicked()
    {
        Hint.SetActive(true);
    }

    // 当箭头被点击时
    public void DoOperate()
    {
        Hint.SetActive(false);
    }

    // 组件被选中时变色
    public void BeChoosed(bool choosed)
    {
        if(choosed)
        {
            gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0.5441177f, 0.3567668f, 0.1560337f, 1));
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material.SetColor("_EmissionColor", new Color(0, 0, 0, 1));
        }
    }

    // 完成操作时的回调
    public void Finish()
    {
        CanOperate = false;
        if (CheckCanOperate(transform.parent))
        {
            // 设置父节点
            if (transform.parent.tag == "Root")
            {
                // 是根节点，组装完成
                Debug.Log("finish");
            }
            else
            {
                // 父节点可操作，则设置操作
                if(transform.parent.GetComponent<Element>().Hint)
                    transform.parent.GetComponent<Element>().CanOperate = true;
                else// 父节点不可操作，则finish
                    transform.parent.GetComponent<Element>().Finish();
            }
        }
    }

    // 从层级关系角度检查任意一个节点是否是可操作的
    private bool CheckCanOperate(Transform t)
    {
        bool root = (t.tag == "Root");
        var childs = t.GetComponentsInChildren<Element>();
        for (int i = root?0:1; i < childs.Length; i++)// 0一般是自己，跳过
        {
            if (childs[i].CanOperate)
                return false;
        }
        return true;
    }
}
