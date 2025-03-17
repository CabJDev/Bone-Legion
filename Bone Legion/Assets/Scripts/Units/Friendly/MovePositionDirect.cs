// Author: Hope Oberez (H.O)
using UnityEngine;

// Handle unit postioning
public class MovePositionDirect : MonoBehaviour
{
    private Vector3 movePosition;

    private void Awake()
    {
        movePosition = transform.position;
    }

    public void SetMovePosition(Vector3 movePosition)
    {
        this.movePosition = movePosition;
    }

    void Update()
    {
        Vector3 moveDir = (movePosition - transform.position).normalized;
        if (Vector3.Distance(movePosition, transform.position) < 1f) moveDir = Vector3.zero;

        moveDir.z = 0;
        GetComponent<MoveVelocity>().SetVelocity(moveDir);
    }
}
