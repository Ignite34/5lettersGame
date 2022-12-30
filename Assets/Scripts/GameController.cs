using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

public partial class GameController : MonoBehaviour
{
    // Curve for animating the wordboxes
    public AnimationCurve wordBoxInteractionCurve;

    public GameObject popup;
    public TextAsset textFile;
    public CanvasGroup canvasGr;
    public CanvasGroup chooseColor;
    public GameObject masterHelper;
    public GameObject menu;
    public GameObject enterMenu;
    public GameObject classicalUI;
    public GameObject reversedUI;
    public GameObject wordBoxesUI;
    public GameObject gameGuessUI;
    public GameObject chooseColorObject;
    public string currentWord;
    private bool started = false;
    public bool classic = true;
    private bool gameGuess = false;

    //
    // Our different colors that we use
    private Color colorCorrect = new Color(0.7921569f, 0.7529413f, 0.1019608f);
    private Color colorIncorrectPlace = new Color(1f, 1f, 1f);
    private Color colorUnused = new Color(0.1f, 0.1f, 0.1f);
    private Color colorTextBlack = new Color(0.1f, 0.1f, 0.1f, 1f);
    private Color colorTextWhite = new Color(1f, 1f, 1f, 1f);

    // The sprite that used when a box "cleared"
    public Sprite clearedWordBoxSprite;
    public Sprite defaultSprite;

    // Reference to the player controller script
    public PlayerController playerController;

    // Amount of rows of wordboxes
    private int amountOfRows = 5;

    // List with all the words
    public List<string> dictionary = new List<string>();

    // List with words that can be chosen as correct words
    public List<string> guessingWords = new List<string>();

    public string correctWord;

    // All wordboxes
    public List<Transform> wordBoxes = new List<Transform>();
    public List<Transform> GameGuessWordBoxes = new List<Transform>();
    public List<Transform> reverseWordBoxes = new List<Transform>();

    // Current wordbox that we're inputting in
    private int currentWordBox;

    // The current row that we're currently at
    private int currentRow;

    // How many characters are there per row
    private int charactersPerRowCount = 5;
    public int index = 0;
    private int gameGuessIndex = 0;

    List<char> usedLetters = new List<char>();
    List<char> correctLetters = new List<char>();
    List<char> inLetters = new List<char>();
    List<char> bannedLetters = new List<char>();

    char[] placeArray = new char[] { '0', '0', '0', '0', '0' };

    // Start is called before the first frame update
    void Start()
    {
        Screen.SetResolution(Screen.width, Screen.height, true);
    }

    public void StartGame() { }

    public string GetRandomWord()
    {
        string randomWord = guessingWords[UnityEngine.Random.Range(0, guessingWords.Count)];
        Debug.Log(randomWord);
        return randomWord;
    }

    public void ShowHelpWord()
    {
        var token = new CancellationTokenSource();
        ShowPopupAsync(GetHelpWord(), token.Token, false);
    }

    public void ShowMasterWord()
    {
        var token = new CancellationTokenSource();
        ShowPopupAsync(GetMasterWord(), token.Token, false);
    }

    string GetHelpWord()
    {
        char[] correctArray = correctWord.ToCharArray();
        for (int i = 0; i < 5; i++)
        {
            if (!usedLetters.Contains(correctArray[i]))
            {
                for (int j = 0; j < guessingWords.Count; j++)
                {
                    string word = guessingWords[UnityEngine.Random.Range(0, guessingWords.Count)];
                    if (word.Contains(correctArray[i]))
                    {
                        return ("Попробуйте слово: " + word.ToUpper());
                    }
                }
            }
        }
        return "Больше помочь нельзя";
    }

    string GetMasterWord()
    {
        var token = new CancellationTokenSource();
        for (int i = 0; i < guessingWords.Count; i++)
        {
            string word = guessingWords[UnityEngine.Random.Range(0, guessingWords.Count)];
            char[] wordArray = word.ToCharArray();
            bool wordCorrect = true;
            bool allLetters = true;

            //check if we have any banned letters
            for (int j = 0; j < bannedLetters.Count; j++)
            {
                if (word.Contains(bannedLetters[j]))
                {
                    wordCorrect = false;
                }
            }
            if (inLetters.Count != 0)
            {
                Debug.Log("Букв в списке - " + inLetters.Count);

                //check if we have all the marked letters from word
                for (int j = 0; j < inLetters.Count; j++)
                {
                    if (!word.Contains(inLetters[j]))
                    {
                        allLetters = false;
                    }
                }

                //check if we have correct letters on the correct positions
                if (allLetters && wordCorrect)
                {
                    if (correctLetters.Count != 0)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            if (placeArray[j] != '0' && wordArray[j] != placeArray[j])
                            {
                                wordCorrect = false;
                            }
                        }
                        if (wordCorrect)
                        {
                            Debug.Log("Вывожу слово с правильными подходящими буквами");
                            return (word);
                        }
                    }
                    else
                    {
                        Debug.Log("Вывожу слово без правильных букв");
                        return (word);
                    }
                }
            }
            else if (wordCorrect)
            {
                return (word);
            }
        }
        if (gameGuess)
        {
            ShowPopupAsync("Что-то пошло не так - слова кончились :(", token.Token, true);
        }
        Debug.Log("Нет входящих букв");
        return (guessingWords[UnityEngine.Random.Range(0, guessingWords.Count)]);
    }

    public void AddLetterToWordBox(string letter)
    {
        var token = new CancellationTokenSource();
        if (classic || started)
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
                AnimateWordBoxAsync(
                    wordBoxes[(currentRow * charactersPerRowCount) + currentWordBox],
                    token.Token
                );
            }

            if (
                (currentRow * charactersPerRowCount) + currentWordBox
                < (currentRow * charactersPerRowCount) + 4
            )
            {
                currentWordBox++;
            }
        }
        else
        {
            if (currentRow > 1)
            {
                Debug.Log("No more rows available");
                return;
            }

            if (
                reverseWordBoxes[(currentRow * charactersPerRowCount) + currentWordBox]
                    .GetChild(0)
                    .GetComponent<TextMeshProUGUI>()
                    .text == ""
            )
            {
                reverseWordBoxes[(currentRow * charactersPerRowCount) + currentWordBox]
                    .GetChild(0)
                    .GetComponent<TextMeshProUGUI>()
                    .text = letter;
                AnimateWordBoxAsync(
                    reverseWordBoxes[(currentRow * charactersPerRowCount) + currentWordBox],
                    token.Token
                );
            }

            if (
                (currentRow * charactersPerRowCount) + currentWordBox
                < (currentRow * charactersPerRowCount) + 4
            )
            {
                currentWordBox++;
            }
        }
    }

    public void AddWordsToList(List<string> listOfWords)
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

    public void RemoveLetterFromWordBox()
    {
        var token = new CancellationTokenSource();
        if (currentRow > amountOfRows)
        {
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
        AnimateWordBoxAsync(wordBoxes[currentlySelectedWordbox], token.Token);
    }

    public void addUsedLetter(char usedLetter)
    {
        if (!usedLetters.Contains(usedLetter))
        {
            usedLetters.Add(usedLetter);
            Debug.Log("Проверена новая буква  " + usedLetter);
        }
    }

    void addInLetter(char inLetter)
    {
        if (!inLetters.Contains(inLetter))
        {
            inLetters.Add(inLetter);
            Debug.Log("Добавлена новая словная буква  " + inLetter);
        }
    }

    public void addBannedLetter(char bannedLetter)
    {
        if (!bannedLetters.Contains(bannedLetter) && !inLetters.Contains(bannedLetter))
        {
            bannedLetters.Add(bannedLetter);
            Debug.Log("Исключена буква  " + bannedLetter);
        }
    }

    async UniTaskVoid CheckWordAsync(string guess, CancellationToken cancellationToken)
    {
        // Set up variables
        index = 0;
        char[] playerGuessArray = guess.ToCharArray();
        string tempPlayerGuess = guess;
        char[] correctWordArray = correctWord.ToCharArray();
        string tempCorrectWord = correctWord;
        var token = new CancellationTokenSource();

        // Swap correct characters with '0'
        for (int i = 0; i < 5; i++)
        {
            addUsedLetter(playerGuessArray[i]);
            if (playerGuessArray[i] == correctWordArray[i])
            {
                int correctCount = 0;
                int letterCount = 0;
                // Correct place
                if (placeArray[i] == '0')
                {
                    correctLetters.Add(playerGuessArray[i]);
                    placeArray[i] = playerGuessArray[i];
                    correctLetters.Add(Convert.ToChar(i));
                }

                for (int j = 0; j < 5; j++)
                {
                    if (correctWordArray[j] == playerGuessArray[i])
                    {
                        correctCount++;
                    }
                }

                for (int j = 0; j < inLetters.Count; j++)
                {
                    if (inLetters[j] == playerGuessArray[i])
                    {
                        letterCount++;
                    }
                }

                if (letterCount < correctCount)
                {
                    inLetters.Add(playerGuessArray[i]);
                }
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
                int correctCount = 0;
                int letterCount = 0;

                for (int j = 0; j < 5; j++)
                {
                    if (correctWordArray[j] == playerGuessArray[i])
                    {
                        correctCount++;
                    }
                }

                for (int j = 0; j < inLetters.Count; j++)
                {
                    if (inLetters[j] == playerGuessArray[i])
                    {
                        letterCount++;
                    }
                }

                if (letterCount < correctCount)
                {
                    inLetters.Add(playerGuessArray[i]);
                }
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
                addBannedLetter(tempPlayerGuess[i]);
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

            await currentWordboxImage.transform.DOScale(Vector3.zero, duration);

            // Set the scale again if we overshoot end anim 1
            currentWordboxImage.transform.localScale = Vector3.zero;

            // Change the sprite
            currentWordboxImage.sprite = clearedWordBoxSprite;

            // Set the color of the wordbox to the "new color"
            currentWordboxImage.color = newColor;
            currentWordboxColor.color = newTextColor;

            await currentWordboxImage.transform.DOScale(Vector3.one, duration);

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
            ShowPopupAsync("Поздравляем, это правильное слово!", token.Token, true);
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
            ShowPopupAsync(
                "Повезёт в следующий раз :(\n" + "Правильное слово: " + correctWord,
                token.Token,
                true
            );
        }
    }

    async UniTaskVoid ShowPopupAsync(
        string message,
        CancellationToken cancellationToken,
        bool stayForever = false
    )
    {
        // Set the message of the popup
        popup.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = message;
        // Activate the popup


        canvasGr.DOFade(1, 0.5f);

        // If it should stay forever or not

        if (stayForever)
        {
            await UniTask.Delay(TimeSpan.FromHours(1));
        }

        // Wait for the duration time
        await UniTask.Delay(millisecondsDelay: 2000);

        // Deactivate the popup

        canvasGr.DOFade(0, 0.5f);
    }

    async UniTaskVoid AnimateWordBoxAsync(
        Transform wordboxToAnimate,
        CancellationToken cancellationToken
    )
    {
        // Duration of the animation 0.15f
        float duration = 0.15f;

        //Set up startscale and end-scale of the wordbox
        Vector3 startScale = Vector3.one;

        // End-scale is just a little bit bigger than the original scale
        Vector3 scaledUp = Vector3.one * 1.2f;

        // Set the wordbox-scale to the starting scale, in case we're entering in the middle of another transition
        wordboxToAnimate.localScale = Vector3.one;

        await wordboxToAnimate.transform.DOScale(scaledUp, duration);

        await wordboxToAnimate.transform.DOScale(startScale, duration);

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
