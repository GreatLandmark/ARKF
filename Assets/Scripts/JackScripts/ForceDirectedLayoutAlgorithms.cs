// __Author__='Jack Luo'
// __Email__='greatlandmark@outlook.com'
// 力导向布局算法；
/*斥力：Fr=Kr*Q1*Q2/r^2 ；引力：Fs=Ks*(x1-x2)；偏移量 Δx=FxΔt;Δy=FyΔt；Δt为步长；
*/

using UnityEngine;
using System;
public class ForceDirectedLayoutAlgorithms
{
    int[,] edgeMatrix;//邻接矩阵
    int matrixSize;//点的个数
    Vector2[] points;//记录个点的坐标
    Vector2[] delta;//每次迭代的位移
    float kr = 30;//斥力系数
    float ks = 1;//引力系数
    float dis0;//两点最小距离;
    float delta_t;//
    float[,] force;//x:0;y:1
    float MAX_DISPLACEMENT_SQ = 0.3f;
    int iterateNum = 0;
    public ForceDirectedLayoutAlgorithms(int[,] edgeMatrix, int iterateNum = 5)
    {
        this.edgeMatrix = edgeMatrix;
        this.iterateNum = iterateNum;
        matrixSize = edgeMatrix.GetLength(0);
        Debug.Log("matrixSize:" + matrixSize);
        points = new Vector2[matrixSize];
        delta = new Vector2[matrixSize];
        force = new float[matrixSize, 2];//x:0;y:1
        //初始化 点的位置
        var a = (float)Math.Sqrt(matrixSize);
        for (int i = 0; i < matrixSize; i++)
        {
            points[i].x = 0.5f + 0.5f * (float)Math.Cos(a);
            points[i].y = 0.5f + (float)Math.Sin(a) * 0.5f;
            a += a;
        }
        //
        dis0 = 1f / matrixSize;
        delta_t = dis0;
    }
    //
    public Vector2[] Run()
    {
        RepeatCalc(iterateNum);
        var scale=1.0f+(1.0f/matrixSize);
        for(int i=0;i<matrixSize;i++)
        {
            points[i].x*=scale;
            points[i].y*=scale;
        }
        return points;
    }
    //更新库伦斥力
    void UpdateReplusion()
    {
        float dx, dy, f, fx, fy, d, dsq;
        for (int i = 0; i < matrixSize - 1; i++)
        {
            for (int j = i + 1; j < matrixSize; j++)
            {
                dx = points[j].x - points[i].x;
                dy = points[j].y - points[i].y;
                if (dx != 0 || dy != 0)
                {
                    dsq = dx * dx + dy * dy;
                    d = (float)Math.Sqrt(dsq);
                    f = kr / dsq;//Q1、Q2取1；
                    fx = f * dx / d;
                    fy = f * dy / d;
                    force[i, 0] -= fx;
                    force[i, 1] -= fy;
                    force[j, 0] += fx;
                    force[j, 1] += fy;
                    // points[j].force_x += fx;
                    // points[j].force_y += fy;
                }
            }
        }
    }
    //更新弹簧引力
    void UpdateSpring()
    {
        float dx, dy, f, fx, fy, d;
        for (int i = 0; i < matrixSize - 1; i++)
        {
            for (int j = i + 1; j < matrixSize; j++)//(Vector<edge>::iterator j = points[i].adjList.begin(); j != points[i].adjList.end(); j++)
            {
                if (edgeMatrix[i, j] == 1)
                {
                    dx = points[j].x - points[i].x;
                    dy = points[j].y - points[i].y;
                    if (dx != 0 || dy != 0)
                    {
                        d = (float)Math.Sqrt(dx * dx + dy * dy);
                        f = ks * (d - dis0);
                        fx = f * dx / d;
                        fy = f * dy / d;
                        force[i, 0] += fx;
                        force[i, 1] += fy;
                        force[j, 0] -= fx;
                        force[j, 1] -= fy;
                    }
                }
            }
        }
    }
    //更新位置
    void UpdatePosition()
    {
        float dx, dy, dsq, s;
        for (int i = 0; i < matrixSize; i++)
        {
            dx = delta_t * force[i, 0];
            dy = delta_t * force[i, 1];
            dsq = dx * dx + dy * dy;
            if (dsq > MAX_DISPLACEMENT_SQ)
            {
                s =(float) Math.Sqrt(MAX_DISPLACEMENT_SQ / dsq);
                dx *= s;
                dy *= s;
            }
            points[i].x += dx;
            points[i].y += dy;
        }
    }
    //迭代n次
    void RepeatCalc(int n)
    {
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < matrixSize; j++)
            {
                force[j, 0] = 0.0f;
                force[j, 1] = 0.0f;
            }
            UpdateReplusion();
            UpdateSpring();
            UpdatePosition();
        }
    }

}