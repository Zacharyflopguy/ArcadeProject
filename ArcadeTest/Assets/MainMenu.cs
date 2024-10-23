using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource logoMusic;         // Audio for the logo theme
    public AudioSource mainMenuMusic;     // Audio for the main menu theme

    [Header("UI Elements")]
    public Image logo;                    // UI Image of the logo to fade in and out
    public GameObject mainMenuPanel;      // Reference to the main menu panel

    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 1f;     // Speed of the fade in/out effect
    [SerializeField] private float logoDisplayTime = 2f; // Time to display the logo before fading out

    private bool isFadingIn = true;       // Control whether we're fading in or out
    private float fadeAmount = 0f;        // Current fade value
    
    [Header("Input Settings")]
    public InputActionAsset inputActions;  // Reference to the InputActionAsset
    private InputAction startAction;       // Reference to the "Start" action
    private System.Action<InputAction.CallbackContext> startGameHandler;  // Stored reference to the handler


    // Start is called before the first frame update
    void Start()
    {
        // Set the initial alpha to 0 (invisible)
        Color logoColor = logo.color;
        logoColor.a = 0f;
        logo.color = logoColor;
        
        // Get the "Start" action from the InputActionAsset
        startAction = inputActions.FindAction("Enter");

        // Start the coroutine for the logo sequence
        StartCoroutine(LogoSequence());
    }

    // Coroutine for handling the logo fade and audio transition
    IEnumerator LogoSequence()
    {
        // Step 1: Fade in the logo and play the logo music
        yield return new WaitForSeconds(1f);
        logoMusic.Play();
        yield return StartCoroutine(FadeInLogo());
        
        
        // Step 2: Wait for the specified logo display time
        yield return new WaitForSeconds(logoDisplayTime);

        // Step 3: Fade out the logo
        yield return StartCoroutine(FadeOutLogo());

        // Step 4: Stop the logo music and start the main menu music
        logoMusic.Stop();
        yield return new WaitForSeconds(1f);
        
        // Enable the main menu panel
        mainMenuPanel.SetActive(true);
        mainMenuMusic.Play();
        
        //assign the start action to the "Start" button
        startGameHandler = ctx => StartGame();  // Store the handler reference
        startAction.performed += startGameHandler;
        startAction.Enable();
    }

    // Coroutine to fade in the logo
    IEnumerator FadeInLogo()
    {
        while (logo.color.a < 1f)
        {
            fadeAmount += fadeSpeed * Time.deltaTime;
            SetLogoAlpha(fadeAmount);
            yield return null;
        }
    }

    // Coroutine to fade out the logo
    IEnumerator FadeOutLogo()
    {
        while (logo.color.a > 0f)
        {
            fadeAmount -= fadeSpeed * Time.deltaTime;
            SetLogoAlpha(fadeAmount);
            yield return null;
        }
    }

    // Helper method to set the logo alpha
    private void SetLogoAlpha(float alpha)
    {
        Color logoColor = logo.color;
        logoColor.a = Mathf.Clamp01(alpha);  // Ensure alpha stays within [0, 1]
        logo.color = logoColor;
    }
    
    private void StartGame()
    {
        //TODO: Needs to remove this action
        startAction.performed -= startGameHandler;
        SceneManager.LoadScene("Space");
    }
}