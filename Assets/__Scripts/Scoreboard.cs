using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Класс Scoreboard управляет отображением очков игрока
public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;
    [Header("Set in I")]
    public GameObject prefabFloatingScore;
    [Header("Set D")]
    [SerializeField]
    private int _score = 0;
    [SerializeField]
    private string _scoreString;
    private Transform canvasTrans;
    // Свойство score также устанавливает scorestring
    public int score
    {
        get { return (_score); }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");
        }
    }
    // Свойство scorestring также устанавливает Text.text
    public string scoreString
    {
        get
        {
            return (_scoreString);
        }
        set
        {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }
    private void Awake()
    {
        if (S == null)
        {
            S = this;
        }
        else
        {
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set!");
        }
        canvasTrans = transform.parent;
    }
    // Когда вызывается методом SendMessage, прибавляет fs.score к this.score
    public void FSCallback(FloatingScore fs)
    {
        score += fs.score;
    }
    //создает и инициализирует новый игровой обьект FloatingScore
    //возвращает указатель на созданный экземпляр FloatingScore
    // вызывающая функция могла выполнить с ним дополнительные операции
    // (например, определить список fontsizes и т.д.)
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts)
    {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);
        go.transform.SetParent(canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;// Настроить обратный вызов
        fs.Init(pts);
        return (fs);
    }
}
