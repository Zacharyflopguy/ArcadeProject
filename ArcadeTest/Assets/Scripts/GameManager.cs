using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    
    public static GameManager instance; //Singleton instance
    
    public InputActionAsset playerInput; //Player's input actions
    
    public float staminaRegenRate = 2f; //Stamina regeneration rate
    
    public Image energyBar; //Reference to the energy bar UI element
    
    public Image healthBar; //Reference to the health bar UI element
    
    [NonSerialized]
    public int stamina = 100; //Player's stamina
    
    [NonSerialized]
    public int health = 100; //Player's health

    private Rumbler rumble; //Reference to the Rumbler component
    
    private InputAction shieldAction;


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        rumble = gameObject.GetComponent<Rumbler>();
        
        shieldAction = playerInput.FindAction("Shield");
        
        shieldAction.Enable();
        
        shieldAction.performed += _ => smallRumble();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StaminaRegen());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator StaminaRegen()
    {
        while (true)
        {
            yield return new WaitForSeconds(staminaRegenRate);
            if (stamina < 100 && !shieldAction.IsPressed())
            {
                stamina += 1;
            }
        }
    }

    public void invalidRumble()
    {
        rumble.RumbleConstant(0.6f, 0.8f, 0.4f);
    }

    public void smallRumble()
    {
        rumble.RumbleConstant(0.6f, 0.6f, 0.2f);
    }
}
