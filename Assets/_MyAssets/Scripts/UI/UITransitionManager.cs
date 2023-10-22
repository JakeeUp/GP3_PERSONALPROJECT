using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UITransitionManager : MonoBehaviour
{
    public CinemachineVirtualCamera currentCamera;
    public Button startButton;
    public Button exitButton;

    private void Start()
    {
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }

        if (exitButton != null)  
        {
            exitButton.onClick.AddListener(ExitGame);  
        }

        currentCamera.Priority++;
    }

    private void StartGame()
    {
        SceneManager.LoadScene("MainScene");
    }
    
    private void ExitGame()  
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#else
            Debug.Log("Exit Pressed");
            Application.Quit();  
#endif
    }

    public void UpdateCamera(CinemachineVirtualCamera target)
    {
        Debug.Log("pressed");
        currentCamera.Priority--;

        currentCamera = target;

        currentCamera.Priority++;
    }
}
