using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager m_GameManager;
    private PlayerController m_Player;

    public Fade m_Fade;
    public GameObject m_GameOverUI;
    
    public AudioSource m_MusicSource;
    public AudioClip m_BackgroundMusic;

    private bool m_IsGameOver = false;

    private void Awake()
    {
        if (m_GameManager != null)
        {
            Destroy(gameObject);
            return;
        }
        if (m_MusicSource != null && m_BackgroundMusic != null)
        {
            if (!m_MusicSource.isPlaying)
            {
                m_MusicSource.clip = m_BackgroundMusic;
                m_MusicSource.loop = true;
                m_MusicSource.volume = 0.5f; 
                m_MusicSource.Play();
            }
        }

        m_GameManager = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
      
        if (!m_IsGameOver) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel(true);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            QuitGame();
        }
    }

    public static GameManager GetGameManager()
    {
        return m_GameManager;
    }

    public void RestartLevel(bool resetStats)
    {
        Time.timeScale = 1f;
        m_IsGameOver = false;

        if (m_GameOverUI != null)
            m_GameOverUI.SetActive(false);

        m_Player.Restart(resetStats);

        m_Fade.FadeOut(() =>
        {
            m_Fade.gameObject.SetActive(false);
        });
    }

    public void GameOver()
    {
        if (m_IsGameOver) return;

        m_IsGameOver = true;
        Time.timeScale = 0f; 

        if (m_GameOverUI != null)
            m_GameOverUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();

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
