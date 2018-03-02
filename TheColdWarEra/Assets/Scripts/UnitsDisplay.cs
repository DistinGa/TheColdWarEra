using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class UnitsDisplay : MonoBehaviour {
    public List<GameObject> UnitsList;
    public List<GameObject> NeutralUnitsList;

    void Start()
    {
        Transform Neutral = transform.Find("Neutral");
        if (Neutral != null)
            foreach (var item in Neutral.GetComponentsInChildren<CanvasRenderer>().OrderBy(x => x.name))
                NeutralUnitsList.Add(item.gameObject);

        foreach (var item in transform.Find("Units").GetComponentsInChildren<CanvasRenderer>().OrderBy(x => x.name))
            UnitsList.Add(item.gameObject);

        foreach (var item in UnitsList)
            item.SetActive(false);

        foreach (var item in NeutralUnitsList)
            item.SetActive(false);
    }

    public void SetAmount(int amount, bool neutrals = false)
    {
        for (int i = 0; i < UnitsList.Count; i++)
        {
            GameObject item = UnitsList[i];
            item.SetActive(i < amount && !neutrals);
        }
        for (int i = 0; i < NeutralUnitsList.Count; i++)
        {
            GameObject item = NeutralUnitsList[i];
            item.SetActive(i < amount && neutrals);
        }
    }
}
