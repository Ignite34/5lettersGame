using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System;

public class GameController : MonoBehaviour
{
    // Curve for animating the wordboxes
    public AnimationCurve wordBoxInteractionCurve;

    public GameObject popup;
    public TextAsset textFile;
    public CanvasGroup canvasGr;

    //
    // Our different colors that we use
    private Color colorCorrect = new Color(0.7921569f, 0.7529413f, 0.1019608f);
    private Color colorIncorrectPlace = new Color(1f, 1f, 1f);
    private Color colorUnused = new Color(0.1f, 0.1f, 0.1f);
    private Color colorTextBlack = new Color(0.1f, 0.1f, 0.1f, 1f);
    private Color colorTextWhite = new Color(1f, 1f, 1f, 1f);

    // The sprite that used when a box "cleared"
    public Sprite clearedWordBoxSprite;

    // Reference to the player controller script
    public PlayerController playerController;

    // Amount of rows of wordboxes
    private int amountOfRows = 5;

    // List with all the words
    private List<string> dictionary = new List<string>();

    // List with words that can be chosen as correct words
    private List<string> guessingWords = new List<string>();

    public string correctWord;

    // All wordboxes
    public List<Transform> wordBoxes = new List<Transform>();

    // Current wordbox that we're inputting in
    private int currentWordBox;

    // The current row that we're currently at
    private int currentRow;

    // How many characters are there per row
    private int charactersPerRowCount = 5;

    // Start is called before the first frame update
    void Start()
    {
        //string textPath = Path.Combine(Application.streamingAssetsPath, "words.txt");
        // Populate the dictionary


        AddWordsToList(dictionary);

        // Populate the guessing words
        AddWordsToList(guessingWords);

        // Choose a random correct word
        correctWord = GetRandomWord();

        Screen.SetResolution(Screen.width, Screen.height, true);
    }

    string GetRandomWord()
    {
        string randomWord = guessingWords[UnityEngine.Random.Range(0, guessingWords.Count)];
        Debug.Log(randomWord);
        return randomWord;
    }

    public void AddLetterToWordBox(string letter)
    {
        if (currentRow > amountOfRows)
        {
            Debug.Log("No more rows available");
            return;
        }

        if (
            wordBoxes[(currentRow * charactersPerRowCount) + currentWordBox]
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>()
                .text == ""
        )
        {
            wordBoxes[(currentRow * charactersPerRowCount) + currentWordBox]
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>()
                .text = letter;
            AnimateWordBox(wordBoxes[(currentRow * charactersPerRowCount) + currentWordBox]);
        }

        if (
            (currentRow * charactersPerRowCount) + currentWordBox
            < (currentRow * charactersPerRowCount) + 4
        )
        {
            currentWordBox++;
        }
    }

    void AddWordsToList(List<string> listOfWords)
    {
        TextAsset tAsset = Resources.Load("words") as TextAsset;
        // Read the text from the file
        //StreamReader reader = new StreamReader(path);
        string text = tAsset.ToString();

        // Separate them for each ',' character
        char[] separator = { ',' };
        string[] singleWords = text.Split(separator);

        // Add everyone of them to the list provided as a variable
        foreach (string newWord in singleWords)
        {
            listOfWords.Add(newWord);
        }

        // Close the reader
        //reader.Close();
    }

    public void SubmitWord()
    {
        // The players guess
        string guess = "";
        for (
            int i = (currentRow * charactersPerRowCount);
            i < (currentRow * charactersPerRowCount) + currentWordBox + 1;
            i++
        )
        {
            // Add each letter to the players guess
            guess += wordBoxes[i].GetChild(0).GetComponent<TextMeshProUGUI>().text;
        }

        if (guess.Length != 5)
        {
            Debug.Log("Слово должно быть из 5 букв");
            ShowPopup("Слово должно быть из 5 букв", 2f, false);
            Handheld.Vibrate();
            for (
                int i = (currentRow * charactersPerRowCount);
                i < (currentRow * charactersPerRowCount) + charactersPerRowCount;
                i++
            )
            {
                wordBoxes[i].transform.DOShakePosition(
                    0.5f,
                    5f,
                    10,
                    0,
                    true,
                    true,
                    ShakeRandomnessMode.Harmonic
                );
            }
            return;
        }

        // All words are in the list is in lowercase, so let's convert the guess to that as well
        guess = guess.ToLower();

        // Check if the word exists in the dictionary
        bool wordExists = false;
        foreach (var word in dictionary)
        {
            if (guess == word)
            {
                wordExists = true;
                break;
            }
        }

        if (wordExists == false)
        {
            ShowPopup("Некорректное слово", 2f, false);

            Handheld.Vibrate();
            for (
                int i = (currentRow * charactersPerRowCount);
                i < (currentRow * charactersPerRowCount) + charactersPerRowCount;
                i++
            )
            {
                wordBoxes[i].transform.DOShakePosition(
                    0.5f,
                    5f,
                    10,
                    0,
                    true,
                    true,
                    ShakeRandomnessMode.Harmonic
                );
            }
            return;
        }

        // Output the guess to the console
        Debug.Log("Player guess:" + guess);
        CheckWord(guess);
        // If the guess was correct, output that the player has won into the console
    }

    public void RemoveLetterFromWordBox()
    {
        if (currentRow > amountOfRows)
        {
            Debug.Log("No more rows available");
            return;
        }

        int currentlySelectedWordbox = (currentRow * charactersPerRowCount) + currentWordBox;

        // If the text in the current wordbox is empty, go back a step and clear the one
        // that comes after
        if (
            wordBoxes[currentlySelectedWordbox].GetChild(0).GetComponent<TextMeshProUGUI>().text
            == ""
        )
        {
            if (currentlySelectedWordbox > ((currentRow * charactersPerRowCount)))
            {
                // Step back
                currentWordBox--;
            }
            // Update the variable
            currentlySelectedWordbox = (currentRow * charactersPerRowCount) + currentWordBox;

            wordBoxes[currentlySelectedWordbox].GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "";
        }
        else
        {
            // If it wasn't empty, we clear the one selected instead
            wordBoxes[currentlySelectedWordbox].GetChild(0).GetComponent<TextMeshProUGUI>().text =
                "";
        }
        AnimateWordBox(wordBoxes[currentlySelectedWordbox]);
    }

    async UniTaskVoid CheckWord(string guess)
    {
        // Set up variables
        char[] playerGuessArray = guess.ToCharArray();
        string tempPlayerGuess = guess;
        char[] correctWordArray = correctWord.ToCharArray();
        string tempCorrectWord = correctWord;

        // Swap correct characters with '0'
        for (int i = 0; i < 5; i++)
        {
            if (playerGuessArray[i] == correctWordArray[i])
            {
                // Correct place
                playerGuessArray[i] = '0';
                correctWordArray[i] = '0';
            }
        }

        // Update the information
        tempPlayerGuess = "";
        tempCorrectWord = "";
        for (int i = 0; i < 5; i++)
        {
            tempPlayerGuess += playerGuessArray[i];
            tempCorrectWord += correctWordArray[i];
        }

        // Check for characters in wrong place, but correct letter
        for (int i = 0; i < 5; i++)
        {
            if (
                tempCorrectWord.Contains(playerGuessArray[i].ToString())
                && playerGuessArray[i] != '0'
            )
            {
                char playerCharacter = playerGuessArray[i];
                playerGuessArray[i] = '1';
                tempPlayerGuess = "";
                for (int j = 0; j < 5; j++)
                {
                    tempPlayerGuess += playerGuessArray[j];
                }

                // Update the correct word string with a '.'
                // so that we only check for the correct amount of characters.
                int index = tempCorrectWord.IndexOf(playerCharacter, 0);
                correctWordArray[index] = '.';
                tempCorrectWord = "";
                for (int j = 0; j < 5; j++)
                {
                    tempCorrectWord += correctWordArray[j];
                }
            }
        }

        // Set the fallback color to gray and white for text
        Color newColor = colorUnused;
        Color newTextColor = colorTextWhite;

        // Go through the players answer and color each button and wordbox accordingly
        for (int i = 0; i < 5; i++)
        {
            if (tempPlayerGuess[i] == '0')
            {
                // Correct placement
                newColor = colorCorrect;
                newTextColor = colorTextBlack;
            }
            else if (tempPlayerGuess[i] == '1')
            {
                // Correct character, wrong placement
                newColor = colorIncorrectPlace;
                newTextColor = colorTextBlack;
            }
            else
            {
                // Character not used
                newColor = colorUnused;
                newTextColor = colorTextWhite;
            }

            // Reference variable
            Image currentWordboxImage = wordBoxes[
                i + (currentRow * charactersPerRowCount)
            ].GetComponent<Image>();
            TextMeshProUGUI currentWordboxColor = wordBoxes[
                i + (currentRow * charactersPerRowCount)
            ]
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>();

            // Duration of the animation
            float duration = 0.15f;

            /* Old anim:
            // Loop for the duration
            while (timer <= duration)
            {
                 
                // Value will go from 0 to 1
                float value = timer / duration;

                // Interpolate linearly from a scale of (1, 1, 1) to a scale of (0, 0, 0)
                currentWordboxImage.transform.localScale = Vector3.Lerp(
                    Vector3.one,
                    Vector3.zero,
                    value
                );
                currentWordboxImage.transform.DOScale(Vector3.zero, duration);
            

                // Increase timer
                timer += Time.deltaTime;
                yield return null;


            }
            */
            //New anim:
            currentWordboxImage.transform.DOScale(Vector3.zero, duration);
            await UniTask.Delay(millisecondsDelay: 150);
            //await UniTask.WaitUntil(() => (currentWordboxImage.transform.localScale == Vector3.zero));
            // Set the scale again if we overshoot end anim 1
            currentWordboxImage.transform.localScale = Vector3.zero;

            // Change the sprite
            currentWordboxImage.sprite = clearedWordBoxSprite;

            // Set the color of the wordbox to the "new color"
            currentWordboxImage.color = newColor;
            currentWordboxColor.color = newTextColor;
            Debug.Log("New text color= " + currentWordboxColor);

            /* Old anim:

            // Same loop as before, but in reverse from from a scale of (0, 0, 0) to a scale of (1, 1, 1)
            while (timer <= duration)
            {
                // Value will go from 0 to 1
                float value = timer / duration;

                // Interpolate linearly from a scale of (0, 0, 0) to a scale of (1, 1, 1)
                currentWordboxImage.transform.localScale = Vector3.Lerp(
                    Vector3.zero,
                    Vector3.one,
                    value
                );

                // Increase timer
                timer += Time.deltaTime;
                yield return null;
            }
            */
            //New Anim:

            currentWordboxImage.transform.DOScale(Vector3.one, duration);
            await UniTask.Delay(millisecondsDelay: 150);
            //await UniTask.WaitUntil(() => (currentWordboxImage.transform.localScale == Vector3.one));


            // Set the scale again if we overshoot end anim 2
            //currentWordboxImage.transform.localScale = Vector3.one;

            // Set the color of the keyboard character to the "new color", only if it's "better" than the previous one

            // Saving a variable for the current keyboard image
            Image keyboardImage = playerController.GetKeyboardImage(guess[i].ToString());
            TextMeshProUGUI keyboardText = playerController.GetKeyboardText(guess[i].ToString());
            ;

            // Always possible to set the correct placement color
            if (newColor == colorCorrect)
            {
                keyboardImage.color = newColor;
                keyboardText.color = newTextColor;
            }

            // Only set the colorIncorrectPlace if it's not the colorCorrect
            if (newColor == colorIncorrectPlace && keyboardImage.color != colorCorrect)
            {
                keyboardImage.color = newColor;
                keyboardText.color = newTextColor;
            }

            // Only set the unused color if it's not colorIncorrectPlace and colorCorrect
            if (
                newColor == colorUnused
                && keyboardImage.color != colorCorrect
                && keyboardImage.color != colorIncorrectPlace
            )
            {
                keyboardImage.color = newColor;
                keyboardText.color = newTextColor;
            }
        }

        if (guess == correctWord)
        {
            ShowPopup("Поздравляем, это правильное слово!", 0f, true);
            Debug.Log("Correct word!");
        }
        else
        {
            // If the guess was incorrect, go to the next row
            currentWordBox = 0;
            currentRow++;
        }

        if (currentRow > amountOfRows)
        {
            ShowPopup(
                "Повезёт в следующий раз :(\n" + "Правильное слово: " + correctWord,
                0f,
                true
            );
        }
    }

    async UniTaskVoid ShowPopup(string message, float duration, bool stayForever = false)
    {
        // Set the message of the popup
        popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        // Activate the popup


        canvasGr.DOFade(1, 0.5f);

        // If it should stay forever or not
        /*
        if (stayForever)
        {
                await UniTask.Delay(TimeSpan.FromHours(1));
        }
        */
        // Wait for the duration time
        await UniTask.Delay(millisecondsDelay: 500);

        // Deactivate the popup

        canvasGr.DOFade(0, 0.5f);
    }

    async UniTaskVoid AnimateWordBox(Transform wordboxToAnimate)
    {
        // Duration of the animation
        float duration = 0.15f;

        //Set up startscale and end-scale of the wordbox
        Vector3 startScale = Vector3.one;

        // End-scale is just a little bit bigger than the original scale
        Vector3 scaledUp = Vector3.one * 1.2f;

        // Set the wordbox-scale to the starting scale, in case we're entering in the middle of another transition
        wordboxToAnimate.localScale = Vector3.one;

        wordboxToAnimate.transform.DOScale(scaledUp, duration);
        await UniTask.Delay(millisecondsDelay: 150);
        //await UniTask.WaitUntil(() => (wordboxToAnimate.transform.localScale == scaledUp));

        wordboxToAnimate.transform.DOScale(startScale, duration);
        await UniTask.Delay(millisecondsDelay: 150);
        //await UniTask.WaitUntil(() => (wordboxToAnimate.transform.localScale == startScale));

        // Since we're checking if the timer is smaller and/or equals to the duration in the loop above,
        // the value might go above 1 which would give the wordbox a scale that is not equals to the desired scale.
        // To prevent slightly scaled wordboxes, we set the scale of the wordbox to the startscale
        wordboxToAnimate.localScale = startScale;
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
