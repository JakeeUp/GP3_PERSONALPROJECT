using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healthIncrease = 20; // The amount of health to increase when picked up.

    private void OnTriggerEnter(Collider other)
    {
        // Replace 'GameController' with the tag you're using for your game controller.
        if (other.CompareTag("GameController"))
        {
            Controller controller = other.GetComponent<Controller>();
            if (controller != null)
            {
                Debug.Log("health pickup");
                controller.currentHealth += 10;
                // Here you can set the new health value to the controller, 
                // ensuring it does not exceed the maximum allowed health.

                // Destroy the health pickup item after the player picks it up
                Destroy(this.gameObject);
            }
        }
    }
}