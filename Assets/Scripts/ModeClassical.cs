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
    //Classical
    public void StartClassic()
    {
        classic = true;
        classicalUI.gameObject.SetActive(true);
        wordBoxesUI.gameObject.SetActive(true);

        // Populate the dictionary
        menu.gameObject.SetActive(false);

        AddWordsToList(dictionary);

        // Populate the guessing words
        AddWordsToList(guessingWords);

        // Choose a random correct word
        correctWord = GetRandomWord();

        char[] correctWordLetters = correctWord.ToCharArray();
    }

    public void SubmitWord()
    {
        // The players guess
        string guess = "";
        var token = new CancellationTokenSource();
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
            ShowPopupAsync("Слово должно быть из 5 букв", token.Token, false);
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
            ShowPopupAsync("Некорректное слово", token.Token, false);

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

        CheckWordAsync(guess, token.Token);
    }
}
