using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// ����� ����� ���� ��������� ��� ��
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
    public List<CardBartok> hand;//����� � ����� ������
    //��������� ����� � ����
    public CardBartok AddCard(CardBartok eCB)
    {
        if (hand == null) hand = new List<CardBartok>();
        //�������� �����
        hand.Add(eCB);

        // ���� ��� �������, ������������� ����� �� ����������� � ������� LINQ
        if (type == PlayerType.human)
        {
            CardBartok[] cards = hand.ToArray();/*LINQ �������� � ��������� ��������, ������� �� ������� ������
CardBartok[] ���� �� ������ List<CardBartok> hand.*/
            // ��� ����� LINQ
            cards = cards.OrderBy(cd => cd.rank).ToArray();/*��� ������ � ����� LINQ, ������� ������������ ������ ����. �� ���������
����� ��������� ������� ������� ����� foreach(CardBartok cd in cards)
� ��������� �� �� �������� rank (�� ��� ��������� �������� cd => cd. rank).
��������������� ������ ������������� ���������� cards, ������� ������,
��������������� ������. ��������� LINQ ���������� �� �������� ����������
����� �#, ��-�� ���� ������ ����� �� ����� �������� ��� ��������.
�������� ��������, ��� �������� LINQ ����������� �������� �������� �
�� ���������� ������ ������ ����� ������������� ��������� �����������,
�� ��� ��� �� �������� LINQ ������ ���� ��� � ������ ����, ��� �� ��������
���������.*/
            hand = new List<CardBartok>(cards);/*����� ���������� ������� cards �� ���� ��������� ����� ������ ���� � ������������
� hand ������ �������, ���������������� ������.*/
            // ����������: LINQ ��������� �������� �������� ��������
            // (���������� �� ��������� �����������), �� ��� ���
            // �� ������ ��� ���� ��� �� �����, ��� �� ��������.
        }
        eCB.SetSortingLayerName("10");// ��������� ������������ ����� � �������
                                      // ����
        eCB.eventualSortLayer = handSlotDef.layerName;
        FanHand();
        return eCB;
    }
    //������� ����� �� ���
    public CardBartok RemoveCard(CardBartok cb)
    {
        // ���� ������ hand ���� ��� �� �������� ����� cb, ������� null
        // if (hand == null || !hand.Contains(cb)) return null;
        hand.Remove(cb);
        FanHand();
        return cb;
    }
    public void FanHand()
    /*startRot � ���� �������� ������ ����� ������������ ��� Z (����������
���� �������� ������ ������� �������). ������������� �������� ��������,
��������� � BartokLayoutXML, � ����� ����������� ������� ������ �������
������� ���, ����� ����� ����� ������ ������������ ������ � �������
����. ����� ���������� �������� startRot ������ ����������� ����� ��������������
������������ ���������� �� ���� Bartok.S.handFanDegrees
�� ������� �������.*/
    {
        // startRot - ���� �������� ������ ����� ������������ ��� Z
        float startRot = 0;
        startRot = handSlotDef.rot;
        if (hand.Count > 1)
        {
            startRot += Bartok.S.handFanDegrees * (hand.Count - 1) / 2;
        }
        // ����������� ��� ����� � ����� �������
        Vector3 pos;
        float rot;
        Quaternion rotQ;
        for (int i = 0; i < hand.Count; i++)
        {
            rot = startRot - Bartok.S.handFanDegrees * i;
            rotQ = Quaternion.Euler(0, 0, rot);/*rotQ ������ ��������� Quaternion, �������������� ���� �������� ������������
��� Z.*/
            pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;/*����� ����������� pos � ������ Vector3 � ������������ �����, �����������
�� �������� ������ ����� ��� ������� (�� ���� localPosition = [ 0, 0, 0 ])
��� �������� ������, ��������������, ������������� pos ��������� ��������
[�, 1.75,0].*/
            pos = rotQ * pos;
            // ��������� ���������� ������� ���� ������
            // (����� � ������ ����� ����)
            pos += handSlotDef.pos;
            pos.z = -0.5f * i;

            // ���� ��� �� ��������� �������, ������ ����������� ����� ����������,
            if (Bartok.S.phase != TurnPhase.idel)
            {
                hand[i].timeStart = 0;
            }

            // ���������� ��������� ������� � ������� i-� ����� � �����
            hand[i].MoveTo(pos, rotQ);// �������� �����, ��� ��� ������ ������
                                      // ������������
            hand[i].state = CBState.toHand;
            // �������� �����������, ����� ������� � ���� state ��������
            // CBState.hand

            /* hand[i].transform.localPosition = pos;
             hand[i].transform.rotation = rotQ;
             hand[i].state = CBState.hand;
            */
            hand[i].faceUp = (type == PlayerType.human);

            // ���������� SortOrder ����, ����� ���������� ���������� ����������
            hand[i].eventualSortOrder = i * 4;
        }
    }
    // ������� TakeTurn() ��������� �� ��� �������, ����������� �����������
    public void TakeTurn()
    {
        Utils.tr("Player.TakeTurn");
        // ������ �� ������ ��� ������-��������.
        if (type == PlayerType.human) { return; }

        Bartok.S.phase = TurnPhase.waiting;
        CardBartok cb;
        // ���� ���� ������� ��������� ���������, ����� ������� ����� ��� ����
        // ����� ���������� ����
        List<CardBartok> validCards = new List<CardBartok>();
        foreach (CardBartok tCB in hand)
        {
            if (Bartok.S.ValidPlay(tCB))
            {
                validCards.Add(tCB);
            }
        }
        // ���� ���������� ����� ���
        if (validCards.Count == 0)
        {
            // ... ����� �����
            cb = AddCard(Bartok.S.Draw());
            cb.callbackPlayer = this;
            return;
        }
        // ����, � ��� ���� ���� ��� ��������� ����, �������� ����� �������
        // ������ ����� ������� ���� �� ���
        cb = validCards[Random.Range(0, validCards.Count)];
        RemoveCard(cb);
        Bartok.S.MoveToTarget(cb);
        cb.callbackPlayer = this;
    }
    public void CBCallback(CardBartok tCB)
    {
        Utils.tr("Player.CBCallback()", tCB.name, "Player " + playerNum);
        // ����� ��������� �����������, �������� ����� ����
        Bartok.S.PassTurn();
    }
}
