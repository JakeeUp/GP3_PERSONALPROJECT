using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownDisplay : MonoBehaviour
{
    public AIController aiController;
    public GameObject timerUI;  // Reference to the parent GameObject of all timer UI elements
    public TextMeshProUGUI countdownTextMeshPro;

    void Update()
    {
        if (aiController == null)
        {
            Debug.LogError("AIController reference is not assigned in CountdownDisplay.");
            return;
        }

        float countdownValue = aiController.alarmTimer;
        Debug.Log($"CountdownDisplay - Accessed Alarm Time: {countdownValue}");

        if (countdownValue > 1)
        {
            Debug.Log("TimerUI should be active");
            timerUI.SetActive(true);
            countdownTextMeshPro.text = countdownValue.ToString("F2");
        }
        else
        {
            Debug.Log("TimerUI should be inactive");
            timerUI.SetActive(false);
        }
    }
}
