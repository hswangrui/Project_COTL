using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardHUD : BaseMonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
	public GameObject cardUnit;

	public bool dragOnSurfaces = true;

	private GameObject m_DraggingIcon;

	private RectTransform m_DraggingPlane;

	private Canvas canvas;

	private Vector3 originalPosition;

	private Vector3 offSetPosition;

	private RectTransform rt;

	private bool dragging;

	public Sprite cardImage;

	public Sprite placeImage;

	private Image image;

	public bool isOver;

	private Vector3 OverY = Vector3.zero;

	private void Start()
	{
		originalPosition = base.transform.position;
		rt = GetComponent<RectTransform>();
		image = GetComponent<Image>();
	}

	private void Update()
	{
		offSetPosition = Vector3.Lerp(Vector3.zero, offSetPosition, 40f * Time.deltaTime);
		if (!dragging)
		{
			Vector3 position = rt.transform.position;
			position += (originalPosition + OverY - position) / 5f;
			rt.transform.position = position;
		}
		else if (rt.transform.position.y > 100f)
		{
			if (image.sprite != placeImage)
			{
				image.sprite = placeImage;
				image.SetNativeSize();
			}
		}
		else if (image.sprite != cardImage)
		{
			image.sprite = cardImage;
			image.SetNativeSize();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Debug.Log("Mouse enter");
		isOver = true;
		OverY = new Vector3(0f, 30f, 0f);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Debug.Log("Mouse exit");
		isOver = false;
		OverY = Vector3.zero;
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		dragging = true;
		canvas = FindInParents<Canvas>(base.gameObject);
		if (!(canvas == null))
		{
			m_DraggingIcon = base.gameObject;
			if (dragOnSurfaces)
			{
				m_DraggingPlane = base.transform as RectTransform;
			}
			else
			{
				m_DraggingPlane = canvas.transform as RectTransform;
			}
			offSetPosition = eventData.position - new Vector2(rt.position.x, rt.position.y);
			SetDraggedPosition(eventData);
		}
	}

	public void OnDrag(PointerEventData data)
	{
		if (m_DraggingIcon != null)
		{
			SetDraggedPosition(data);
		}
	}

	private void SetDraggedPosition(PointerEventData data)
	{
		if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
		{
			m_DraggingPlane = data.pointerEnter.transform as RectTransform;
		}
		Vector3 worldPoint;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out worldPoint))
		{
			Vector3 position = rt.transform.position;
			Vector3 position2 = worldPoint - offSetPosition;
			if (data.position.y < 100f)
			{
				position.x += (originalPosition.x - position.x) / 5f;
				position2.x = position.x;
			}
			rt.position = position2;
			rt.rotation = m_DraggingPlane.rotation;
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		dragging = false;
		if (!(rt.position.y < 100f))
		{
			Vector3 mousePosition = Input.mousePosition;
			mousePosition.z = 0f - Camera.main.transform.position.z;
			Object.Instantiate(cardUnit, Camera.main.ScreenToWorldPoint(mousePosition), Quaternion.identity, canvas.transform.parent);
			if (m_DraggingIcon != null)
			{
				Object.Destroy(m_DraggingIcon);
			}
		}
	}

	public static T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null)
		{
			return null;
		}
		T component = go.GetComponent<T>();
		if ((Object)component != (Object)null)
		{
			return component;
		}
		Transform parent = go.transform.parent;
		while (parent != null && (Object)component == (Object)null)
		{
			component = parent.gameObject.GetComponent<T>();
			parent = parent.parent;
		}
		return component;
	}
}
