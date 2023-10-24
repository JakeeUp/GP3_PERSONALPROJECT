using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LoadSceneTrigger : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController")) 
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            LoadScene();
        }
    }

    private void LoadScene()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
