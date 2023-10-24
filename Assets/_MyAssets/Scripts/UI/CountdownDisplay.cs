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
    public AudioSource audioSource;
    public AudioClip audioClip;
    public bool alertOn;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        alertOn = false;
    }
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

            if (!alertOn) // If the alert has not been activated yet
            {
                alertOn = true; // Set alertOn to true to indicate that the alert has been activated
                audioSource.PlayOneShot(audioClip); // Play the audio once
            }
        }
        else
        {
            timerUI.SetActive(false);
            alertOn = false; // Reset alertOn for the next time
        }
    }

    // Method to update the AIController reference
    public void SetAIController(AIController newAIController)
    {
        aiController = newAIController;
    }
}
