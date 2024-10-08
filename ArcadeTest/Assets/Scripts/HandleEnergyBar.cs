using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandleEnergyBar : MonoBehaviour
{
    public Sprite[] barIcons = new Sprite[21];
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GameManager.instance.energyBar.sprite = GameManager.instance.stamina switch
        {
            > 95 => barIcons[20],
            > 90 => barIcons[19],
            > 85 => barIcons[18],
            > 80 => barIcons[17],
            > 75 => barIcons[16],
            > 70 => barIcons[15],
            > 65 => barIcons[14],
            > 60 => barIcons[13],
            > 55 => barIcons[12],
            > 50 => barIcons[11],
            > 45 => barIcons[10],
            > 40 => barIcons[9],
            > 35 => barIcons[8],
            > 30 => barIcons[7],
            > 25 => barIcons[6],
            > 20 => barIcons[5],
            > 15 => barIcons[4],
            > 10 => barIcons[3],
            > 5 => barIcons[2],
            > 0 => barIcons[1],
            _ => barIcons[0]
        };
    }
}
