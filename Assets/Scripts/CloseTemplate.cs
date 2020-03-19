using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这个是挂在点击小球后展开的面板的右上角的那个关闭按钮上的，用于作为button的点击响应脚本，执行CloseTemp函数

public class CloseTemplate : MonoBehaviour
{
    public GameObject scrollView;

    public void CloseTemp()
    {
        scrollView.SetActive(false);
    }
}
