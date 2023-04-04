using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shell : MonoBehaviour {
    #region Variables movent shell
    float inputX;
    float impulse;

    // Control velocity
    const float maxVelocity = 12f;
    const float acceleration = 150f;

    // Control raycast
    const float raySize = .3f;
    const float rayLenght = .7f;

    // Control movent shell
    bool isMoving;

    // component rb
    private Rigidbody2D rbShell;
    public Transform raycast;
    #endregion

    #region Public access methods for move the shell
    public bool IsMove() {
        return this.isMoving;
    }

    public void IsMove(bool moving) {
        this.isMoving = moving;
    }
    #endregion

    private void Awake() {
        rbShell = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate() {
        if (rbShell != null) {
            inputX = rbShell.velocity.x;

            #region Direction shell and can move
            rbShell.AddForce(
                new Vector2((inputX < 0)
                ? -acceleration : (inputX > 0)
                ? acceleration : 0, 0)
            );
            
            if (inputX != 0) {
                isMoving = true;
                impulse = inputX;
            } else {
                isMoving = false;
            }
            #endregion

            #region Max velocity
            Vector2 velocity = rbShell.velocity;
            velocity.x = Mathf.Clamp(velocity.x, -maxVelocity, maxVelocity);
            rbShell.velocity = velocity;
            #endregion

            #region Position raycast
            Vector2 startRaycastPos = new Vector2(raycast.position.x, raycast.position.y + raySize);

            Vector2 rightRaycastPos = new Vector2(raycast.position.x + rayLenght, raycast.position.y + raySize);
            Vector2 leftRaycastPos = new Vector2(raycast.position.x - rayLenght, raycast.position.y + raySize);
            #endregion

            #region Hits raycast
            RaycastHit2D[] rightHitRaycast = Physics2D.LinecastAll(startRaycastPos, rightRaycastPos);
            RaycastHit2D[] leftHitRaycast = Physics2D.LinecastAll(startRaycastPos, leftRaycastPos);

            Debug.DrawLine(startRaycastPos, rightRaycastPos, Color.red);
            Debug.DrawLine(startRaycastPos, leftRaycastPos, Color.red);
            #endregion

            #region Elements of raycast rigth and left
            foreach (RaycastHit2D hit in rightHitRaycast) {
                Collider2D collider = hit.collider;

                if (collider != null && collider.CompareTag("Wall")) {
                    rbShell.velocity = new Vector2(-Mathf.Abs(rbShell.velocity.x), rbShell.velocity.y);
                    rbShell.AddForce(new Vector2(maxVelocity, 0));
                }
            }

            foreach (RaycastHit2D hit in leftHitRaycast) {
                Collider2D collider = hit.collider;

                if (collider != null && collider.CompareTag("Wall")) {
                    rbShell.velocity = new Vector2(Mathf.Abs(rbShell.velocity.x), rbShell.velocity.y);
                    rbShell.AddForce(new Vector2(maxVelocity, 0));
                }
            }
            #endregion
        }
    }
}