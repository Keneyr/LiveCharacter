using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : Singleton<CharacterManager>
{
    [SerializeField]
    [Tooltip("控制台输出框对象")]
    private RawImage rawImage;

    RectTransform rt;

    public Texture2D tx;
    void Awake()
    {
        base.Awake();
        rawImage = this.rawImage;
        rt = GetComponent<RectTransform>();
    }
    public void LoadCharacter(string imagePath)
    {
        tx = new Texture2D(100,100);
        tx.LoadImage(GetImageByte(imagePath));
        rawImage.texture = tx;
        ConsoleController.instance.ShowMessage("image width,height is: " + tx.width + ", " + tx.height);
        // screen: w 512 h 294
        float sx = 512, sy = 294;
        float aspect= sx / sy;
        if (tx.height >= tx.width && tx.height > sy) {
            float ratio = tx.height / sy;
            sx = tx.width / ratio;
        }
        else if (tx.width > tx.height && tx.width > sx)
        {
            if (tx.width < aspect * tx.height)
            {

                float ratio = tx.height / sy;
                sx = tx.width / ratio;
            }
            else {
                float ratio = tx.width / sx;
                sy = tx.height / ratio;
            }
        }
        rt.sizeDelta = new Vector2(sx, sy);
    }
    //根据图片路径返回图片的字节流byte[]
    private byte[] GetImageByte(string imagePath)
    {
        FileStream files = new FileStream(imagePath, FileMode.Open);
        byte[] imgByte = new byte[files.Length];
        files.Read(imgByte, 0, imgByte.Length);
        files.Close();
        return imgByte;
    }
}
