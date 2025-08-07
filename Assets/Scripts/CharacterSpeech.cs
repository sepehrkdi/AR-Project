using TMPro;
using UnityEngine;

public class CharacterSpeech : MonoBehaviour
{
    public TextMeshProUGUI textBubble;

    public void SetDescription(string description)
    {
        if (textBubble != null)
            textBubble.text = description;
    }
}
