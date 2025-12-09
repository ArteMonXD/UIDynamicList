using System;
using UnityEngine;


[Serializable]
public class Content
{
    [SerializeField] private string mainText;
    [SerializeField] private Sprite icon;
    [SerializeField] private string fullText;
    public string Main => mainText;
    public string Full => fullText;
    public Sprite Icon => icon;

    public bool SetData(string main, string full, Sprite img)
    {
        if(main == null || full == null || icon == null)
            return false;

        mainText = main;
        fullText = full;
        icon = img;
        return true;
    }
}
