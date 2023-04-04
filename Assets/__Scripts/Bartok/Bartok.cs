using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
// Это перечисление определяет разные этапы в течение одного игрового хода
public enum TurnPhase
{
    idel,
    pre,
    waiting,
    post,
    gameOver
}

public class Bartok : MonoBehaviour
{
    static public Bartok S;
    static public Player CURRENT_PLAYER;
    [Header("Set in I")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;
    public float handFanDegrees = 10f;/*handFanDegrees определяет угол поворота каждой карты относительно предыдущей
в одних руках.*/
    public int numStartingCards = 7;
    public float drawTimeStagger = 0.1f;
    [Header("Set D")]
    public Deck deck;
    public List<CardBartok> drawPile;
    public List<CardBartok> discardPile;
    public List<Player> players;
    public CardBartok targetCard;
    public TurnPhase phase = TurnPhase.idel;

    private BartokLayout layout;
    private Transform layoutAnchor;

    public GameObject turnLight;
    public GameObject GTGameOver;
    public GameObject GTRoundResult;
    private void Awake()
    {
        S = this;
        if (turnLight == null)
        {
            turnLight = GameObject.Find("TurnLight");
        }
        if (GTGameOver == null)
        {
            GTGameOver = GameObject.Find("GTGameOver");
        }
        if (GTRoundResult == null)
        {
            GTRoundResult = GameObject.Find("GTRoundResult");
        }
        GTGameOver.SetActive(false);
        GTRoundResult.SetActive(false);
    }
    private void Start()
    {
        deck = GetComponent<Deck>();// Получить компонент Deck
        deck.InitDeck(deckXML.text);// Передать ему DeckXML
        Deck.Shuffle(ref deck.cards);// Перетасовать колоду

        layout = GetComponent<BartokLayout>();// Получить ссылку на компонент
                                              // Layout
        layout.ReadLayout(layoutXML.text);// Передать ему LayoutXML
        drawPile = UpgradeCardsList(deck.cards);
        LayoutGame();
    }
    List<CardBartok> UpgradeCardsList(List<Card> lCD)
    {/*Этот метод приводит все карты в списке List<Card> 1CD к типу CardBartoks
и сохраняет их в новом списке List<CardBartok>. Он действует точно так же,
как аналогичный метод в классе Prospector, то есть сами карты изначально
имеют тип CardBartok, но таким способом мы явно сообщаем об этом движку
Unity.*/
        List<CardBartok> lCB = new List<CardBartok>();
        foreach (Card tCD in lCD)
        {
            lCB.Add(tCD as CardBartok);
        }
        return lCB;
    }
    // Позиционирует все карты в drawPile
    public void ArrangeDrawPile()
    {
        CardBartok tCB;
        for (int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.parent = layoutAnchor;
            tCB.transform.localPosition = layout.drawPile.pos;
            // Угол поворота начинается с 0
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4);// Упорядочить от первых к последним
            tCB.state = CBState.drawpile;
        }
    }
    // Выполняет первоначальную раздачу карт в игре
    void LayoutGame()
    {
        // Создать пустой GameObject - точку привязки для раскладки
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");/*layoutAnchor — это экземпляр Тransf orm, цель которого — служить родителем
всех карт в раскладке в панели Hierarchy (Иерархия). Здесь сначала создается
пустой игровой объект с именем _LayoutAnchor. Затем ссылка на компонент
TransformaToro игрового объекта присваивается полю layoutAnchor. Наконец,
layoutAnchor перемещается в позицию, определяемую полем layoutcenter*/
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }
        ArrangeDrawPile();
        //настроить игроков
        Player pl;
        players = new List<Player>();
        foreach (SlotDef tSD in layout.slotDefs)
        {
            pl = new Player();
            pl.handSlotDef = tSD;
            players.Add(pl);
            pl.playerNum = players.Count;
        }
        players[0].type = PlayerType.human;// 0-й игрок - человек
        CardBartok tCB;
        // Раздать игрокам по семь карт
        for (int i = 0; i < numStartingCards; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                tCB = Draw();// Снять карту
                             // Немного отложить начало перемещения карты
                tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
                players[(j + 1) % 4].AddCard(tCB);
            }
        }
        Invoke("DrawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
    }
    public void DrawFirstTarget()
    {
        // Перевернуть первую целевую карту лицевой стороной вверх
        CardBartok tCB = MoveToTarget(Draw());
        // Вызвать метод CBCallback сценария Bartok, когда карта закончит
        // перемещение
        tCB.reportFinishTo = this.gameObject;
    }
    // Этот обратный вызов используется последней розданной картой в начале игры
    public void CBCallback(CardBartok cb)
    {
        // Иногда желательно сообщить о вызове метода, как здесь
        Utils.tr("Batrok:CBCallback()", cb.name);
        StartGame();
    }
    public void StartGame()
    {
        // Право первого хода принадлежит игроку слева от человека
        PassTurn(1);
    }
    public void PassTurn(int num = -1)
    {
        // Если порядковый номер игрока не указан, выбрать следующего по кругу
        if (num == -1)
        {
            int ndx = players.IndexOf(CURRENT_PLAYER);
            num = (ndx + 1) % 4;
        }
        int lastPlayerNum = -1;
        if (CURRENT_PLAYER != null)
        {
            lastPlayerNum = CURRENT_PLAYER.playerNum;
            // Проверить завершение игры и необходимость перетасовать
            // стопку сброшенных карт
            if (CheckGameOver())
            {
                return;
            }
        }
        CURRENT_PLAYER = players[num];
        phase = TurnPhase.pre;
        CURRENT_PLAYER.TakeTurn();
        Vector3 lPos = CURRENT_PLAYER.handSlotDef.pos + Vector3.back * 5;
        turnLight.transform.position = lPos;
        // Сообщить о передаче хода
        Utils.tr("Batrok:PassTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
    }
    // ValidPlay проверяет возможность сыграть выбранной картой
    public bool ValidPlay(CardBartok cb)
    {
        // Картой можно сыграть, если она имеет такое же достоинство,
        // как целевая карта
        if (cb.rank == targetCard.rank) { return true; }
        // Картой можно сыграть, если ее масть совпадает с мастью целевой карты
        if (cb.suit == targetCard.suit)
        {
            return true;
        }
        return false;
    }
    // Делает указанную карту целевой
    public CardBartok MoveToTarget(CardBartok tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.faceUp = true;

        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if (targetCard != null)
        {
            MoveToDiscard(targetCard);
        }
        targetCard = tCB;
        return tCB;
    }
    public CardBartok MoveToDiscard(CardBartok tCB)
    {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;
        return tCB;
    }
    // Функция Draw снимает верхнюю карту со стопки свободных карт
    // и возвращает ее
    public CardBartok Draw()
    {
        CardBartok cd = drawPile[0];// Извлечь 0-ю карту
        /* if (drawPile.Count == 0)
         {
             // Если список drawPile опустел
             // нужно перетасовать сброшенные карты
             // и переложить их в стопку свободных карт
             int ndx;
             while (discardPile.Count > 0)
             {
                 // Вынуть случайную карту из стопки сброшенных карт
                 ndx = Random.Range(0, discardPile.Count);
                 drawPile.Add(discardPile[ndx]);
                 discardPile.RemoveAt(ndx);
             }
             ArrangeDrawPile();
             // Показать перемещение карт в стопку свободных карт
             float t = Time.time;
             foreach (CardBartok tCB in drawPile)
             {
                 tCB.transform.localPosition = layout.discardPile.pos;
                 tCB.callbackPlayer = null;
                 tCB.MoveTo(layout.discardPile.pos);
                 tCB.timeStart = t;
                 t += 0.02f;
                 tCB.state = CBState.toDrawpile;
                 tCB.eventualSortLayer = "0";
             }
         }
        */

        drawPile.RemoveAt(0);// Удалить ее из списка drawPile
        return cd;// и вернуть
    }
    public void CardClicked(CardBartok tCB)
    {
        if (CURRENT_PLAYER.type != PlayerType.human) return;
        if (phase == TurnPhase.waiting) return;
        switch (tCB.state)
        {
            case CBState.drawpile:
                // Взять верхнюю карту, не обязательно ту,
                // по которой выполнен щелчок.
                CardBartok cb = CURRENT_PLAYER.AddCard(Draw());
                cb.callbackPlayer = CURRENT_PLAYER;
                Utils.tr("Bartok:CardClicked()", "Draw", cb.name);
                phase = TurnPhase.waiting;
                break;
            case CBState.hand:
                // Проверить допустимость выбранной карты
                if (ValidPlay(tCB))
                {
                    CURRENT_PLAYER.RemoveCard(tCB);
                    MoveToTarget(tCB);
                    tCB.callbackPlayer = CURRENT_PLAYER;
                    Utils.tr("Bartok:CardClicked()", "Play", tCB.name, targetCard.name + " is target");
                    phase = TurnPhase.waiting;
                }
                else
                {
                    // Игнорировать выбор недопустимой карты,
                    // но сообщить о попытке игрока
                    Utils.tr("Bartok:CardClicked()", "Attempted to Play", tCB.name, targetCard.name + " is target");
                }
                break;
        }
    }
    public bool CheckGameOver()
    {
        // Проверить, нужно ли перетасовать стопку сброшенных карт и
        // перенести ее в стопку свободных карт
        if (drawPile.Count == 0)
        {
            List<Card> cards = new List<Card>();
            foreach (CardBartok cb in discardPile)
            {
                cards.Add(cb);
            }
            discardPile.Clear();
            Deck.Shuffle(ref cards);
            drawPile = UpgradeCardsList(cards);
            ArrangeDrawPile();
        }
        // Проверить победу текущего игрока
        if (CURRENT_PLAYER.hand.Count == 0)
        {
            if (CURRENT_PLAYER.type == PlayerType.human)
            {
                GTGameOver.GetComponent<TextMeshPro>().text = "You won!";
                GTRoundResult.GetComponent<TextMeshPro>().text = "";
            }
            else
            {
                GTGameOver.GetComponent<TextMeshPro>().text = "Game over!!";
                GTRoundResult.GetComponent<TextMeshPro>().text = "Player" + CURRENT_PLAYER.playerNum + " won";
            }
            GTGameOver.SetActive(true);
            GTRoundResult.SetActive(true);
            // Игрок, только что сделавший ход, победил!
            phase = TurnPhase.gameOver;
            Invoke("RestartGame", 1);
            return true;
        }
        return false;
    }
    public void RestartGame()
    {
        CURRENT_PLAYER = null;
        SceneManager.LoadScene("SampleScene");
    }
    /* private void Update()
     {
         if (Input.GetKeyDown(KeyCode.Alpha1))
         {
             players[0].AddCard(Draw());
         }
         if (Input.GetKeyDown(KeyCode.Alpha2))
         {
             players[1].AddCard(Draw());
         }
         if (Input.GetKeyDown(KeyCode.Alpha3))
         {
             players[2].AddCard(Draw());
         }
         if (Input.GetKeyDown(KeyCode.Alpha4))
         {
             players[3].AddCard(Draw());
         }
     }
    */
}

