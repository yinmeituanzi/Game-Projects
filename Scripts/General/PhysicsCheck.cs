using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{
    private CapsuleCollider2D coll;
    private PlayerController playerController;
    private Rigidbody2D rb;

    [Header("¼ì²â²ÎÊý")]
    public bool manual;
    public bool isPlayer;
    public float checkRaduis;
    public LayerMask groundLayer;
    public Vector2 bottomOffset;
    public Vector2 leftOffset;
    public Vector2 rightOffset;

    [Header("×´Ì¬")]
    public bool isGround;
    public bool touchLeftWall;
    public bool touchRightWall;
    public bool onWall;

    private void Awake()
    {
        coll = GetComponent<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        if(!manual)
        {
            rightOffset = new Vector2((coll.bounds.size.x  + coll.offset.x)/2, coll.bounds.size.y/2);
            leftOffset = new Vector2(-rightOffset.x, rightOffset.y);
        }

        if (isPlayer)
            playerController = GetComponent<PlayerController>();
    }
 
    private void Update()
    {
        Check();
    }
    public void Check()
    {    //ÅÐ¶ÏÊÇ·ñÔÚµØÃæ
        if(onWall)
            isGround = Physics2D.OverlapCircle((Vector2)transform.position+new Vector2(bottomOffset.x* transform.localScale.x,bottomOffset.y), checkRaduis, groundLayer);
        else
            isGround = Physics2D.OverlapCircle((Vector2)transform.position + new Vector2(bottomOffset.x * transform.localScale.x,0), checkRaduis, groundLayer);


        //ÅÐ¶ÏÇ½ÌåÅö×²
        touchLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, checkRaduis, groundLayer);
        touchRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, checkRaduis, groundLayer);

        if (isPlayer)
            onWall = ((touchLeftWall&&playerController.inputDirection.x<0f) || (touchRightWall && playerController.inputDirection.x > 0f)) && (rb.velocity.y<0f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)transform.position + bottomOffset, checkRaduis);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, checkRaduis);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, checkRaduis);
    }
}
