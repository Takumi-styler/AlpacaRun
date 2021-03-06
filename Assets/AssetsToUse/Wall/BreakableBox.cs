﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableBox : Enemy
{
    public ParticleSystem[] particle;
    public AudioClip breakSound;

    public override bool destroyCondition()
    {
        return this.transform.position.x < -20.0f;
    }
    public void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.GetComponent<AlpacaController>())
        {
            return;
        }
        if (collision.gameObject.GetComponent<AlpacaController>().state==2)
        {
            collision.gameObject.GetComponent<AlpacaController>().broken = true;
            StartCoroutine(Break());
        }
    }
    private IEnumerator Break()
    {
        foreach (ParticleSystem i in particle)
        {
            i.Play();
        }
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        gameObject.GetComponent<AudioSource>().PlayOneShot(breakSound);
        yield return new WaitForSeconds(particle[0].main.startLifetime.constantMax);
        Destroy(gameObject);
    }

}
