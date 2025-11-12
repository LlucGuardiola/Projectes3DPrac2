using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    private static GameManager m_GameManager;
    PlayerController m_Player;
    //public Transform m_DestroyObjects;
    public Fade m_Fade;
    //public PlayerHUD hud;


    private void Awake()
    {
        if (m_GameManager != null)
        {
            Destroy(gameObject);
            return;
        }

        m_GameManager = this;
        DontDestroyOnLoad(gameObject);
    }
    private void Update()
    {
       
    }
    public static GameManager GetGameManager()
    {
        return m_GameManager;
    }
    public void RestartLevel(bool resetStats)
    {
        /*for (int i = 0; i < m_DestroyObjects.childCount; i++)
        {
            Destroy(m_DestroyObjects.GetChild(i).gameObject);
        }*/

        m_Player.Restart(resetStats);
        m_Fade.FadeOut(() =>
        {
            m_Fade.gameObject.SetActive(false);
        });
    }
    public PlayerController GetPlayer()
    {
        return m_Player;
    }
    public void SetPlayer(PlayerController Player)
    {
        m_Player = Player;
    }
}
