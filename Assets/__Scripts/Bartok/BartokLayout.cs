using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class SlotDef
{
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();// He используетс€ в Bartok
    public float rot;// поворот в зависимости от игрока
    public string type = "slot";
    public Vector2 stagger;
    public int player;// пор€дковый номер игрока
    public Vector3 pos;// вычисл€етс€ на основе х, у и multiplier

}
public class BartokLayout : MonoBehaviour
{
    [Header("Set D")]
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;// —мещение в раскладке
    // —сылки на SlotDef
    public List<SlotDef> slotDefs;
    public SlotDef drawPile;
    public SlotDef discardPile;
    public SlotDef target;
    // Ётот метод вызываетс€ дл€ чтени€ файла BartokLayoutXML.xml
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);// «агрузить XML
        xml = xmlr.xml["xml"][0];// » определить xml дл€ ускорени€ доступа к XML
                                 // ѕрочитать множители, определ€ющие рассто€ние между картами
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));
        //прочитать слоты
        SlotDef tSD;
        // slotsX используетс€ дл€ ускорени€ доступа к элементам <slot>
        PT_XMLHashList slotsX = xml["slot"];
        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef();// —оздать новый экземпл€р SlotDef
            if (slotsX[i].HasAtt("type"))
            {
                // ≈сли <slot> имеет атрибут type, прочитать его
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                // »наче определить тип как "slot"; это отдельна€ карта в р€ду
                tSD.type = "slot";
            }
            // ѕреобразовать некоторые атрибуты в числовые значени€
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);
            // —лои сортировки
            /*¬ этой игре сло€м сортировки присваиваютс€ имена 1, 2, 3, ... 10. —лои гарантируют
            правильное перекрытие одних карт другими. ¬ проектах Unity
            двумерных игр все ресурсы получают одну и ту же координату Z, поэтому
            дл€ их упор€дочени€ по глубине используютс€ слои сортировки.*/
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();/*ѕреобразование числа layerlD в текст с записью в layerName*/
            // ѕрочитать дополнительные атрибуты, опира€сь на тип слота
            switch (tSD.type)
            {
                case "slot":
                    // игнорировать слоты с типом "slot"
                    break;
                case "drawpile":
                    /*јтрибут xstagger дл€ стопки свободных карт все еще извлекаетс€ из XML-
файла, но его значение не используетс€ в Bartok, потому что игрокам не
требуетс€ знать количество оставшихс€ свободных карт.*/
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
                case "target":
                    target = tSD;
                    break;
                case "hand":
                    /*Ётот фрагмент извлекает сведени€ о картах в руках каждого игрока в отдельности,
включа€ угол поворота и пор€дковый номер игрока*/
                    tSD.player = int.Parse(slotsX[i].att("player"));
                    tSD.rot = float.Parse(slotsX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }

}
