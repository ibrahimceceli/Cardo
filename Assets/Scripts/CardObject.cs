using UnityEngine;

public class CardObject : MonoBehaviour
{
    public CardData cardInfo;

    [HideInInspector]
    public HandManager handManager;

    [HideInInspector]
    public bool isSelected = false;

    private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer
    {
        get
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            return spriteRenderer;
        }
    }
    
    private Collider cardCollider;
    public Collider CardCollider
    {
        get
        {
            if (cardCollider == null)
            {
                cardCollider = GetComponent<Collider>();
            }
            return cardCollider;
        }
    }
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private float selectionOffset = 0.5f;
    private float selectionScale = 1.1f;
    private Color originalColor;
    private Color selectedColor = new Color(1f, 0.8f, 0.2f, 1f);
    private static Camera mainCamera;

    public void Initialize(CardData data, HandManager manager)
    {
        cardInfo = data;
        handManager = manager;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        if (cardCollider == null)
        {
            cardCollider = GetComponent<Collider>();
        }

        if (spriteRenderer != null && data.cardFrontGraphic != null)
        {
            spriteRenderer.sprite = data.cardFrontGraphic;
            OptimizeSpriteForRotation();
            
            AdjustColliderToSprite();
        }
    }
    
    private void AdjustColliderToSprite()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null || cardCollider == null)
        {
            return;
        }
        
        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        
        BoxCollider boxCollider = cardCollider as BoxCollider;
        if (boxCollider != null)
        {
            boxCollider.size = new Vector3(spriteBounds.size.x, spriteBounds.size.y, boxCollider.size.z);
            boxCollider.center = spriteBounds.center;
        }
    }

    // Awake runs inside Instantiate, so these are cached before the card can be selected.
    // (Caching them in Start left the scale at zero if a card was clicked on its first frame.)
    void Awake()
    {
        originalPosition = transform.position;
        originalScale = transform.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (cardCollider == null)
        {
            cardCollider = GetComponent<Collider>();
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    // Position of the card in the hand layout, ignoring the selection lift.
    public Vector3 RestingPosition
    {
        get { return originalPosition; }
    }

    // Lift along the camera's up axis so the selection is visible from the top-down camera.
    private Vector3 GetSelectionLift()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        Vector3 screenUp = mainCamera != null ? mainCamera.transform.up : Vector3.up;
        return screenUp * selectionOffset;
    }

    void OnMouseDown()
    {
    }
    
    private bool IsTopmostCardAtPosition()
    {
        if (handManager == null || !handManager.currentHand.Contains(this))
        {
            return false;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return false;
            }
        }
        
        if (CardCollider == null || SpriteRenderer == null)
        {
            return false;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] allHits = Physics.RaycastAll(ray, Mathf.Infinity);
        
        System.Collections.Generic.List<System.Tuple<CardObject, RaycastHit>> hitCardsWithInfo = 
            new System.Collections.Generic.List<System.Tuple<CardObject, RaycastHit>>();
        
        foreach (RaycastHit hit in allHits)
        {
            CardObject card = hit.collider.GetComponent<CardObject>();
            if (card == null || !handManager.currentHand.Contains(card))
            {
                continue;
            }
            
            hitCardsWithInfo.Add(new System.Tuple<CardObject, RaycastHit>(card, hit));
        }
        
        hitCardsWithInfo.Sort((a, b) => a.Item2.distance.CompareTo(b.Item2.distance));
        
        bool thisCardHit = false;
        foreach (var hitInfo in hitCardsWithInfo)
        {
            if (hitInfo.Item1 == this)
            {
                thisCardHit = true;
                break;
            }
        }
        
        if (!thisCardHit)
        {
            return false;
        }
        
        if (hitCardsWithInfo.Count == 1)
        {
            return true;
        }
        
        int highestSortingOrder = int.MinValue;
        CardObject topmostCard = null;
        System.Collections.Generic.List<CardObject> cardsWithHighestOrder = 
            new System.Collections.Generic.List<CardObject>();
        
        foreach (var hitInfo in hitCardsWithInfo)
        {
            CardObject card = hitInfo.Item1;
            
            if (card == null || card.SpriteRenderer == null)
            {
                continue;
            }
            
            int sortingOrder = card.SpriteRenderer.sortingOrder;
            
            if (sortingOrder > highestSortingOrder)
            {
                highestSortingOrder = sortingOrder;
                cardsWithHighestOrder.Clear();
                cardsWithHighestOrder.Add(card);
                topmostCard = card;
            }
            else if (sortingOrder == highestSortingOrder)
            {
                cardsWithHighestOrder.Add(card);
            }
        }
        
        int thisSortingOrder = SpriteRenderer != null ? SpriteRenderer.sortingOrder : 0;
        
        if (thisSortingOrder < highestSortingOrder)
        {
            return false;
        }
        
        if (cardsWithHighestOrder.Count == 1)
        {
            return topmostCard == this;
        }
        
        CardObject closestCardWithHighestOrder = null;
        float closestDistance = float.MaxValue;
        
        foreach (var hitInfo in hitCardsWithInfo)
        {
            CardObject card = hitInfo.Item1;
            
            if (!cardsWithHighestOrder.Contains(card))
            {
                continue;
            }
            
            if (hitInfo.Item2.distance < closestDistance)
            {
                closestDistance = hitInfo.Item2.distance;
                closestCardWithHighestOrder = card;
            }
        }
        
        return closestCardWithHighestOrder == this;
    }
    
    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return Vector3.zero;
            }
        }
        
        Plane cardPlane = new Plane(Vector3.up, transform.position);
        Ray mouseRay = mainCamera.ScreenPointToRay(Input.mousePosition);
        
        float distance;
        if (cardPlane.Raycast(mouseRay, out distance))
        {
            return mouseRay.GetPoint(distance);
        }
        
        RaycastHit[] allHits = Physics.RaycastAll(mouseRay, Mathf.Infinity);
        if (allHits.Length > 0)
        {
            System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));
            return allHits[0].point;
        }
        
        return Vector3.zero;
    }
    
    private bool IsPointInsideCollider(Collider collider, Vector3 worldPoint)
    {
        if (collider == null)
        {
            return false;
        }
        
        BoxCollider boxCollider = collider as BoxCollider;
        if (boxCollider != null)
        {
            Vector3 localPoint = boxCollider.transform.InverseTransformPoint(worldPoint);
            
            Vector3 center = boxCollider.center;
            Vector3 size = boxCollider.size;
            
            float margin = 0.01f;
            return localPoint.x >= center.x - size.x * 0.5f - margin && 
                   localPoint.x <= center.x + size.x * 0.5f + margin &&
                   localPoint.y >= center.y - size.y * 0.5f - margin && 
                   localPoint.y <= center.y + size.y * 0.5f + margin &&
                   localPoint.z >= center.z - size.z * 0.5f - margin && 
                   localPoint.z <= center.z + size.z * 0.5f + margin;
        }
        
        return collider.bounds.Contains(worldPoint);
    }

    public void DetectClick(bool playSound = true)
    {
        if (playSound && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX();
        }

        if (!isSelected && handManager != null)
        {
            if (!handManager.CanSelectCard(this))
            {
                return;
            }
        }

        isSelected = !isSelected;

        if (isSelected)
        {
            transform.position = originalPosition + GetSelectionLift();
            transform.localScale = originalScale * selectionScale;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = selectedColor;
            }
        }
        else
        {
            transform.position = originalPosition;
            transform.localScale = originalScale;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }

        if (handManager != null)
        {
            handManager.UpdateSelectedHandType();
        }
    }

    public void UpdateOriginalPosition(Vector3 newPosition)
    {
        originalPosition = newPosition;

        if (isSelected)
        {
            transform.position = originalPosition + GetSelectionLift();
        }
        else
        {
            transform.position = originalPosition;
        }
        
        AdjustColliderToSprite();
    }

    private void OptimizeSpriteForRotation()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        Texture2D spriteTexture = spriteRenderer.sprite.texture;
        if (spriteTexture != null)
        {
            spriteTexture.filterMode = FilterMode.Bilinear;
            spriteTexture.anisoLevel = 16;
            spriteTexture.wrapMode = TextureWrapMode.Clamp;
        }
    }
}
