using UnityEngine;
// using UnityEngine.UI;
using System;
//点击事件处理类；
public class OnClickedEventHandler : MonoBehaviour
{
    //AR场景中，二级沙盘上的树点击事件处理；
    public void TreeOnClicked()
    {
        // MainScript.statusFlag = 6;
        // Debug.Log("TreeOnClicked()");
    }
    //AR场景中，一级沙盘上的community圆盘点击事件处理；
    public void CommunityOnClicked()
    {
        // MainScript.statusFlag = 5;
        // Debug.Log("CommunityOnClicked()");
    }
}

// public class OpenScanButton : MonoBehaviour
// {    
//     public GameObject clusterChoice;
//     //public GameObject clusterContent;
//     public GameObject startBtn;
//     public GameObject restartBtn;
//     public Text tipsText;
//     //
//     //public Button OpenScanButton;
//     /* public void OpenScanButtonOnClicked()
//     {
//         MainScript.statusFlag = 112;
//         Destroy(this);
//     } */
//     void OpenScanButtonOnClicked(GameObject obj)
//     {
//         MainScript.statusFlag = 2;
//         startBtn.SetActive(true);
//         restartBtn.SetActive(true);
//         obj.SetActive(false);
//         tipsText.text="请对准要扫面的页面，在字体清晰可见时，点击确定按钮，将进行拍照识别关键词";

//     }
//     void Start()
//     {
//         //获取按钮游戏对象
//         GameObject btnObj = GameObject.Find("Canvas/OpenScanButton");
//         //获取按钮脚本组件
//         Button btn = (Button)btnObj.GetComponent<Button>();
//         //添加点击侦听
//         btn.onClick.AddListener(delegate ()
//         {
//             DestroyClusterChoice();
//             OpenScanButtonOnClicked(btnObj);
//         });
//     }
//     void DestroyClusterChoice(){
//         /* Button[] p= clusterContent.GetComponents<Button>();
//         for (int i=1;i<=p.Length;i++){
// Debug.Log(p[i].gameObject);
//             //Destroy(p[i].gameObject);
//         }
//         clusterChoice.SetActive(false); */
//         ClusterButtonObj[] clusterButtonObjs = clusterChoice.GetComponentsInChildren<ClusterButtonObj>();
//         foreach (var itemClusterButton in clusterButtonObjs)
//         {
//             Destroy(itemClusterButton.gameObject);
//         }

//         clusterChoice.SetActive(false);
//     }

// } 