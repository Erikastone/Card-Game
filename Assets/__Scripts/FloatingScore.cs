using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Перечисление со всеми возможными состояниями FloatingScore
public enum eFSState
{
    idel,
    pre,
    active,
    post
}
//FloatingScore может перемещаться на экране по траектории,
//которая определяется кривой Безье
public class FloatingScore : MonoBehaviour
{
    [Header("Set D")]
    public eFSState state = eFSState.idel;
    [SerializeField]
    protected int _score = 0;
    public string scoreString;
    //ойство score устанавливает два поля, _score и scorestring
    public int score
    {
        get { return _score; }
        set
        {
            _score = value;
            scoreString = _score.ToString("N0");
            GetComponent<Text>().text = scoreString;
        }//NO требует добавить очки в число
    }
    public List<Vector2> bezierPts;// Точки, определяющие кривую Безье
    public List<float> fontsizes;// Точки кривой Безье для масштабирования
                                 // шрифта
    public float timestart = -1f;
    public float timeDuration = -1f;
    public string easingCurve = Easing.InOut;// Функция сглаживания
                                             // из Utils.cs
                                             // Игровой объект, для которого будет вызван метод SendMessage, когда этот
                                             // экземпляр FloatingScore закончит движение
    public GameObject reportFinishTo = null;
    private RectTransform rectTrans;
    private Text txt;
    // Настроить FloatingScore и параметры движения
    // Обратите внимание, что для параметров eTimeS и eTimeD определены
    // значения по умолчанию
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;
        txt = GetComponent<Text>();
        bezierPts = new List<Vector2>(ePts);
        if (ePts.Count == 1)
        {
            // Если задана только одна точка
            // ...просто переместиться в нее.
            transform.position = ePts[0];
            return;
        }
        // Если eTimeS имеет значение по умолчанию,
        // запустить отсчет от текущего времени
        if (eTimeS == 0) eTimeS = Time.time;
        timestart = eTimeS;
        timeDuration = eTimeD;
        state = eFSState.pre;// Установить состояние pre - готовность
                             // начать движение
    }
    public void FSCallback(FloatingScore fs)
    {
        // Когда SendMessage вызовет эту функцию,
        // она должна добавить очки из вызвавшего экземпляра FloatingScore
        score += fs.score;
    }
    private void Update()
    {
        // Если этот объект никуда не перемещается, просто выйти
        if (state == eFSState.idel) return;
        // Вычислить u на основе текущего времени и продолжительности движения
        // и изменяется от 0 до 1 (обычно)
        float u = (Time.time - timestart) / timeDuration;
        // Использовать класс Easing из Utils для корректировки значения и
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)// Если и<0, объект не должен двигаться
        {
            state = eFSState.pre;
            txt.enabled = false;//изначаьно скрыть число
        }
        else
        {
            if (u >= 1)// Если u>=l, выполняется движение
            {
                uC = 1;// Установить uC=l, чтобы не выйти за крайнюю точку
                state = eFSState.post;
                if (reportFinishTo != null)// Если игровой объект указан
                                           // использовать SendMessage для вызова метода FSCallback
                                           // и передачи ему текущего экземпляра в параметре.
                {
                    reportFinishTo.SendMessage("FSCallback", this);
                    // После отправки сообщения уничтожить gameObject
                    Destroy(gameObject);
                }
                else// Если не указан
                    // ...не уничтожать текущий экземпляр. Просто оставить его
                    // в покое.
                {
                    state = eFSState.idel;
                }
            }
            else
            {
                // Если 0<=и<1, значит, текущий экземпляр активен и движется
                state = eFSState.active;
                txt.enabled = true;//показать число очков
            }
            // Использовать кривую Безье для перемещения к заданной точке
            Vector2 pos = Utils.Bezier(uC, bezierPts);
            // Опорные точки RectTransform можно использовать для позиционирования
            // объектов пользовательского интерфейса относительно общего
            // размера экрана
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if (fontsizes != null && fontsizes.Count > 0)
            {
                // Если список fontsizes содержит значения
                //...скорректировать fontsize этого объекта GUIText
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontsizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
