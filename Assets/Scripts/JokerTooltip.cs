using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class JokerTooltip : MonoBehaviour
{
    [Header("Tooltip References")]
    [Tooltip("Unity Editor'da oluşturduğun tooltip panel GameObject'i")]
    public GameObject tooltipPanel;
    [Tooltip("Name text GameObject'i (tooltipPanel'in child'ı)")]
    public TextMeshProUGUI nameText;
    [Tooltip("Description text GameObject'i (tooltipPanel'in child'ı)")]
    public TextMeshProUGUI descriptionText;
    
    [Header("Tooltip Settings")]
    public float padding = 10f;
    public float spacing = 5f;
    public float minWidth = 50f;
    public float minHeight = 70f;
    [Header("Position Settings")]
    [Tooltip("Tooltip'in mouse pozisyonuna göre X offset'i (0 = ortalanmış)")]
    public float positionOffsetX = 0f;
    [Tooltip("Tooltip'in mouse pozisyonuna göre Y offset'i (negatif = aşağıda, 0 = mouse'un tam dibinde)")]
    public float positionOffsetY = 0f;
    [Header("Font Settings")]
    public TMP_FontAsset tooltipFont;
    
    private RectTransform tooltipRectTransform;
    private Image tooltipBackground;
    private Canvas canvas;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }
        
        InitializeReferences();
    }
    
    private void InitializeReferences()
    {
        if (tooltipPanel != null)
        {
            tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
            tooltipBackground = tooltipPanel.GetComponent<Image>();
            
            if (tooltipBackground != null)
            {
                tooltipBackground.raycastTarget = false;
            }
        }
        
        if (tooltipFont != null)
        {
            if (nameText != null)
            {
                nameText.font = tooltipFont;
                nameText.raycastTarget = false;
            }
            if (descriptionText != null)
            {
                descriptionText.font = tooltipFont;
                descriptionText.raycastTarget = false;
            }
        }
    }

    public void ShowTooltip(CardData jokerData, Vector2 position)
    {
        if (jokerData == null || !jokerData.IsJoker)
        {
            return;
        }

        if (tooltipPanel == null)
        {
            return;
        }

        UpdateTooltipContent(jokerData);
        
        tooltipPanel.SetActive(true);
        
        if (tooltipRectTransform != null)
        {
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
        }
        
        UpdateTooltipPosition(position);
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
    }

    private void UpdateTooltipContent(CardData jokerData)
    {
        if (nameText != null)
        {
            nameText.text = jokerData.name;
        }

        if (descriptionText != null)
        {
            descriptionText.text = ApplySuitColors(jokerData.JokerDescription);
        }

        if (nameText != null)
        {
            nameText.ForceMeshUpdate();
        }
        if (descriptionText != null)
        {
            descriptionText.ForceMeshUpdate();
        }
    }

    private void UpdateTooltipPosition(Vector2 screenPosition)
    {
        if (tooltipRectTransform == null || canvas == null)
        {
            return;
        }

        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            return;
        }

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, 
            screenPosition, 
            canvas.worldCamera, 
            out localPoint);
        
        float offsetX = positionOffsetX;
        float offsetY = positionOffsetY;

        localPoint.x += offsetX;
        localPoint.y += offsetY;

        tooltipRectTransform.anchoredPosition = localPoint;
    }

    private string ApplySuitColors(string description)
    {
        if (string.IsNullOrEmpty(description))
        {
            return description;
        }

        string result = description;
        
        // Apply color tags to suit names
        result = result.Replace("Glitch", "<color=#00ffff>Glitch</color>");
        result = result.Replace("Core", "<color=#ff00cc>Core</color>");
        result = result.Replace("Neural", "<color=#fa9c1c>Neural</color>");
        result = result.Replace("Protocol", "<color=#8a00c4>Protocol</color>");
        
        return result;
    }

}

