using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(ScrollRect))]
public class ScrollToSelected : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 10f;
    [SerializeField] private float edgePadding = 50f; // Отступ от краев
    [SerializeField] private bool smoothScrolling = true;

    [SerializeField] private bool scrollOnSelectionChange = true;
    [SerializeField] private bool scrollOnPanelExpand = true;

    private ScrollRect scrollRect;
    private RectTransform content;
    private RectTransform viewport;
    private GameObject lastSelected;
    private Coroutine scrollCoroutine;

    void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        content = scrollRect.content;
        viewport = scrollRect.viewport != null ? scrollRect.viewport :
                  (RectTransform)scrollRect.transform;

        if (scrollOnSelectionChange)
        {
            StartCoroutine(MonitorSelection());
        }
    }

    void OnEnable()
    {
        lastSelected = null;
    }

    void OnDisable()
    {
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    IEnumerator MonitorSelection()
    {
        while (true)
        {
            var currentSelected = EventSystem.current.currentSelectedGameObject;

            if (currentSelected != null &&
                currentSelected != lastSelected &&
                IsInScrollView(currentSelected))
            {
                lastSelected = currentSelected;
                ScrollTo(currentSelected.GetComponent<RectTransform>());
            }

            yield return null;
        }
    }

    bool IsInScrollView(GameObject obj)
    {
        if (obj == null) return false;

        return obj.transform.IsChildOf(content);
    }

    public void ScrollTo(RectTransform target)
    {
        if (target == null || scrollRect == null) return;

        // Останавливаем предыдущую прокрутку
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
        }

        if (smoothScrolling)
        {
            scrollCoroutine = StartCoroutine(SmoothScrollTo(target));
        }
        else
        {
            SnapTo(target);
        }
    }

    private void SnapTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        // Получаем позиции в локальных координатах content
        Vector3[] targetCorners = new Vector3[4];
        Vector3[] viewportCorners = new Vector3[4];

        target.GetWorldCorners(targetCorners);
        viewport.GetWorldCorners(viewportCorners);

        // Преобразуем к локальным координатам content
        for (int i = 0; i < 4; i++)
        {
            targetCorners[i] = content.InverseTransformPoint(targetCorners[i]);
            viewportCorners[i] = content.InverseTransformPoint(viewportCorners[i]);
        }

        // Вычисляем необходимое смещение
        Vector3 targetLocalPos = content.InverseTransformPoint(target.position);
        Vector3 viewportLocalCenter = content.InverseTransformPoint(viewport.position);

        // Вычисляем границы
        float viewportTop = viewportCorners[1].y;
        float viewportBottom = viewportCorners[0].y;
        float targetTop = targetCorners[1].y;
        float targetBottom = targetCorners[0].y;

        // Определяем направление прокрутки
        float scrollAmount = 0f;

        if (targetBottom < viewportBottom + edgePadding)
        {
            // Элемент ниже видимой области
            scrollAmount = targetBottom - viewportBottom - edgePadding;
        }
        else if (targetTop > viewportTop - edgePadding)
        {
            // Элемент выше видимой области
            scrollAmount = targetTop - viewportTop + edgePadding;
        }

        // Применяем смещение
        if (Mathf.Abs(scrollAmount) > 0.1f)
        {
            Vector2 newPosition = content.anchoredPosition;
            newPosition.y -= scrollAmount;
            content.anchoredPosition = newPosition;
        }
    }

    private IEnumerator SmoothScrollTo(RectTransform target)
    {
        Canvas.ForceUpdateCanvases();

        // Получаем границы
        Vector3[] targetCorners = new Vector3[4];
        Vector3[] viewportCorners = new Vector3[4];

        target.GetWorldCorners(targetCorners);
        viewport.GetWorldCorners(viewportCorners);

        // Преобразуем к локальным координатам content
        for (int i = 0; i < 4; i++)
        {
            targetCorners[i] = content.InverseTransformPoint(targetCorners[i]);
            viewportCorners[i] = content.InverseTransformPoint(viewportCorners[i]);
        }

        // Вычисляем границы
        float viewportTop = viewportCorners[1].y;
        float viewportBottom = viewportCorners[0].y;
        float targetTop = targetCorners[1].y;
        float targetBottom = targetCorners[0].y;

        // Вычисляем необходимое смещение
        float targetScrollPosition = 0f;

        if (targetBottom < viewportBottom + edgePadding)
        {
            // Элемент ниже - скроллим вниз
            targetScrollPosition = content.anchoredPosition.y + (viewportBottom - targetBottom) + edgePadding;
        }
        else if (targetTop > viewportTop - edgePadding)
        {
            // Элемент выше - скроллим вверх
            targetScrollPosition = content.anchoredPosition.y - (targetTop - viewportTop) - edgePadding;
        }
        else
        {
            // Элемент уже виден
            scrollCoroutine = null;
            yield break;
        }

        // Плавная анимация
        float startPosition = content.anchoredPosition.y;
        float elapsedTime = 0f;

        while (elapsedTime < 0.3f) // Фиксированное время анимации
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsedTime / 0.3f);
            float newY = Mathf.Lerp(startPosition, targetScrollPosition, t);

            content.anchoredPosition = new Vector2(content.anchoredPosition.x, newY);

            yield return null;
        }

        // Финальная позиция
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetScrollPosition);
        scrollCoroutine = null;
    }

    public void OnPanelExpanded(GameObject panel)
    {
        if (!scrollOnPanelExpand || panel == null) return;

        // Если расширенная панель сейчас выделена - прокручиваем к ней
        if (EventSystem.current.currentSelectedGameObject == panel)
        {
            ScrollTo(panel.GetComponent<RectTransform>());
        }
    }
}