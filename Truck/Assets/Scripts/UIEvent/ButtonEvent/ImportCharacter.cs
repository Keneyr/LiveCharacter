using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using UnityEngine.UI;

public enum ImportCharacterResult
{
    Success = 0,
    FilePathError = 1,
}

public class ImportCharacter : MonoBehaviour
{
    public void OpenFile()
    {
        OpenFileDlg pth = new OpenFileDlg();
        pth.structSize = System.Runtime.InteropServices.Marshal.SizeOf(pth);
        pth.filter = null;
        pth.file = new string(new char[256]);
        pth.maxFile = pth.file.Length;
        pth.fileTitle = new string(new char[64]);
        pth.maxFileTitle = pth.fileTitle.Length;
        pth.initialDir = Application.dataPath;  // default path  
        pth.title = "打开角色文件";
        pth.defExt = "png,jpg,psd";
        pth.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;
        if (BaseFileDialog.GetOpenFileName(pth))
        {
            string filepath = pth.file; //选择的文件路径;
            Console.Log("导入角色图像文件...");
            Console.Log(filepath);

            //将路径中\转化为/
            filepath = filepath.Replace("\\", "/");

            ImportCharacterResult code = CharacterManager.instance.LoadCharacter(filepath);
            switch(code)
            {
                case ImportCharacterResult.Success:
                    Console.Log("ImportCharacter Complete");
                    break;
                case ImportCharacterResult.FilePathError:
                    Console.Log("ImportCharacter FilePathError");
                    break;
                default:
                    Console.LogError("Error:PoseExtract end with an Unknown resultCode");
                    break;
            }
        }
    }


}
