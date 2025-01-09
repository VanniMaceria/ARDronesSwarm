using UnityEngine;

public class UIFollowPlayer : MonoBehaviour
{
    public Transform player;
    public Vector3 offset;

    private void LateUpdate()
    {
        transform.position = player.position + player.forward * offset.z 
                                                 + player.up * offset.y 
                                                 + player.right * offset.x;
        transform.LookAt(player);
        // Aggiungi la rotazione per correggere la visualizzazione specchiata.
        transform.rotation *= Quaternion.Euler(0, 180, 0);
    }
}