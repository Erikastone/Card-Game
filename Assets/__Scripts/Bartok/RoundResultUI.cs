using TMPro;
using UnityEngine;

public class RoundResultUI : MonoBehaviour
{
    private TextMeshPro txt;
    private void Awake()
    {
        txt = GetComponent<TextMeshPro>();
        txt.text = "";
    }
    private void Update()
    {
        if (Bartok.S.phase != TurnPhase.gameOver)
        {
            txt.text = "";
            return;
        }
        // � ��� ����� �� ��������, ������ ����� ���� �����������
        Player cP = Bartok.CURRENT_PLAYER;
        if (cP == null || cP.type == PlayerType.human)
        {
            txt.text = "";
        }
        else
        {
            txt.text = "Player " + (cP.playerNum + " Won!");
        }
    }
}
