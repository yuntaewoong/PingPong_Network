using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public enum HitType { BASIC = 0,GOODHIT,GREATHIT}
public class Player : NetworkBehaviour
{
    public float moveSpeed;
    public float rotateSpeed;
    public float spaceRoutineTime;
    public float greatTime;
    public float greatTimeSpeed;
    public float goodTimeSpeed;

    private Rigidbody2D rgb2D;
    private Rigidbody2D ballRgb2D;
    private CapsuleCollider2D detectingTrigger;
    private SoundManager soundM;
    private bool onSpaceInputRoutine = false;
    private HitType hitType = HitType.BASIC;


    private void Start()
    {
        soundM = GameObject.Find("SoundManager").GetComponent<SoundManager>();
        detectingTrigger = GetComponentInChildren<CapsuleCollider2D>();
        rgb2D = GetComponent<Rigidbody2D>();
    }
    private void Update()
    {
        if (!isLocalPlayer)
            return;
        float horInput = Input.GetAxis("Horizontal");//입력
        Move((Vector2)transform.position + new Vector2(horInput * Time.deltaTime * moveSpeed, 0));

        float klInput = Input.GetAxis("KL");
        Rotate(klInput);
        if (!onSpaceInputRoutine && Input.GetButtonDown("Jump"))//스페이스 루틴이 끝나야 다시 스페이스 입력을 받음
        {
            onSpaceInputRoutine = true;
            StartCoroutine(TimingHit(Time.time));
        }
    }
    private void OnTriggerExit2D(Collider2D collision)//detectingTrigger관련
    {
        
        ballRgb2D = collision.gameObject.GetComponent<Rigidbody2D>();
        ballRgb2D.isKinematic = false;
        hitType = HitType.BASIC;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ballRgb2D = collision.gameObject.GetComponent<Rigidbody2D>();
        ballRgb2D.isKinematic = true;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isServer)
            return;
        switch (hitType)
        {
            case HitType.GREATHIT:
                collision.rigidbody.velocity = Vector2.Reflect(collision.rigidbody.velocity, collision.contacts[0].normal)
                    * greatTimeSpeed;
                break;
            case HitType.GOODHIT:
                collision.rigidbody.velocity = Vector2.Reflect(collision.rigidbody.velocity, collision.contacts[0].normal)
                    * goodTimeSpeed;
                break;
            case HitType.BASIC:
                collision.rigidbody.velocity = Vector2.Reflect(collision.rigidbody.velocity, collision.contacts[0].normal);
                break;
        }
        soundM.RpcMakeHitSound();
    }
    [Command]
    void CmdChangeHitType(HitType hitType)//클라에서 서버에 스페이스입력으로 변한 hitType전달
    {
        this.hitType = hitType;
        Debug.Log(this.hitType);
    }
    void Move(Vector2 endPosition)
    {
        if (!isLocalPlayer)//localplayer만 이동
            return;
        rgb2D.MovePosition(endPosition);//이동
    }
    void Rotate(float amount)
    {
        if (!isLocalPlayer)
            return;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, 0, -amount * Time.deltaTime * rotateSpeed));
    }
    IEnumerator TimingHit(float startTime)//이 코루틴이 끝날때까지 사용자는 스페이스 입력을 주지못함
    {
        while(Time.time - startTime < spaceRoutineTime)//스페이스루틴시간이 모두 지나면 코루틴 종료
        {
            if(detectingTrigger.IsTouchingLayers(LayerMask.GetMask("Ball")))//공 감지
            {
                if(Time.time - startTime < greatTime)//great hit
                {
                    CmdChangeHitType(HitType.GREATHIT);
                    Debug.Log("GreatHit");
                    break;//코루틴 종료
                }
                else//good hit
                {
                    CmdChangeHitType(HitType.GOODHIT);
                    Debug.Log("GoodHit");
                    break;//코루틴 종료
                }
                
            }
            yield return null;
        }
        onSpaceInputRoutine = false;
    }
}
