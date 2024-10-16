using System.Collections;
using TMPro;
using UnityEngine;

public class BossTimer : MonoBehaviour
{
    public Transform startPos;          // Offscreen start position
    public Transform endPos;            // Onscreen end position
    public TextMeshProUGUI textObject;  // UI text to display the timer
    public float startTime = 5f;        // Starting time for the timer
    public float tweenDuration = 1f;    // How long it takes for the timer to tween in/out
    private float currentTime;
    private bool timerRunning = false;
    private RectTransform rectTransform;
    private bool tweeningIn = false;
    private bool tweeningOut = false;
    private float tweenTime = 0f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        currentTime = startTime;
        textObject.text = "Boss Incoming in: " + startTime.ToString("F1");  // Set initial text with 1 decimal point
        MoveOutOfScreen();  // Initially hide the timer
    }

    void Update()
    {
        // Start the boss timer if GameManager.instance.isBoss becomes true
        if (GameManager.instance.isBoss && !timerRunning)
        {
            StartCoroutine(StartBossTimer());
        }

        // Handle the Lerp tweening in and out
        if (tweeningIn)
        {
            TweenIn();
        }
        else if (tweeningOut)
        {
            TweenOut();
        }
    }

    IEnumerator StartBossTimer()
    {
        // Start tweening in
        tweeningIn = true;

        // Start the countdown
        timerRunning = true;
        currentTime = startTime;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            textObject.text = "Boss Incoming in: " + currentTime.ToString("F1");  // Display time with one decimal
            yield return null;
        }

        // Ensure timer doesn't show negative time
        textObject.text = "Boss Incoming in: 0.0";

        // Start tweening out once the countdown ends
        tweeningOut = true;
        
        // Wait for the tween out to finish before stopping the timer
        yield return new WaitForSeconds(tweenDuration);
        
        yield return new WaitUntil(() => !GameManager.instance.isBoss);
        
        timerRunning = false;
        
    }

    // Lerp the timer in from the startPos to the endPos
    void TweenIn()
    {
        if (tweenTime < tweenDuration)
        {
            tweenTime += Time.deltaTime;
            float t = tweenTime / tweenDuration;
            rectTransform.position = Vector3.Lerp(startPos.position, endPos.position, t);
        }
        else
        {
            tweenTime = 0f;
            tweeningIn = false;
        }
    }

    // Lerp the timer out from the endPos to the startPos
    void TweenOut()
    {
        if (tweenTime < tweenDuration)
        {
            tweenTime += Time.deltaTime;
            float t = tweenTime / tweenDuration;
            rectTransform.position = Vector3.Lerp(endPos.position, startPos.position, t);
        }
        else
        {
            tweenTime = 0f;
            tweeningOut = false;
        }
    }

    // Initially place the timer offscreen
    void MoveOutOfScreen()
    {
        rectTransform.position = startPos.position;
    }
}