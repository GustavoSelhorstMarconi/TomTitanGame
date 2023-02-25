using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActiveTextControl : MonoBehaviour
{
    public void SetStateText(bool state)
    {
        gameObject.SetActive(state);
    }
}