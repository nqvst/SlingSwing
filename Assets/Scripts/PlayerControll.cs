﻿using UnityEngine;
using System.Collections;

public class PlayerControll : MonoBehaviour
{

	public Transform manager;
	public Transform exposionPrefab;

	private AudioSource audioSource;
	public AudioClip hurt;
	public SpringJoint2D spring;

	bool hitSet = false;
	bool iMustDie = false;
	bool isDead = false;
	bool isHurt = false;
	bool isHurting = false;
	float maxSpeed = 13;

	SpriteRenderer spriteRenderer;

	public enum Health { Full = 3, Half = 2, Low = 1 }

	public Health health;

	void Start ()
	{
		spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
		if(spriteRenderer == null){
			Debug.LogError(" AAAAAAAH!");
		}

		audioSource = GetComponent<AudioSource> ();

		spring = GetComponent<SpringJoint2D> ();
		spring.enabled = false;
		spring.connectedBody = GameObject.FindGameObjectWithTag("GrapplePoint").rigidbody2D;
		iMustDie = false;
		manager = GameObject.FindGameObjectWithTag("GameManager").transform;
		health = Health.Full;

	}

	void Update ()
	{
		if (TouchExit|| Input.GetMouseButtonUp(0)) {
			hitSet = false;
		}
		spring.enabled = hitSet;

		if(iMustDie && !isDead){
			Die();
		}

		if( isHurt && isHurting){
			isHurting = false;
			Invoke( "StopBeingHurt", 2 );
		}

		if(isHurt){
			float lerp = Mathf.PingPong(Time.time, 0.2f) / 0.2f;
			spriteRenderer.color = Color.Lerp(Color.magenta, Color.cyan, lerp);
		}else{
			spriteRenderer.color = Color.cyan;
		}

	}

	void FixedUpdate ()
	{
		rigidbody2D.velocity = Vector2.ClampMagnitude(rigidbody2D.velocity, maxSpeed);
	}

	void StopBeingHurt(){
		isHurt = false;
	}

	void OnHitSet()
	{
		Debug.Log("OnHitSet()");
		hitSet = true;
	}

	void OnHitExit()
	{
		Debug.Log("OnHitExit()");
		hitSet = false;
	}
	
	public virtual bool TouchExit 
	{
		get {
			if( Input.touchCount > 0 )
			{
				return Input.GetTouch(0).phase == TouchPhase.Ended;
			} else {
				return false;
			}
		} 
	}

	void OnCollisionEnter2D (Collision2D coll)
	{

		if( coll.transform.CompareTag("Finish") ){
			Finish();
			iMustDie = true;
		} else {
			if( !isHurt ){
				isHurting 	= true;
				isHurt 		= true;

				audio.PlayOneShot(hurt);
				ReduceHealth();
			}
		}
	}

	void ReduceHealth()
	{
		Debug.Log(health);

		if (health == Health.Low || (int) health < 1) {
			iMustDie = true;
		}
		else {
			health = (Health)((int) health -1);
		}
	}

	void Die()
	{
		isDead = true;
		Instantiate(exposionPrefab, transform.position, Quaternion.identity);
		Debug.Log("died!");
		manager.SendMessage("OnPlayerDied", SendMessageOptions.DontRequireReceiver);
		Destroy(gameObject);
	}

	void Finish(){
		manager.SendMessage("OnPlayerFinished", SendMessageOptions.DontRequireReceiver);
	}

	void OnGUI()
	{
		GUI.Label(new Rect(5, 300, 200, 50), "Velocity: " + rigidbody2D.velocity.magnitude);
		GUILayout.Space(50);
		GUILayout.Label("hurt: " + isHurt);
		GUILayout.Label("hurting: " + isHurting);
	}

}