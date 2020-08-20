using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_State : MonoBehaviour
{
   public enum playerState
    {
        Idle,
        Moving,
        Dashing,
        Attacking,
        Hitstun
    };
    public playerState state;
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void changeState(string s)
    {
        switch (s)
        {
            case "idle":
                state = playerState.Idle;
                break;
            case "dash":
                state = playerState.Dashing;
                break;
            case "attack":
                state = playerState.Attacking;
                break;
            case "hitstun":
                state = playerState.Hitstun;
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(state);

        if(state == playerState.Idle)
        {
            if (GetComponent<Player_Attack>().isCharging())
            {
                GetComponent<Player_Move>().moveSpeed = 5f;
                GetComponent<Player_Move>().moveSmooth = 0f;
            }
            else
            {
                GetComponent<Player_Move>().moveSpeed = 13f;
                GetComponent<Player_Move>().moveSmooth = 0.14f;
            }
            GetComponent<Player_Move>().mobile = true;
            GetComponent<Player_Move>().control = true;
            
        }

        if (state == playerState.Dashing)
        {
            GetComponent<Player_Move>().control = false;
            animator.SetBool("Dashing", true);
        }
        else
        {
            animator.SetBool("Dashing", false);
        }

        if (state == playerState.Hitstun)
        {
            GetComponent<Player_Move>().control = false;
            //updates the hitstun variable in the animator to tell when to finish the hurt animation
            animator.SetBool("Hitstun", true);
        }
        else
        {
            animator.SetBool("Hitstun", false);
        }

        if (state == playerState.Attacking)
        {
            GetComponent<Player_Move>().control = false;
            //updates the hitstun variable in the animator to tell when to finish the hurt animation
            animator.SetBool("Attacking", true);
        }
        else
        {
            animator.SetBool("Attacking", false);
        }
        

    }
}
