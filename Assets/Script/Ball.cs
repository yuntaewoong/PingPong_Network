using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Ball : NetworkBehaviour
{
    public int firstSpeed;
    public int breedNumMin;
    public int breedNumMax;
    public float maximumVectorSize;

    private Rigidbody2D rgb;
    private StageManager stageM;
    private CircleCollider2D collider;
    private void Start()
    {
        if (!isServer)//ball은 서버에만 존재
            return;
        stageM = GameObject.Find("StageManager").GetComponent<StageManager>();

        collider = GetComponent<CircleCollider2D>();
        rgb = GetComponent<Rigidbody2D>();
        if (rgb.position == Vector2.zero)//stage 시작시 스폰되는 공인경우에만
        {
            int direct = Random.Range(-1, 1);
            if (direct == 0)
                direct = 1;
            rgb.velocity = new Vector2(0, direct * firstSpeed);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)//아이템 트리거에 접촉
    {
        if (collision.gameObject.tag != "breedItem")
            return;
        stageM.BreedBall(Random.Range(breedNumMin, breedNumMax + 1), rgb.position, rgb.velocity.normalized, firstSpeed,collider.radius);
        stageM.UnregisterGrid((Vector2)collision.gameObject.transform.position);
        
        Destroy(collision.gameObject);//breedItem삭제
    }
    private void Update()
    {
        if (rgb.velocity.magnitude > maximumVectorSize)
            rgb.velocity = rgb.velocity.normalized * maximumVectorSize;//최대 속도제한
    }
}
