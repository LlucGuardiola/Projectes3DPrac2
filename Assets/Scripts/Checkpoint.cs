using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    GameObject m_Player;

    private void Start()
    {
        m_Player = GameManager.GetGameManager().GetPlayer().gameObject;
    }

    
}
