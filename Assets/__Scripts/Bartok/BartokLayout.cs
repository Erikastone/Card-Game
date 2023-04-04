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
    public List<int> hiddenBy = new List<int>();// He ������������ � Bartok
    public float rot;// ������� � ����������� �� ������
    public string type = "slot";
    public Vector2 stagger;
    public int player;// ���������� ����� ������
    public Vector3 pos;// ����������� �� ������ �, � � multiplier

}
public class BartokLayout : MonoBehaviour
{
    [Header("Set D")]
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;// �������� � ���������
    // ������ �� SlotDef
    public List<SlotDef> slotDefs;
    public SlotDef drawPile;
    public SlotDef discardPile;
    public SlotDef target;
    // ���� ����� ���������� ��� ������ ����� BartokLayoutXML.xml
    public void ReadLayout(string xmlText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(xmlText);// ��������� XML
        xml = xmlr.xml["xml"][0];// � ���������� xml ��� ��������� ������� � XML
                                 // ��������� ���������, ������������ ���������� ����� �������
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));
        //��������� �����
        SlotDef tSD;
        // slotsX ������������ ��� ��������� ������� � ��������� <slot>
        PT_XMLHashList slotsX = xml["slot"];
        for (int i = 0; i < slotsX.Count; i++)
        {
            tSD = new SlotDef();// ������� ����� ��������� SlotDef
            if (slotsX[i].HasAtt("type"))
            {
                // ���� <slot> ����� ������� type, ��������� ���
                tSD.type = slotsX[i].att("type");
            }
            else
            {
                // ����� ���������� ��� ��� "slot"; ��� ��������� ����� � ����
                tSD.type = "slot";
            }
            // ������������� ��������� �������� � �������� ��������
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.pos = new Vector3(tSD.x * multiplier.x, tSD.y * multiplier.y, 0);
            // ���� ����������
            /*� ���� ���� ����� ���������� ������������� ����� 1, 2, 3, ... 10. ���� �����������
            ���������� ���������� ����� ���� �������. � �������� Unity
            ��������� ��� ��� ������� �������� ���� � �� �� ���������� Z, �������
            ��� �� ������������ �� ������� ������������ ���� ����������.*/
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            tSD.layerName = tSD.layerID.ToString();/*�������������� ����� layerlD � ����� � ������� � layerName*/
            // ��������� �������������� ��������, �������� �� ��� �����
            switch (tSD.type)
            {
                case "slot":
                    // ������������ ����� � ����� "slot"
                    break;
                case "drawpile":
                    /*������� xstagger ��� ������ ��������� ���� ��� ��� ����������� �� XML-
�����, �� ��� �������� �� ������������ � Bartok, ������ ��� ������� ��
��������� ����� ���������� ���������� ��������� ����.*/
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
                    /*���� �������� ��������� �������� � ������ � ����� ������� ������ � �����������,
������� ���� �������� � ���������� ����� ������*/
                    tSD.player = int.Parse(slotsX[i].att("player"));
                    tSD.rot = float.Parse(slotsX[i].att("rot"));
                    slotDefs.Add(tSD);
                    break;
            }
        }
    }

}
