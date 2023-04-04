using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// Игрок может быть человеком или ИИ
public enum PlayerType
{
    human,
    ai
}
[System.Serializable]
public class Player
{
    public PlayerType type = PlayerType.ai;
    public int playerNum;
    public SlotDef handSlotDef;
    public List<CardBartok> hand;//карты в руках игрока
    //добавляем карту в руки
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null) hand = new List<CardBartok>();
        //добавить карту
        hand.Add(eCB);

        // Если это человек, отсортировать карты по достоинству с помощью LINQ
        if (type == PlayerType.human)
        {
            CardBartok[] cards = hand.ToArray();/*LINQ работает с массивами значений, поэтому мы создаем массив
CardBartok[] карт из списка List<CardBartok> hand.*/
            // Это вызов LINQ
            cards = cards.OrderBy(cd => cd.rank).ToArray();/*Эта строка — вызов LINQ, который обрабатывает массив карт. Он выполняет
обход элементов массива подобно циклу foreach(CardBartok cd in cards)
и сортирует их по значению rank (на что указывает аргумент cd => cd. rank).
Отсортированный массив присваивается переменной cards, затирая старый,
несортированный массив. Синтаксис LINQ отличается от обычного синтаксиса
языка С#, из-за чего первое время он может казаться вам странным.
Обратите внимание, что операции LINQ выполняются довольно медленно —
на выполнение одного вызова может потребоваться несколько миллисекунд,
но так как мы вызываем LINQ только один раз в каждом ходе, это не является
проблемой.*/
            hand = new List<CardBartok>(cards);/*После сортировки массива cards из него создается новый список карт и записывается
в hand взамен старого, несортированного списка.*/
            // Примечание: LINQ выполняет операции довольно медленно
            // (затрачивая по несколько миллисекунд), но так как
            // мы делаем это один раз за раунд, это не проблема.
        }
        eCB.SetSortingLayerName("10");// Перенести перемещаемую карту в верхний
                                      // слой
        eCB.eventualSortLayer = handSlotDef.layerName;
        FanHand();
        return eCB;
    }
    //удаляем карту из рук
    public CardBartok RemoveCard(CardBartok cb)
    {
        // Если список hand пуст или не содержит карты cb, вернуть null
        // if (hand == null || !hand.Contains(cb)) return null;
        hand.Remove(cb);
        FanHand();
        return cb;
    }
    public void FanHand()
    /*startRot — угол поворота первой карты относительно оси Z (наибольший
угол поворота против часовой стрелки). Первоначально получает значение,
указанное в BartokLayoutXML, а затем добавляется поворот против часовой
стрелки так, чтобы карты легли веером относительно центра в позиции
руки. После вычисления значения startRot каждая последующая карта поворачивается
относительно предыдущей на угол Bartok.S.handFanDegrees
по часовой стрелке.*/
    {
        // startRot - угол поворота первой карты относительно оси Z
        float startRot = 0;
        startRot = handSlotDef.rot;
        if (hand.Count > 1)
        {
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }
        // Переместить все карты в новые позиции
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);/*rotQ хранит экземпляр Quaternion, представляющий угол поворота относительно
оси Z.*/
            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;/*Далее вычисляется pos — вектор Vector3 с координатами точки, находящейся
на половину высоты карты над центром (то есть localPosition = [ 0, 0, 0 ])
для текущего игрока, соответственно, первоначально pos принимает значение
[О, 1.75,0].*/
            pos = rotQ * pos;
            // Прибавить координаты позиции руки игрока
            // (внизу в центре веера карт)
            pos += handSlotDef.pos;
            pos.z = -0.5f * i;

            // Если это не начальная раздача, начать перемещение карты немедленно,
            if (Bartok.S.phase != TurnPhase.idel)
            {
                hand[i].timeStart = 0;
            }

            // Установить локальную позицию и поворот i-й карты в руках
            hand[i].MoveTo(pos, rotQ);// Сообщить карте, что она должна начать
                                      // интерполяцию
            hand[i].state = CBState.toHand;
            // Закончив перемещение, карта запишет в поле state значение
            // CBState.hand

            /* hand[i].transform.localPosition = pos;
             hand[i].transform.rotation = rotQ;
             hand[i].state = CBState.hand;
            */
            hand[i].faceUp = (type == PlayerType.human);

            // Установить SortOrder карт, чтобы обеспечить правильное перекрытие
            hand[i].eventualSortOrder = i * 4;
        }
    }
    // Функция TakeTurn() реализует ИИ для игроков, управляемых компьютером
    public void TakeTurn()
    {
        Utils.tr("Player.TakeTurn");
        // Ничего не делать для игрока-человека.
        if (type == PlayerType.human) { return; }

        Bartok.S.phase = TurnPhase.waiting;
        CardBartok cb;
        // Если этим игроком управляет компьютер, нужно выбрать карту для хода
        // Найти допустимые ходы
        List<CardBartok> validCards = new List<CardBartok>();
        foreach (CardBartok tCB in hand)
        {
            if (Bartok.S.ValidPlay(tCB))
            {
                validCards.Add(tCB);
            }
        }
        // Если допустимых ходов нет
        if (validCards.Count == 0)
        {
            // ... взять карту
            cb = AddCard(Bartok.S.Draw());
            cb.callbackPlayer = this;
            return;
        }
        // Итак, у нас есть одна или несколько карт, которыми можно сыграть
        // теперь нужно выбрать одну из них
        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }
    public void CBCallback(CardBartok tCB)
    {
        Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        // Карта завершила перемещение, передать право хода
        Bartok.S.PassTurn();
    }
}
