// __Author__='Jack Luo'
// __Email__='greatlandmark@outlook.com'


//mainscript 调用

using System.Net;
using System.IO;
using System.Text;
using Baidu.Aip.Ocr;
//using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;


public class GetDataFromWeb
{//包含所有与网络请求相关的方法。
    #region PARAMETERS
    // 设置百度接口数据
    //private const string APP_ID = "18612985";
    private const string API_KEY = "BIrTfDnT6LUkVH0VAehysLPD";
    private const string SECRET_KEY = "THQt3gdYgMcR0f4zb91Xn4FQaByIuRLs";
    private static Ocr baiduOCRapi = new Ocr(API_KEY, SECRET_KEY);  // 初始化百度接口

    /* 实验室的几个API：
    API1:"http://yotta.xjtushilei.com:8000/crystal/judgeDomainAndGetLocation/?OCR_result=" + ocrResult;
    API2:"http://yotta.xjtushilei.com:8000/crystal/clusterDivided/?domain=" + clusterName;
    API3:"http://yotta.xjtushilei.com:8000/crystal/get_AllInByTopicNameAndDomainId/?topic=" + itemName + "&domainid=" + domainid;
    API4:"http://yotta.xjtushilei.com:8000/crystal/getConfigurations/";
     */
    /*
    课程名列表的json数据格式：
   {"id":0 ,"items_num":1 ,"courses_name_list":["计算机组成原理","C语言","操作系统","计算机系统结构","数据结构","低年级(1-2)语文","低年级(1-2)科学","高年级(5-6)数学","高二数学","高一语文"]}；
    */
    #endregion
    //获取app的配置，例如：初始化的课程列表；
    public static string GetConfiguration()
    {
        // 开始请求API4获得配置信息；
        string api4Url = "http://yotta.xjtushilei.com:8000/crystal/getConfigurations/";
        var result= HttpHelperGetJson(api4Url);
        //Debug.Log("api4result:"+result);
        return result;
    }
    public static void GetKeywordsT(object imageObject)
    {//用于被 MainScript 使用子线程 调用，
        MainScript.OCR_RESULT = GetKeywords("", imageObject);
        if (MainScript.OCR_RESULT != "")
        {
            MainScript.ocrThreadFlag = true;
        }
    }
    public static void GetKeywordsT2(object ocrResult)
    {//用于被 MainScript 使用子线程 调用，

        MainScript.OCR_RESULT = JudgeDomainAPI1(ocrResult.ToString());
        if (MainScript.OCR_RESULT != "")
        {
            MainScript.ocrThreadFlag = true;
        }
    }
    /*
     * 百度OCR接口部分
     * */
    public static string BaiduOCR(object imageObject)
    {//文字识别，使用百度。

        baiduOCRapi.Timeout = 60000; // 修改OCR超时时间
        byte[] image = (byte[])imageObject;    // 拆箱
        var options = new Dictionary<string, object>{
            {"detect_direction", "true"}
        };  // 增加识别方向参数,识别单字结果
        var ocrResultJson = baiduOCRapi.General(image, options);    // 带参数调用通用文字识别（含位置信息版）
        string ocrResultString = ocrResultJson.ToString();   // ocrResultJson为OCR结果json格式
        Debug.Log(ocrResultString);
        return ocrResultString;
    }

    public static string GetKeywords(string ocrResult = "", object imageObject = null)
    {//将OCR结果 通过实验室API1 与 实验室数据库匹配，判断是否有该 关键字（词）。
        if (ocrResult == "")
        {
            if (imageObject == null)
            {
                return "";
            }
            ocrResult = BaiduOCR(imageObject);
        }
        return JudgeDomainAPI1(ocrResult);
    }
    public static string JudgeDomainAPI1(string ocrResult)
    {
        // 开始请求API1获得追踪位置坐标与关键词及其位置坐标
        string api1Url = "http://yotta.xjtushilei.com:8000/crystal/judgeDomainAndGetLocation/?OCR_result=" + ocrResult;
        string OCR_RESULT=HttpHelperGetJson(api1Url);
        /* HttpWebRequest api1Request = (HttpWebRequest)WebRequest.Create(api1Url);
        api1Request.Method = "GET";
        api1Request.ContentType = "application/json";
        // 捕获请求失败的异常
        try
        {
            HttpWebResponse api1Response = (HttpWebResponse)api1Request.GetResponse();
            StreamReader reader = new StreamReader(api1Response.GetResponseStream(), Encoding.Default);
            OCR_RESULT = reader.ReadToEnd();
            Debug.Log(OCR_RESULT);
            //判断线程结束，ture。
        }
        catch (WebException e)
        {
            WebResponse webRsp = (HttpWebResponse)e.Response;
            StreamReader reader = new StreamReader(webRsp.GetResponseStream(), Encoding.Default);
            Debug.Log("网络请求出现了问题");
            Debug.Log(e.ToString());

            OCR_RESULT = "";
        }

        api1Request.Abort();    // 结束网络请求 */
        return OCR_RESULT;
    }

    //从实验室API3 获取课程全部数据。
    public static string GetAllByTopicNameAndDomainId(string itemName, string domainid)
    {
        
        string api3Url = "http://yotta.xjtushilei.com:8000/crystal/get_AllInByTopicNameAndDomainId/?topic=" + itemName + "&domainid=" + domainid;
        string api3_result=HttpHelperGetJson(api3Url);
        /* HttpWebRequest api3Request = (HttpWebRequest)WebRequest.Create(api3Url);
        api3Request.Method = "GET";
        api3Request.ContentType = "application/json";
        try
        {
            HttpWebResponse api3Response = (HttpWebResponse)api3Request.GetResponse();
            StreamReader reader = new StreamReader(api3Response.GetResponseStream(), Encoding.Default);
            api3_result = reader.ReadToEnd();
            Debug.Log(api3_result);
        }
        catch (WebException e)
        {
            Debug.Log(e.ToString());
            api3_result = "";
        }
        api3Request.Abort(); */
        return api3_result;
    }

    //API2 得到这门课程所有主题划分好的簇，以及簇间的顺序,同时会返回domainid.
    public static string GetClusterDivided(string clusterName)
    {
        
        string api2Url = "http://yotta.xjtushilei.com:8000/crystal/clusterDivided/?domain=" + clusterName;
        string api2_result=HttpHelperGetJson(api2Url);
        /* HttpWebRequest api2Request = (HttpWebRequest)WebRequest.Create(api2Url);
        api2Request.Method = "GET";
        api2Request.ContentType = "application/json";

        try
        {
            HttpWebResponse api2Response = (HttpWebResponse)api2Request.GetResponse();
            StreamReader reader = new StreamReader(api2Response.GetResponseStream(), Encoding.Default);
            api2_result = reader.ReadToEnd();
            Debug.Log(api2_result);
        }
        catch
        {
            Debug.Log("网络请求出现异常，请检查网络链接");
            api2_result = "";
        }

        api2Request.Abort(); */
        return api2_result;
    }

    static string HttpHelperGetJson(string url)
    {
        string result;
        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        request.Method = "GET";
        request.ContentType = "application/json";
        // 捕获请求失败的异常
        try
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);
            result = reader.ReadToEnd();
            Debug.Log("Http请求响应的结果："+result);
        }
        catch (WebException e)
        {
            //  WebResponse webRsp = (HttpWebResponse)e.Response;
            // StreamReader reader = new StreamReader(webRsp.GetResponseStream(), Encoding.Default);
            Debug.Log("网络请求出现了问题");
            Debug.Log(e.ToString());
            result = "";
        }
        request.Abort();
        return result;
    }
}