using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class CameraScript : MonoBehaviour
{
    const float UnitsPerPixel = 0.01f;
    float fieldWidth, fieldHeight;      //размеры карты
    float cameraWidth, cameraHeight;    //половинные размеры камеры

    Vector3 StartMovingPoint = Vector3.zero;
    Vector3 delta;
    Vector3 NewCamPosition;

    public GameObject BackGround;
    // Use this for initialization
    void Start()
    {
        //Настройка камеры под разрешение монитора
        Camera Camera = GetComponent<Camera>();
        Camera.orthographicSize = Screen.height * 0.5f * UnitsPerPixel;
        cameraHeight = Camera.orthographicSize;
        cameraWidth = Screen.width * 0.5f * UnitsPerPixel;
        Camera.aspect = cameraWidth / cameraHeight;

        Sprite Spr = BackGround.GetComponent<SpriteRenderer>().sprite;

        //Ширину и длину поля считаем в юнитах
        fieldWidth = Spr.rect.width * UnitsPerPixel * 0.5f;
        fieldHeight = Spr.rect.height * UnitsPerPixel * 0.5f;
    }

    void LateUpdate()
    {

        if (StartMovingPoint != Vector3.zero)
        {
            delta = (Input.mousePosition - StartMovingPoint) * UnitsPerPixel;
            delta.z = 0;
            NewCamPosition = transform.position - delta;

            //Проверка границ карты по Y
            if (NewCamPosition.y + cameraHeight > BackGround.transform.position.y + fieldHeight)
                NewCamPosition.y = BackGround.transform.position.y + fieldHeight - cameraHeight;
            if (NewCamPosition.y - cameraHeight < BackGround.transform.position.y - fieldHeight)
                NewCamPosition.y = BackGround.transform.position.y - fieldHeight + cameraHeight;

            //Проверка границ карты по Х
            if (NewCamPosition.x + cameraWidth > BackGround.transform.position.x + fieldWidth)
                NewCamPosition.x = BackGround.transform.position.x + fieldWidth - cameraWidth;
            if (NewCamPosition.x - cameraWidth < BackGround.transform.position.x - fieldWidth)
                NewCamPosition.x = BackGround.transform.position.x - fieldWidth + cameraWidth;

            transform.position = NewCamPosition;
            StartMovingPoint = Input.mousePosition;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartMovingPoint = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            StartMovingPoint = Vector3.zero;
        }
    }

}
