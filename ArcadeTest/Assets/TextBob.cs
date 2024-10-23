using UnityEngine;
using TMPro;

public class TextBob : MonoBehaviour
{
    // Serialized fields to tweak in the Inspector
    [SerializeField] private float bobHeight = 10f;  // The height of the bobbing motion
    [SerializeField] private float bobSpeed = 2f;    // The speed of the bobbing motion
    [SerializeField] private TextMeshProUGUI textMeshPro;  // The TextMeshPro component
    
    private Vector3 startPosition;  // To store the initial position of the text

    private void Start()
    {
        // Store the initial position of the text
        startPosition = textMeshPro.rectTransform.anchoredPosition;
    }

    private void Update()
    {
        // Apply a sinusoidal "bobbing" motion to the text
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        textMeshPro.rectTransform.anchoredPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}

