using UnityEngine;

interface IEnemyMoveable
{
    Rigidbody2D RB { get; set; }
    bool FacingRight { get; set; }

    void MoveEnemy(Vector2 velocity);
    void CheckForLeftOrRightFacing(Vector2 velocity);
}