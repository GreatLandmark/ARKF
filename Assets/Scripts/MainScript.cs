using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;
using System.Threading;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.TrackingModule;
using LitJson;
using UnityEngine.UI;
using System.Net;
using System.IO;
using System.Text;
using System;
using System.Collections;
using UnityEngine.EventSystems;

public class MainScript : MonoBehaviour
{
    /* 
     * 参数部分
     * */
    #region PARAMETERS
    /*
     * 
     * */
    /*  
    statusFlag 标识 系统所处状态：(0)零初始状态,（1）初始界面,可选的课程列表和扫面页面按钮，(2)相机扫描界面，
    (3)选择课程界面，(4)第一级沙盘场景,（5）第二级沙盘，（6）树场景，（7）碎片详情网页界面，
    (8)关键词追踪界面，。。。（999）其他状态。 
    */
    public static int statusFlag = 0;
    //启动界面
    //private string initialCourses = "{                         \"log_id\": 2794890503604904799,                         \"direction\": 2,                         \"words_result_num\": 8,                         \"words_result\": [                             {                             \"location\": {                                 \"width\": 122,                                 \"top\": 352,                                 \"left\": 424,                                 \"height\": 34                             },                             \"words\": \"数据结构\"                             },                             {                             \"location\": {                                 \"width\": 125,                                 \"top\": 293,                                 \"left\": 308,                                 \"height\": 35                             },                             \"words\": \"函数\"                             },                             {                             \"location\": {                                 \"width\": 157,                                 \"top\": 231,                                 \"left\": 72,                                 \"height\": 36                             },                             \"words\": \"计算机视觉\"                             },                             {                             \"location\": {                                 \"width\": 67,                                 \"top\": 169,                                 \"left\": 365,                                 \"height\": 35                             },                             \"words\": \"算法\"                             },                             {                             \"location\": {                                 \"width\": 126,                                 \"top\": 107,                                 \"left\": 218,                                 \"height\": 33                             },                             \"words\": \"这是C语言的\"                             }                         ]                         }";
    //private string initialCoursesJson="     {"id":0 ,"items_num":1 ,"courses_name_list":["计算机组成原理","C语言","操作系统","计算机系统结构","数据结构","低年级(1-2)语文","低年级(1-2)科学","高年级(5-6)数学","高二数学","高一语文"]};
    private string coursesNameList;
    public GameObject openScanButton; //启动界面中，扫描页面获取学习内容的按钮；
    public GameObject guidanceButton;// how to use button;
    string guidanceUrl;
    public Text tipsText;    // 主屏幕提示符
    public GameObject startButton;  // 确定按钮
    public GameObject restartButton;    // 返回按钮

    /*
     * 百度OCR接口部分
     * */

    // 设置百度接口数据
    //GetDataFromWeb.cs
    private Texture2D frameOCR; // 用于OCR分析的相机帧数据
    public static bool ocrThreadFlag = false;    // OCR线程标志位，为true表示线程结束
    public static string OCR_RESULT = "";  // 存储 OCR文字识别再通过API判断后 的结果的全局变量
    const int OCR_IMAGE_QUALITY = 40;
    /*
     * Vuforia部分
     * */

    public GameObject groundPlaneFinder;    // Ground Plane Finder
    public GameObject arCamera; // ARCamera

    private Texture tempFrame;  // Vuforia相机取帧暂存


    /*
     * OpenCV追踪部分
     * */

    private int TrackingFlag = 0;    // 定义追踪状态的flag，0表示未开始追踪，1表示初始化，2表示更新追踪，3保留
    private Texture2D trackingFrameOld; // 处理前的帧数据
    private Texture2D trackingFrameNew; // 处理后的帧数据
    private Mat trackingFrame;  // 处理追踪的Mat帧
    private Mat trackingFrameGray;  // 计算跟踪的灰度图
    private TrackerMOSSE trackers;  // 定义追踪器
    private Rect2d trackingWindow;  // 定义追踪框
    private Scalar scalar = new Scalar(64, 157, 248);  // 追踪框的好 颜色值
    private Point xyLow;    // 定义追踪框的左上角坐标
    private Point xyHigh;   // 定义追踪框的右下角坐标
    private int TimeFlag;
    private int TIMEFLAG = 3;

    /*
     * Collider按钮部分
     * */
    public GameObject cubeButtonPrefab; // 按钮Prefab

    private double[] points;    // 文字区域及关键词位置信息
    private string[] items; // 关键词内容
    private Point xyKeywordsLow = new Point();  // 关键词的坐标
    private Point xyKeywordsHigh = new Point(); // 关键词的坐标
    private GameObject[] cubeButton;    // 实例化后的按钮数组
    private string domainID;    // 存储domain id


    /*
     * 生成树部分
     * */

    public GameObject groundPlaneStage; // ground plane stage
    public GameObject trunk;    // 树干Prefab
    public GameObject b1_1; // 一级存放球的分支
    public GameObject b2_1; // 二级分枝，带球
    public GameObject b2_2;
    public GameObject b2_3;
    public GameObject b2_4;
    public GameObject b2_5;
    public GameObject b3_1; // 三级分支，带球
    public GameObject b3_2;
    public GameObject b3_3;
    public GameObject b3_4;
    public GameObject b3_5;
    public GameObject b3_6;
    private Dictionary<int, Dictionary<int, string[]>> leaves;    // 存储数据结构的字典
    const float ORIGIN_TREE_SCALE_FACTER = 20f;
    public Text CourseNameInTree;   // 用于在树的场景下显示课程名
    public GameObject CourseNameInTreeImage; // 用于在树的场景下显示课程名的底牌

    /*
     * 面板展开部分
     * */

    public ScrollRect scrollView;   // 滚动视图
    public Button textMessagePrefab;    // 可供点击的message button
    public GameObject content;
    public Text tittleTextObj;  // 写标题的Text
    private GameObject clickedSphere;   // 点击物体
    private bool clickedFlag = false;   // 标志位，为true表示有球被点击
    private Vector2 screenPos;  // 转换坐标
    private Vector2 screenScale;    // 转换坐标
    private Transform platformPoint;    // 球上的面板点
    private Button[] textButton;    // 可点击的button集合

    /*
     * 手势控制部分
     * */

    public static int ROTATE_FECTOR = 8;
    public static int SCALE_FECTOR = 10000;
    public static float SCALE_MIN_FECTOR = 0.02f;
    public static float SCALE_MAX_FECTOR = 0.5f;
    public static Touch oldTouch1;  //上次触摸点1(手指1)  
    public static Touch oldTouch2;  //上次触摸点2(手指2)  
    public static Transform treeTransform;    // 生成树的Transform


    /*
     * cluster 选择部分
     * */

    public GameObject clusterChoice;    // cluster choice UI
    public ScrollRect clusterScrollRect;    // 滚动视图
    public Button clusterButtonPrefab;  // cluster的按钮
    public GameObject clusterContent;   // clusterContent
    public MainScript selfClass;    // 我调用我自己


    /*
     * 构建沙盘
     * */

    public GameObject shapanPrefab;
    public GameObject[] treeModels;
    public GameObject[] treeModeltars;
    public LineRenderer line;
    public GameObject[] clusterBases;

    const float XMAX = 0.4f;
    const float ZMAX = 0.27f;
    const float TREEMAXSIZE = 3;
    const float PI = (float)Math.PI;
    const float PERIMETER = XMAX * ZMAX * PI;

    private JsonData[] community;
    private int communityNum;

    private GameObject shapan;
    private MyClusterBase[] myClusterBase;
    private LineRenderer[] bigLines;    // 连接簇之间的线
    public GameObject returnFromSecondStateButton;    // 从沙盘第二层返回按钮
    public GameObject returnFromTreeButton;    // 从树返回沙盘当场景
    public GameObject returnFromTreeToShapanButton;    // 从树直接返回沙盘当场景

    private GameObject[] littleTreesinPartTwo;

    private bool returntoShapan = false;
    private string treeReturnToShapanName; // 记录从树返回到沙盘到沙盘名称

    private GameObject shapanBig;   // 整个沙盘
    private GameObject treeTrunkFromShapan;
    private GameObject mytrunk;

    public GameObject linesParent;  // 用于存储连线的GameObject

    //public Canvas canvas;
    #endregion

    /*
     * 函数部分
     * */
    void Awake()
    {
        //VuforiaRenderer.Instance.SetLegacyRenderingEnabledCondition(() => true); //Currently post processing effects using the video background texture are only supported by legacy rendering;
    }
    // 第一帧开始渲染时调用
    private void Start()
    {
        //初始化界面，列出可选课程
        /* Thread initCoursesThread = new Thread(new ParameterizedThreadStart(GetDataFromWeb.GetKeywordsT2));  // 新建一个线程
        object initialCoursesObj = initialCourses;
        initCoursesThread.Start(initialCoursesObj); */
        Thread initialThread = new Thread(new ThreadStart(initialThreadFunction));
        initialThread.Start();
        //
        statusFlag = 0;
        groundPlaneFinder.SetActive(false);  // 取消掉Ground Plane Finder
        scrollView.gameObject.SetActive(false); // 取消掉scroll view
        clusterChoice.SetActive(true);  //  取消选择cluster的scroll view
        TimeFlag = TIMEFLAG - 2;    // 设置刷新帧控制值
        startButton.SetActive(false);
        restartButton.SetActive(false); // 将返回按钮设置成不可用
        returnFromSecondStateButton.SetActive(false);    // 将用于从沙盘第二个场景返回第一个场景的按钮设置成不可用
        returnFromTreeButton.SetActive(false);
        returnFromTreeToShapanButton.SetActive(false);
        CourseNameInTreeImage.SetActive(false); // 将用于显示树场景下的课程名的底牌不可见
    }

    // 帧刷新时调用
    private void Update()
    {
        if (statusFlag == 0)
        {
            if (ocrThreadFlag == false)
            {
                return;
            }
            statusFlag = 1; //AfterOcr();
        }
        else if (statusFlag == 1)
        {
            if (openScanButton.activeInHierarchy == false)
            {
                OCR_RESULT = coursesNameList;
                startButton.SetActive(false);
                openScanButton.SetActive(true);
                guidanceButton.SetActive(true);
                AfterOcr();
                //ocrThreadFlag = true;
                Debug.Log("可重新选择课程");
            }
        }
        else
        {
            if (guidanceButton.activeInHierarchy == true)
            {
                guidanceButton.SetActive(false);
            }
        }
        // if(statusFlag==6){
        //     Debug.Log("statusFlag==6");
        // }
        if (ocrThreadFlag)
        {
            AfterOcr();
        }
        // 对应初始化阶段
        if (TrackingFlag == 0)
        {
            if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
            {
                tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取到一帧
                BackgroundPlaneBehaviour vuforiaBackgroundPlane = FindObjectOfType<BackgroundPlaneBehaviour>(); //获取屏幕
                if (vuforiaBackgroundPlane != null)
                {
                    vuforiaBackgroundPlane.GetComponent<Renderer>().material.mainTexture = tempFrame;
                }
            }
        }

        // 在OCR与API1返回正确的结果后进入

        // OCR确定关键词信息和追踪区域后初始化Tracker时调用
        if (TrackingFlag == 1 && TimeFlag == TIMEFLAG)
        {
            if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
            {
                tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取到一帧
                Utils.textureToTexture2D(tempFrame, trackingFrameOld);
                Utils.texture2DToMat(trackingFrameOld, trackingFrame, true, -1);   // 将帧转换成Mat处理
                Imgproc.cvtColor(trackingFrame, trackingFrameGray, Imgproc.COLOR_BGR2GRAY); // 转化成灰度图计算

                trackers = TrackerMOSSE.create();

                if (trackers.init(trackingFrameGray, trackingWindow))  // 初始化追踪
                {
                    DrawTrackingAndButton();    // 绘制关键词的追踪框和button

                    Utils.matToTexture2D(trackingFrame, trackingFrameNew, true, -1);   // 将Mat格式的frame转换成Texture2D

                    BackgroundPlaneBehaviour vuforiaBackgroundPlane = FindObjectOfType<BackgroundPlaneBehaviour>(); // 获取屏幕
                    if (vuforiaBackgroundPlane != null)
                    {
                        vuforiaBackgroundPlane.GetComponent<Renderer>().material.mainTexture = trackingFrameNew;    // 绘制处理后的图像
                        TrackingFlag = 2;   // 更新flag到更新格式
                        TimeFlag = 1;   // time标志位归一
                        startButton.SetActive(false);
                        restartButton.SetActive(true);
                    }
                }
            }
        }

        // Tracker初始化正常，追踪阶段（更新Tracker阶段）调用
        if (TrackingFlag == 2 && TimeFlag == TIMEFLAG)
        {
            if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
            {
                tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取到一帧
                Utils.textureToTexture2D(tempFrame, trackingFrameOld);
                Utils.texture2DToMat(trackingFrameOld, trackingFrame, true, -1);   // 将帧转换成Mat处理
                Imgproc.cvtColor(trackingFrame, trackingFrameGray, Imgproc.COLOR_BGR2GRAY);

                if (trackers.update(trackingFrameGray, trackingWindow))    // 更新追踪器
                {
                    TimeFlag = 1;

                    // 计算新的追踪框的位置
                    xyLow.x = trackingWindow.x;
                    xyLow.y = trackingWindow.y;
                    xyHigh.x = trackingWindow.x + trackingWindow.width;
                    xyHigh.y = trackingWindow.y + trackingWindow.height;

                    DrawTrackingAndButton();

                    if (Input.GetMouseButton(0))
                    {
                        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                        RaycastHit hit;

                        if (Physics.Raycast(ray, out hit))
                        {
                            GameObject clickedGameObject = hit.collider.gameObject;
                            if (clickedGameObject.tag == "CubeButton")
                            {
                                int clickedName = int.Parse(clickedGameObject.name);
                                InitializeGroundPlane(items[clickedName], domainID);
                            }
                        }
                    }

                    Utils.matToTexture2D(trackingFrame, trackingFrameNew, true, -1);   // 将Mat格式的frame转换成Texture2D

                    BackgroundPlaneBehaviour vuforiaBackgroundPlane = FindObjectOfType<BackgroundPlaneBehaviour>(); // 获取屏幕
                    if (vuforiaBackgroundPlane != null)
                    {
                        vuforiaBackgroundPlane.GetComponent<Renderer>().material.mainTexture = trackingFrameNew;    // 绘制处理后的图像
                    }
                }
            }
        }

        // 每次time标志位没有到达TIMEFLAG值的时候正常刷新
        if (TimeFlag != TIMEFLAG && TrackingFlag != 3 && TrackingFlag != 4 && TrackingFlag != 5 && TrackingFlag != 0)
        {
            TimeFlag++;
            if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
            {
                tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取到一帧
                Utils.textureToTexture2D(tempFrame, trackingFrameOld);
                Utils.texture2DToMat(trackingFrameOld, trackingFrame, true, -1);   // 将帧转换成Mat处理

                DrawTrackingAndButton();

                if (Input.GetMouseButton(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit))
                    {
                        GameObject clickedGameObjectHere = hit.collider.gameObject;
                        if (clickedGameObjectHere.tag == "CubeButton")
                        {
                            int clickedName = int.Parse(clickedGameObjectHere.name);
                            InitializeGroundPlane(items[clickedName], domainID);
                        }
                    }
                }

                Utils.matToTexture2D(trackingFrame, trackingFrameNew, true, -1);   // 将Mat格式的frame转换成Texture2D

                BackgroundPlaneBehaviour vuforiaBackgroundPlane = FindObjectOfType<BackgroundPlaneBehaviour>(); //获取屏幕
                if (vuforiaBackgroundPlane != null)
                {
                    vuforiaBackgroundPlane.GetComponent<Renderer>().material.mainTexture = trackingFrameNew;
                }
            }
        }

        // 用户确定查看关键词，进入AR阶段
        if (TrackingFlag == 3)
        {
            if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
            {
                tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取到一帧
                BackgroundPlaneBehaviour vuforiaBackgroundPlane = FindObjectOfType<BackgroundPlaneBehaviour>(); //获取屏幕
                if (vuforiaBackgroundPlane != null)
                {
                    vuforiaBackgroundPlane.GetComponent<Renderer>().material.mainTexture = tempFrame;
                }
            }

            // 点击球的逻辑
            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    clickedSphere = hit.collider.gameObject;
                    int sKey = clickedSphere.GetInstanceID();

                    Dictionary<int, string[]> sValue;

                    if (leaves.TryGetValue(sKey, out sValue))
                    {
                        // 书写标题
                        tittleTextObj.text = clickedSphere.transform.parent.gameObject.GetComponentInChildren<TextMesh>().text;

                        GameObject fatherGameObject = clickedSphere.transform.parent.gameObject;
                        platformPoint = fatherGameObject.GetComponentsInChildren<MyPlatformPoint>()[0].gameObject.transform;

                        scrollView.gameObject.SetActive(true);


                        // 先销毁所有content下的button

                        OnMessageButtonClicked[] buttonsInContent = content.GetComponentsInChildren<OnMessageButtonClicked>();
                        for (int b = 0; b < buttonsInContent.Length; b++)
                        {
                            Destroy(buttonsInContent[b].gameObject);
                        }

                        int num = sValue.Count;

                        content.GetComponents<RectTransform>()[0].anchoredPosition3D = new Vector3(0, 0, 0);
                        content.GetComponents<RectTransform>()[0].sizeDelta = new Vector2(0, 50 * num + 100);

                        textButton = new Button[num];
                        int h = 0;

                        foreach (int messageKey in sValue.Keys)
                        {
                            textButton[h] = Instantiate(textMessagePrefab);
                            textButton[h].transform.parent = content.transform;
                            textButton[h].GetComponentsInChildren<Text>()[0].text = sValue[messageKey][0];
                            textButton[h].GetComponents<RectTransform>()[0].anchoredPosition3D = new Vector3(0, (h + 1) * (-50) - 30, 0);

                            OnMessageButtonClicked onMessageButton = textButton[h].GetComponents<OnMessageButtonClicked>()[0];
                            onMessageButton.messageValue = sValue[messageKey][1];

                            h++;
                        }

                        clickedFlag = true; // 将点击球标志位置为true
                    }
                }
            }

            // 如果点击中了一个球
            if (clickedFlag)
            {
                screenPos = Camera.main.WorldToScreenPoint(platformPoint.position);
                RectTransform rectTrans = scrollView.GetComponent<RectTransform>();
                rectTrans.position = screenPos;

            }

            //debug
            // var objs = GetComponents<UniWebView>();
            // Debug.Log("UniWebView num:" + objs.Count());
            // foreach (var i in objs)
            // {
            //     Debug.Log("UniWebView name:" + i.name);
            // }
            // var obj2=GetComponents<MainScript>();
            // Debug.Log("obj2 name:"+obj2[0].name);
            // 手势控制
            TouchControl.FingersControl();
        }

        // 进入沙盘模式
        if (TrackingFlag == 4)
        {
            if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
            {
                tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取到一帧
                BackgroundPlaneBehaviour vuforiaBackgroundPlane = FindObjectOfType<BackgroundPlaneBehaviour>(); //获取屏幕
                if (vuforiaBackgroundPlane != null)
                {
                    vuforiaBackgroundPlane.GetComponent<Renderer>().material.mainTexture = tempFrame;
                }
            }

            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    GameObject clickedGameObject = hit.collider.gameObject;

                    if (clickedGameObject.CompareTag("ClusterBase"))
                    {
                        int comID = int.Parse(clickedGameObject.name);
                        if (comID < communityNum)
                        {
                            int newComID = comID % treeModeltars.Length;
                            // 进入某个主题

                            // 将所有簇不可用
                            myClusterBase = shapan.GetComponentsInChildren<MyClusterBase>();
                            foreach (MyClusterBase myClusterBaseItem in myClusterBase)
                            {
                                GameObject clusterParent = myClusterBaseItem.gameObject;
                                clusterParent.SetActive(false);
                            }

                            // 将所有的连线不可用
                            bigLines = shapan.GetComponentsInChildren<LineRenderer>();
                            foreach (LineRenderer eachBigLine in bigLines)
                            {
                                GameObject eachBigLineObject = eachBigLine.gameObject;
                                eachBigLineObject.SetActive(false);
                            }

                            JsonData clickedCommunity = community[comID];
                            JsonData edges = clickedCommunity["edge"];
                            JsonData topics = clickedCommunity["topic"];

                            int topicNum = int.Parse(clickedCommunity["topic_num"].ToString());
                            int[,] edgeMatrix = new int[topicNum, topicNum + 1];

                            for (int a = 0; a < topicNum; a++)
                            {
                                for (int b = 0; b <= topicNum; b++)
                                {
                                    edgeMatrix[a, b] = 0;
                                }
                            }

                            for (int i = 0; i < topicNum; i++)
                            {
                                string sourceName = topics[i].ToString();   // 获得行的名称
                                for (int j = 0; j < topicNum; j++)
                                {
                                    if (j == i) continue;   // 对角元素略过
                                    string targetName = topics[j].ToString();
                                    for (int k = 0; k < edges.Count; k++)   // 遍历所有边
                                    {
                                        string edgeSourceName = edges[k][0].ToString();
                                        string edgeTargetName = edges[k][1].ToString();
                                        if (edgeSourceName == sourceName && edgeTargetName == targetName)
                                        {
                                            edgeMatrix[i, j] = 1;
                                            edgeMatrix[j, i] = 1;
                                            break;
                                        }
                                    }
                                }
                            }

                            for (int i = 0; i < topicNum; i++)
                            {
                                for (int j = 0; j < topicNum; j++)
                                {
                                    edgeMatrix[i, topicNum] += edgeMatrix[i, j];
                                }
                                Debug.Log("sum of " + i + " is " + edgeMatrix[i, topicNum]);
                            }

                            //改进的布局算法
                            SpringyGraph springyGraph = new SpringyGraph(edgeMatrix);
                            float[,] pointsPosition = springyGraph.SpringyGraphMain();
                            littleTreesinPartTwo = new GameObject[topicNum];
                            // 绘制树
                            float scale = 1.0f;
                            if (topicNum < 5) scale = 2.5f;
                            for (int p = 0; p < topicNum; p++)
                            {
                                littleTreesinPartTwo[p] = Instantiate(treeModeltars[newComID]);

                                int order = (int)pointsPosition[p, 2];  // 获取正确的编号
                                littleTreesinPartTwo[p].name = clickedCommunity["topic"][order].ToString();  // 命名
                                Debug.Log("littleTreesinPartTwo[p].name = " + clickedCommunity["topic"][order].ToString());
                                TextMesh treeNameMesh = littleTreesinPartTwo[p].GetComponentInChildren<TextMesh>();
                                treeNameMesh.text = clickedCommunity["topic"][order].ToString();
                                littleTreesinPartTwo[p].transform.parent = shapan.transform;    // 设置位姿
                                littleTreesinPartTwo[p].transform.localPosition = new Vector3(pointsPosition[p, 0] * XMAX * scale, pointsPosition[p, 1] * ZMAX * scale, 0);
                                //littleTree.transform.localPosition = new Vector3(pointsPosition[p, 0], 0, pointsPosition[p, 1]);
                                littleTreesinPartTwo[p].transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                            }

                            //力导向布局
                            /* ForceDirectedLayoutAlgorithms fd = new ForceDirectedLayoutAlgorithms(edgeMatrix,30);
                            var pointsPosition = fd.Run();
                            littleTreesinPartTwo = new GameObject[topicNum];
                            // 绘制树
                            for (int p = 0; p < topicNum; p++)
                            {
                                littleTreesinPartTwo[p] = Instantiate(treeModeltars[newComID]);
                                littleTreesinPartTwo[p].name = clickedCommunity["topic"][p].ToString();  // 命名
                                Debug.Log("littleTreesinPartTwo[p].name = " + clickedCommunity["topic"][p].ToString());
                                TextMesh treeNameMesh = littleTreesinPartTwo[p].GetComponentInChildren<TextMesh>();
                                treeNameMesh.text = clickedCommunity["topic"][p].ToString();
                                littleTreesinPartTwo[p].transform.parent = shapan.transform;    // 设置位姿
                                littleTreesinPartTwo[p].transform.localPosition = new Vector3(pointsPosition[p].x * XMAX*0.1f, pointsPosition[p].y * ZMAX*0.1f, 0);
                                littleTreesinPartTwo[p].transform.localScale = new Vector3(0.03f, 0.03f, 0.03f);
                            } */
                            // 进入沙盘的第二层
                            TrackingFlag = 5;
                            restartButton.SetActive(false);
                            returnFromSecondStateButton.SetActive(true);

                            // 绘制线
                            for (int e = 0; e < edges.Count; e++)
                            {
                                string sourceName = edges[e][0].ToString();
                                string targetName = edges[e][1].ToString();

                                GameObject sourceTree = GameObject.Find("/GroundPlaneStage/shapan(Clone)/mianban/" + sourceName);
                                GameObject targetTree = GameObject.Find("/GroundPlaneStage/shapan(Clone)/mianban/" + targetName);

                                if (sourceTree && targetTree)
                                {
                                    LineRenderer aline = Instantiate(line);
                                    aline.transform.parent = linesParent.transform;
                                    Vector3 sourcePosition = sourceTree.transform.position;
                                    Vector3 targetPosition = targetTree.transform.position;

                                    aline.SetPosition(0, sourcePosition);
                                    aline.SetPosition(1, targetPosition);
                                }

                            }

                            linesParent.transform.parent = shapan.transform;

                        }
                    }

                }
            }

            //手势控制
            TouchControl.FingersControl();
        }

        // 进入沙盘的第二层
        if (TrackingFlag == 5)
        {
            if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
            {
                tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取到一帧
                BackgroundPlaneBehaviour vuforiaBackgroundPlane = FindObjectOfType<BackgroundPlaneBehaviour>(); //获取屏幕
                if (vuforiaBackgroundPlane != null)
                {
                    vuforiaBackgroundPlane.GetComponent<Renderer>().material.mainTexture = tempFrame;
                }
            }

            if (Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    GameObject clickedGameObject = hit.collider.gameObject;


                    if (clickedGameObject.CompareTag("mediumTree"))

                    {

                        API3StartReturnTree(clickedGameObject.name, domainID);

                        shapanBig.SetActive(false);

                        Debug.Log("name: " + clickedGameObject.name + " , domainid: " + domainID + "return? " + returntoShapan);
                    }

                }
            }
            // 手势控制
            TouchControl.FingersControl();
        }
    }
    // Ocr与API1 之后结果 场景渲染
    void AfterOcr()
    {
        ocrThreadFlag = false;  // 线程结束收到，将线程标志位复原
        if (statusFlag != 0 && statusFlag != 1)
        {
            openScanButton.SetActive(false);
            guidanceButton.SetActive(false);
        }

        tipsText.text = "";

        // 判断result_id
        // 如果是2，则是没有结果。
        // 如果是0，则是沙盘场景，根据result_num判定，如果为0，错误，如果为1，直接构建沙盘，如果大于1，UI选择
        // 如果是1，则是树的场景，根据result_num判定，如果为0，错误，如果不是0，归入树的逻辑

        JsonData resultJson = JsonMapper.ToObject(OCR_RESULT);
        JsonData resultID = resultJson["result_id"];
        //resultID.ToString() == "2"：无正确的课程
        if (resultID.ToString() == "2")
        {
            tipsText.text = "未识别到关键词\n请重新拍照";
            startButton.SetActive(true);
            restartButton.SetActive(true);
        }
        else
        {
            JsonData resultsNum = resultJson["result_num"];
            // 判定result_id
            // 进入沙盘场景
            if (resultID.ToString() == "0")
            {
                if (statusFlag == 2)
                {
                    statusFlag = 3;
                    restartButton.SetActive(true);
                }
                //else { statusFlag = 99; }

                //startButton.SetActive(false);
                int clustersNum = int.Parse(resultsNum.ToString());

                if (clustersNum == 0)
                {
                    tipsText.text = "对不起，您拍摄的页面没有识别到关键词\n请尝试重新拍摄";
                    startButton.SetActive(true);
                    restartButton.SetActive(true);
                }
                else if (clustersNum == 1)//只有一门课程
                {
                    JsonData resultClusterName = resultJson["results"][0][0];

                    NewAPI2Start(resultClusterName.ToString());
                }
                else//多门课程。展示课程列表。
                {

                    JsonData resultsClusters = resultJson["results"][0];

                    clusterChoice.SetActive(true);

                    clusterContent.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 0, 0);
                    clusterContent.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 100 * clustersNum);

                    Button[] clusterButtons = new Button[clustersNum];

                    for (int rc = 0; rc < resultsClusters.Count; rc++)
                    {
                        clusterButtons[rc] = Instantiate(clusterButtonPrefab);

                        clusterButtons[rc].transform.parent = clusterContent.transform;
                        clusterButtons[rc].GetComponentInChildren<Text>().text = resultsClusters[rc].ToString();
                        clusterButtons[rc].GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, -100 * rc + 50 * clustersNum - 50, 0);

                        ClusterButtonObj clusterButtonObj = clusterButtons[rc].GetComponentInChildren<ClusterButtonObj>();
                        clusterButtonObj.clusterName = resultsClusters[rc].ToString();
                        clusterButtonObj.mainScript = selfClass;
                    }
                }
            }
            // 直接进入树场景
            else
            {

                int wordsNum = int.Parse(resultsNum.ToString());

                if (wordsNum == 0)
                {
                    tipsText.text = "对不起，您拍摄的页面没有识别到关键词\n请尝试重新拍摄";
                }
                else
                {
                    domainID = resultJson["domain_id"].ToString();

                    GetResult(wordsNum);   // 获取关键词位置

                    cubeButton = new GameObject[wordsNum];  // 生成Collider Button的数组

                    for (int c = 0; c < wordsNum; c++)  // 实例化Collider Button
                    {
                        cubeButton[c] = Instantiate(cubeButtonPrefab);
                        cubeButton[c].name = c.ToString();
                        cubeButton[c].tag = "CubeButton";
                    }

                    // 初始跟踪框坐标与框
                    xyLow = new Point(points[0], points[1]);
                    xyHigh = new Point(points[2], points[3]);
                    trackingWindow = new Rect2d(xyLow, xyHigh);

                    // 创建各种图像资源的引用
                    if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
                    {
                        tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取帧Texture
                        trackingFrameOld = new Texture2D(tempFrame.width, tempFrame.height, TextureFormat.RGB24, false);    // 取帧Texture2D
                        Utils.textureToTexture2D(tempFrame, trackingFrameOld);
                        trackingFrame = new Mat(trackingFrameOld.height, trackingFrameOld.width, CvType.CV_8UC3);   // 构造Mat
                        trackingFrameGray = new Mat(trackingFrameOld.height, trackingFrameOld.width, CvType.CV_8UC1);   // 灰度图Mat
                        trackingFrameNew = new Texture2D(trackingFrame.cols(), trackingFrame.rows(), TextureFormat.RGB24, false);  // 初始化新的Vuforia帧
                    }

                    TrackingFlag = 1;   // 将TrackingFlag置1，开启识别模式
                }

            }
        }
    }
    // 绘制关键词的追踪框与button
    private void DrawTrackingAndButton()
    {
        float imageWidth = frameOCR.width;
        float imageHeight = frameOCR.height;

        GameObject vuforiaScreen = GameObject.Find("ARCamera/BackgroundPlane");

        float K = 2 * vuforiaScreen.transform.localScale.z / imageHeight;

        // 绘制关键词框
        for (int n = 0; n < items.Length; n++)
        {
            xyKeywordsLow.x = xyLow.x + points[(n + 1) * 4] - 1;
            xyKeywordsLow.y = xyLow.y + points[(n + 1) * 4 + 1] - 1;
            xyKeywordsHigh.x = xyLow.x + points[(n + 1) * 4 + 2] + 1;
            xyKeywordsHigh.y = xyLow.y + points[(n + 1) * 4 + 3] + 1;

            Imgproc.rectangle(trackingFrame, xyKeywordsLow, xyKeywordsHigh, scalar, 2); // 绘制关键词矩形框

            //计算Button位置
            float targetWidth = (float)xyKeywordsHigh.x - (float)xyKeywordsLow.x;
            float targetHeight = (float)xyKeywordsHigh.y - (float)xyKeywordsLow.y;

            float targetX = (float)xyKeywordsLow.x;
            float targetY = (float)xyKeywordsLow.y;

            float buttonPositionX;
            float buttonPositionY;

            float buttonScaleX = K * targetWidth;
            float buttonScaleY = 1;
            float buttonScaleZ = K * targetHeight;

            TextMesh contentText = cubeButton[n].GetComponentInChildren<TextMesh>();
            contentText.text = items[n];
            contentText.gameObject.transform.localPosition = new Vector3(0, 0, 0);

            if (Screen.orientation.ToString() == "LandscapeLeft")
            {
                buttonPositionX = (-1) * K * (targetX + targetWidth / 2 - imageWidth / 2);
                buttonPositionY = K * (targetY + targetHeight / 2 - imageHeight / 2);
                contentText.gameObject.transform.localEulerAngles = new Vector3(90, 0, 0);
                contentText.gameObject.transform.localScale = new Vector3(0.05f, 0.05f * buttonScaleX / buttonScaleZ, 1);
            }
            else if (Screen.orientation.ToString() == "LandscapeRight")
            {
                buttonPositionX = K * (targetX + targetWidth / 2 - imageWidth / 2);
                buttonPositionY = (-1) * K * (targetY + targetHeight / 2 - imageHeight / 2);
            }
            else if (Screen.orientation.ToString() == "Portrait")
            {
                buttonPositionY = K * (targetX + targetWidth / 2 - imageWidth / 2);
                buttonPositionX = K * (targetY + targetHeight / 2 - imageHeight / 2);
                contentText.gameObject.transform.localEulerAngles = new Vector3(90, 0, 90);
                contentText.gameObject.transform.localScale = new Vector3(0.2f * buttonScaleX / buttonScaleZ, 0.2f, 1);
            }
            else
            {
                buttonPositionY = (-1) * K * (targetX + targetWidth / 2 - imageWidth / 2);
                buttonPositionX = (-1) * K * (targetY + targetHeight / 2 - imageHeight / 2);
            }

            Vector3 buttonPosition = new Vector3(buttonPositionX, buttonPositionY, 0);
            Vector3 buttonLocalScale = new Vector3(buttonScaleX, buttonScaleY, buttonScaleZ);

            cubeButton[n].transform.parent = arCamera.transform;
            cubeButton[n].transform.localPosition = vuforiaScreen.transform.localPosition + buttonPosition;
            cubeButton[n].transform.localRotation = vuforiaScreen.transform.localRotation;
            cubeButton[n].transform.localScale = buttonLocalScale;
        }
    }

    // 按钮点击后调用;开始拍照 OCR 识别关键字。
    public void OnButtonClicked()
    {
        if (VuforiaRenderer.Instance != null && VuforiaRenderer.Instance.VideoBackgroundTexture != null)
        {
            startButton.SetActive(false);
            restartButton.SetActive(false);
            tempFrame = VuforiaRenderer.Instance.VideoBackgroundTexture;    // 取到一帧
            frameOCR = new Texture2D(tempFrame.width, tempFrame.height, TextureFormat.RGB24, false);
            Utils.textureToTexture2D(tempFrame, frameOCR);  // texture 转换成 texture2D
            Mat imageMat = new Mat(frameOCR.height, frameOCR.width, CvType.CV_8UC3);
            Utils.fastTexture2DToMat(frameOCR, imageMat);
            Mat grayMat = new Mat(frameOCR.height, frameOCR.width, CvType.CV_8UC3);
            Mat binMat = new Mat(frameOCR.height, frameOCR.width, CvType.CV_8UC3);
            Imgproc.cvtColor(imageMat, grayMat, Imgproc.COLOR_BGR2GRAY);
            Imgproc.threshold(grayMat, binMat, 180, 255, Imgproc.THRESH_BINARY);
            //Texture2D frameOCRnew = new Texture2D(binMat.cols(), binMat.rows(), TextureFormat.RGB24, false);
            Utils.matToTexture2D(binMat, frameOCR);
            // var frameOCR1 = ProcessImages.HorizontalFlipPicture(frameOCR); // 翻转图片。需要这一步操作，否则是镜像的。
            // byte[] frameOCRbyte = frameOCR1.EncodeToJPG(OCR_IMAGE_QUALITY);   // 转换成PNG格式的图片
            // SaveImage.Save(frameOCRbyte);
            var frameOCR2 = ProcessImages.VerticalFlipPicture(frameOCR); // 翻转图片。需要这一步操作，否则是镜像的。
            byte[] frameOCRbyte = frameOCR2.EncodeToJPG(OCR_IMAGE_QUALITY);   // 转换成PNG格式的图片

            //SaveImage.Save(frameOCRbyte, "image3.jpeg");

            object imageOCR = frameOCRbyte;    // 装箱
            Thread thread = new Thread(new ParameterizedThreadStart(GetDataFromWeb.GetKeywordsT));  // 新建一个线程
            thread.Start(imageOCR);    // 开启线程，并传入参数image
            tipsText.text = "正在识别，请稍等……";    // 开启等待提示符
        }
    }
    // 分析OCR和API1返回结果，得到关键词的数据
    private void GetResult(int wordsNum)
    {
        points = new double[(wordsNum + 1) * 4];    // 初始化数组大小存储坐标点
        items = new string[wordsNum];   // 初始化数组存储关键词

        JsonData resultJson = JsonMapper.ToObject(OCR_RESULT);
        JsonData wordsBorder = resultJson["border"];    // 获取border信息
        JsonData wordsItems = resultJson["results"];    // 获取所有关键词

        // 边界点
        points[0] = double.Parse(wordsBorder["left"].ToString());
        points[1] = double.Parse(wordsBorder["top"].ToString());
        points[2] = double.Parse(wordsBorder["right"].ToString());
        points[3] = double.Parse(wordsBorder["down"].ToString());

        // 文字区域的宽高
        double borderWidth = points[2] - points[0];
        double borderHeight = points[3] - points[1];

        // 关键词信息
        for (int i = 0; i < wordsNum; i++)
        {
            JsonData itemLocation = wordsItems[i]["location"];
            items[i] = wordsItems[i]["words"].ToString();   // 获取关键词
            points[4 * (i + 1)] = double.Parse(itemLocation["left"].ToString());
            points[4 * (i + 1) + 1] = double.Parse(itemLocation["top"].ToString());
            points[4 * (i + 1) + 2] = double.Parse(itemLocation["left"].ToString()) + double.Parse(itemLocation["width"].ToString());
            points[4 * (i + 1) + 3] = double.Parse(itemLocation["top"].ToString()) + double.Parse(itemLocation["height"].ToString());
        }
    }

    // 进入AR阶段
    void InitializeGroundPlane(string item, string domainid)
    {
        API2Start(item, domainid);

        treeTransform = groundPlaneStage.GetComponentInChildren<MyTreeTrunkT>().transform;

        TrackingFlag = 3;

        groundPlaneFinder.SetActive(true);  // 初始化groundPlaneFinder

        if (trackers != null)
            trackers.Dispose();

        if (cubeButton != null)
        {
            // 销毁cubeButton
            for (int c = 0; c < items.Length; c++)
            {
                Destroy(cubeButton[c]);
            }
        }

        // 判断是否是返回到沙盘到树
        if (returntoShapan)
        {
            // 应该返回沙盘
            restartButton.SetActive(false);
            returnFromTreeToShapanButton.SetActive(true);
        }
    }

    // API2调用以及生成树
    private void API2Start(string itemName, string domainid)
    {
        string api3_result = GetDataFromWeb.GetAllByTopicNameAndDomainId(itemName, domainid);
        JsonData returnData = JsonMapper.ToObject(api3_result);
        string success = returnData["msg"].ToString();

        Debug.Log("sandbox: " + returnData["sandbox_able"].ToString() + ", domain: " + returnData["domain"].ToString());

        if (int.Parse(returnData["sandbox_able"].ToString()) == 0)
        {
            returntoShapan = true;
            treeReturnToShapanName = returnData["domain"].ToString();
        }
        else
        {
            returntoShapan = false;
        }

        if (success == "成功")//成功则显示树，不成功则提示
        {
            CourseNameInTree.text = treeReturnToShapanName;
            CourseNameInTreeImage.SetActive(true);

            //实例化一个ArrayList字典，存储叶子的信息
            leaves = new Dictionary<int, Dictionary<int, string[]>>();

            Vector3 position = new Vector3(0, 0, 0);
            Vector3 scale = new Vector3(1, 1, 1);

            //实例化树干
            mytrunk = Instantiate(trunk);
            mytrunk.GetComponentInChildren<TextMesh>().text = itemName;
            mytrunk.transform.parent = groundPlaneStage.transform;
            mytrunk.transform.localPosition = position;
            mytrunk.transform.localScale = scale / ORIGIN_TREE_SCALE_FACTER;

            //得到树干所有的字物体
            Transform[] allChildren1 = mytrunk.GetComponentsInChildren<Transform>();

            JsonData first_branch = returnData["data"]["children"];
            int first_count = first_branch.Count;

            //只生成一个球的flag
            int first_flag = 0;

            //实例化一个dictionary,存储这个小球上所有的碎片信息
            Dictionary<int, string[]> first_dic = new Dictionary<int, string[]>();

            int first_sphere = new int();//小球的id
            for (int i = 0; i < first_count; i++)
            {
                //第一层循环
                string first_type = first_branch[i]["type"].ToString();//第一层判断

                if (first_type == "leaf")
                {
                    //将小球信息加入到dictionary里面
                    string[] first_webpage = new string[2];
                    string first_url = first_branch[i]["url"].ToString();
                    int first_name = i;
                    string first_content = first_branch[i]["assembleContent"].ToString();
                    first_webpage[0] = first_content;
                    first_webpage[1] = first_url;
                    first_dic.Add(first_name, first_webpage);
                    //生成树叶
                    if (first_flag == 0)
                    {
                        first_flag = 1;
                        //实例化一个球
                        GameObject first_s = Instantiate(b1_1);
                        Transform[] allChildren_s1 = first_s.GetComponentsInChildren<Transform>();
                        first_sphere = allChildren_s1[1].gameObject.GetInstanceID();
                        first_s.transform.parent = allChildren1[i + 1];
                        first_s.transform.localPosition = position;
                        first_s.transform.localEulerAngles = position;
                        first_s.transform.localScale = scale;
                    }
                }
                else
                {
                    string first_name = first_branch[i]["facetName"].ToString();
                    //生成树枝
                    GameObject first_b = Branch2Random();
                    Transform[] allChildren2 = first_b.GetComponentsInChildren<Transform>();
                    first_b.transform.parent = allChildren1[i + 1];
                    first_b.transform.localPosition = position;
                    first_b.transform.localScale = scale;
                    first_b.transform.localEulerAngles = position;
                    //写上文字
                    TextMesh[] first_textMeshes = first_b.GetComponentsInChildren<TextMesh>();
                    for (int t = 0; t < first_textMeshes.Length; t++)
                    {
                        if (first_textMeshes[t].gameObject.tag == "FirstBranchText")
                        {
                            TextMesh first_textMesh = first_textMeshes[t];
                            first_textMesh.text = first_name;
                            break;
                        }
                    }

                    JsonData second_branch = first_branch[i]["children"];
                    //第二层的flag
                    int second_flag = 0;
                    Dictionary<int, string[]> second_dic = new Dictionary<int, string[]>();
                    int second_sphere = new int();
                    for (int j = 0; j < second_branch.Count; j++)
                    {
                        //第二层循环
                        string second_type = second_branch[j]["type"].ToString();//第二层判断
                        if (second_type == "leaf")
                        {
                            string[] second_webpage = new string[2];
                            string second_url = second_branch[j]["url"].ToString();
                            string second_content = second_branch[j]["assembleContent"].ToString();
                            int second_name = j;
                            second_webpage[0] = second_content;
                            second_webpage[1] = second_url;
                            second_dic.Add(second_name, second_webpage);
                            //生成树叶
                            if (second_flag == 0)
                            {
                                second_flag = 1;
                                second_sphere = allChildren2[1].gameObject.GetInstanceID();
                            }
                        }
                        else
                        {

                            string second_name = second_branch[j]["facetName"].ToString();
                            //生成树枝
                            GameObject second_b = Branch3Random();
                            Transform[] allChildren3 = second_b.GetComponentsInChildren<Transform>();
                            second_b.transform.parent = allChildren2[j + 2];
                            second_b.transform.localPosition = position;
                            Vector3 aNewScale = new Vector3(0.75f, 0.75f, 0.75f);
                            second_b.transform.localScale = aNewScale;
                            second_b.transform.localEulerAngles = position;
                            //写上文字
                            TextMesh second_textMesh = second_b.GetComponentsInChildren<TextMesh>()[0];
                            second_textMesh.text = second_name;

                            JsonData third_branch = second_branch[j]["children"];
                            //第三层的flag
                            int third_flag = 0;
                            Dictionary<int, string[]> third_dic = new Dictionary<int, string[]>();
                            int third_sphere = new int();
                            int third_NUM = third_branch.Count > 3 ? 3 : third_branch.Count;
                            for (int k = 0; k < third_NUM; k++)
                            {
                                //第三层循环，第三层一定是叶子，不需要判断。
                                string[] third_webpage = new string[2];
                                string third_url = third_branch[k]["url"].ToString();
                                string third_content = third_branch[k]["assembleContent"].ToString();
                                int third_name = k;
                                third_webpage[0] = third_content;
                                third_webpage[1] = third_url;
                                third_dic.Add(third_name, third_webpage);
                                if (third_flag == 0)
                                {
                                    third_flag = 1;
                                    third_sphere = allChildren3[1].gameObject.GetInstanceID();
                                }
                            }
                            if (third_flag == 1)
                            {
                                leaves.Add(third_sphere, third_dic);
                            }

                        }
                    }
                    if (second_flag == 1)
                    {
                        leaves.Add(second_sphere, second_dic);
                    }
                }
            }
            if (first_flag == 1)
            {
                leaves.Add(first_sphere, first_dic);
            }

            // 去除生成树的所有renderer、collider、canvas
            var rendererComponents = groundPlaneStage.GetComponentsInChildren<Renderer>(true);
            var colliderComponents = groundPlaneStage.GetComponentsInChildren<Collider>(true);
            var canvasComponents = groundPlaneStage.GetComponentsInChildren<Canvas>(true);

            // Disable rendering:
            foreach (var component in rendererComponents)
                component.enabled = false;

            // Disable colliders:
            foreach (var component in colliderComponents)
                component.enabled = false;

            // Disable canvas':
            foreach (var component in canvasComponents)
                component.enabled = false;
        }
    }

    // 当已经有沙盘当时候调用API3, 获取树的信息。
    private void API3StartReturnTree(string itemName, string domainid)
    {
        string api3_result = GetDataFromWeb.GetAllByTopicNameAndDomainId(itemName, domainid);

        JsonData returnData = JsonMapper.ToObject(api3_result);
        string success = returnData["msg"].ToString();

        if (returnData["sandbox_able"].ToString() == "0")
        {
            returntoShapan = true;
            treeReturnToShapanName = returnData["domain"].ToString();
        }
        else
            returntoShapan = false;


        if (success == "成功")//成功则显示树，不成功则提示
        {
            CourseNameInTree.text = treeReturnToShapanName;
            CourseNameInTreeImage.SetActive(true);

            //实例化一个ArrayList字典，存储叶子的信息
            leaves = new Dictionary<int, Dictionary<int, string[]>>();

            Vector3 position = new Vector3(0, 0, 0);
            Vector3 scale = new Vector3(1, 1, 1);

            //实例化树干
            treeTrunkFromShapan = Instantiate(trunk);
            treeTransform = treeTrunkFromShapan.GetComponent<MyTreeTrunkT>().transform;
            treeTrunkFromShapan.GetComponentInChildren<TextMesh>().text = itemName;
            treeTrunkFromShapan.transform.parent = groundPlaneStage.transform;
            treeTrunkFromShapan.transform.localPosition = position;
            treeTrunkFromShapan.transform.localScale = scale / ORIGIN_TREE_SCALE_FACTER;

            //得到树干所有的字物体
            Transform[] allChildren1 = treeTrunkFromShapan.GetComponentsInChildren<Transform>();

            JsonData first_branch = returnData["data"]["children"];
            int first_count = first_branch.Count;

            //只生成一个球的flag
            int first_flag = 0;

            //实例化一个dictionary,存储这个小球上所有的碎片信息
            Dictionary<int, string[]> first_dic = new Dictionary<int, string[]>();

            int first_sphere = new int();//小球的id
            for (int i = 0; i < first_count; i++)
            {
                //第一层循环
                string first_type = first_branch[i]["type"].ToString();//第一层判断

                if (first_type == "leaf")
                {
                    //将小球信息加入到dictionary里面
                    string[] first_webpage = new string[2];
                    string first_url = first_branch[i]["url"].ToString();
                    int first_name = i;
                    string first_content = first_branch[i]["assembleContent"].ToString();
                    first_webpage[0] = first_content;
                    first_webpage[1] = first_url;
                    first_dic.Add(first_name, first_webpage);
                    //生成树叶
                    if (first_flag == 0)
                    {
                        first_flag = 1;
                        //实例化一个球
                        GameObject first_s = Instantiate(b1_1);
                        Transform[] allChildren_s1 = first_s.GetComponentsInChildren<Transform>();
                        first_sphere = allChildren_s1[1].gameObject.GetInstanceID();
                        first_s.transform.parent = allChildren1[i + 1];
                        first_s.transform.localPosition = position;
                        first_s.transform.localEulerAngles = position;
                        first_s.transform.localScale = scale;
                    }
                }
                else
                {
                    string first_name = first_branch[i]["facetName"].ToString();
                    //生成树枝
                    GameObject first_b = Branch2Random();
                    Transform[] allChildren2 = first_b.GetComponentsInChildren<Transform>();
                    first_b.transform.parent = allChildren1[i + 1];
                    first_b.transform.localPosition = position;
                    first_b.transform.localScale = scale;
                    first_b.transform.localEulerAngles = position;
                    //写上文字
                    TextMesh[] first_textMeshes = first_b.GetComponentsInChildren<TextMesh>();
                    for (int t = 0; t < first_textMeshes.Length; t++)
                    {
                        if (first_textMeshes[t].gameObject.tag == "FirstBranchText")
                        {
                            TextMesh first_textMesh = first_textMeshes[t];
                            first_textMesh.text = first_name;
                            break;
                        }
                    }

                    JsonData second_branch = first_branch[i]["children"];
                    //第二层的flag
                    int second_flag = 0;
                    Dictionary<int, string[]> second_dic = new Dictionary<int, string[]>();
                    int second_sphere = new int();
                    for (int j = 0; j < second_branch.Count; j++)
                    {
                        //第二层循环
                        string second_type = second_branch[j]["type"].ToString();//第二层判断
                        if (second_type == "leaf")
                        {
                            string[] second_webpage = new string[2];
                            string second_url = second_branch[j]["url"].ToString();
                            string second_content = second_branch[j]["assembleContent"].ToString();
                            int second_name = j;
                            second_webpage[0] = second_content;
                            second_webpage[1] = second_url;
                            second_dic.Add(second_name, second_webpage);
                            //生成树叶
                            if (second_flag == 0)
                            {
                                second_flag = 1;
                                second_sphere = allChildren2[1].gameObject.GetInstanceID();
                            }
                        }
                        else
                        {

                            string second_name = second_branch[j]["facetName"].ToString();
                            //生成树枝
                            GameObject second_b = Branch3Random();
                            Transform[] allChildren3 = second_b.GetComponentsInChildren<Transform>();
                            second_b.transform.parent = allChildren2[j + 2];
                            second_b.transform.localPosition = position;
                            Vector3 aNewScale = new Vector3(0.75f, 0.75f, 0.75f);
                            second_b.transform.localScale = aNewScale;
                            second_b.transform.localEulerAngles = position;
                            //写上文字
                            TextMesh second_textMesh = second_b.GetComponentsInChildren<TextMesh>()[0];
                            second_textMesh.text = second_name;

                            JsonData third_branch = second_branch[j]["children"];
                            //第三层的flag
                            int third_flag = 0;
                            Dictionary<int, string[]> third_dic = new Dictionary<int, string[]>();
                            int third_sphere = new int();
                            int third_NUM = third_branch.Count > 3 ? 3 : third_branch.Count;
                            for (int k = 0; k < third_NUM; k++)
                            {
                                //第三层循环，第三层一定是叶子，不需要判断。
                                string[] third_webpage = new string[2];
                                string third_url = third_branch[k]["url"].ToString();
                                string third_content = third_branch[k]["assembleContent"].ToString();
                                int third_name = k;
                                third_webpage[0] = third_content;
                                third_webpage[1] = third_url;
                                third_dic.Add(third_name, third_webpage);
                                if (third_flag == 0)
                                {
                                    third_flag = 1;
                                    third_sphere = allChildren3[1].gameObject.GetInstanceID();
                                }
                            }
                            if (third_flag == 1)
                            {
                                leaves.Add(third_sphere, third_dic);
                            }

                        }
                    }
                    if (second_flag == 1)
                    {
                        leaves.Add(second_sphere, second_dic);
                    }
                }
            }
            if (first_flag == 1)
            {
                leaves.Add(first_sphere, first_dic);
            }
        }

        returnFromSecondStateButton.SetActive(false);
        restartButton.SetActive(false);
        returnFromTreeButton.SetActive(true);

        TrackingFlag = 3;

    }

    // 新API2调用。向服务器请求 课程 的数据，并构建沙盘。
    public void NewAPI2Start(string clusterName)
    {
        openScanButton.SetActive(false);
        string tipsTextMessage = "请稍等，正在构建" + clusterName + "的AR沙盘模型";
        tipsText.text = tipsTextMessage;

        Debug.Log(clusterName);

        string api2_result = GetDataFromWeb.GetClusterDivided(clusterName);
        // 实例化沙盘模型
        shapanBig = Instantiate(shapanPrefab);
        shapanBig.transform.parent = groundPlaneStage.transform;
        shapanBig.transform.localPosition = Vector3.zero;
        shapanBig.transform.localScale = Vector3.one / 10;

        TextMesh courseNameMesh = shapanBig.GetComponentInChildren<TextMesh>();
        courseNameMesh.text = clusterName;

        shapan = shapanBig.GetComponentInChildren<MyShapanMianban>().gameObject;

        // 沙盘构建
        JsonData resultJson = JsonMapper.ToObject(api2_result);
        JsonData communities = resultJson["communities"];  // 社团数组
        JsonData communitySequence = resultJson["sequence"];   // 认知顺序
        JsonData communitySize = resultJson["topic_num"];   // 簇的大小
        domainID = resultJson["domainid"].ToString();

        // 获取社团内容，形成一个JsonData数组
        // ！！！数学运算
        // 求总数
        communityNum = communitySize.Count;

        // 尺寸均衡
        float[] sizeWeight = new float[communityNum];
        for (int i = 0; i < communityNum; i++)
        {
            sizeWeight[i] =(float)Math.Pow(int.Parse(communitySize[i].ToString()), 0.33);
        //sizeWeight[i]=int.Parse(communitySize[i].ToString())/PI;
        }

        // 求权重和
        float weightSum = 0;
        for (int i = 0; i < communityNum; i++)
        {
            weightSum += sizeWeight[i];
        }

        // 安置圆盘
        float[] weights = new float[communityNum];
        float sitaNow = 0;
        community = new JsonData[communityNum];
        //const float clusterPerimeter = (float)0.35 * (float)0.35 * (float)Math.PI;
        const float clusterRadius = (float)0.38;

        Vector3 lastLinePoint = new Vector3(0, 0, 0);

        for (int i = 0; i < communityNum; i++)
        {
            weights[i] = sizeWeight[i] / weightSum; // 计算归一化权重
            float angle = weights[i] * 2 * PI;  // 计算当前所占角度
            if(i!=0) sitaNow -= angle / 2;   // 将角度记录增加一般当前角
            float radius = 0.25f*weights[i] * PERIMETER*(float)Math.Log(communityNum);  // 计算当前半径
            //float radius=0.5f*weights[i]*PERIMETER;
            GameObject cluster = Instantiate(clusterBases[i % clusterBases.Length]);  // 初始化圆盘
            string communityName = "community" + communitySequence[i];
            Debug.Log(communityName);
            community[i] = communities[i][communityName];   // 获取社团信息，存入数组

            TextMesh clusterNameMesh = cluster.GetComponentInChildren<TextMesh>();
            clusterNameMesh.text = community[i]["cluster_name"].ToString();

            //Debug.Log(community[i]["cluster_name"]);    // 此处拿到了簇的名字

            cluster.name = i.ToString();    // 用数字命名object
            cluster.transform.parent = shapan.transform;    // 设置位姿
            cluster.transform.localPosition = new Vector3(XMAX * (float)Math.Cos(sitaNow), ZMAX * (float)Math.Sin(sitaNow), 0);
            //cluster.transform.localPosition = new Vector3(XMAX * (float)Math.Cos(sitaNow), 0, ZMAX * (float)Math.Sin(sitaNow));
            //cluster.transform.localPosition = new Vector3(XMAX * (float)Math.Cos(sitaNow)*(float)Math.Abs(Math.Sin(i+1)), 0, ZMAX * (float)Math.Sin(sitaNow) * (float)Math.Abs(Math.Sin(i + 1)));
            cluster.transform.localScale = new Vector3(radius, radius, radius);
            sitaNow -= angle /2; // 再增加一般当前角

            // 连线
            if (i > 0)
            {
                LineRenderer aline = Instantiate(line);
                aline.transform.parent = shapan.transform;
                Vector3 sourcePosition = lastLinePoint;
                Vector3 targetPosition = cluster.transform.position;

                aline.SetPosition(0, sourcePosition);
                aline.SetPosition(1, targetPosition);
            }

            lastLinePoint = cluster.transform.position;

            // 绘制圆盘上的树
            int topicNum = int.Parse(community[i]["topic_num"].ToString());
            float singleAngle = 2 * (float)Math.PI / topicNum;
            for (int j = 0; j < topicNum; j++)
            {
                GameObject littleTree = Instantiate(treeModels[i % treeModels.Length]);
                littleTree.transform.parent = cluster.transform;
                //littleTree.transform.localPosition = new Vector3(clusterRadius * (float)Math.Cos(singleAngle * j) * 10 * (float)Math.Abs(Math.Sin(j + 1)), clusterRadius * (float)Math.Sin(singleAngle * j) * 10 * (float)Math.Abs(Math.Sin(j + 1)),0);
                littleTree.transform.localPosition = new Vector3(clusterRadius * (float)Math.Cos(singleAngle * j) * 10 * (float)Math.Abs(Math.Sin(j + 1)), 0, clusterRadius * (float)Math.Sin(singleAngle * j) * 10 * (float)Math.Abs(Math.Sin(j + 1)));
                littleTree.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            }
        }

        //treeTransform = groundPlaneStage.GetComponentsInChildren<Transform>()[1];

        treeTransform = groundPlaneStage.GetComponentInChildren<MyTreeTrunkT>().transform;

        TrackingFlag = 4;

        groundPlaneFinder.SetActive(true);  // 初始化groundPlaneFinder

        // 去除生成树的所有renderer、collider、canvas
        var rendererComponents = groundPlaneStage.GetComponentsInChildren<Renderer>(true);
        var colliderComponents = groundPlaneStage.GetComponentsInChildren<Collider>(true);
        var canvasComponents = groundPlaneStage.GetComponentsInChildren<Canvas>(true);

        // Disable rendering:
        foreach (var component in rendererComponents)
            component.enabled = false;

        // Disable colliders:
        foreach (var component in colliderComponents)
            component.enabled = false;

        // Disable canvas':
        foreach (var component in canvasComponents)
            component.enabled = false;

        ClusterButtonObj[] clusterButtonObjs = clusterChoice.GetComponentsInChildren<ClusterButtonObj>();
        foreach (var itemClusterButton in clusterButtonObjs)
        {
            Destroy(itemClusterButton.gameObject);
        }

        clusterChoice.SetActive(false);
        restartButton.SetActive(true);
        tipsText.text = "";
    }

    // 重新开始拍照
    public void OnRestartButtonClicked()
    {
        restartButton.SetActive(false);
        statusFlag = 1;
        ClusterButtonObj[] clusterButtonObjs = clusterChoice.GetComponentsInChildren<ClusterButtonObj>();
        foreach (var itemClusterButton in clusterButtonObjs)
        {
            Destroy(itemClusterButton.gameObject);
        }

        clusterChoice.SetActive(false);

        if (TrackingFlag == 2)
        {
            // 对应正在追踪但还没有点击关键词但情况
            // 销毁cillider button
            for (int c = 0; c < cubeButton.Length; c++)
            {
                Destroy(cubeButton[c]);
            }
            // 关闭追踪器
            trackers.Dispose();
        }

        if (TrackingFlag == 4)
        {
            linesParent.transform.parent = null;
            // 对应出现沙盘并且没有进入第二层的情况
            // 将沙盘销毁
            if (treeTransform != null)
                Destroy(treeTransform.gameObject);

            if (shapanBig != null)
                Destroy(shapanBig);
            // 将ground plane finder置为不可见
            //groundPlaneFinder.SetActive(false);
        }

        if (TrackingFlag == 3 && !returntoShapan)
        {
            // 对应点击关键词后生成了树但情况
            // 将树销毁
            if (treeTransform != null)
                Destroy(treeTransform.gameObject);
            // 将ground plane finder置为不可见
            //groundPlaneFinder.SetActive(false);
            // 销毁所有content下的button
            OnMessageButtonClicked[] buttonsInContent = content.GetComponentsInChildren<OnMessageButtonClicked>();
            for (int b = 0; b < buttonsInContent.Length; b++)
            {
                Destroy(buttonsInContent[b].gameObject);
            }
            // 将scroll view置为不可见
            scrollView.gameObject.SetActive(false);
        }

        if (TrackingFlag == 3 && returntoShapan)
        {

        }
        //tipsText.text = "请对准要扫面的页面，在字体清晰可见时，点击确定按钮，将进行拍照识别关键词";
        // 将tracking flag置为0
        TrackingFlag = 0;
    }

    // 从沙盘场景的第二层返回第一层
    public void ReturnFromSecondPart()
    {
        // 将所有的小树销毁
        foreach (GameObject littleTrees in littleTreesinPartTwo)
        {
            Destroy(littleTrees);
        }

        // 将所有的连线销毁
        Transform[] allLines = linesParent.GetComponentsInChildren<Transform>();
        for (int al = 1; al < allLines.Length; al++)
        {
            Destroy(allLines[al].gameObject);
        }

        // 将所有簇变得可用
        foreach (MyClusterBase myClusterBaseItem in myClusterBase)
        {
            GameObject clusterParent = myClusterBaseItem.gameObject;
            clusterParent.SetActive(true);
        }

        // 将所有的连线变得可用
        foreach (LineRenderer eachBigLine in bigLines)
        {
            GameObject eachBigLineObject = eachBigLine.gameObject;
            eachBigLineObject.SetActive(true);
        }

        returnFromSecondStateButton.SetActive(false);
        restartButton.SetActive(true);

        TrackingFlag = 4;

    }

    // 从树这里返回沙盘
    public void ReturnFromTreeToShapan()
    {
        clickedFlag = false;
        Destroy(treeTrunkFromShapan);

        CourseNameInTree.text = "";
        CourseNameInTreeImage.SetActive(false);

        OnMessageButtonClicked[] buttonsInContent = content.GetComponentsInChildren<OnMessageButtonClicked>();
        for (int b = 0; b < buttonsInContent.Length; b++)
        {
            Destroy(buttonsInContent[b].gameObject);
        }
        // 将scroll view置为不可见
        scrollView.gameObject.SetActive(false);

        shapanBig.SetActive(true);
        shapanBig.AddComponent<MyTreeTrunkT>();
        treeTransform = shapanBig.GetComponent<MyTreeTrunkT>().transform;
        TrackingFlag = 5;
        returnFromTreeButton.SetActive(false);
        returnFromSecondStateButton.SetActive(true);
    }

    // 从直接生成到树返回沙盘
    public void ReturnFromTreeToShapanDirectly()
    {
        Debug.Log("flag: " + returntoShapan + ", domain: " + treeReturnToShapanName);

        Destroy(mytrunk);

        CourseNameInTree.text = "";
        CourseNameInTreeImage.SetActive(false);


        OnMessageButtonClicked[] buttonsInContent = content.GetComponentsInChildren<OnMessageButtonClicked>();
        for (int b = 0; b < buttonsInContent.Length; b++)
        {
            Destroy(buttonsInContent[b].gameObject);
        }
        // 将scroll view置为不可见
        scrollView.gameObject.SetActive(false);

        string api2_result = GetDataFromWeb.GetClusterDivided(treeReturnToShapanName);
        // 实例化沙盘模型
        shapanBig = Instantiate(shapanPrefab);
        shapanBig.transform.parent = groundPlaneStage.transform;
        shapanBig.transform.localPosition = Vector3.zero;
        shapanBig.transform.localScale = Vector3.one / 10;

        TextMesh courseNameMesh = shapanBig.GetComponentInChildren<TextMesh>();
        courseNameMesh.text = treeReturnToShapanName;

        shapan = shapanBig.GetComponentInChildren<MyShapanMianban>().gameObject;

        // 沙盘构建
        JsonData resultJson = JsonMapper.ToObject(api2_result);
        JsonData communities = resultJson["communities"];  // 社团数组
        JsonData communitySequence = resultJson["sequence"];   // 认知顺序
        JsonData communitySize = resultJson["topic_num"];   // 簇的大小
        domainID = resultJson["domainid"].ToString();

        // 获取社团内容，形成一个JsonData数组

        // ！！！数学运算

        // 求总数
        communityNum = communitySize.Count;

        // 尺寸均衡
        float[] sizeWeight = new float[communityNum];
        for (int i = 0; i < communityNum; i++)
        {
            sizeWeight[i] = (float)Math.Pow(int.Parse(communitySize[i].ToString()), 0.5);
        }

        // 求权重和
        float weightSum = 0;
        for (int i = 0; i < communityNum; i++)
        {
            weightSum += sizeWeight[i];
        }

        // 安置圆盘
        float[] weights = new float[communityNum];
        float sitaNow = 0;
        community = new JsonData[communityNum];
        const float clusterRadius = (float)0.35;

        Vector3 lastLinePoint = new Vector3(0, 0, 0);

        for (int i = 0; i < communityNum; i++)
        {
            weights[i] = sizeWeight[i] / weightSum; // 计算归一化权重
            float angle = weights[i] * 2 * PI;  // 计算当前所占角度
            if (i != 0) sitaNow -= angle / 2;   // 将角度记录增加一般当前角
            float radius = weights[i] * PERIMETER / 2;  // 计算当前半径

            if (communityNum <= 4)
            {
                radius = radius / 2;
            }

            GameObject cluster = Instantiate(clusterBases[i % clusterBases.Length]);  // 初始化圆盘
            string communityName = "community" + communitySequence[i];
            Debug.Log(communityName);
            community[i] = communities[i][communityName];   // 获取社团信息，存入数组

            TextMesh clusterNameMesh = cluster.GetComponentInChildren<TextMesh>();
            clusterNameMesh.text = community[i]["cluster_name"].ToString();

            cluster.name = i.ToString();    // 用数字命名object
            cluster.transform.parent = shapan.transform;    // 设置位姿
            cluster.transform.localPosition = new Vector3(XMAX * (float)Math.Cos(sitaNow), ZMAX * (float)Math.Sin(sitaNow), 0);
            if (communityNum <= 4)
            {
                cluster.transform.localPosition = new Vector3(XMAX * (float)Math.Cos(sitaNow) * 0.7f, ZMAX * (float)Math.Sin(sitaNow) * 0.7f, 0);
            }
            cluster.transform.localScale = new Vector3(radius, radius, radius);
            sitaNow -= angle / 2; // 再增加一般当前角

            // 连线
            if (i > 0)
            {
                LineRenderer aline = Instantiate(line);
                aline.transform.parent = shapan.transform;
                Vector3 sourcePosition = lastLinePoint;
                Vector3 targetPosition = cluster.transform.position;

                aline.SetPosition(0, sourcePosition);
                aline.SetPosition(1, targetPosition);
            }

            lastLinePoint = cluster.transform.position;

            // 绘制圆盘上的树
            int topicNum = int.Parse(community[i]["topic_num"].ToString());
            float singleAngle = 2 * (float)Math.PI / topicNum;
            for (int j = 0; j < topicNum; j++)
            {
                GameObject littleTree = Instantiate(treeModels[i % treeModels.Length]);
                littleTree.transform.parent = cluster.transform;
                //littleTree.transform.localPosition = new Vector3(clusterRadius * (float)Math.Cos(singleAngle * j) * 10 * (float)Math.Abs(Math.Sin(j + 1)), clusterRadius * (float)Math.Sin(singleAngle * j) * 10 * (float)Math.Abs(Math.Sin(j + 1)),0);
                littleTree.transform.localPosition = new Vector3(clusterRadius * (float)Math.Cos(singleAngle * j) * 10 * (float)Math.Abs(Math.Sin(j + 1)), 0, clusterRadius * (float)Math.Sin(singleAngle * j) * 10 * (float)Math.Abs(Math.Sin(j + 1)));
                littleTree.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
            }
        }
        shapanBig.AddComponent<MyTreeTrunkT>();
        treeTransform = shapanBig.GetComponent<MyTreeTrunkT>().transform;

        treeTransform = shapanBig.GetComponent<MyTreeTrunkT>().transform;

        Debug.Log("IIIIII" + treeTransform.gameObject.name);

        TrackingFlag = 4;

        returnFromTreeToShapanButton.SetActive(false);
        restartButton.SetActive(true);
    }
    public void OpenScanButtonOnClicked()
    {
        statusFlag = 2;
        openScanButton.SetActive(false);
        guidanceButton.SetActive(false);
        startButton.SetActive(true);
        restartButton.SetActive(true);

        tipsText.text = "请对准要扫面的页面，在字体清晰可见时，点击确定按钮，将进行拍照识别关键词";
        ClusterButtonObj[] clusterButtonObjs = clusterChoice.GetComponentsInChildren<ClusterButtonObj>();
        foreach (var itemClusterButton in clusterButtonObjs)
        {
            Destroy(itemClusterButton.gameObject);
        }
        clusterChoice.SetActive(false);
    }
    public void GuidanceButtonOnClicked()
    {
        var webViewGameObject = new GameObject("UniWebView");
        var webView = webViewGameObject.AddComponent<UniWebView>();
        webView.insets = new UniWebViewEdgeInsets(0, 0, 0, 0);
        webView.toolBarShow = true;
        //Debug.Log("guidanceUrl:"+guidanceUrl);
        //guidanceUrl="http://47.95.145.72:8083/assemble/getAssembleContentById?assembleId=2917964";
        webView.Load("http://" + guidanceUrl);
        webView.Show();
    }
    void initialThreadFunction()
    {
        var str = GetDataFromWeb.GetConfiguration();
        JsonData tempJson = JsonMapper.ToObject(str);
        if (tempJson["id"] != null)
        {
            var coursesJson = tempJson["courses_name_list"];
            // guidanceUrl = JsonMapper.ToJson(tempJson["guidance_url"]);
            // Debug.Log("guidance_url:" + guidanceUrl);
            guidanceUrl = tempJson["guidance_url"].ToString();
            //Debug.Log("guidance_url:" + guidanceUrl);
            var num = coursesJson.Count;
            //Debug.Log("num:" + num);
            JsonData replacement = new JsonData();
            replacement["result_id"] = 0;
            replacement["result_num"] = num;
            replacement["results"] = new JsonData();
            replacement["results"].SetJsonType(JsonType.Array);
            replacement["results"].Add(0);
            replacement["results"][0].SetJsonType(JsonType.Array);
            replacement["results"][0] = coursesJson;
            OCR_RESULT = coursesNameList = JsonMapper.ToJson(replacement);
            //Debug.Log("Thread:ocr: " + OCR_RESULT);
            ocrThreadFlag = true;
        }
    }
    private GameObject Branch3Random()
    {
        int branchName = UnityEngine.Random.Range(1, 7);
        GameObject branch3;
        switch (branchName)
        {
            case 1:
                branch3 = Instantiate(b3_1);
                break;
            case 2:
                branch3 = Instantiate(b3_2);
                break;
            case 3:
                branch3 = Instantiate(b3_3);
                break;
            case 4:
                branch3 = Instantiate(b3_4);
                break;
            case 5:
                branch3 = Instantiate(b3_5);
                break;
            case 6:
                branch3 = Instantiate(b3_6);
                break;
            default:
                branch3 = Instantiate(b3_3);
                break;
        }
        return branch3;
    }
    private GameObject Branch2Random()
    {
        int branchName = UnityEngine.Random.Range(1, 6);
        GameObject branch2;
        switch (branchName)
        {
            case 1:
                branch2 = Instantiate(b2_1);
                break;
            case 2:
                branch2 = Instantiate(b2_2);
                break;
            case 3:
                branch2 = Instantiate(b2_3);
                break;
            case 4:
                branch2 = Instantiate(b2_4);
                break;
            case 5:
                branch2 = Instantiate(b2_5);
                break;
            default:
                branch2 = Instantiate(b2_3);
                break;

        }
        return branch2;
    }
}
