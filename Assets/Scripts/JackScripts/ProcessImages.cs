
// __Author__='Jack Luo'
// __Email__='greatlandmark@outlook.com'


using UnityEngine;
public class ProcessImages
{
    //处理图片的类。

    // 左右翻转图片
    public static Texture2D HorizontalFlipPicture(Texture2D texture2d)
    {
        int width = texture2d.width;    // 图片宽度  
        int height = texture2d.height;  // 图片高度 

        Texture2D newTexture2d = new Texture2D(width, height);  // 创建等大小的新Texture2D 

        int i = 0;
        while (i < width)
        {
            newTexture2d.SetPixels(i, 0, 1, height, texture2d.GetPixels(width - i - 1, 0, 1, height));
            i++;
        }
        newTexture2d.Apply();

        return newTexture2d;
    }

    //上下反转图片
    public static Texture2D VerticalFlipPicture(Texture2D texture2d)
    {
        int width = texture2d.width;    // 图片宽度  
        int height = texture2d.height;  // 图片高度 

        Texture2D newTexture2d = new Texture2D(width, height);  // 创建等大小的新Texture2D 

        int i = 0;
        while (i < height)
        {
            newTexture2d.SetPixels(0, i, width, 1, texture2d.GetPixels(0, height - i - 1, width, 1));
            i++;
        }
        newTexture2d.Apply();

        return newTexture2d;
    }
}