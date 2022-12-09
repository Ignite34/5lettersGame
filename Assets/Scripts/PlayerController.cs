using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // A list populated with the text components of the keyboard letters
    public List<Button> classicalKeyboardCharacterButtons = new List<Button>();
    public List<Button> reversedKeyboardCharacterButtons = new List<Button>();

    // All characters in the keyboard, named from top row to bottom row
    private string characterNames = "ЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮ";

    // Reference to gameController
    public GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        SetupButtons();
    }

    void SetupButtons()
    {
        // Starting from the top row, set the text of the keyboard-texts to the ones in the list
        for (int i = 0; i < classicalKeyboardCharacterButtons.Count; i++)
        {
            // Here we use GetChild and then GetComponent, it's not the most efficient way performance wise.
            // For setting things up and one shots it is usually fine, but doing it every frame inside of
            // Update() for example is not good practice and might give you dips in performance. Just a tip!
            classicalKeyboardCharacterButtons[i].transform
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>()
                .text = characterNames[i].ToString();
        }

        // Whenever we click a button, run the function ClickCharacter and output the character to the Console.
        foreach (var keyboardButton in classicalKeyboardCharacterButtons)
        {
            string letter = keyboardButton.transform
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>()
                .text;
            keyboardButton.GetComponent<Button>().onClick.AddListener(() => ClickCharacter(letter));
        }

        // Starting from the top row, set the text of the keyboard-texts to the ones in the list
        for (int i = 0; i < reversedKeyboardCharacterButtons.Count; i++)
        {
            // Here we use GetChild and then GetComponent, it's not the most efficient way performance wise.
            // For setting things up and one shots it is usually fine, but doing it every frame inside of
            // Update() for example is not good practice and might give you dips in performance. Just a tip!
            reversedKeyboardCharacterButtons[i].transform
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>()
                .text = characterNames[i].ToString();
        }

        // Whenever we click a button, run the function ClickCharacter and output the character to the Console.
        foreach (var keyboardButton in reversedKeyboardCharacterButtons)
        {
            string letter = keyboardButton.transform
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>()
                .text;
            keyboardButton.GetComponent<Button>().onClick.AddListener(() => ClickCharacter(letter));
        }
    }

    void ClickCharacter(string letter)
    {
        // add the letters to the wordboxes.
        gameController.AddLetterToWordBox(letter);
    }

    public Image GetKeyboardImage(string letter)
    {
        // The letters on the keyboard are in uppercase, so first we need to make sure that the letter we check for is in uppercase
        letter = letter.ToUpper();

        // Go through every key and return the one with the correct letter
        foreach (var keyboardLetter in classicalKeyboardCharacterButtons)
        {
            if (keyboardLetter.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == letter)
            {
                return keyboardLetter.transform.GetComponent<Image>();
            }
        }

        foreach (var keyboardLetter in reversedKeyboardCharacterButtons)
        {
            if (keyboardLetter.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == letter)
            {
                return keyboardLetter.transform.GetComponent<Image>();
            }
        }
        Debug.Log("This letter does not exist on the current keyboard.");
        return null;
    }

    public TextMeshProUGUI GetKeyboardText(string letter)
    {
        // The letters on the keyboard are in uppercase, so first we need to make sure that the letter we check for is in uppercase
        letter = letter.ToUpper();

        // Go through every key and return the one with the correct letter
        foreach (var keyboardLetter in classicalKeyboardCharacterButtons)
        {
            if (keyboardLetter.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == letter)
            {
                return keyboardLetter.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
        }

        // Go through every key and return the one with the correct letter
        foreach (var keyboardLetter in reversedKeyboardCharacterButtons)
        {
            if (keyboardLetter.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text == letter)
            {
                return keyboardLetter.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            }
        }

        return null;
    }

    // Update is called once per frame
    void Update() { }
}
