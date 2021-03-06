﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AlpacaController : MonoBehaviour
{
    public float yax = -0.8f;
    Rigidbody2D rig;
    public float force = 1000.0f;
    Vector2 startPos;
    public float speed=1.0f;
    public float tackleTime = 2.0f, duckTime = 1.0f;
    public bool disable = false;
    public ParticleSystem dustSlide;
    public int state = 0;
    float seconds = 0;
    private Vector2 afterTackle;
    public GameObject shougeki;
    Vector2 tempPos;
    bool korobiBool = false;

    public AudioClip jumpSound,DuckSound,fallSound,tackleSound;


    Animator animator;
    public float korobiTime;
    public float speedAdjust;
    private bool blockGoingOver;
    public bool broken=false;

    void Start()
    {
        Application.targetFrameRate = 30;
        this.rig = GetComponent<Rigidbody2D>();
        this.animator = GetComponent<Animator>();
        this.animator.SetBool("Ground", true);
        disable = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Mathf.Approximately(Time.timeScale, 0f)||disable)
        {
            this.transform.Translate((-this.speed / speedAdjust) * (Time.deltaTime * 60), 0, 0);
            return;
        }
        this.animator.speed = speed * (Time.deltaTime * 60);
        if (state != 3)
        {
            if (Input.GetMouseButtonDown(0))
            {
                this.startPos = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                Vector2 endPos = Input.mousePosition;
                Vector2 result = endPos - startPos;
                switch (fourDirection(result))
                {
                    case Direction.UP:
                        Debug.Log("UP");
                        if (rig.velocity.y == 0 && state == 0)
                        {
                            this.rig.AddForce(transform.up * this.force);
                            gameObject.GetComponent<AudioSource>().PlayOneShot(jumpSound);
                        }
                        break;
                    case Direction.DOWN:
                        Debug.Log("DOWN");
                        if (state == 0 && rig.velocity.y == 0)
                        {
                            state = 1;
                            gameObject.GetComponent<AudioSource>().PlayOneShot(DuckSound);
                        }
                        break;
                    case Direction.RIGHT:
                        Debug.Log("RIGHT");
                        if (state == 0)
                        {
                            GameObject go = Instantiate(shougeki) as GameObject;
                            go.transform.parent = this.transform;
                            go.transform.localPosition = new Vector3(-1.0f, 0, 0);
                            tempPos = transform.position;
                            gameObject.GetComponent<AudioSource>().PlayOneShot(tackleSound);
                            afterTackle = gameObject.transform.position;
                            broken = false;
                            state = 2;
                        }
                        break;
                    case Direction.LEFT:
                        Debug.Log("LEFT");
                        break;
                }
            }//input
        }
        switch (state)
        {
            case 0://walk or jump
               
                if (transform.localPosition.y> yax)//important
                {
                    this.animator.SetBool("Ground", false);
                }
                else if(this.korobiBool)
                {
                    state = 3;
                }
                else {
                    this.animator.SetBool("Ground", true);
                }
                break;
            case 1://duck
                seconds += Time.deltaTime;
                if (seconds < duckTime)
                {
                    duck();
                }
                else if (seconds > duckTime)
                {
                    duckOnFinished();
                }
                break;
            case 2://tackle
                seconds += Time.deltaTime;
                if (seconds < tackleTime)
                {
                    tackle();
                    if (this.korobiBool)
                    {
                        tackleOnFinished();
                        state = 3;
                    }
                }
                else if (seconds > tackleTime)
                {
                    tackleOnFinished();
                }
                break;
            case 3:
                seconds += Time.deltaTime;
                if (seconds < korobiTime)
                {
                    korobi();
                }
                else if (seconds > korobiTime)
                {
                    korobiOnFinished();
                }
                break;

        }
        this.animator.SetInteger("State", state);

    }

    private void korobiOnFinished()
    {
        Debug.Log("korobifin");
        speed = 1.0f;
        {
            state = 0;
            seconds = 0;
            korobiBool = false;
        }
    }

    private void korobi()
    {
        Debug.Log("korobing");
        this.transform.Translate((-this.speed / speedAdjust) * (Time.deltaTime * 60), 0, 0);
    }

    private void duckOnFinished()
    {
        Debug.Log("duckfin");
        speed = 1.0f;
        {
            state = 0;
            seconds = 0;
        }
        blockGoingOver = false;
    }

    enum Direction
    {
        UP,
        DOWN,
        RIGHT,
        LEFT
    }

    private Direction fourDirection(Vector2 vector2)
    {
        if (Mathf.Abs(vector2.y) > Mathf.Abs(vector2.x))
        {
            return (vector2.y > 0) ? Direction.UP : Direction.DOWN;
        }
        else {
            return (vector2.x > 0) ? Direction.RIGHT : Direction.LEFT;
        }
    }

    private void duck()
    {
        Debug.Log("duck");
        if (speed == 1.0f)
        {
            dustSlide.Play();
        }
        if (!blockGoingOver)
        {
            speed = Mathf.MoveTowards(speed, 3.0f, 0.5f);
        }
    }

    private void tackle()
    {
        Debug.Log("tackle");
        if (!blockGoingOver)
        {
            speed = Mathf.MoveTowards(speed, 5.0f, 0.5f);
            Debug.Log("tttt" + afterTackle.x);
        }
    }
    private void tackleOnFinished()
    {
        Debug.Log("tacklefin");
        speed = 1.0f;
        {
            state = 0;
            blockGoingOver = false;
            seconds = 0;
        }
        if (broken)
        {
            gameObject.transform.position = afterTackle;
        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<Background1>())
        {
            return;
        }
        if (collision.gameObject.GetComponent<Enemy>())
        {
            Debug.Log("cast success");
            if (rig.velocity.y <= 0)
            {
                if (this.state != 2 && this.state != 1)
                {
                    korobiBool = true;
                }
                if (this.state == 2 && collision.gameObject.GetComponent<Bird>())
                {
                    korobiBool = true;
                }
            }
            
        }
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        if (this.state == 2 || this.state==1)
        {
            if (collision.gameObject.GetComponent<Cat>() ||
                collision.gameObject.GetComponent<Capibala>() ||
                collision.gameObject.GetComponent<Rabbit>())
            {
                blockGoingOver = true;
            }
        }
    }


}
