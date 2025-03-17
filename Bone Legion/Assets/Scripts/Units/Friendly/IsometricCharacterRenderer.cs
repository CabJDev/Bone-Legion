// Author: Hope Oberez (H.O)
using UnityEngine;

// Handle unit animations
public class IsometricCharacterRenderer : MonoBehaviour
{
    public static readonly string[] staticDirections = { "Static N", "Static NW", "Static W", "Static SW", "Static S", "Static SE", "Static E" , "Static NE"};
    public static readonly string[] walkDirections = { "Walk N", "Walk NW", "Walk W", "Walk SW", "Walk S", "Walk SE", "Walk E", "Walk NE" };

    Animator animator;
    int lastDirection;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetDirection(Vector3 direction)
    {

        string[] directionArray = null;

        if (direction.magnitude < .01f) // idle
        {
            directionArray = staticDirections;
        }
        else // walk
        {
            directionArray = walkDirections;
            lastDirection = DirectionToIndex(direction, 8);
        }
        animator.Play(directionArray[lastDirection]);

    }

    // Divides into the 8 directions
    public static int DirectionToIndex(Vector2 dir, int sliceCount)
    {
        Vector2 normDir = dir.normalized;
        float step = 360f /sliceCount;
        float halfstep = step / 2;
        float angle = Vector2.SignedAngle(Vector2.up, normDir);
        angle += halfstep;

        if (angle < 0)
        {
            angle += 360;
        }

        float stepCount = angle / step;

        return Mathf.FloorToInt(stepCount);
    }
}
