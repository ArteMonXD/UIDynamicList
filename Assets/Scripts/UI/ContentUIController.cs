using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContentUIController : MonoBehaviour
{
    [SerializeField] private GameObject contentPrefab;
    [SerializeField] private Transform listParent;
    [SerializeField] private EventSystem eventSystem;

    private ContentSystem contentSystem;
    private List<ContentElement> elements = new List<ContentElement>();

    void Start()
    {
        contentSystem = FindFirstObjectByType<ContentSystem>();
        eventSystem = FindFirstObjectByType<EventSystem>();

        contentSystem.OnContentAdded += AddElement;
        contentSystem.OnContentRemoved += RemoveElement;
        contentSystem.OnContentChanged += ChangeElement;

        FillStartList();
        eventSystem.firstSelectedGameObject = elements[0].gameObject;
    }

    private void FillStartList()
    {
        int count = contentSystem.CountContent;

        for(int i = 0; i<count; i++)
        {
            var elementGO = Instantiate(contentPrefab, listParent);
            var elementUI = elementGO.GetComponent<ContentElement>();

            if(!contentSystem.GetContent(i, out Content content))
                continue;

            elementUI.SetContent(content);

            elements.Add(elementUI);
        }
    }

    private void AddElement(Content content)
    {
        if(content == null)
            return;

        var elementGO = Instantiate(contentPrefab, listParent);
        var elementUI = elementGO.GetComponent<ContentElement>();

        elementUI.SetContent(content);

        elements.Add(elementUI);
    }

    private void RemoveElement(int index)
    {
        if (index < 0 || index >= elements.Count)
            return;

        GameObject element = elements[index].gameObject;
        elements.RemoveAt(index);
        Destroy(element);
    }

    private void ChangeElement(int index, Content content)
    {
        if (index < 0 || index >= elements.Count || content == null)
            return;

        elements[index].SetContent(content);
    }

    private void OnDestroy()
    {
        if (contentSystem == null)
            return;

        contentSystem.OnContentAdded -= AddElement;
        contentSystem.OnContentRemoved -= RemoveElement;
        contentSystem.OnContentChanged -= ChangeElement;
    }
}
