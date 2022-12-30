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

public partial class GameController
{
    //GameGuess
    public void StartGameGuess()
    {
        gameGuess = true;
        gameGuessUI.gameObject.SetActive(true);

        // Populate the dictionary
        menu.gameObject.SetActive(false);

        AddWordsToList(dictionary);

        // Populate the guessing words
        AddWordsToList(guessingWords);

        string word = guessingWords[UnityEngine.Random.Range(0, guessingWords.Count)];
        GameGuessEnterWord(word.ToUpper());
    }

    void GameGuessEnterWord(string word)
    {
        currentWord = word.ToLower();
        char[] wordArray = word.ToCharArray();
        for (int i = 0; i < 5; i++)
        {
            Image currentWordboxImage = GameGuessWordBoxes[i].GetComponent<Image>();
            TextMeshProUGUI currentWordboxText = GameGuessWordBoxes[i]
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>();
            currentWordboxImage.sprite = defaultSprite;
            currentWordboxImage.color = colorCorrect;
            currentWordboxText.color = colorTextWhite;
            GameGuessAddLetterToWordBox(wordArray[i].ToString());
        }
    }

    public void CorrectButton()
    {
        var token = new CancellationTokenSource();
        HideColorChoosePopupAsync(token.Token);
        GameGuessPaintCorrectWordBoxAsync(token.Token);
    }

    public void BannedButton()
    {
        var token = new CancellationTokenSource();
        HideColorChoosePopupAsync(token.Token);
        GameGuessPaintBannedWordBoxAsync(token.Token);
    }

    public void IncorrectPlaceButton()
    {
        var token = new CancellationTokenSource();
        HideColorChoosePopupAsync(token.Token);
        GameGuessIncorrectPlaceWordBoxAsync(token.Token);
    }

    public void GameGuessWordBoxPressed(int buttonNumber)
    {
        var token = new CancellationTokenSource();
        gameGuessIndex = buttonNumber;
        ShowColorChoosePopupAsync(token.Token);
    }

    async UniTaskVoid ShowColorChoosePopupAsync(CancellationToken cancellationToken)
    {
        chooseColorObject.SetActive(true);
        await chooseColor.DOFade(1, 0.15f);
        Debug.Log("Вывожу меню цвета");
    }

    async UniTaskVoid HideColorChoosePopupAsync(CancellationToken cancellationToken)
    {
        await chooseColor.DOFade(0, 0.15f);
        chooseColorObject.SetActive(false);
    }

    async UniTaskVoid GameGuessPaintCorrectWordBoxAsync(CancellationToken cancellationToken)
    {
        Image currentWordboxImage = GameGuessWordBoxes[gameGuessIndex].GetComponent<Image>();
        TextMeshProUGUI currentWordboxText = GameGuessWordBoxes[gameGuessIndex]
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
        currentWordboxImage.color = colorCorrect;
        currentWordboxText.color = colorTextBlack;

        await currentWordboxImage.transform.DOScale(Vector3.one, duration);
    }

    async UniTaskVoid GameGuessPaintBannedWordBoxAsync(CancellationToken cancellationToken)
    {
        Image currentWordboxImage = GameGuessWordBoxes[gameGuessIndex].GetComponent<Image>();
        TextMeshProUGUI currentWordboxText = GameGuessWordBoxes[gameGuessIndex]
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
        currentWordboxImage.color = colorUnused;
        currentWordboxText.color = colorTextWhite;

        await currentWordboxImage.transform.DOScale(Vector3.one, duration);
    }

    async UniTaskVoid GameGuessIncorrectPlaceWordBoxAsync(CancellationToken cancellationToken)
    {
        Image currentWordboxImage = GameGuessWordBoxes[gameGuessIndex].GetComponent<Image>();
        TextMeshProUGUI currentWordboxText = GameGuessWordBoxes[gameGuessIndex]
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
        currentWordboxImage.color = colorIncorrectPlace;
        currentWordboxText.color = colorTextBlack;

        await currentWordboxImage.transform.DOScale(Vector3.one, duration);
    }

    public void SubmitGameGuessWord()
    {
        char[] wordArray = currentWord.ToCharArray();
        bool wordCorrect = true;
        int winCount = 0;
        var token = new CancellationTokenSource();
        for (int i = 0; i < 5; i++)
        {
            Image currentWordboxImage = GameGuessWordBoxes[i].GetComponent<Image>();
            TextMeshProUGUI currentWordboxText = GameGuessWordBoxes[i]
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>();
            addUsedLetter(wordArray[i]);
            if (
                currentWordboxImage.color == colorCorrect
                && currentWordboxText.color == colorTextBlack
            )
            {
                winCount++;
                addInLetter(wordArray[i]);
                correctLetters.Add(wordArray[i]);
                placeArray[i] = wordArray[i];
                correctLetters.Add(Convert.ToChar(i));
                Debug.Log("Верно");
            }
            else if (currentWordboxImage.color == colorIncorrectPlace)
            {
                addInLetter(wordArray[i]);
                Debug.Log("Почти");
            }
            else if (
                currentWordboxImage.color == colorCorrect
                && currentWordboxText.color != colorTextBlack
            )
            {
                wordCorrect = false;
                ShowPopupAsync("Пометьте все буквы", token.Token, false);
                Debug.Log("Не все");
            }
        }
        for (int i = 0; i < 5; i++)
        {
            Image currentWordboxImage = GameGuessWordBoxes[i].GetComponent<Image>();
            TextMeshProUGUI currentWordboxText = GameGuessWordBoxes[i]
                .GetChild(0)
                .GetComponent<TextMeshProUGUI>();
            if (currentWordboxImage.color == colorUnused)
            {
                addBannedLetter(wordArray[i]);
                Debug.Log("Неверно");
            }
        }
        if (wordCorrect && winCount == 5)
        {
            ShowPopupAsync("Слово угадано! Это успех!", token.Token, true);
        }
        else if (wordCorrect)
        {
            currentWordBox = 0;
            GameGuessEnterWord(GetMasterWord().ToUpper());
        }
        else
        {
            wordCorrect = false;
            ShowPopupAsync("Пометьте все буквы", token.Token, false);
            Debug.Log("Не все 1");
        }
    }

    public void GameGuessAddLetterToWordBox(string letter)
    {
        var token = new CancellationTokenSource();
        GameGuessWordBoxes[currentWordBox].GetChild(0).GetComponent<TextMeshProUGUI>().text =
            letter;
        AnimateWordBoxAsync(GameGuessWordBoxes[currentWordBox], token.Token);
        if (currentWordBox < 4)
        {
            currentWordBox++;
        }
    }
}
