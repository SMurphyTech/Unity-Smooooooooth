 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    public bool mobile = true, control = true;
    public float moveSpeed = 40f;
    public float moveSmooth = 0.1f;
    public float moveX, moveY;
    public bool m_FacingRight = true;
    [SerializeField] private float directionX;

    private Vector3 velocity = Vector3.zero;
    public Rigidbody2D rb;   
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetAxisRaw("Horizontal") != 0)
        {
            directionX = Input.GetAxisRaw("Horizontal");
        }

        if (mobile == true)
        {

            if (control)
            {
                moveX = Input.GetAxisRaw("Horizontal") * moveSpeed;
                moveY = Input.GetAxisRaw("Vertical") * moveSpeed;

                if (moveX > 0f && !m_FacingRight)
                {
                    // ... flip the player.                 
                        Flip();                  
                }
                // Otherwise if the input is moving the player left and the player is facing right...
                else if (moveX < 0f && m_FacingRight)
                {
                    // ... flip the player.                  
                        Flip();                   
                }
            }

            animator.SetFloat("Speed", Mathf.Abs(moveX));

            //moveX = moveX * Time.fixedDeltaTime;
            //moveY = moveY * Time.fixedDeltaTime;

            //actually moves player
            rb.velocity = Vector3.SmoothDamp(rb.velocity, new Vector2(moveX, moveY), ref velocity, moveSmooth);
            
        }
        else
        {
            //When mobile = false, the player is stopped
            moveX = 0;
            moveY = 0;
            rb.velocity = new Vector2(0f, 0f);
        }

        if (control)
        {
            if (Input.GetButtonDown("Dash") && GetComponent<Player_State>().state == Player_State.playerState.Idle)
            {
                GetComponent<Player_State>().changeState("dash");
                
                StartCoroutine("Dash");
            }
        }
        /*
        if (dashing)
        {
            rb.velocity = Vector3.SmoothDamp(rb.velocity, new Vector2(transform.localScale.x * moveSpeed, Input.GetAxisRaw("Vertical") * moveSpeed), ref velocity, moveSmooth);
        }
        */
    }

    void Flip()
    {
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    IEnumerator Dash()
    {
        //starts dashing
        
        moveSmooth = 0.05f;
        //goes forward
        moveX = Input.GetAxisRaw("Horizontal") * 35f;
        moveY = moveY = Input.GetAxisRaw("Vertical") * 35f;
        yield return new WaitForSeconds(.08f);
        //slides to a stop
        moveX = 0;
        moveY = 0;
        moveSmooth = 0.15f;
        yield return new WaitForSeconds(.08f);
        //stops dashing

        GetComponent<Player_State>().changeState("idle");
    }

    public void stopVelocity()
    {
        moveX = 0;
        moveY = 0;
    }

    //to cancel the dash coroutine if they dash into an enemy
    public void CancelDash()
    {
        StopCoroutine("Dash");
    }

    public float GetDirectionX()
    {
        return directionX;
    }
}
