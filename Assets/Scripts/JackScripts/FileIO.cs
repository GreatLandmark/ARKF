// __Author__='Jack Luo'
// __Email__='greatlandmark@outlook.com'


// mainscript 调用

using System;
using System.IO;

class SaveImage
{
    static string rootPath = System.IO.Directory.GetCurrentDirectory() + "/Jack/images/";
    //static string path = rootPath + "i5.jpeg";
    SaveImage()
    {
        System.Console.WriteLine(rootPath);
    }
    public static void Save(byte[] bytes,string fileName="it1.jpeg")
    {
        string path=rootPath+fileName;
        try
        {
            BinaryWriter bw = new BinaryWriter(new FileStream(path,
                            FileMode.Create));
            bw.Write(bytes);
            bw.Close();
            
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
        }

    }

}