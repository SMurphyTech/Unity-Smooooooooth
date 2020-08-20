using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Health : MonoBehaviour
{
    public int health;
    public GameObject Attacker;
    int kbx, kby;
    public Animator animator;
    public Sprite blank_sprite;
    private bool flashing;

    // Start is called before the first frame update
    void Start()
    {
        health = 20;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //Debug.Log(health);

        if (health <= 0)
        {
            Die();
        }
        
        //updates the invincible variable in the animator to tell when the invincible animations should be played
        //animator.SetBool("Invincible", gameObject.layer == 10);
    }

    public static void Die()
    {
        SceneManager.LoadScene("SampleScene");
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        /*
        if (collision.gameObject.tag == "Enemy")
        {            
            health -= 1;
            Debug.Log(health);
            Attacker = collision.gameObject;

            //knockback speed for x and y
            kbx = 0;
            kby = 0;

            //determines which part of the hitbox the player collided with, and determines direction of knockback acordingly
            if (collision.GetContact(collision.contactCount - 1).point.y >= collision.collider.bounds.center.y + collision.collider.bounds.extents.y)
            {
                //top collision
                kby = 1;
                checkKnockbackX();
            }
            else if (collision.GetContact(collision.contactCount - 1).point.y <= collision.collider.bounds.center.y - collision.collider.bounds.extents.y)
            {
                //bottom collision
                kby = -1;
                checkKnockbackX();
            }

            if (collision.GetContact(collision.contactCount - 1).point.x >= collision.collider.bounds.center.x + collision.collider.bounds.extents.x)
            {
                //right collision
                kbx = 1;
                checkKnockbackY();
            }
            else if (collision.GetContact(collision.contactCount - 1).point.x <= collision.collider.bounds.center.x - collision.collider.bounds.extents.x)
            {
                //left collision
                kbx = -1;
                checkKnockbackY();
            }

            

            //applies the knockback
            hitstun = true;
            animator.SetTrigger("Hit");

            StartCoroutine("Knockback");
        }
        */

        if(collision.gameObject.tag == "EnemyAttack" || collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Hazard")
        {
            //decreases health by one (will change later to give enemies variable damage)
            health -= 1;
            Debug.Log(health);           

            //puts player on the "invincible" layer, making them unable to collide with any enemy
            gameObject.layer = 10;
            //starts invincibility timer
            StartCoroutine("Invincibility");

            //saves the enemy the player collided with to a variable
            Attacker = collision.gameObject;

            //resets knockback direction variables
            kbx = 0;
            kby = 0;

            if (collision.gameObject.tag == "EnemyAttack" || collision.gameObject.tag == "Enemy")
            {
                //determines which side of the enemies' hitbox the player hit and defines the direction of the knockback applied in the "knockbock" coroutine
                Vector3 closest = GetComponent<BoxCollider2D>().bounds.ClosestPoint(collision.collider.bounds.center);
                if (closest.x > collision.collider.bounds.center.x)
                {
                    kbx = 1;
                }
                else if (closest.x < collision.collider.bounds.center.x)
                {
                    kbx = -1;
                }
                if (closest.y > collision.collider.bounds.center.y)
                {
                    kby = 1;
                }
                else if (closest.y < collision.collider.bounds.center.y)
                {
                    kby = -1;
                }
            }
            else if (collision.gameObject.tag == "Hazard")
            {
                if (Attacker.transform.position.x > collision.collider.bounds.center.x)
                {
                    kbx = 1;
                }
                else if (Attacker.transform.position.x < collision.collider.bounds.center.x)
                {
                    kbx = -1;
                }
                if (Attacker.transform.position.y > collision.collider.bounds.center.y)
                {
                    kby = 1;
                }
                else if (Attacker.transform.position.y < collision.collider.bounds.center.y)
                {
                    kby = -1;
                }
            }

            //changes animation to hurt animation
            animator.SetTrigger("Hit");

            //flips player if they are attacked from behind
            if(Attacker.transform.position.x < transform.position.x && transform.localScale.x == 1)
            {
                AttackFromBehind(-1);
            }
            else if (Attacker.transform.position.x > transform.position.x && transform.localScale.x == -1)
            {
                AttackFromBehind(1);
            }

            //cancels attack if the player was hit during one
            if (GetComponent<Player_State>().state == Player_State.playerState.Attacking)
            {
                GetComponent<Player_Attack>().stopMidAttack();
            }

            //cancels dash if the player was hit during one
            if (GetComponent<Player_State>().state == Player_State.playerState.Dashing)
            {
                GetComponent<Player_Move>().CancelDash();
            }

            GetComponent<Player_State>().changeState("hitstun");

            //applies the knockback
            StartCoroutine("Knockback");
        }

    }

    IEnumerator Knockback()
    {
        //freezes the player for a time   
        GetComponent<Player_Move>().stopVelocity();
        GetComponent<Player_Move>().moveSmooth = 0f;
        GetComponent<Player_Move>().mobile = false;

        yield return new WaitForSeconds(0.08f);

        GetComponent<Player_Move>().mobile = true;
        //applies initial knockback for .05 seconds
        GetComponent<Player_Move>().moveX = 30f * kbx;
        GetComponent<Player_Move>().moveY = 30f * kby;

        yield return new WaitForSeconds(.05f);

        //sliding period
        GetComponent<Player_Move>().stopVelocity();
        GetComponent<Player_Move>().moveSmooth = 0.15f;

        yield return new WaitForSeconds(.2f);

        //resets variables to pre-hit state
        animator.SetTrigger("HitOver");

        //Triggers the invincibilty flashing
        StartCoroutine("InvincibilityFlash");

        GetComponent<Player_State>().changeState("idle");
    }

    //currently unused
    void checkKnockbackX()
    {
        if(GetComponent<Player_Move>().moveX > 0)
        {
            kbx = -1;
        }
        else if(GetComponent<Player_Move>().moveX < 0)
        {
            kbx = 1;
        }
    }

    //currently unused
    void checkKnockbackY()
    {
        if (GetComponent<Player_Move>().moveY > 0)
        {
            kby = -1;
        }
        else if (GetComponent<Player_Move>().moveY < 0)
        {
            kby = 1;
        }
    }

    IEnumerator Invincibility()
    {
        yield return new WaitForSeconds(2.5f);
        
        gameObject.layer = 8;
    }

    IEnumerator InvincibilityFlash()
    {
        for (int i = 0; i < 16; i++)
        {
            flashing = true;
           
            //Debug.Log("Flash");
            yield return new WaitForSeconds(.0625f);

            flashing = false;

            yield return new WaitForSeconds(.0625f);
        }
    }

    //used to make the player sprite a blank after the animator updates
    void LateUpdate()
    {
        if (flashing == true)
        {
            GetComponent<SpriteRenderer>().sprite = blank_sprite;
        }        
    }

    void AttackFromBehind(int dir)
    {
        //changes a variable in Player_Move to stop controls from being reversed
        GetComponent<Player_Move>().m_FacingRight = !GetComponent<Player_Move>().m_FacingRight;

        Vector3 theScale = transform.localScale;
        theScale.x = dir;
        transform.localScale = theScale;
    }
}
