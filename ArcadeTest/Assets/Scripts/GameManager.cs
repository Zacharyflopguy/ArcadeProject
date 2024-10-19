using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance; //Singleton instance
    
    public InputActionAsset playerInput; //Player's input actions
    
    public float staminaRegenRate = 2f; //Stamina regeneration rate
    
    public Image energyBar; //Reference to the energy bar UI element
    
    public Image healthBar; //Reference to the health bar UI element
    
    public TextMeshProUGUI scoreText; //Reference to the score text UI element
    
    [NonSerialized]
    public int stamina = 100; //Player's stamina
    
    [NonSerialized]
    public int health = 100; //Player's health
    
    [NonSerialized]
    public long score = 0; //Player's score

    private Rumbler rumble; //Reference to the Rumbler component
    
    private InputAction shieldAction;
    
    public Transform[] spawnPoints; //Array of spawn points for enemies

    private float difficulty = 0f;

    public string currentScene;
    
    public bool isBoss = false;

    public Transform deathExplosion1;
    public Transform deathExplosion2;
    public Transform deathExplosion3;
    public Transform deathExplosion4;
    
    [FormerlySerializedAs("teleportEffectPrefab")] 
    public GameObject explosionEffectPrefab; //Reference to the explosion effect prefab
    public GameObject bigExplosionEffectPrefab; //Reference to the big explosion effect prefab
    
    public AudioSource explosionSound; //Reference to the explosion sound effect

    [Header("Enemy Prefabs")] 
    public GameObject baseEnemyPrefab;
    public GameObject doubleEnemyPrefab;
    public GameObject bombEnenmyPrefab;
    public GameObject homingEnemyPrefab;
    public GameObject multiplyBossPrefab;
    public GameObject chargeBossPrefab;
    public GameObject laserBossPrefab;
    
    private IEnumerator increseDifficultyCoroutine;
    private IEnumerator staminaRegenCoroutine;
    private IEnumerator spawnBaseEnemyCoroutine;
    private IEnumerator spawnDoubleEnemyCoroutine;
    private IEnumerator spawnBombEnemyCoroutine;
    private IEnumerator spawnHomingEnemyCoroutine;
    private IEnumerator updateScoreCoroutine;
    private IEnumerator spawnBossCoroutine;
    private bool isDead = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        
        currentScene = SceneManager.GetActiveScene().name;
        
        rumble = gameObject.GetComponent<Rumbler>();

        if (currentScene == "Space")
        {
            shieldAction = playerInput.FindAction("Shield");

            shieldAction.Enable();

            shieldAction.performed += _ => smallRumble();
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if (currentScene == "Space")
        {
            //Correct timeScale
            Time.timeScale = 1;
            isDead = false;
            
            increseDifficultyCoroutine = IncreaseDifficulty();
            staminaRegenCoroutine = StaminaRegen();
            spawnBaseEnemyCoroutine = SpawnBaseEnemy();
            spawnDoubleEnemyCoroutine = SpawnDoubleEnemy();
            spawnBombEnemyCoroutine = SpawnBombEnemy();
            spawnHomingEnemyCoroutine = SpawnHomingEnemy();
            updateScoreCoroutine = UpdateScore();
            spawnBossCoroutine = spawnBoss();
            
            
            StartCoroutine(increseDifficultyCoroutine);
            StartCoroutine(staminaRegenCoroutine);
            StartCoroutine(spawnBaseEnemyCoroutine);
            StartCoroutine(spawnDoubleEnemyCoroutine);
            StartCoroutine(spawnBombEnemyCoroutine);
            StartCoroutine(spawnHomingEnemyCoroutine);
            StartCoroutine(updateScoreCoroutine);
            StartCoroutine(spawnBossCoroutine);
        }
    }

    private void Update()
    {
        if (currentScene == "Space")
        {
            if (health <= 0 && !isDead)
            {
                StartCoroutine(HandleDeath());
            }
        }
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
            if (!isBoss)
            {
                yield return new WaitForSeconds(Mathf.Max(3.5f, 7f - difficulty));
                if (isBoss) continue;
                var obj = Instantiate(baseEnemyPrefab, getRandomSpawnpoint().position, Quaternion.identity);
                obj.SetActive(true);
            }
            else
            {
                yield return new WaitUntil(() => !isBoss);
            }
        }
    }
    
    private IEnumerator SpawnDoubleEnemy()
    {
        yield return new WaitForSeconds(30f);
        while (true)
        {
            if (!isBoss)
            {
                yield return new WaitForSeconds(Mathf.Max(9.5f, 14f - difficulty));
                if (isBoss) continue;
                var obj = Instantiate(doubleEnemyPrefab, getRandomSpawnpoint().position, Quaternion.identity);
                obj.SetActive(true);
            }
            else
            {
                yield return new WaitUntil(() => !isBoss);
            }
        }
    }
    
    private IEnumerator SpawnBombEnemy()
    {
        yield return new WaitForSeconds(120f);
        while (true)
        {
            if (!isBoss)
            {
                yield return new WaitForSeconds(Mathf.Max(15f, 25f - difficulty));
                if (isBoss) continue;
                var obj = Instantiate(bombEnenmyPrefab, getRandomSpawnpoint().position, Quaternion.identity);
                obj.SetActive(true);
            }
            else
            {
                yield return new WaitUntil(() => !isBoss);
            }
        }
    }
    
    private IEnumerator SpawnHomingEnemy()
    {
        yield return new WaitForSeconds(60f);
        while (true)
        {
            if (!isBoss)
            {
                yield return new WaitForSeconds(Mathf.Max(12f, 20f - difficulty));
                if (isBoss) continue;
                var obj = Instantiate(homingEnemyPrefab, getRandomSpawnpoint().position, Quaternion.identity);
                obj.SetActive(true);
            }
            else
            {
                yield return new WaitUntil(() => !isBoss);
            }
        }
    }
    
    private IEnumerator spawnBoss()
    {
        yield return new WaitForSeconds(60f);
        while (true)
        {
            //Random wait time before spawning boss
            yield return new WaitForSeconds(Mathf.Max(60f, UnityEngine.Random.Range(85f, 105f) - difficulty));
            isBoss = true;
            yield return new WaitForSeconds(5f);
            var prefab = getRandomBoss();
            var obj = Instantiate(prefab, getRandomSpawnpoint().position, Quaternion.identity);
            obj.SetActive(true);
            yield return new WaitUntil(() => !isBoss);
        }
    }
    
    private GameObject getRandomBoss()
    {
        int randomIndex = UnityEngine.Random.Range(0, 3);
        return randomIndex switch
        {
            0 => multiplyBossPrefab,
            1 => chargeBossPrefab,
            2 => laserBossPrefab,
            _=> multiplyBossPrefab
        };
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
    
    public void spawnBigExplosionEffect(Vector3 pos)
    {
        StartCoroutine(BigExplosionEffect(pos));
    }
    
    private IEnumerator ExplosionEffect(Vector3 pos)
    {
        explosionSound.Play();
        GameObject obj = Instantiate(explosionEffectPrefab, pos, Quaternion.identity);
        obj.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        Destroy(obj);
    }
    
    private IEnumerator BigExplosionEffect(Vector3 pos)
    {
        explosionSound.Play();
        GameObject obj = Instantiate(bigExplosionEffectPrefab, pos, Quaternion.identity);
        obj.SetActive(true);
        yield return new WaitForSecondsRealtime(0.5f);
        Destroy(obj);
    }
    
    private IEnumerator UpdateScore()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            score += 10 + Mathf.RoundToInt(difficulty * 2);
            scoreText.text = "Score: " + score.ToString("N0");
        }
    }
    
    public void addScore(int amount)
    {
        score += amount;
        //Update score text and format for commas
        scoreText.text = "Score: " + score.ToString("N0");
    }

    private IEnumerator HandleDeath()
    {
        isDead = true;
        
        yield return new WaitForSecondsRealtime(1);
        
        spawnBigExplosionEffect(deathExplosion1.position);
        
        yield return new WaitForSecondsRealtime(0.3f);
        
        spawnBigExplosionEffect(deathExplosion2.position);
        
        yield return new WaitForSecondsRealtime(0.3f);
        
        spawnBigExplosionEffect(deathExplosion3.position);
        
        yield return new WaitForSecondsRealtime(0.3f);
        
        spawnBigExplosionEffect(deathExplosion4.position);
        
        //DeactivateShip
        deathExplosion1.gameObject.SetActive(false);
        
        yield return new WaitForSecondsRealtime(2f);
        
        
        
        SceneManager.LoadScene("Leaderboard");
        currentScene = "Leaderboard";
        StopCoroutine(increseDifficultyCoroutine);
        StopCoroutine(staminaRegenCoroutine);
        StopCoroutine(spawnBaseEnemyCoroutine);
        StopCoroutine(spawnDoubleEnemyCoroutine);
        StopCoroutine(spawnBombEnemyCoroutine);
        StopCoroutine(spawnHomingEnemyCoroutine);
        StopCoroutine(updateScoreCoroutine);
        StopCoroutine(spawnBossCoroutine);
    }
    
}



//Leaderboard 
//Classes
//Below



[System.Serializable]
public class LeaderboardEntry
{
    public string name;
    public long score;

    public LeaderboardEntry(string name, long score)
    {
        this.name = name;
        this.score = score;
    }
}

[System.Serializable]
public class Leaderboard
{
    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();
}

public class LeaderboardManager
{
    public int maxEntries = 10; // Maximum entries to store in the leaderboard
    private string leaderboardFilePath;

    private Leaderboard leaderboard;

    public void Awake()
    {
        // Define the path to store the leaderboard JSON file
        leaderboardFilePath = Path.Combine(Application.persistentDataPath, "leaderboard.json");

        // Load leaderboard on game start
        LoadLeaderboard();
    }

    // Save a new entry to the leaderboard
    public void SaveEntry(string name, long score)
    {
        // Create a new entry
        LeaderboardEntry newEntry = new LeaderboardEntry(name, score);

        // Add the entry to the list
        leaderboard.entries.Add(newEntry);

        // Sort the leaderboard by score in descending order
        leaderboard.entries.Sort((entry1, entry2) => entry2.score.CompareTo(entry1.score));

        // Limit to top 'maxEntries'
        if (leaderboard.entries.Count > maxEntries)
        {
            leaderboard.entries.RemoveAt(maxEntries); // Remove the lowest score
        }

        // Save the updated leaderboard to file
        SaveLeaderboard();
    }

    // Save the leaderboard to a JSON file
    private void SaveLeaderboard()
    {
        // Serialize leaderboard object to JSON
        string json = JsonUtility.ToJson(leaderboard, true);

        // Write the JSON string to the file
        File.WriteAllText(leaderboardFilePath, json);
    }

    // Load the leaderboard from the JSON file
    private void LoadLeaderboard()
    {
        // Check if the file exists
        if (File.Exists(leaderboardFilePath))
        {
            // Read the file into a string
            string json = File.ReadAllText(leaderboardFilePath);

            // Deserialize the JSON string into a Leaderboard object
            leaderboard = JsonUtility.FromJson<Leaderboard>(json);
        }
        else
        {
            // If the file does not exist, create a new empty leaderboard
            leaderboard = new Leaderboard();
        }
    }

    // Retrieve the leaderboard entries for display
    public List<LeaderboardEntry> GetLeaderboardEntries()
    {
        return leaderboard.entries;
    }
    
    public bool DoesScoreQualify(long score)
    {
        if (leaderboard.entries.Count < maxEntries)
        {
            return true;
        }
        else
        {
            return score > leaderboard.entries[^1].score;
        }
    }
}
