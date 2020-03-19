// __Author__='Jack Luo'
// __Email__='greatlandmark@outlook.com'



//mainscript调用

using UnityEngine;

/* 
手势控制，等触控。
 */
public class TouchControl : MonoBehaviour
{
    public static void FingersControl()
    {
        if (Input.touchCount <= 0)
        {
            return;
        }

        // 手势控制
        //单点触摸控制旋转

        if (1 == Input.touchCount)
        {

            Debug.Log("单手控制");
            Touch touch = Input.GetTouch(0);
            Vector2 deltaPos = touch.deltaPosition;
            MainScript.treeTransform.Rotate(Vector3.down * deltaPos.x / MainScript.ROTATE_FECTOR, Space.World);
            Debug.Log("成功旋转");
            return;
        }

        //多点触摸控制防缩 
        Touch newTouch1 = Input.GetTouch(0);
        Touch newTouch2 = Input.GetTouch(1);

        if (newTouch2.phase == TouchPhase.Began)
        {
            Debug.Log("双手控制");
            MainScript.oldTouch2 = newTouch2;
            MainScript.oldTouch1 = newTouch1;
            return;
        }

        float oldDistance = Vector2.Distance(MainScript.oldTouch1.position, MainScript.oldTouch2.position);
        float newDistance = Vector2.Distance(newTouch1.position, newTouch2.position);
        float offset = newDistance - oldDistance;

        float scaleFactor = offset / MainScript.SCALE_FECTOR;

        Vector3 localScale = MainScript.treeTransform.localScale;
        Vector3 scale = new Vector3(localScale.x + scaleFactor, localScale.y + scaleFactor, localScale.z + scaleFactor);

        if (scale.x > MainScript.SCALE_MIN_FECTOR && scale.y > MainScript.SCALE_MIN_FECTOR 
        && scale.z > MainScript.SCALE_MIN_FECTOR && scale.x < MainScript.SCALE_MAX_FECTOR 
        && scale.y < MainScript.SCALE_MAX_FECTOR && scale.z < MainScript.SCALE_MAX_FECTOR)
        {
            MainScript.treeTransform.localScale = scale;
            Debug.Log("成功缩放");
        }

        MainScript.oldTouch1 = newTouch1;
        MainScript.oldTouch2 = newTouch2;

    }
}