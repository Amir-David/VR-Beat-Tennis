using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ImageClickHandler : MonoBehaviour, IPointerClickHandler
{
    public RectTransform firstImageTransform;
    public RectTransform secondImageTransform;
    public GameObject circlePrefab;
    private float circleRadius = 25f;
    private Dictionary<GameObject, GameObject> circlePairs = new Dictionary<GameObject, GameObject>();

    public IReadOnlyDictionary<GameObject, GameObject> CirclePairs => circlePairs;

    public event System.Action<float, float, float, float> OnCircleAdded;
    public event System.Action<GameObject> OnCircleDragged;

    public event System.Action<float, float, float, float> OnCircleRemoved;


    public void OnPointerClick(PointerEventData eventData)
    {
        if (RectTransformUtility.RectangleContainsScreenPoint(firstImageTransform, eventData.position, eventData.pressEventCamera))
        {
            OnFirstImageClicked(eventData);
        }
    }

    private void OnFirstImageClicked(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(firstImageTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            GameObject clickedCircle = GetClickedCircle(localPoint);
            if (clickedCircle != null)
            {
                RemoveCircle(clickedCircle);
            }
            else
            {
                var (circle1, circle2) = CreateCircle(localPoint);
                Vector2 normalizedPoint1 = GetNormalizedPosition(circle1.GetComponent<RectTransform>().anchoredPosition, firstImageTransform);
                Vector2 normalizedPoint2 = GetNormalizedPosition(circle2.GetComponent<RectTransform>().anchoredPosition, secondImageTransform);
                OnCircleAdded?.Invoke(normalizedPoint1.x, normalizedPoint1.y, normalizedPoint2.x, normalizedPoint2.y);
            }
        }
    }

    public (GameObject, GameObject) CreateCircleFromCoord(float normalizedX, float normalizedY)
    {
        Vector2 localPoint = NormalizedToLocalPoint(new Vector2(normalizedX, normalizedY), firstImageTransform);
        
        return CreateCircle(localPoint);
    }

    public (GameObject, GameObject) CreateCircleFromCoord1(float normalizedX1, float normalizedY1, float normalizedX2, float normalizedY2)
    {
        Vector2 localPoint1 = NormalizedToLocalPoint(new Vector2(normalizedX1, normalizedY1), firstImageTransform);
        Vector2 localPoint2 = NormalizedToLocalPoint(new Vector2(normalizedX2, normalizedY2), secondImageTransform);
        
        return CreateCircle1(localPoint1, localPoint2);
    }

    private (GameObject, GameObject) CreateCircle(Vector2 localPoint)
    {
        localPoint = ConstrainToImageBounds(localPoint, firstImageTransform);
        Vector2 randomPoint = GetRandomPosition(secondImageTransform);

        GameObject circle1 = SpawnCircle(localPoint, firstImageTransform, false);
        GameObject circle2 = SpawnCircle(randomPoint, secondImageTransform, true);

        circlePairs[circle1] = circle2;
        circlePairs[circle2] = circle1;

        PrintBothCirclePositions(circle1, circle2);

        return (circle1, circle2);
    }

    private (GameObject, GameObject) CreateCircle1(Vector2 localPoint1, Vector2 localPoint2)
    {
        localPoint1 = ConstrainToImageBounds(localPoint1, firstImageTransform);
        localPoint2 = ConstrainToImageBounds(localPoint2, secondImageTransform);

        GameObject circle1 = SpawnCircle(localPoint1, firstImageTransform, false);
        GameObject circle2 = SpawnCircle(localPoint2, secondImageTransform, true);

        circlePairs[circle1] = circle2;
        circlePairs[circle2] = circle1;

        PrintBothCirclePositions(circle1, circle2);

        return (circle1, circle2);
    }


    GameObject SpawnCircle(Vector2 position, RectTransform targetTransform, bool isDraggable)
    {
        if (circlePrefab == null)
        {
            return null;
        }

        GameObject circleObject = Instantiate(circlePrefab, targetTransform);
        RectTransform circleTransform = circleObject.GetComponent<RectTransform>();
        
        if (circleTransform != null)
        {
            circleTransform.anchoredPosition = position;
            
            if (isDraggable)
            {
                DraggableCircle draggableComponent = circleObject.AddComponent<DraggableCircle>();
                draggableComponent.Initialize(targetTransform, circleRadius, this);
            }
        }

        return circleObject;
    }

    Vector2 GetRandomPosition(RectTransform targetTransform)
    {
        return new Vector2(
            Random.Range(-targetTransform.rect.width / 2 + circleRadius, targetTransform.rect.width / 2 - circleRadius),
            Random.Range(-targetTransform.rect.height / 2 + circleRadius, targetTransform.rect.height / 2 - circleRadius)
        );
    }

    Vector2 ConstrainToImageBounds(Vector2 position, RectTransform imageTransform)
    {
        return new Vector2(
            Mathf.Clamp(position.x, -imageTransform.rect.width / 2 + circleRadius, imageTransform.rect.width / 2 - circleRadius),
            Mathf.Clamp(position.y, -imageTransform.rect.height / 2 + circleRadius, imageTransform.rect.height / 2 - circleRadius)
        );
    }

    GameObject GetClickedCircle(Vector2 clickPosition)
    {
        foreach (var pair in circlePairs)
        {
            if (Vector2.Distance(pair.Key.GetComponent<RectTransform>().anchoredPosition, clickPosition) < circleRadius)
            {
                return pair.Key;
            }
        }
        return null;
    }

    void RemoveCircle(GameObject clickedCircle)
    {
        if (circlePairs.TryGetValue(clickedCircle, out GameObject pairedCircle))
        {
            Vector2 pos1 = GetNormalizedPositionForCircle(clickedCircle);
            Vector2 pos2 = GetNormalizedPositionForCircle(pairedCircle);

            if (clickedCircle.transform.parent == secondImageTransform)
            {
                (pos1, pos2) = (pos2, pos1);
            }

            OnCircleRemoved?.Invoke(pos1.x, pos1.y, pos2.x, pos2.y);

            Destroy(clickedCircle);
            Destroy(pairedCircle);
            circlePairs.Remove(clickedCircle);
            circlePairs.Remove(pairedCircle);
        }
    }

    public void PrintBothCirclePositions(GameObject circle1, GameObject circle2)
    {
        Vector2 pos1 = GetNormalizedPosition(circle1.GetComponent<RectTransform>().anchoredPosition, firstImageTransform);
        Vector2 pos2 = GetNormalizedPosition(circle2.GetComponent<RectTransform>().anchoredPosition, secondImageTransform);
    }

    private Vector2 GetNormalizedPosition(Vector2 position, RectTransform imageTransform)
    {
        Vector2 normalizedPoint = new Vector2(
            (position.x + imageTransform.rect.width / 2 - circleRadius) / (imageTransform.rect.width - 2 * circleRadius),
            (position.y + imageTransform.rect.height / 2 - circleRadius) / (imageTransform.rect.height - 2 * circleRadius)
        );

        return new Vector2(normalizedPoint.x * 3f, normalizedPoint.y * 2f);
    }

    private Vector2 NormalizedToLocalPoint(Vector2 normalizedPoint, RectTransform imageTransform)
    {
        return new Vector2(
            (normalizedPoint.x / 3f) * (imageTransform.rect.width - 2 * circleRadius) - imageTransform.rect.width / 2 + circleRadius,
            (normalizedPoint.y / 2f) * (imageTransform.rect.height - 2 * circleRadius) - imageTransform.rect.height / 2 + circleRadius
        );
    }

    public Vector2 GetNormalizedPositionForCircle(GameObject circle)
    {
        RectTransform circleTransform = circle.GetComponent<RectTransform>();
        RectTransform imageTransform = circle.transform.parent.GetComponent<RectTransform>();
        return GetNormalizedPosition(circleTransform.anchoredPosition, imageTransform);
    }

    public void HandleCircleDragged(GameObject draggedCircle)
    {
        if (circlePairs.TryGetValue(draggedCircle, out GameObject pairedCircle))
        {
            if (draggedCircle.transform.parent == secondImageTransform)
            {
                PrintBothCirclePositions(pairedCircle, draggedCircle);
            }
            else
            {
                PrintBothCirclePositions(draggedCircle, pairedCircle);
            }
            OnCircleDragged?.Invoke(draggedCircle);
        }
    }

    public void InitializeCircles()
    {
        foreach (var pair in circlePairs)
        {
            InitializeCircle(pair.Key);
            InitializeCircle(pair.Value);
        }
    }

    private void InitializeCircle(GameObject circle)
    {
        DraggableCircle draggableComponent = circle.GetComponent<DraggableCircle>();
        if (draggableComponent == null)
        {
            draggableComponent = circle.AddComponent<DraggableCircle>();
        }
        draggableComponent.Initialize(circle.transform.parent.GetComponent<RectTransform>(), circleRadius, this);
    }
}

public class DraggableCircle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform parentRectTransform;
    private RectTransform rectTransform;
    private float circleRadius;
    private ImageClickHandler imageClickHandler;

    public void Initialize(RectTransform parent, float radius, ImageClickHandler handler)
    {
        parentRectTransform = parent;
        rectTransform = GetComponent<RectTransform>();
        circleRadius = radius;
        imageClickHandler = handler;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (transform.parent != imageClickHandler.secondImageTransform)
        {
            eventData.pointerDrag = null;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            localPoint = new Vector2(
                Mathf.Clamp(localPoint.x, -parentRectTransform.rect.width / 2 + circleRadius, parentRectTransform.rect.width / 2 - circleRadius),
                Mathf.Clamp(localPoint.y, -parentRectTransform.rect.height / 2 + circleRadius, parentRectTransform.rect.height / 2 - circleRadius)
            );
            rectTransform.anchoredPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (transform.parent == imageClickHandler.secondImageTransform)
        {
            imageClickHandler.HandleCircleDragged(gameObject);
        }
    }
}