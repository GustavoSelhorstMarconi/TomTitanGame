using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitanLifeBarControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Slider sliderLife;

    public void SetHealth(int maxLife, int currentLife)
    {
        sliderLife.maxValue = maxLife;
        sliderLife.value = currentLife;
    }

    public void ActiveLifeBar(bool state)
    {
        gameObject.SetActive(state);
    }
}