using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Jacob.Utilities;



public class ItemSlot : MonoBehaviour
{
    public Image img;
    public Item targetItem;
    // Start is called before the first frame update
    void Start()
    {
        IconMaker.RequestIcon(targetItem);
    }

    // Update is called once per frame
    void Update()
    {
        img.sprite = targetItem.inventoryIcon;
    }
}
