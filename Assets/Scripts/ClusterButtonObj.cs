using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 这个是挂在跟踪框框的那个按钮上的脚本，就是主题的那个蓝色的跟踪框button，点击button会触发这个脚本下的onClusterButtonClicked()
// 从而调用mainscript的NewAPI2Start函数，出入参数

public class ClusterButtonObj : MonoBehaviour
{
    public string clusterName;
    public MainScript mainScript;

    public void onClusterButtonClicked()
    {
        MainScript.statusFlag=4;
        mainScript.NewAPI2Start(clusterName);
    }

}
