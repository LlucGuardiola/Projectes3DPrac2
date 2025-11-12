using UnityEngine;

public class CompanionSpawner : MonoBehaviour
{
    public GameObject m_CompanionCubePrefab;
    public Transform m_SpawnerTransform;

    private bool m_PlayerInZone = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_PlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            m_PlayerInZone = false;
        }
    }

    private void Update()
    {
        if (m_PlayerInZone && Input.GetKeyDown(KeyCode.E))
        {
            Spawn();
        }
    }

    void Spawn()
    {
        var l_GameObject = Instantiate(m_CompanionCubePrefab);
        l_GameObject.transform.position = m_SpawnerTransform.position;
        l_GameObject.transform.rotation = m_SpawnerTransform.rotation;
        l_GameObject.transform.localScale = new Vector3(.75f, .75f, .75f);
    }
}
