using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class BackgroundPanelManager : MonoBehaviour
{
    [Header("Background Configuration")]
    [Tooltip("Tüm background panelleri için kullanılacak renk")]
    private Color backgroundColor = new Color(0f, 0f, 0f, 0.2f);
    
    private const float BACKGROUND_ALPHA = 0.2f;
    
    public Color BackgroundColor
    {
        get { return backgroundColor; }
        set { backgroundColor = new Color(0f, 0f, 0f, BACKGROUND_ALPHA); }
    }
    
    [Header("Background Padding")]
    [Tooltip("Arka plan dikdörtgenlerinin text'e göre yatay padding'i")]
    public float backgroundPaddingX = 8f;
    [Tooltip("Arka plan dikdörtgenlerinin text'e göre dikey padding'i")]
    public float backgroundPaddingY = 4f;
    
    [Header("Background Position Offset")]
    [Tooltip("Arka plan dikdörtgenlerinin pozisyon offset'i (X)")]
    public Vector2 backgroundPositionOffset = Vector2.zero;
    [Tooltip("Arka plan dikdörtgenlerinin boyut çarpanı")]
    public Vector2 backgroundSizeMultiplier = Vector2.one;
    
    private Canvas cachedCanvas;
    private static Sprite cachedRoundedRectSprite = null;
    
    private Canvas GetCanvas(TMP_Text fallbackText = null)
    {
        if (cachedCanvas == null)
        {
            cachedCanvas = GetComponentInParent<Canvas>();
            if (cachedCanvas == null && fallbackText != null)
            {
                cachedCanvas = fallbackText.GetComponentInParent<Canvas>();
            }
        }
        return cachedCanvas;
    }
    
    /// <summary>
    /// Creates all background panels for the given text components in editor mode
    /// </summary>
    public void CreateBackgroundPanelsIfNeeded(TextBackgroundData[] backgroundData, System.Collections.Generic.Dictionary<string, System.Action<Image>> backgroundRefs)
    {
        if (Application.isPlaying)
        {
            return;
        }
        
        // Try to find canvas from the first available text component
        TMP_Text fallbackText = null;
        if (backgroundData != null && backgroundData.Length > 0)
        {
            foreach (var data in backgroundData)
            {
                if (data.text != null)
                {
                    fallbackText = data.text;
                    break;
                }
            }
        }
        
        Canvas rootCanvas = GetCanvas(fallbackText);
        if (rootCanvas == null)
        {
            return;
        }
        
        Transform[] allChildren = rootCanvas.GetComponentsInChildren<Transform>(true);
        var existingBackgrounds = new System.Collections.Generic.HashSet<string>();
        
        foreach (Transform child in allChildren)
        {
            if (child == null || child.gameObject == null)
                continue;
            
            string childName = child.name;
            
            if (backgroundRefs.TryGetValue(childName, out var setRef))
            {
                existingBackgrounds.Add(childName);
                setRef(child.GetComponent<Image>());
            }
        }
        
        foreach (var data in backgroundData)
        {
            Image createdBg = CreateBackgroundIfNeeded(data.bgName, existingBackgrounds, data.text);
            if (createdBg != null && backgroundRefs.TryGetValue(data.bgName, out var setRef))
            {
                setRef(createdBg);
            }
        }
    }
    
    /// <summary>
    /// Creates background panels at runtime with a delay
    /// </summary>
    public IEnumerator CreateBackgroundPanelsDelayed(
        System.Collections.Generic.Dictionary<string, System.Action<Transform>> backgroundMap,
        System.Action configureTextSettingsAction,
        System.Action ensureTextsVisibleAction)
    {
        yield return null;
        
        if (Application.isPlaying)
        {
            Canvas rootCanvas = GetCanvas(null);
            
            if (rootCanvas != null)
            {
                Transform[] allChildren = rootCanvas.GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child == null || child.gameObject == null)
                        continue;
                    
                    string childName = child.name;
                    
                    if (backgroundMap.TryGetValue(childName, out var setupAction))
                    {
                        setupAction(child);
                    }
                }
                
                configureTextSettingsAction?.Invoke();
                ensureTextsVisibleAction?.Invoke();
            }
        }
    }
    
    /// <summary>
    /// Sets up an existing background transform
    /// </summary>
    public void SetupBackground(Transform child, ref Image bgImage, ref TMP_Text textRef, System.Action<TMP_Text> configureTextAction)
    {
        bgImage = child.GetComponent<Image>();
        DisableLayoutGroups(child);
        child.gameObject.SetActive(true);
        
        if (bgImage != null)
        {
            bgImage.enabled = true;
        }
        
        if (textRef != null && textRef.transform.parent != child)
        {
            RectTransform textRect = textRef.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.SetParent(child, false);
                SetTextRectCenterSettings(textRect);
            }
        }
        
        EnsureTextVisibleInBackground(child, ref textRef, configureTextAction);
    }
    
    private Image CreateBackgroundIfNeeded(string bgName, System.Collections.Generic.HashSet<string> existing, TMP_Text textRef)
    {
        if (!existing.Contains(bgName) && textRef != null && textRef.gameObject != null)
        {
            BackgroundColor = new Color(0f, 0f, 0f, BACKGROUND_ALPHA);
            Color fixedBackgroundColor = new Color(0f, 0f, 0f, BACKGROUND_ALPHA);
            
            if (textRef.gameObject != null)
            {
                textRef.gameObject.SetActive(true);
            }
            return CreateBackgroundPanel(textRef.gameObject, fixedBackgroundColor, bgName);
        }
        return null;
    }
    
    private Image CreateBackgroundPanel(GameObject textObject, Color bgColor, string backgroundName)
    {
        if (textObject == null || Application.isPlaying)
        {
            return null;
        }

        RemoveExistingBackground(textObject, backgroundName);
        
        GameObject backgroundObj = CreateBackgroundGameObject(backgroundName, textObject);
        if (backgroundObj == null)
        {
            return null;
        }
        
        Image backgroundImage = SetupBackgroundImage(backgroundObj, bgColor);
        RectTransform bgRect = backgroundObj.GetComponent<RectTransform>();
        
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        
        Vector2 textSize = CalculateTextSize(textRect, tmpText);
        SetupBackgroundTransform(bgRect, textRect, textSize);
        SetupTextTransform(textRect, tmpText, textSize);
        
        backgroundObj.SetActive(true);
        
        return backgroundImage;
    }
    
    private void RemoveExistingBackground(GameObject textObject, string backgroundName)
    {
        Transform existingBg = textObject.transform.parent != null ? 
            textObject.transform.parent.Find(backgroundName) : null;
        if (existingBg != null)
        {
            DestroyImmediate(existingBg.gameObject);
        }
    }
    
    private GameObject CreateBackgroundGameObject(string backgroundName, GameObject textObject)
    {
        GameObject backgroundObj = new GameObject(backgroundName);
        RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
        
        Transform textParent = GetTextParent(textObject);
        if (textParent == null)
        {
            return null;
        }
        
        bgRect.SetParent(textParent, false);
        
        if (textObject.transform.parent == textParent)
        {
            bgRect.SetSiblingIndex(textObject.transform.GetSiblingIndex());
        }
        
        return backgroundObj;
    }
    
    private Transform GetTextParent(GameObject textObject)
    {
        Transform textParent = textObject.transform.parent;
        if (textParent == null)
        {
            Canvas rootCanvas = textObject.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                textParent = rootCanvas.transform;
            }
        }
        return textParent;
    }
    
    private Image SetupBackgroundImage(GameObject backgroundObj, Color bgColor)
    {
        Color fixedBackgroundColor = new Color(0f, 0f, 0f, BACKGROUND_ALPHA);
        
        Image backgroundImage = backgroundObj.AddComponent<Image>();
        Sprite roundedRectSprite = CreateRoundedRectangleSprite();
        
        backgroundImage.sprite = roundedRectSprite;
        backgroundImage.color = fixedBackgroundColor;
        backgroundImage.type = Image.Type.Sliced;
        backgroundImage.enabled = true;
        
        return backgroundImage;
    }
    
    private Vector2 CalculateTextSize(RectTransform textRect, TMP_Text tmpText)
    {
        float textWidth = tmpText != null ? tmpText.preferredWidth : textRect.sizeDelta.x;
        float textHeight = tmpText != null ? tmpText.preferredHeight : textRect.sizeDelta.y;
        
        if (!Application.isPlaying && tmpText != null)
        {
            tmpText.ForceMeshUpdate();
            textWidth = tmpText.preferredWidth;
            textHeight = tmpText.preferredHeight;
        }
        
        if (textWidth < 1f || textHeight < 1f)
        {
            Rect textRectRect = textRect.rect;
            textWidth = textRectRect.width > 0 ? textRectRect.width : 100f;
            textHeight = textRectRect.height > 0 ? textRectRect.height : 30f;
        }
        
        return new Vector2(textWidth, textHeight);
    }
    
    private void SetupBackgroundTransform(RectTransform bgRect, RectTransform textRect, Vector2 textSize)
    {
        Vector3 textWorldPosition = textRect.position;
        Transform textParentForBg = GetTextParentForBackground(textRect);
        
        bgRect.SetParent(textParentForBg, false);
        bgRect.anchorMin = new Vector2(0.5f, 0.5f);
        bgRect.anchorMax = new Vector2(0.5f, 0.5f);
        bgRect.pivot = new Vector2(0.5f, 0.5f);
        bgRect.position = textWorldPosition;
        bgRect.anchoredPosition += backgroundPositionOffset;
        
        float bgWidth = (textSize.x + backgroundPaddingX * 2) * backgroundSizeMultiplier.x;
        float bgHeight = (textSize.y + backgroundPaddingY * 2) * backgroundSizeMultiplier.y;
        bgRect.sizeDelta = new Vector2(bgWidth, bgHeight);
    }
    
    private Transform GetTextParentForBackground(RectTransform textRect)
    {
        Transform textParentForBg = textRect.parent;
        if (textParentForBg == null)
        {
            Canvas rootCanvas = textRect.GetComponentInParent<Canvas>();
            if (rootCanvas != null)
            {
                textParentForBg = rootCanvas.transform;
            }
        }
        return textParentForBg;
    }
    
    private void SetupTextTransform(RectTransform textRect, TMP_Text tmpText, Vector2 textSize)
    {
        GameObject backgroundObj = textRect.parent != null ? textRect.parent.gameObject : null;
        if (backgroundObj == null)
        {
            return;
        }
        
        textRect.SetParent(backgroundObj.transform, false);
        SetTextRectCenterSettings(textRect);
        
        float finalWidth = textSize.x > 10f ? textSize.x : 200f;
        float finalHeight = textSize.y > 10f ? textSize.y : 50f;
        textRect.sizeDelta = new Vector2(finalWidth, finalHeight);
        
        if (tmpText != null)
        {
            tmpText.textWrappingMode = TextWrappingModes.NoWrap;
            tmpText.overflowMode = TextOverflowModes.Overflow;
            tmpText.gameObject.SetActive(true);
            tmpText.ForceMeshUpdate();
        }
    }
    
    private void SetTextRectCenterSettings(RectTransform textRect)
    {
        if (textRect == null) return;
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
    }

    private void EnsureTextVisibleInBackground(Transform backgroundTransform, ref TMP_Text textRef, System.Action<TMP_Text> configureTextAction)
    {
        if (backgroundTransform == null) return;
        
        for (int i = 0; i < backgroundTransform.childCount; i++)
        {
            Transform child = backgroundTransform.GetChild(i);
            if (child == null) continue;
            
            TMP_Text textComponent = child.GetComponent<TMP_Text>();
            if (textComponent != null)
            {
                textRef = textComponent;
                configureTextAction?.Invoke(textComponent);
                
                RectTransform textRect = textComponent.GetComponent<RectTransform>();
                if (textRect != null)
                {
                    if (textRect.sizeDelta.x < 10f || textRect.sizeDelta.y < 10f)
                    {
                        textComponent.ForceMeshUpdate();
                        float preferredWidth = textComponent.preferredWidth;
                        float preferredHeight = textComponent.preferredHeight;
                        
                        float finalWidth = preferredWidth > 10f ? preferredWidth : 200f;
                        float finalHeight = preferredHeight > 10f ? preferredHeight : 50f;
                        
                        textRect.sizeDelta = new Vector2(finalWidth, finalHeight);
                    }
                    
                    SetTextRectCenterSettings(textRect);
                    
                    if (textRect.localScale.x == 0f || textRect.localScale.y == 0f || textRect.localScale.z == 0f)
                    {
                        textRect.localScale = Vector3.one;
                    }
                }
                
                textComponent.ForceMeshUpdate();
                
                break;
            }
        }
    }
    
    private void DisableLayoutGroups(Transform backgroundTransform)
    {
        if (backgroundTransform == null) return;
        
        DisableComponents<UnityEngine.UI.LayoutGroup>(backgroundTransform);
        DisableComponents<UnityEngine.UI.ContentSizeFitter>(backgroundTransform);
    }
    
    private void DisableComponents<T>(Transform target) where T : Behaviour
    {
        T[] components = target.GetComponents<T>();
        foreach (var component in components)
        {
            if (component != null)
            {
                component.enabled = false;
            }
        }
    }
    
    private Sprite CreateRoundedRectangleSprite()
    {
        if (cachedRoundedRectSprite != null)
        {
            return cachedRoundedRectSprite;
        }
        
        const int width = 64;
        const int height = 64;
        const float cornerRadius = 8f;
        
        Texture2D roundedRectTexture = CreateRoundedRectangleTexture(width, height, cornerRadius);
        cachedRoundedRectSprite = CreateSpriteFromTexture(roundedRectTexture, width, height, cornerRadius);
        
        return cachedRoundedRectSprite;
    }
    
    private Texture2D CreateRoundedRectangleTexture(int width, int height, float cornerRadius)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = CalculateCornerPixels(width, height, cornerRadius);
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return texture;
    }
    
    private Color[] CalculateCornerPixels(int width, int height, float cornerRadius)
    {
        Color[] pixels = new Color[width * height];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool shouldBeTransparent = IsPixelInCornerCutoff(x, y, width, height, cornerRadius);
                pixels[y * width + x] = shouldBeTransparent ? Color.clear : Color.white;
            }
        }
        
        return pixels;
    }
    
    private bool IsPixelInCornerCutoff(int x, int y, int width, int height, float cornerRadius)
    {
        float leftDist = x;
        float rightDist = width - 1 - x;
        float bottomDist = y;
        float topDist = height - 1 - y;
        
        float cornerDist = float.MaxValue;
        bool nearCorner = false;
        
        if (leftDist < cornerRadius && bottomDist < cornerRadius)
        {
            cornerDist = CalculateCornerDistance(leftDist, bottomDist, cornerRadius);
            nearCorner = true;
        }
        else if (leftDist < cornerRadius && topDist < cornerRadius)
        {
            cornerDist = CalculateCornerDistance(leftDist, topDist, cornerRadius);
            nearCorner = true;
        }
        else if (rightDist < cornerRadius && bottomDist < cornerRadius)
        {
            cornerDist = CalculateCornerDistance(rightDist, bottomDist, cornerRadius);
            nearCorner = true;
        }
        else if (rightDist < cornerRadius && topDist < cornerRadius)
        {
            cornerDist = CalculateCornerDistance(rightDist, topDist, cornerRadius);
            nearCorner = true;
        }
        
        return nearCorner && cornerDist > cornerRadius;
    }
    
    private float CalculateCornerDistance(float distX, float distY, float cornerRadius)
    {
        float dx = distX - cornerRadius;
        float dy = distY - cornerRadius;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
    
    private Sprite CreateSpriteFromTexture(Texture2D texture, int width, int height, float cornerRadius)
    {
        return Sprite.Create(texture, 
            new Rect(0, 0, width, height), 
            new Vector2(0.5f, 0.5f), 
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(cornerRadius, cornerRadius, cornerRadius, cornerRadius));
    }
}

// Helper struct for background data
public struct TextBackgroundData
{
    public TMP_Text text;
    public string bgName;
    
    public TextBackgroundData(TMP_Text text, string bgName)
    {
        this.text = text;
        this.bgName = bgName;
    }
}
