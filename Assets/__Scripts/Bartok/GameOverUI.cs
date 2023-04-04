using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
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
        // В эту точку мы попадаем, только когда игра завершилась
        if (Bartok.CURRENT_PLAYER == null)
        {
            return;
        }
        if (Bartok.CURRENT_PLAYER.type == PlayerType.human)
        {
            txt.text = "You Won!";
        }
        else
        {
            txt.text = "Game Over!";
        }
    }
}
