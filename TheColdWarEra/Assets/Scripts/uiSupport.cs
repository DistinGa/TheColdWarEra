using UnityEngine;
using UnityEngine.UI;

public class uiSupport : MonoBehaviour {
    [SerializeField]
    Image scale;
    [SerializeField]
    GameObject scale_0, scale_100;

    float support;

    public float Support
    {
        set
        {
            support = value;
            if (support > 0 && support < 1f)
                support = 1f;

            scale.fillAmount = support / 100f;

            scale_0.SetActive(support == 0);
            scale_100.SetActive(support == 100);
        }
    }
}
