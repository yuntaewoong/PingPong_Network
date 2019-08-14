using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private StageManager stageM;
    void Start()
    {
        stageM = GameObject.Find("StageManager").GetComponent<StageManager>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        stageM.UnregisterGrid(gameObject.transform.position);//공에 닿았으므로 격자등록해제
        Destroy(gameObject); //제거
    }
}
