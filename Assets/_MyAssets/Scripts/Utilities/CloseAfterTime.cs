using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Jacob.Utilities
{
    public class CloseAfterTime : MonoBehaviour
    {
        public float lifeTime = 2;
        float timer;
        private void OnEnable()
        {
            timer = lifeTime;
        }
        private void Update()
        {
            if(timer > 0)
            {
                timer -= Time.deltaTime;

                if(timer<=0)
                {
                    this.gameObject.SetActive(false);
                }
            }
        }
    }

}
