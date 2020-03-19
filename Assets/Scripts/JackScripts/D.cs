// __Author__='Jack Luo'
// __Email__='greatlandmark@outlook.com'

// for debug
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
/*
public class D : MonoBehaviour
{
    public Text debugText;
    public static string textString;
    private int counter = 0;
    // public void Start(){}
    private void Update()
    {
        debugText.text = textString;
        if (counter++ == 100000)
        {
            textString = "";
            counter = 0;
        }

    }
    // public void OnDestroy(){}

}
*/
//debug for raycast
#if UNITY_EDITOR
public class DebugUILine : MonoBehaviour
{
    static Vector3[] fourCorners = new Vector3[4];
    void Start(){
        OnDrawGizmos();
    }
    void Update()
    {
        OnDrawGizmos();
    }
    void OnDrawGizmos()
    {
        foreach (var g in GameObject.FindObjectsOfType<MaskableGraphic>())
        {
            if (g.raycastTarget)
            {
                RectTransform rectTransform = g.transform as RectTransform;
                rectTransform.GetWorldCorners(fourCorners);
                Gizmos.color = Color.blue;
                for (int i = 0; i < 4; i++)
                    Gizmos.DrawLine(fourCorners[i], fourCorners[(i + 1) % 4]);

            }
        }
    }
}
#endif
