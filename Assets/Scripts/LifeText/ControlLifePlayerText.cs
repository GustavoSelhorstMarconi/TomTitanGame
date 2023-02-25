using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlLifePlayerText : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TextMeshProUGUI textLifePlayer;

    public void SetLifeTextPlayer(int maxLife, int currentLife)
    {
        textLifePlayer.SetText("Health: " + currentLife + "/" + maxLife);
    }
}