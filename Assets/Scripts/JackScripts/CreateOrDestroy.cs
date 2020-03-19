using LitJson;
using UnityEngine;
using UnityEngine.UI;


public class CreateOrDestroy:MonoBehaviour
{
    private static MainScript mainScript;
    //多门课程。展示课程列表。
    public static void ShowCoursesList()
    {
        GameObject  clusterChoice=GameObject.FindGameObjectWithTag("ClusterChoice");
        //D.textString="clusterChoice:"+clusterChoice.ToString();
        //GameObject clusterContent=clusterChoice.GetComponentInChildren<GameObject>();
        Transform[] p=clusterChoice.GetComponentsInChildren<Transform>();
        /* foreach( var i in p){
             //.gameObject;
        Debug.Log("clusterContent:"+i.ToString());
        
        } */
        GameObject clusterContent=p[4].gameObject;
        Debug.Log("clusterContent:"+clusterContent.ToString());
       //D.textString="clusterContent:"+clusterContent.ToString();
        Debug.Log("OCR_RESULT:"+MainScript.OCR_RESULT);
        JsonData resultJson = JsonMapper.ToObject(MainScript.OCR_RESULT);
        JsonData resultID = resultJson["result_id"];
        JsonData resultsNum = resultJson["result_num"];
        int clustersNum = int.Parse(resultsNum.ToString());
        JsonData resultsClusters = resultJson["results"][0];

        clusterChoice.SetActive(true);

        clusterContent.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
        clusterContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100 * clustersNum);

        Button[] clusterButtons = new Button[clustersNum];

        for (int rc = 0; rc < resultsClusters.Count; rc++)
        {
            clusterButtons[rc] = Object.Instantiate<Button>(mainScript.clusterButtonPrefab);

            clusterButtons[rc].transform.parent =clusterContent.transform;
            clusterButtons[rc].GetComponentInChildren<Text>().text = resultsClusters[rc].ToString();
            clusterButtons[rc].GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, -100 * rc + 50 * clustersNum - 50, 0);

            ClusterButtonObj clusterButtonObj = clusterButtons[rc].GetComponentInChildren<ClusterButtonObj>();
            clusterButtonObj.clusterName = resultsClusters[rc].ToString();
            clusterButtonObj.mainScript = mainScript;
        }
    }
}