using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class fpscounter : MonoBehaviour
{
    int totalframe = 0;
    int count = 0;
    void LateUpdate() {
        int currentfps = (int)(1f / Time.unscaledDeltaTime);
        totalframe += currentfps;
        count++;
        this.GetComponent<TextMeshProUGUI>().text = "FPS : "+(int)(1f / Time.unscaledDeltaTime) + "\nAVG.FPS : "+(totalframe / count) ;
    }
}
