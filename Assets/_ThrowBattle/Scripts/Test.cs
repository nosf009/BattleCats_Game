using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {
    Rigidbody2D rigid;
	// Use this for initialization
	void Start () {
        rigid=GetComponent<Rigidbody2D>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rigid.AddForce(new Vector2(-1, 1) * 1000);
            rigid.AddTorque(200);
        }
	}
}
