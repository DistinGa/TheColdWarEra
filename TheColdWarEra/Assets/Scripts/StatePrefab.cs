using UnityEngine;
using UnityEngine.UI;

public class StatePrefab : MonoBehaviour {
    public Sprite[] spriteAmer;
    public Sprite[] spriteSov;


    public void Init(int sprIndx, Authority authority)
    {
        GetComponent<Image>().sprite = TakePicture(sprIndx, authority);
    }

    public Sprite TakePicture(int sprIndx, Authority authority)
    {
        if (authority == Authority.Amer)
            return spriteAmer[sprIndx];
        if (authority == Authority.Soviet)
            return spriteSov[sprIndx];

        return null;
    }
}
