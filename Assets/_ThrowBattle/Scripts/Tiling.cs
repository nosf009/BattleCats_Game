using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]

public class Tiling : MonoBehaviour {

    public int offsetX = 2;

    public bool hasRight;
    public bool hasLeft;

    public bool reverseScale = true;

    private float spriteWidth  = 0;

    private Camera cam;
    private Transform myTrans;
    private void Awake()
    {
        cam = Camera.main;
        myTrans = transform;
    }

    // Use this for initialization
    void Start () {
        SpriteRenderer sRenderer = GetComponent<SpriteRenderer>();
        spriteWidth = sRenderer.sprite.bounds.size.x;

    }
	
	// Update is called once per frame
	void Update () {
        if((cam.transform.position.x - myTrans.position.x)> 15)
        {
            //Destroy(gameObject);
        }

        if (!hasLeft || !hasRight)
        {
            float camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;

            float visiblePosRight = (myTrans.position.x + spriteWidth / 2) - camHorizontalExtend;
            float visiblePosLeft = (myTrans.position.x - spriteWidth / 2) + camHorizontalExtend;

            if(cam.transform.position.x>= visiblePosRight - offsetX && !hasRight)
            {
                NewPlatformer(1);
                hasRight = true;
            }
            else if (cam.transform.position.x <= visiblePosLeft - offsetX && !hasLeft)
            {
                NewPlatformer(-1);
                hasLeft = true;
            }
        }
	}

    void NewPlatformer(int LeftorRight)
    {
        Vector3 newPos = new Vector3((myTrans.position.x + spriteWidth * LeftorRight *Mathf.Abs(myTrans.localScale.y)), myTrans.position.y, myTrans.position.z);
        Transform newBuddy = Instantiate(myTrans, newPos, myTrans.rotation) as Transform;

        if (reverseScale == true)
        {
            newBuddy.localScale = new Vector3(newBuddy.localScale.x * -1, newBuddy.localScale.y, newBuddy.localScale.z);
        }
        
        newBuddy.parent = myTrans.parent;
        if (LeftorRight>0)
        {
            newBuddy.GetComponent<Tiling>().hasLeft = true;
        }
        else
        {
            newBuddy.GetComponent<Tiling>().hasRight = true;
        }
    }
}
