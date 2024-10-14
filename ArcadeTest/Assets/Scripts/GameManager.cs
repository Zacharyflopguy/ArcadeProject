using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
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
    
    public Transform[] spawnPoints; //Array of spawn points for enemies

    private float difficulty = 0f;
    
    [FormerlySerializedAs("teleportEffectPrefab")] 
    public GameObject explosionEffectPrefab; //Reference to the explosion effect prefab

    [Header("Enemy Prefabs")] 
    public GameObject baseEnemyPrefab;

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
        StartCoroutine(IncreaseDifficulty());
        StartCoroutine(StaminaRegen());
        StartCoroutine(SpawnBaseEnemy());
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
    
    private Transform getRandomSpawnpoint()
    {
        int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
        return spawnPoints[randomIndex];
    }
    
    private IEnumerator SpawnBaseEnemy()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(1.5f, 5f - difficulty));
            var obj = Instantiate(baseEnemyPrefab, getRandomSpawnpoint().position, Quaternion.identity);
            obj.SetActive(true);
        }
    }
    
    private IEnumerator IncreaseDifficulty()
    {
        while (true)
        {
            yield return new WaitForSeconds(20f);
            difficulty += 0.1f;
        }
    }
    
    public void spawnExplosionEffect(Vector3 pos)
    {
        StartCoroutine(ExplosionEffect(pos));
    }
    
    private IEnumerator ExplosionEffect(Vector3 pos)
    {
        GameObject obj = Instantiate(explosionEffectPrefab, pos, Quaternion.identity);
        obj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        Destroy(obj);
    }
}
