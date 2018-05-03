using UnityEngine;
using System.Collections;

public class CollidersPanel : MonoBehaviour {
    [SerializeField]
    int width, height;
    [SerializeField]
    RectTransform TechCanvas;

    const float initScale = 0.2351563f;

    void OnEnable ()
    {
        //float k = CameraScript.UnitsPerPixel;
        float k = CameraScript.UnitsPerPixel * Camera.main.pixelWidth / width / initScale;
        transform.localScale = new Vector3(k, k, 1);
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, -1);
	}
	
}
