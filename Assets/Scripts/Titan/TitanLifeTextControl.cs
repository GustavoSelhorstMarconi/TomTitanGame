using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TitanLifeTextControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TextMeshProUGUI textLife;

    public void SetTextLife(int maxLife, int currentLife)
    {
        textLife.SetText("Tom: " + currentLife + "/" + maxLife);
    }

    public void ActiveLifeText(bool state)
    {
        gameObject.SetActive(state);
    }
}