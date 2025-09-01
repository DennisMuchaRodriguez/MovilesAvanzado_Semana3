using Unity.Netcode;
using UnityEngine;

public class NetworkEnemy : NetworkBehaviour
{
    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    private Transform targetPlayer;
    private GameManager gameManager;

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
    }

    void Update()
    {
        if (!IsServer) return;

        FindNearestPlayer();

        if (targetPlayer != null)
        {
            
            Vector3 direction = (targetPlayer.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    void FindNearestPlayer()
    {
        float closestDistance = Mathf.Infinity;
        targetPlayer = null;

        foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                float distance = Vector3.Distance(transform.position, client.PlayerObject.transform.position);
                if (distance < detectionRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    targetPlayer = client.PlayerObject.transform;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (other.CompareTag("Player"))
        {
            // Notificar al GameManager que este enemigo fue destruido
            if (gameManager != null)
            {
                gameManager.EnemyDestroyed(gameObject);
            }

            // Destruir el enemigo
            GetComponent<NetworkObject>().Despawn();
            Destroy(gameObject, 0.1f);
        }
    }
}