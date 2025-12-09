using System.Collections.Generic;
using UnityEngine;

public class ContentSystem : MonoBehaviour
{
    [SerializeField] private List<Content> contentList = new List<Content>();
    public int CountContent => contentList.Count;

    public event System.Action<Content> OnContentAdded;
    public event System.Action<int> OnContentRemoved;
    public event System.Action<int, Content> OnContentChanged;

    public bool GetContent(int index, out Content result)
    {
        result = null;

        if(index < 0 || index >= CountContent) return false;

        Content content = contentList[index];
        
        if(content == null) return false;
        else
        {
            result = content;
            return true;
        }
    }

    public bool AddContent(Content content)
    {
        if(content == null) return false;

        contentList.Add(content);
        OnContentAdded?.Invoke(content);
        return true;
    }

    public bool AddContent(string main, string full, Sprite icon)
    {
        Content content = new Content();

        if (content.SetData(main, full, icon))
        {
            OnContentAdded?.Invoke(content);
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool RemoveContent(int index) 
    {
        if (index < 0 || index >= CountContent) return false;

        contentList.RemoveAt(index);
        OnContentRemoved?.Invoke(index);
        return true;
    }

    public bool ChangeContent(int index, Content content)
    {
        if(index < 0 || index >= CountContent) return false;

        contentList[index] = content;
        OnContentChanged?.Invoke(index, content);
        return true;
    }

    public bool ChangeContent(int index, string main, string full, Sprite icon)
    {
        if (index < 0 || index >= CountContent) return false;

        Content content = new Content();

        if (content.SetData(main, full, icon))
        {
            contentList[index] = content;
            OnContentChanged?.Invoke(index, content);
            return true;
        }
        else
        {
            return false;
        }
    }
}
