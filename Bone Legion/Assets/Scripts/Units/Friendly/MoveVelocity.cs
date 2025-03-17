// Author: Hope Oberez (H.O)
using UnityEngine;

// Handle unit movement velocity
public class MoveVelocity : MonoBehaviour
{
    [SerializeField] private float moveSpeed;

    private Vector3 velocityVector;
    private IsometricCharacterRenderer isoRenderer;

    private void Awake()
    {
       isoRenderer = GetComponentInChildren<IsometricCharacterRenderer>();
    }

    public void SetVelocity(Vector3 velocityVector)
    {
        this.velocityVector = velocityVector;
    }

    private void Update()
    {
        Vector3 position = transform.position;
        position += velocityVector * moveSpeed * 10 * Time.deltaTime;
        position.z = position.y;

        transform.position = position;

        isoRenderer.SetDirection(velocityVector);
    }
}