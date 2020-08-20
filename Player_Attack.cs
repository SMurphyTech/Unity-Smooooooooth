using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Attack : MonoBehaviour
{
    public GameObject swordBox;
    private GameObject instanceOfSword;
    public Animator animator;
    public int mashlevel = 0;
    bool holding = false;
    public enum attackState{
        Idle,
        Normal,
        Mash,
        Dash,
        Charging,
        Smash
    };
    public attackState currentState = attackState.Idle;
    public float knockback;
    public int damage;
    //amount of time the enemy is frozen before knockback is dealt
    public float freezetime;
    //tells if the attack button has been held long enough to trigger a smash attack
    private bool smashAttackCharged = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //updates the position of the attackhitbox with the player
        if (currentState != attackState.Smash)
        {
            updateHitbox();
        }

        //Debug.Log(animator.enabled);

        animator.SetInteger("AttackLevel", mashlevel);
        animator.SetBool("DashAttacking", currentState == attackState.Dash);
        animator.SetBool("NuetralAttack", currentState == attackState.Normal);
        animator.SetBool("Charged", smashAttackCharged);

        if (Input.GetButtonDown("Attack") && GetComponent<Player_State>().state != Player_State.playerState.Hitstun)
        {

            //stops player's momentum
            GetComponent<Player_Move>().stopVelocity();
            GetComponent<Player_Move>().moveSmooth = 0.2f;

            //decides what attack to perform based on player's condition
            decideAttack();

            GetComponent<Player_State>().changeState("attack");

            //tells that the player is holding the attack button
            holding = true;

            //StartCoroutine("Attack");

            //re-enables the animator if it was disabled by the hitstun function
            if (animator.enabled == false)
            {
                animator.enabled = true;
            }
        }

        //resets the holding variable so you can not initiate a jab attack by holding the attack button
        if (Input.GetButtonUp("Attack"))
        {
            holding = false;
        }

        //triggers the smash attack
        if(isCharging() && Input.GetButtonUp("Attack"))
        {
            if (smashAttackCharged)
            {
                Debug.Log("SMASHH!");
                smashAttackCharged = false;

                currentState = attackState.Smash;

                GetComponent<Player_Move>().stopVelocity();

                GetComponent<Player_State>().changeState("attack");

                //StartCoroutine("smashAttack");
                //no need for a smash attack coroutine, as the hitbox is moved by animation events; all you need
                //to do is initialize the hitbox

                setHitboxSize(30, 5, 0.03f, 0.5f, 0.5f);
                //the size is small enough so nothing will be hit as the animation is winding up
                //the hitbox must be instantiated here, or the moveHitbox function won't have a hitbox to move                

                //moving the hitbox into position is triggered by animation events
            }
            else
            {
                Debug.Log("welp...");
                animator.SetBool("Charging", false);
                currentState = attackState.Idle;
                StopCoroutine("ChargeSmashAttack");
            }
            
        }

    }

    void decideAttack()
    {
        //so the dash attack can't be performed traveling vertically 
        if (GetComponent<Player_State>().state == Player_State.playerState.Dashing && Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) > 2)
        {
            Debug.Log("Dash Attaack" + GetComponent<Rigidbody2D>().velocity);
            StartCoroutine("dashAttack");
        }

        //so you can't perform a normal attack while dashing vertically
        else if(GetComponent<Player_State>().state != Player_State.playerState.Dashing)
        {
            switch (currentState)
            {
                case attackState.Idle:
                    StartCoroutine("normalAttack");
                    break;
                case attackState.Normal:
                    if (holding == false)
                    {
                        mashlevel = 1;
                        StartCoroutine("mashAttack");
                    }
                    break;
                case attackState.Mash:
                    if (holding == false && mashlevel + 1 <= 3)
                    {
                        mashlevel += 1;
                        StartCoroutine("mashAttack");
                    }
                    break;
            }
        }
    }

    IEnumerator normalAttack()
    {
        //Debug.Log("norm Attaack");
        currentState = attackState.Normal;
        yield return new WaitForSeconds(.01f);
        createHitbox(13, 3, 0.03f);
        yield return new WaitForSeconds(.25f);

        if(currentState == attackState.Mash)
        {
            yield break;
        }

        cancelAttack();

        //checks if holding is true, to prevent letting go of the key before the 
        //normal attack has concluded, which messes up the charging timing
        if (holding)
        {
            currentState = attackState.Charging;
            StartCoroutine("ChargeSmashAttack");
        }     
    }

    IEnumerator mashAttack()
    {
        currentState = attackState.Mash;
        switch (mashlevel)
        {
            case 1:
                destroyHitbox();
                yield return new WaitForSeconds(.01f);
                destroyHitbox(); //prevents two sword hitboxes from showing up for some reason
                createHitbox(15, 3, 0.03f);
                yield return new WaitForSeconds(.25f);

                //cuts off the attack coroutine early if the player presses the attack button again 
                if (mashlevel >= 2)
                {
                    yield break;
                }

                //stops attack
                cancelAttack();
                mashlevel = 0;
                break;

            case 2:

                for(int i = 0; i < 3; i++)
                {
                    destroyHitbox();
                    createHitbox(0, 1, 0.03f);
                    if (mashlevel >= 3)
                    {
                        destroyHitbox();
                        break;
                    }
                    yield return new WaitForSeconds(.2f);
                }

                //cuts off the attack coroutine early if the player presses the attack button again 
                if (mashlevel >= 3)
                {
                    yield break;
                }

                //stops attack
                cancelAttack();
                mashlevel = 0;
                break;

            case 3:
                destroyHitbox();
                yield return new WaitForSeconds(.55f);
                createHitbox(30, 3, 0.03f);
                yield return new WaitForSeconds(.3f);
                cancelAttack();
                mashlevel = 0;
                break;
        }
    }

    IEnumerator dashAttack()
    {
        
        currentState = attackState.Dash;
        GetComponent<Player_Move>().CancelDash();        

        animator.ResetTrigger("DashAttackSlide");

        //puts player on invisible layer
        gameObject.layer = 10;

        //grabs rigid_body velocity
        float speedx = GetComponent<Rigidbody2D>().velocity.x;
        float speedy = GetComponent<Rigidbody2D>().velocity.y;

        //finds the direction the player was traveling in, discarding smaller values
        if(speedx < -5 || speedx > 5)
        {
            speedx = speedx / Mathf.Abs(speedx);
        }
        else
        {
            speedx = 0;
        }

        if (speedy < -5 || speedy > 5)
        {
            speedy = speedy / Mathf.Abs(speedy);
        }
        else
        {
            speedy = 0;
        }      
        
        //applies the speed in the right direction
        GetComponent<Player_Move>().moveX = speedx * 38f;
        GetComponent<Player_Move>().moveY = speedy * 38f;

        //The enemy must be hit again before the travelingwithplayer bool is set to false, 
        //or the enemy will stop in the middle of the attack
        for (int i = 0; i < 3; i++)
        {
            destroyHitbox();
            createHitbox(0, 1, 0.6f);            
            yield return new WaitForSeconds(.2f);
        }
        
        //yield return new WaitForSeconds(.3f);
        
        destroyHitbox();
        
        createHitbox(25, 1, 0f);
        
        GetComponent<Player_Move>().moveX = 0;
        GetComponent<Player_Move>().moveY = 0;
        //GetComponent<Player_Move>().moveSmooth = 0;

        animator.SetTrigger("DashAttackSlide");

        yield return new WaitForSeconds(.15f);

        //makes player vulnerable again
        gameObject.layer = 8;

        //movement is stopped when the control variable becomes true again
        cancelAttack();
    }

    IEnumerator ChargeSmashAttack()
    {
        animator.SetBool("Charging", true);

        yield return new WaitForSeconds(1f);

        Debug.Log("ready");
        smashAttackCharged = true;
        animator.SetBool("Charged", true);

        animator.SetBool("Charging", false);
    }

    //used to move the hitbox during the smash attack animation (triggered by animation events)
    private void moveSmashAttackHitBox(int order)
    {
        switch (order)
        {
            case 1:
                moveHitbox(0.93f * -GetComponent<Player_Move>().GetDirectionX(), -0.95f, 1.7f, 1.8f);
                break;
            case 2:
                moveHitbox(0.51f * GetComponent<Player_Move>().GetDirectionX(), -1.73f, 1.98f, 1.97f);
                break;
            case 3:
                moveHitbox(1.29f * GetComponent<Player_Move>().GetDirectionX(), 0.12f, 3.93f, 5.23f);
                break;
            case 4:
                moveHitbox(0.67f * -GetComponent<Player_Move>().GetDirectionX(), 0.79f, 2.9f, 2.5f);
                break;
            case 5:
                moveHitbox(0.5f * -GetComponent<Player_Move>().GetDirectionX(), -1.2f, 2.67f, 1.8f);
                break;
        }
        
    }

    public void cancelAttack()
    {
        Destroy(instanceOfSword);

        currentState = attackState.Idle;

        GetComponent<Player_State>().changeState("idle");
    }

    void createHitbox(float kb, int dam, float frz)
    {
        //the degree of knockback applied
        knockback = kb;
        //the amount of damage inflicted
        damage = dam;
        //the amount of time before knockback is applied
        freezetime = frz;
        instanceOfSword = Instantiate(swordBox, new Vector3(transform.position.x + 1 * GetComponent<Player_Move>().GetDirectionX() + 0.25f, transform.position.y, 0), Quaternion.identity);
        instanceOfSword.GetComponent<BoxCollider2D>().size = new Vector2(1.9f, 2.1f);
    }

    void setHitboxSize(float kb, int dam, float frz, float sizeX, float sizeY)
    {
        //the degree of knockback applied
        knockback = kb;
        //the amount of damage inflicted
        damage = dam;
        //the amount of time before knockback is applied
        freezetime = frz;
        instanceOfSword = Instantiate(swordBox, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
        instanceOfSword.GetComponent<BoxCollider2D>().size = new Vector2(sizeX, sizeY);
    }

    void moveHitbox(float offX, float offY, float sizeX, float sizeY)
    {
        //resets collider's position to the player's position so offset can be used
        instanceOfSword.GetComponent<BoxCollider2D>().transform.position = new Vector2(transform.position.x, transform.position.y);

        instanceOfSword.GetComponent<BoxCollider2D>().offset = new Vector2(offX, offY);
        instanceOfSword.GetComponent<BoxCollider2D>().size = new Vector2(sizeX, sizeY);
    }

    void updateHitbox()
    {
        if (instanceOfSword != null)
        {
            instanceOfSword.GetComponent<BoxCollider2D>().transform.localPosition = new Vector3(transform.position.x + 1 * GetComponent<Player_Move>().GetDirectionX() + 0.25f, transform.position.y, 0);
        }
    }

    public void destroyHitbox()
    {
        Destroy(instanceOfSword);
    }

    public void stopMidAttack()
    {
        Destroy(instanceOfSword);    
        currentState = attackState.Idle;
        mashlevel = 0;
        StopAllCoroutines();
    }

    public bool isCharging()
    {
        if(currentState == attackState.Charging)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
