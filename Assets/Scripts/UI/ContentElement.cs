using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ContentElement : MonoBehaviour
{
    [SerializeField] private TMP_Text mainText;
    [SerializeField] private TMP_Text fullText;
    [SerializeField] private Image icon;

    public float collapsedHeight = 60f;
    public float expandedHeight = 200f;
    public float animationTime = 0.25f;

    public Button toggleButton;

    private RectTransform rectTransform;
    private bool isExpanded = false;
    private Coroutine animationCoroutine;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(Toggle);
        }

        // Инициализация
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, collapsedHeight);
    }

    public void SetContent(Content content)
    {
        mainText.text = content.Main;
        fullText.text = content.Full;
        icon.sprite = content.Icon;
    }

    public void Toggle()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }

        isExpanded = !isExpanded;
        animationCoroutine = StartCoroutine(AnimateSize());
    }

    private IEnumerator AnimateSize()
    {
        float startHeight = rectTransform.sizeDelta.y;
        float targetHeight = isExpanded ? expandedHeight : collapsedHeight;
        float elapsed = 0f;

        // Анимация изменения высоты
        while (elapsed < animationTime)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationTime;
            float currentHeight = Mathf.Lerp(startHeight, targetHeight, t);

            rectTransform.sizeDelta = new Vector2(
                rectTransform.sizeDelta.x,
                currentHeight
            );

            yield return null;
        }

        // Финальное состояние
        rectTransform.sizeDelta = new Vector2(
            rectTransform.sizeDelta.x,
            targetHeight
        );

        animationCoroutine = null;
    }
}

