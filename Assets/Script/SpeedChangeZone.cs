using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedChangeZone : MonoBehaviour
{
    public float speedInfla;

    private LinkedList<RgbVectorInfo> rgbVectorList; 
    private class RgbVectorInfo
    {
        public RgbVectorInfo(Rigidbody2D rgb)
        {
            this.rgb = rgb;
            this.beforeVectorLength = rgb.velocity.magnitude;
        }
        public void ChangeSpeed(float speedInfla)
        {
            rgb.velocity = rgb.velocity * speedInfla;
        }
        public void ReturnSpeed()
        {
            rgb.velocity = rgb.velocity.normalized * beforeVectorLength;
        }
        public bool IsSameGameObject(GameObject gameObject)
        {
            return rgb.gameObject == gameObject;
        }
        private Rigidbody2D rgb;//rigidbody2d
        private float beforeVectorLength;//벡터크기
    }
    private void Start()
    {
        rgbVectorList = new LinkedList<RgbVectorInfo>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        rgbVectorList.AddFirst(new RgbVectorInfo(collision.gameObject.GetComponent<Rigidbody2D>()));//linkedlist에 rgb,vector정보 저장
        rgbVectorList.First.Value.ChangeSpeed(speedInfla);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        LinkedListNode<RgbVectorInfo> node = rgbVectorList.First;
        
        while(node != null)
        {
            if(node.Value.IsSameGameObject(collision.gameObject))//나가는 오브젝트의 노드를 찾음
            {
                node.Value.ReturnSpeed();//빠르기를 되돌림
                rgbVectorList.Remove(node);//linkedlist에서 삭제
                break;
            }
            node = node.Next;
        }
    }

}
