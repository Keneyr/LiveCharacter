using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class CharacterManager : Singleton<CharacterManager>
{
    [SerializeField]
    [Tooltip("控制台输出框对象")]
    private RawImage rawImage;

    RectTransform rt;

    public Texture2D tx; //角色图片
    void Awake()
    {
        base.Awake();
        //rawImage = this.rawImage;
        rawImage = GetComponent<RawImage>();
        rt = GetComponent<RectTransform>();
    }
    public ImportCharacterResult LoadCharacter(string imagePath)
    {
        ResetPastCharacterInfo();
        if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
        {
            return ImportCharacterResult.FilePathError;
        }

        tx = new Texture2D(100,100);
        tx.LoadImage(Extra.GetImageByte(imagePath));
        tx.name = imagePath; //用文件路径，OpenPose使用

        rawImage.texture = tx;
        Console.Log("image width,height is: " + tx.width + ", " + tx.height);

        Extra.AutoFill(tx,rt);
        
        
        return ImportCharacterResult.Success;
    }
    void ResetPastCharacterInfo()
    {
        CharacterPreprocessing.ResetPastCharacterInfo();
    }
}
