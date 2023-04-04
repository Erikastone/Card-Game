using UnityEngine;
// ѕеречисление co всеми возможными событи€ми начислени€ очков
public enum eScoreEvent
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss

}
// ScoreManager управл€ет подсчетом очков
public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;
    static public int SCORE_FROM_PREV_ROUND = 0;
    public static int HIGH_SCORE = 0;
    [Header("Set Dynamically")]
    // ѕол€ дл€ хранени€ информации о заработанных очках
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;
    private void Awake()
    {
        if (S == null)
        {
            S = this;// ѕодготовка объекта-одиночки
        }
        else
        {
            Debug.LogError("EROR:ScoreManager.Awake():S is already set!");
        }
        // ѕроверить рекорд в PlayerPrefs
        if (PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }
        // ƒобавить очки, заработанные в последнем раунде
        // которые должны быть >0, если раунд завершилс€ победой
        score += SCORE_FROM_PREV_ROUND;
        // » —бросить SCOREFROMPREVROUND
        SCORE_FROM_PREV_ROUND = 0;
    }
    static public void EVENT(eScoreEvent evt)
    {
        try
        {
            // try-catch не позволит ошибке аварийно завершить программу
            S.Event(evt);

        }
        catch (System.NullReferenceException nre)
        {

            Debug.LogError("ScoreManager: EVENT() called while S = null.\n" + nre);
        }
    }
    void Event(eScoreEvent evt)
    {
        switch (evt)
        {
            //в случае победы, проигрыша и завершени€ хода
            //выполн€ютс€ одни и теже действи€
            case eScoreEvent.draw://выбор свободной карты               
            case eScoreEvent.gameWin://победа в раунде
            case eScoreEvent.gameLoss://проигрыш в раунде
                chain = 0;//сбросить цепочку подсчета очков
                score += scoreRun;//добавить scoreRun к общему числу очков
                scoreRun = 0;
                break;
            case eScoreEvent.mine:// ”даление карты из основной раскладки
                chain++;// увеличить количество очков в цепочке
                scoreRun += chain;// добавить очки за карту
                break;
        }
        // Ёта втора€ инструкци€ switch обрабатывает победу и проигрыш в раунде
        switch (evt)
        {
            case eScoreEvent.gameWin:
                // ¬ случае победы перенести очки в следующий раунд
                // статические пол€ Ќ≈ сбрасываютс€ вызовом
                // SceneManager.LoadScene()
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                break;
            case eScoreEvent.gameLoss:
                //в случае проигрыша сравнить с рекордом
                if (HIGH_SCORE <= score)
                {
                    print("You got the high score! High score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                }
                else
                {
                    print("Your final score for the game was: " + score);
                }
                break;
            default:
                print("score: " + score + " scoreRun:" + scoreRun + " chain:" + chain);
                break;
        }
    }
    static public int CHAIN { get { return S.chain; } }
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }
}
