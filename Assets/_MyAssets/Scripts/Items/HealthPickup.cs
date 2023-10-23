using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HealthPickup : MonoBehaviour
{
    [SerializeField] private int healthIncrease = 20; // The amount of health to increase when picked up.
   

    private void Start()
    {
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController"))
        {


            Controller controller = other.GetComponent<Controller>();
            if (controller != null)
            {
                Debug.Log("health pickup");
                if (controller.currentHealth < 100)
                {
                    controller.currentHealth += 10;

                }
                Destroy(this.gameObject);
            }
        }
    }
}