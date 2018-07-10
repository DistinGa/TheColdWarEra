using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class TooltipScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    const float _Delay = 0.5f;

    public string TooltipText;
    public GameObject _TooltipGO;
    public Text _TooltipTextComponent;

    float _Timer;
    bool _Attention;

#if UNITY_EDITOR
    void OnEnable()
    {
        if (_TooltipGO != null)
            return;

        _TooltipGO = GameObject.FindGameObjectWithTag("Tooltip");
        _TooltipTextComponent = _TooltipGO.transform.FindChild("Text").GetComponent<Text>();
    }
#endif

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        Invoke("ShowTooltip", _Delay);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
        CancelInvoke("ShowTooltip");
    }

    public void OnMouseEnter()
    {
        Debug.Log("OnMouseEnter");
        _Timer = _Delay;
        _Attention = true;
    }

    void OnMouseExit()
    {
        HideTooltip();
        _Attention = false;
    }

    void OnMouseOver()
    {
        if (_Attention)
        {
            Debug.Log("Tick");
            _Timer -= Time.deltaTime;

            if (_Timer <= 0)
                ShowTooltip();
        }
    }

    void ShowTooltip()
    {
        float w, h;
        RectTransform rt = _TooltipGO.GetComponent<RectTransform>();
        //CanvasScaler cs = GameObject.Find("Canvas").transform.GetComponentInChildren<CanvasScaler>();
        CanvasScaler cs = transform.GetComponentInParent<CanvasScaler>();
        if (cs == null)
            return;

        rt.SetParent(cs.transform);
        if (cs.name == "Canvas")
            rt.localScale = Vector3.one;

        w = 0.5f * rt.rect.width * Screen.width / cs.referenceResolution.x;
        h = 0.5f * rt.rect.height * Screen.height / cs.referenceResolution.y;
        //Vector3 newPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
        Vector3 newPos = new Vector3(Input.mousePosition.x + w, Input.mousePosition.y - h, 0);
        if (Input.mousePosition.x < w * 2)
            newPos[0] = Input.mousePosition.x + w;
        if (Input.mousePosition.x > Screen.width - w * 2)
            newPos[0] = Input.mousePosition.x - w;
        if (Input.mousePosition.y < h * 2)
            newPos[1] = Input.mousePosition.y + h;
        if (Input.mousePosition.y > Screen.height - h * 2)
            newPos[1] = Input.mousePosition.y - h;

        rt.position = newPos;
        _TooltipTextComponent.text = TooltipText;

        _TooltipGO.SetActive(true);
    }

    void HideTooltip()
    {
        _TooltipGO.SetActive(false);
    }
}
