
using UnityEngine;
using UnityEngine.UI;

// 这个就是挂在展开的蓝色面板中的一个一个的知识碎片的脚本，用于使用webview插件打开知识碎片的页面

public class OnMessageButtonClicked : MonoBehaviour
{
    public string messageValue;

    UniWebView webView;

    public void onMessageButtonClicked()
    {
        var webViewGameObject = new GameObject("UniWebView");
        webView = webViewGameObject.AddComponent<UniWebView>();
        webView.insets = new UniWebViewEdgeInsets(0, 0, 0, 0);
        webView.toolBarShow = true;
        webView.Load("http://" + messageValue);
        webView.Show();
    }
}
