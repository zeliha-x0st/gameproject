using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelManager : MonoBehaviour
{
    [Header("Levels & Board")]
    public LevelData[] levels;
    public BoardManager board;

    [Header("UI")]
    public TextMeshProUGUI redTxt, yellowTxt, greenTxt, blueTxt;
    public GameObject levelClearPanel;
    public TextMeshProUGUI clearText;  // Panel üzerindeki başlık
    public GameObject nextButton;      // Panel üzerindeki NextButton

    int curLevel;
    int needR, needY, needG, needB;

    void Start() => LoadLevel(0);

    void LoadLevel(int index)
    {
        curLevel = index;
        LevelData data = levels[curLevel];

        // Tahtayı hazırla
        board.boardSize = data.boardSize;
        board.ResetBoard();
        board.OnPieceCleared = OnPieceCleared;

        // Hedefleri ayarla
        needR = data.targetRed;
        needY = data.targetYellow;
        needG = data.targetGreen;
        needB = data.targetBlue;

        // UI güncelle, paneli ve butonu hazırla
        UpdateUI();
        levelClearPanel.SetActive(false);
        nextButton.SetActive(true);

        // Panel başlığını varsayılan değere çek
        clearText.text = (curLevel == 0) ? "Tebrikler!" : "LEVEL CLEARED!";
    }

    void OnPieceCleared(BoardManager.PieceType type)
    {
        // Hedef sayaçlarını düşür
        switch (type)
        {
            case BoardManager.PieceType.Red:
                if (needR > 0) needR--;
                break;
            case BoardManager.PieceType.Yellow:
                if (needY > 0) needY--;
                break;
            case BoardManager.PieceType.Green:
                if (needG > 0) needG--;
                break;
            case BoardManager.PieceType.Blue:
                if (needB > 0) needB--;
                break;
        }
        UpdateUI();

        // ── Level 1: sadece kırmızı+sarının bittiğine bak
        if (curLevel == 0 && needR == 0 && needY == 0)
        {
            clearText.text = "Tebrikler!";
            levelClearPanel.SetActive(true);
        }
        // ── Diğer leveller: tüm renkler tamamlandığında
        else if (curLevel > 0 && (needR + needY + needG + needB) == 0)
        {
            clearText.text = "LEVEL CLEARED!";
            levelClearPanel.SetActive(true);
        }
    }

    public void NextLevel()
    {
        // Paneli kapat
        levelClearPanel.SetActive(false);

        int next = curLevel + 1;
        if (next < levels.Length)
        {
            LoadLevel(next);
        }
        else
        {
            // Tüm seviyeler bitti
            ShowGameFinished();
        }
    }

    void ShowGameFinished()
    {
        // Tebrik panelini aç
        levelClearPanel.SetActive(true);

        // Başlığı değiştir
        clearText.text = "Congratulations!\nYou finished the game!";

        // Butonu gizle
        nextButton.SetActive(false);
    }

    void UpdateUI()
    {
        redTxt.text = needR.ToString();
        yellowTxt.text = needY.ToString();
        greenTxt.text = needG.ToString();
        blueTxt.text = needB.ToString();
    }
}
