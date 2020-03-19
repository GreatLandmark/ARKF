using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// 这里完成了点的分类与里导向算法，但是里导向算法没起作用，后续可以更改LOOPTIMES让其进行迭代

public class SpringyGraph// : MonoBehaviour
{
    private int[,] edgeMatrix;  // 邻接矩阵
    private int matrixSize;     // 元素个数
    private int[] sequence;     // 记录重排顺序
    private float[,] points;  // 记录各个点的坐标
    private Vector2[,] pointShifts; // 计算各个点之间的位移
    private float[,] shiftLength;   // 计算各个点之间的距离

    private const float XMAX = 0.7f;
    private const float YMAX = 0.5f;
    private const float K = 40;    // 斥力系数
    private const int LOOPTIMES = 5;    // 循环迭代次数
    private const float DATATIME = 0.0002f;   // 单位时间
    private const float PI = (float)Math.PI;

    private const float type1Length = 0.9f;
    private const float type2Length = 0.6f;
    private const float type3Length = 0.3f;

    public SpringyGraph(int[,] getEdgeMatrix)
    {
        edgeMatrix = getEdgeMatrix;
        matrixSize = edgeMatrix.GetLength(0);
        points = new float[matrixSize, 2];
        pointShifts = new Vector2[matrixSize, matrixSize];
        shiftLength = new float[matrixSize, matrixSize];
    }

    public float[,] SpringyGraphMain()
    {
        float[,] pointsPosition = new float[matrixSize, 3];

        sequence = Rearrange();   // 重排
        SetInitialPosition();   // 手动或随机设置初始点

        if (matrixSize > 4)
        {
            // 使用排布算法
            Springy();
        }

        // 将点与重排后点顺序对应起来
        for (int p = 0; p < matrixSize; p++)
        {
            pointsPosition[p, 0] = points[p, 0];
            pointsPosition[p, 1] = points[p, 1];
            pointsPosition[p, 2] = sequence[p];
        }

        return pointsPosition;
    }

    // 按照连线多少重排邻接矩阵
    private int[] Rearrange()
    {
        // 记录重排后原来的序号的顺序
        int[] arrangedSequence = new int[matrixSize];
        for (int a = 0; a < matrixSize; a++)
            arrangedSequence[a] = a;

        // 冒泡排序
        for (int i = 0; i < matrixSize; i++)
        {
            for (int j = 0; j < matrixSize - i - 1; j++)
            {
                if (edgeMatrix[j, matrixSize] < edgeMatrix[j + 1, matrixSize])
                {
                    int temp;
                    for (int k = 0; k < matrixSize + 1; k++)
                    {
                        temp = edgeMatrix[j, k];
                        edgeMatrix[j, k] = edgeMatrix[j + 1, k];
                        edgeMatrix[j + 1, k] = temp;
                    }
                    temp = arrangedSequence[j];
                    arrangedSequence[j] = arrangedSequence[j + 1];
                    arrangedSequence[j + 1] = temp;
                }
            }
        }

        return arrangedSequence;
    }

    // 设置初始点的位置
    private void SetInitialPosition()
    {
        for (int ss = 0; ss < matrixSize; ss++)
            Debug.Log("sequence " + ss + " : " + sequence[ss]);

        if (matrixSize == 1)
        {
            points[0, 0] = 0.0f;
            points[0, 1] = 0.0f;
        }
        else if (matrixSize == 2)
        {
            points[0, 0] = 0.25f;
            points[0, 1] = 0.0f;
            points[1, 0] = -0.25f;
            points[1, 1] = 0.0f;
        }
        else if (matrixSize == 3)
        {
            points[0, 0] = 0.0f;
            points[0, 1] = 0.0f;
            points[1, 0] = 0.3f;
            points[1, 1] = 0.0f;
            points[2, 0] = -0.3f;
            points[2, 1] = 0.0f;
        }
        else if (matrixSize == 4)
        {
            points[0, 0] = 0.0f;
            points[0, 1] = -0.2f;
            points[1, 0] = 0.3f;
            points[1, 1] = 0.0f;
            points[2, 0] = -0.3f;
            points[2, 1] = 0.0f;
            points[3, 0] = 0.0f;
            points[3, 1] = 0.2f;
        }
        else
        {
            int type1Num = 0, type2Num = 0, type3Num = 0;
            int[,] type1 = new int[matrixSize, 2];   // 记录一类点，0--一类点自身编号，1--所连接点二类点编号
            int[,] type2 = new int[matrixSize, 2];   // 记录二类点，0--二类点自身编号，1--所连接点一类点的个数
            int[] type3 = new int[matrixSize];

            // 寻找一类点与二类点
            int i;  // 用于记录一类点从后往前到多少行截止
            for (i = matrixSize - 1; i >= 0; i--)
            {
                // 寻找一类点
                if (edgeMatrix[i, matrixSize] == 1)
                {
                    type1[type1Num, 0] = i;   // 一类点的标号，即重排后的序号
                    type1Num++;

                    // 找到对应点二类点
                    for (int j = 0; j < matrixSize; j++)
                    {
                        if (edgeMatrix[i, j] == 1)
                        {
                            for (int k = 0; k < matrixSize; k++)
                            {
                                // 循环sequence，寻找对应的重排后的编号
                                if (sequence[k] == j) // 在sequence中找到了对应的二类点，k即其当前编号
                                {
                                    type1[type1Num - 1, 1] = k; // 记录一类点对应的二类点

                                    int existFlag = 0;  // 记录在type2中是否已存在该二类点
                                    for (int l = 0; l < type2Num; l++)
                                    {
                                        if (type2[l, 0] == k)    // 如果现在的二类点在type2中存在
                                        {
                                            existFlag = 1;
                                            type2[l, 1]++;
                                            break;  // 没有必要继续循环type2
                                        }
                                    }
                                    if (existFlag == 0) // 如果type2中还没有这个点，加入该点
                                    {
                                        type2[type2Num, 0] = k;
                                        type2[type2Num, 1] = 1;
                                        type2Num++;
                                    }

                                    break;
                                }
                            }

                            break;  // 没有必要继续循环本行
                        }
                    }
                }
                else
                {
                    // 如果已经不再等于1，直接跳出循环，一类点与其对应的二类点寻找完毕
                    break;
                }
            }

            // 找寻三类点
            for (int l = 0; l < i + 1; l++)
            {
                int existFlag = 0;
                for (int k = 0; k < type2Num; k++)
                {
                    if (l == type2[k, 0])
                    {
                        existFlag = 1;
                        break;
                    }
                }
                if (existFlag == 0)
                {
                    // 表示该值不是二类点
                    type3[type3Num] = l;
                    type3Num++;
                }
            }

            // 打印三个数组

            for (int t1 = 0; t1 < type1Num; t1++)
            {
                Debug.Log("type1: " + type1[t1, 0] + " -- " + type1[t1, 1]);
            }
            for (int t2 = 0; t2 < type2Num; t2++)
            {
                Debug.Log("type2: " + type2[t2, 0] + " -- " + type2[t2, 1]);
            }
            for (int t3 = 0; t3 < type3Num; t3++)
            {
                Debug.Log("type3: " + type3[t3]);
            }


            // 安排一类点二类点的初始位置
            float detaSita = 2 * PI / type1Num; // 按照一类点数量换分扇区
            float type1Sita = 0;
            float type2Sita = 0;
            for (int m = 0; m < type2Num; m++)
            {
                type2Sita += detaSita * type2[m, 1] / 2;

                // 安排二类点的初始位置
                points[type2[m, 0], 0] = type2Length * (float)Math.Cos(type2Sita);
                points[type2[m, 0], 1] = type2Length * (float)Math.Sin(type2Sita);

                type2Sita += detaSita * type2[m, 1] / 2;

                // 安排与刚才安排好的二类点对应的一类点的位置
                for (int n = 0; n < type1Num; n++)
                {
                    // 遍历type1，找到属于现在二类点的一类点
                    if (type1[n, 1] == type2[m, 0])
                    {
                        type1Sita += detaSita / 2;
                        points[type1[n, 0], 0] = type1Length * (float)Math.Cos(type1Sita);
                        points[type1[n, 0], 1] = type1Length * (float)Math.Sin(type1Sita);
                        type1Sita += detaSita / 2;
                    }
                }
            }

            // 安排三类点的初始位置
            float detaSitaforType3 = 2 * PI / type3Num;
            float type3Sita = 0;
            for (int t = 0; t < type3Num; t++)
            {
                type3Sita += detaSitaforType3 / 2;
                points[type3[t], 0] = type3Length * (float)Math.Cos(type3Sita);
                points[type3[t], 1] = type3Length * (float)Math.Sin(type3Sita);
                type3Sita += detaSitaforType3 / 2;
            }
        }

    }

    // 布局算法
    private void Springy()
    {
        // 进入超迭代循环
        for (int loop = 0; loop < LOOPTIMES; loop++)
        {
            CaculateShifts();
            float[,] shifts = new float[matrixSize, 2];

            // 计算位移
            // 遍历每个点
            for (int i = 0; i < matrixSize; i++)
            {
                Vector2 repulsion = new Vector2();
                Vector2 gravitation = new Vector2();
                // 遍历其余每个点
                for (int j = 0; j < matrixSize; j++)
                {
                    if (i == j) continue;
                    // 计算斥力
                    repulsion +=K* pointShifts[j, i] / (float)Math.Pow(shiftLength[i, j], 2);

                    // 计算引力
                    if (edgeMatrix[i, j] == 1)
                    {
                        gravitation += pointShifts[i, j];
                    }
                }

                // 计算合力/加速度
                Vector2 resultant = repulsion + gravitation;
                shifts[i, 0] = resultant.x * DATATIME ;//* DATATIME;
                shifts[i, 1] = resultant.y * DATATIME ;//* DATATIME;
            }

            // 移动点
            for (int k = 0; k < matrixSize; k++)
            {
                points[k, 0] += shifts[k, 0];
                points[k, 1] += shifts[k, 1];
            }
        }
    }

    // 计算点与点之间点位移
    private void CaculateShifts()
    {
        for (int s = 0; s < matrixSize; s++)
        {
            for (int t = 0; t < matrixSize; t++)
            {
                if (s == t) continue;
                pointShifts[s, t] = new Vector2(points[t, 0] - points[s, 0], points[t, 1] - points[s, 1]);
                shiftLength[s, t] = pointShifts[s, t].magnitude;
            }
        }
    }

    // 辅助函数，用于打印
    public void LogMessage()
    {

    }
}
