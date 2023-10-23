using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CountdownDisplay : MonoBehaviour
{
    public AIController aiController;
    public GameObject timerUI;
    public TextMeshProUGUI countdownTextMeshPro;

    void Update()
    {
        if (aiController == null)
        {
            timerUI.SetActive(false); // Hide UI if no AIController is assigned
            return;
        }

        float countdownValue = aiController.alarmTimer;

        if (countdownValue > 1)
        {
            timerUI.SetActive(true);
            countdownTextMeshPro.text = countdownValue.ToString("F2");
        }
        else
        {
            timerUI.SetActive(false);
        }
    }

    // Method to update the AIController reference
    public void SetAIController(AIController newAIController)
    {
        aiController = newAIController;
    }
}
