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
    //Reversed
    public void StartReverse()
    {
        classic = false;

        // Populate the dictionary
        menu.gameObject.SetActive(false);

        AddWordsToList(dictionary);

        // Populate the guessing words
        AddWordsToList(guessingWords);

        enterMenu.gameObject.SetActive(true);
    }

    public void SubmitReverseWord()
    {
        // The players guess
        string winWord = "";
        var token = new CancellationTokenSource();
        for (int i = 0; i < 5; i++)
        {
            // Add each letter to the players guess
            winWord += reverseWordBoxes[i].GetChild(0).GetComponent<TextMeshProUGUI>().text;
        }

        if (winWord.Length != 5)
        {
            Debug.Log("Слово должно быть из 5 букв");
            ShowPopupAsync("Слово должно быть из 5 букв", token.Token, false);
            Handheld.Vibrate();
            for (int i = 0; i < 5; i++)
            {
                reverseWordBoxes[i].transform.DOShakePosition(
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
        winWord = winWord.ToLower();

        // Check if the word exists in the dictionary
        bool wordExists = false;
        foreach (var word in dictionary)
        {
            if (winWord == word)
            {
                wordExists = true;
                break;
            }
        }

        if (wordExists == false)
        {
            ShowPopupAsync("Некорректное слово", token.Token, false);

            Handheld.Vibrate();
            for (int i = 0; i < 5; i++)
            {
                reverseWordBoxes[i].transform.DOShakePosition(
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

        correctWord = winWord;
        Debug.Log("Загаданное слово: " + correctWord);
        currentWordBox = 0;
        currentRow = 0;
        enterMenu.gameObject.SetActive(false);
        wordBoxesUI.gameObject.SetActive(true);
        started = true;
        ReverseGuessWordAsync(token.Token);
    }

    public void RemoveReverseLetterFromWordBox()
    {
        var token = new CancellationTokenSource();
        // If the text in the current wordbox is empty, go back a step and clear the one
        // that comes after
        if (reverseWordBoxes[currentWordBox].GetChild(0).GetComponent<TextMeshProUGUI>().text == "")
        {
            if (currentWordBox > 0)
            {
                // Step back
                currentWordBox--;
            }

            reverseWordBoxes[currentWordBox].GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
        else
        {
            // If it wasn't empty, we clear the one selected instead
            reverseWordBoxes[currentWordBox].GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
        AnimateWordBoxAsync(reverseWordBoxes[currentWordBox], token.Token);
    }

    async UniTaskVoid ReverseGuessWordAsync(CancellationToken cancellationToken)
    {
        var token = new CancellationTokenSource();
        string word = guessingWords[UnityEngine.Random.Range(0, guessingWords.Count)];
        ReverseEnterWord(word.ToUpper());
        CheckWordAsync(word, token.Token);

        for (int i = 0; i < 5; i++)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(3));
            word = GetMasterWord().ToLower();
            ReverseEnterWord(word.ToUpper());
            CheckWordAsync(word, token.Token);
        }
    }

    void ReverseEnterWord(string word)
    {
        char[] wordArray = word.ToCharArray();
        for (int i = 0; i < 5; i++)
        {
            AddLetterToWordBox(wordArray[i].ToString());
        }
    }
}
