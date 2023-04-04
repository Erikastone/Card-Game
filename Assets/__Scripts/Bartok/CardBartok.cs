/*/*using System.Collections.Generic;
using UnityEngine;
// CBState включает состояния игры и состояния to..., описывающие движения 
/*Перечисление CBState включает возможные состояния карты CardBartok
и состояния to..., представляющие разные этапы в течение воспроизведения
анимационного эффекта.*/
/*public enum CBState
{
    toDrawpile,
    drawpile,
    toHard,
    hand,
    toTarget,
    target,
    discard,
    to,
    idle
}
public class CardBartok : Card
{
    static public float MOVE_DURATION = 0.5f;
    static public string MOVE_EASING = Easing.InOut;
    static public float CARD_HEIGHT = 3.5f;
    static public float CARD_WIDTH = 2f;
    [Header("Set D")]
    public CBState state = CBState.drawpile;
    // Поля с информацией, необходимой для перемещения и поворачивания карты
    public List<Vector3> bezierPts;
    public List<Quaternion> beizerRots;
    public float timeStart, timeDuration;
    public int eventualSortOrder;
    public string eventualSortLayer;
    //по завершении перемещения карты будет вызыватся
    //reportFinishTo.SendMessage()
    public GameObject reportFinishTo = null;
    [System.NonSerialized]
    public Player callbackPlayer = null;
    // MoveTo запускает перемещение карты в новое местоположение с заданным
    // поворотом
    private void Awake()
    {
        callbackPlayer = null;
    }
    public void MoveTo(Vector3 ePos, Quaternion eRot)
    {
        // Создать новые списки для интерполяции.
        // Траектории перемещения и поворота определяются двумя точками каждая.
        bezierPts = new List<Vector3>();
        bezierPts.Add(transform.localPosition);// Текущее местоположение
        bezierPts.Add(ePos);//новое метостоположение

        beizerRots = new List<Quaternion>();
        beizerRots.Add(transform.rotation);// Текущий угол поворота
        /*Если переменная timestart хранит 0, записать в нее текущее время (чтобы
немедленно начать перемещение), иначе перемещение начнется в момент
timestart. То есть если прежде в переменную timestart было записано
значение, отличное от 0, это значение не затирается. Это позволит нам синхронизировать
разные анимационные эффекты.*/
/* beizerRots.Add(eRot);//новый

 if (timeStart == 0)
 {
     timeStart = Time.time;
 }
 //timeDuration всегда получает одно и        
 //то же значение, но потом это можно исправить
 timeDuration = MOVE_DURATION;/*Первоначально устанавливается состояние CBState.to. Позднее вызывающий
метод определит фактическое состояние, CBState.toHand или CBState.
toTarget*/
/* state = CBState.to;
}
public void MoveTo(Vector3 ePos)/*Перегруженная версия MoveTо (), которая не требует передачи угла поворота.*/
/*{
    MoveTo(ePos, Quaternion.identity);
}
private void Update()
{
    switch (state)/*Так как инструкция switch допускает возможность «проваливания» между
вариантами, если они не разделяются никаким дополнительным кодом, все
состояния to... (то есть toHand, toTarget и т. д.), когда карта перемещается
из одного местоположения в другое, обрабатываются одним и тем же блоком
кода.*/
/* {
     case CBState.toHard:
     case CBState.toTarget:
     case CBState.drawpile:
     case CBState.to:
         float u = (Time.time - timeStart) / timeDuration;/*Значение переменной float и интерполируется в диапазоне от 0 до 1 в процессе
перемещения этой карты CardBartok. Оно вычисляется как отношение
времени, прошедшего с момента timestart, к желаемой продолжительности
перемещения (например, если timestart = 5, timeDuration = 10 и Time.time
= 11, тогда u = (11-5) / 10 = 0.6). Затем полученное значение и передается в метод Easing. Ease(), в сценарии Utils,cs, для вычисления значения uC,
чтобы обеспечить более естественный эффект перемещения карты. Дополнительную
информацию вы найдете в разделе «Сглаживание для линейной
интерполяции» приложения Б.*/
/* float uC = Easing.Ease(u, MOVE_EASING);/*Обычно и принимает значения в диапазоне от 0 до 1. Здесь обрабатывается
ситуация, когда и < 0. В этом случае карта должна оставаться без движения
в своей текущий позиции. Случай и < 0 возможен, когда переменной timeStart
присвоено время в будущем, чтобы начать перемещение с задержкой.*/
/* if (u < 0)
 {
     transform.localPosition = bezierPts[0];
     transform.rotation = beizerRots[0];
     return;
 }
 else if (u >= 1)/*Если u >= 1, значение и нужно усечь до 1, чтобы карта не переместилась дальше
заданной точки. Также это значение соответствует моменту, когда движение
должно остановиться переключением в другое состояние CBState.*/
/*{
    uC = 1;
    //Перевести из состояния to... в соответствующее
    //сл состояние
    if (state == CBState.toHard) state = CBState.hand;
    if (state == CBState.toTarget) state = CBState.toTarget;
    //if (state == CBState.toDrawpile) state = CBState.drawpile;
    if (state == CBState.to) state = CBState.idle;
    // Переместить в конечное местоположение
    transform.localPosition = bezierPts[bezierPts.Count - 1];
    transform.rotation = beizerRots[bezierPts.Count - 1];
    //сбросить timeStart в 0, чтобы сл раз можно было установить текущее время
    timeStart = 0;
    if (reportFinishTo != null)/*Если задан игровой объект для обратного вызова, тогда с помощью
SendMessage() необходимо вызвать метод CBCallback с параметром this.
После вызова SendMessage() в reportFinishTo нужно записать null, чтобы
в будущем эта карта CardBartok не посылала сообщения о своем перемещении
этому же игровому объекту.*/
/* {
     reportFinishTo.SendMessage("CBCallback", this);
     reportFinishTo = null;
 }
 else if (callbackPlayer != null)
 {
     // Если имеется ссылка на экземпляр Player
     // Вызвать метод CBCallback этого экземпляра
     callbackPlayer.CBCallback(this);
     callbackPlayer = null;
 }
 else
 {
     // Если ничего вызывать не надо
     // Оставить все как есть.
 }
}
else/*Когда 0 <= u < 1, просто интерполировать перемещение из текущей точки
в следующую. Для вычисления новых координат используется функция
интерполяции вдоль кривой Безье. Вычисление нового местоположения
и нового угла поворота выполняется отдельно разными перегружеными
версиями метода Utils. Bezier(). Дополнительную информацию вы найдете
в разделе «Кривые Безье» приложения Б.*/
/* {
     // Нормальный режим интерполяции (0 <= и < 1)
     Vector3 pos = Utils.Bezier(uC, bezierPts);
     transform.localPosition = pos;
     Quaternion rotQ = Utils.Bezier(uC, beizerRots);
     transform.rotation = rotQ;
     if (u > 0.5f && spriteRenderers[0].sortingOrder != eventualSortOrder)
     {
         /* SpriteRenderer sRend = spriteRenderers[0];
          if (sRend.sortingOrder != eventualSortOrder)
          {
              // Установить конечный порядок сортировки
              SetSortOrder(eventualSortOrder);
          }
          if (sRend.sortingLayerName != eventualSortLayer)
          {
              // Установить конечный слой сортировки
              SetSortingLayerName(eventualSortLayer);
          }
         */
/*      SetSortOrder(eventualSortOrder);
  }
  if (u > 0.75f && spriteRenderers[0].sortingLayerName != eventualSortLayer)
  {
      SetSortingLayerName(eventualSortLayer);
  }
}
break;
}
}
// Этот метод определяет реакцию карты на щелчок мышью
public override void OnMouseUpAsButton()
{
// Вызвать метод CardClicked объекта-одиночки Bartok
Bartok.S.CardClicked(this);
base.OnMouseUpAsButton();
}*/

/*}*/

