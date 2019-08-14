using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StageManager : NetworkBehaviour
{
    public GameObject ballPrefab;
    public GameObject blockPrefab;
    public GameObject breedItemPrefab;
    public NetworkManager nManager;
    public int breedItemSpawnCycle;
    public int widthGridSize;
    public int heightGridSize;
    public int blockSpawnProba;

    private BoxCollider2D topOutLine;
    private BoxCollider2D bottomOutLine;
    private int existBallNum = 0;
    private int bottomWinCount = 0;
    private int topWinCount = 0;
    private int roundCount = 0;
    private int gridCount = 0;
    private LinkedList<CircleCollider2D> ballColliders;
    private List<GridInfo> gridsInfo;
    private class GridInfo
    {
        public GridInfo(Vector2 location, bool onUse)
        {
            this.location = location;
            this.onUse = onUse;
        }
        public Vector2 location;
        public bool onUse;
    }
    public void BreedBall(int num,Vector2 location,Vector2 direct,float speed,float radius)//공이 num만큼 부채꼴로 퍼쳐나가게 함(1개는 원래 진행방향으로  무조건 진행)direct는 정규화되어야함
    {
        direct = direct.normalized;
        for (int i = 1; i < num; i++)
        {
            StartCoroutine(WaitNoOverlapAndSpawn(num,location,direct,i,speed,radius));
        }
    }
    IEnumerator WaitNoOverlapAndSpawn(int num,Vector2 location,Vector2 direct,int i,float speed,float radius)
    {
        //yield return new WaitUntil(() => (Physics2D.OverlapCircle(location, radius) == null));//collider가 빌때까지 기달
        while (Physics2D.OverlapCircle(location, radius) != null)
            yield return new WaitForSeconds(0.2f);
        GameObject ball = Instantiate(ballPrefab, location, Quaternion.identity);
        Rigidbody2D rgb = ball.GetComponent<Rigidbody2D>();
        rgb.velocity = ConvertDirect(direct, 360f / num * i) * speed;//부채꼴로 퍼쳐나가도록
        NetworkServer.Spawn(ball);//ball spawn
        existBallNum++;
        ballColliders.AddLast(ball.GetComponent<CircleCollider2D>());
    }
    private Vector2 ConvertDirect(Vector2 direct,float angle)//direct를 기준으로 해당각도만큼 회전시 결과벡터 반환
    {
        float rad = Mathf.Deg2Rad * angle;
        return new Vector2(direct.x * Mathf.Cos(rad) - direct.y * Mathf.Sin(rad), 
            direct.x * Mathf.Sin(rad) + direct.y * Mathf.Cos(rad));
    }
    
    private void Start()
    {
        if (!isServer)
            return;
        ballColliders = new LinkedList<CircleCollider2D>();
        topOutLine = GameObject.Find("TopOutLine").GetComponent<BoxCollider2D>();
        bottomOutLine = GameObject.Find("BottomOutLine").GetComponent<BoxCollider2D>();
        StartCoroutine("SpawnBreedItem");
        gridsInfo = new List<GridInfo>();
        for(int i = widthGridSize;i<Camera.main.scaledPixelWidth;i+=widthGridSize)
        {
            for(int j = heightGridSize;j<Camera.main.scaledPixelHeight;j+=heightGridSize)
            {
                gridsInfo.Add(new GridInfo(Camera.main.ScreenToWorldPoint(new Vector2(i, j)),false));//grid정보들을 list에 모두 등록해놓음,bool변수는 해당 grid를 차지하고 있는지에 관한 정보
                gridCount++;
            }
        }
    }
    private void Update()
    {
        if (nManager.numPlayers != 2)//2명 접속해야 시작
            return;
        if (!isServer)//서버만이 Stage관리
            return;
        if (existBallNum == 0)
            StartNewStage();//새로운 스테이지 시작
        CheckingTopBottomCollider();
    }
    private void StartNewStage()
    {
        roundCount++;//1라운드 증가
        GameObject ball = Instantiate(ballPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        NetworkServer.Spawn(ball);//ball spawn
        for(int i = 0;i<gridsInfo.Count;i++)
        {
            if(Random.Range(1, 101) < blockSpawnProba && !gridsInfo[i].onUse)//확률+다른게 격자를 사용중인지 검사
            {
                GameObject block = Instantiate(blockPrefab, gridsInfo[i].location, Quaternion.identity);
                NetworkServer.Spawn(block);
                GridInfo grid = gridsInfo[i];
                grid.onUse = true;//사용중이라 등록
            }
            
        }
        existBallNum++;
        ballColliders.AddLast(ball.GetComponent<CircleCollider2D>());//ballcollider관리 리스트에 등록
    }
    private void CheckingTopBottomCollider()
    {
        LinkedListNode<CircleCollider2D> iNode = ballColliders.First;
        while(iNode != null)
        {
            
            CircleCollider2D ballCollider = iNode.Value;
            if (ballCollider.IsTouching(topOutLine))//위에 라인에 닿은경우
            {
                iNode = iNode.Next;
                ballColliders.Remove(ballCollider);//등록해제

                Destroy(ballCollider.gameObject);
                existBallNum--;//공 제거

                if (existBallNum == 0)
                    bottomWinCount++;//아래가 이김
            }
            else if (ballCollider.IsTouching(bottomOutLine))
            {
                iNode = iNode.Next;
                ballColliders.Remove(ballCollider);//등록해제

                Destroy(ballCollider.gameObject);
                existBallNum--;//공 제거

                if (existBallNum == 0)
                    topWinCount++;//위가 이김
            }
            else//안 닿은 경우
            {
                iNode = iNode.Next;
            }
        }
    }
    IEnumerator SpawnBreedItem()//일정 주기마다 맨 중앙 스폰위치, 블럭위치를 제외한 곳에 breedItem생성
    {
        while (true)
        {
            if (nManager.numPlayers != 2)//2명 접속해야 시작
            {
                yield return new WaitForSeconds((float)breedItemSpawnCycle);
                continue;
            }
            int gridIndex = Random.Range(0, gridCount);
            while (gridsInfo[gridIndex].onUse)
                gridIndex = Random.Range(0, gridCount);//비어있는 gridIndex를 기어코 찾아냄
            GameObject breedItem = Instantiate(breedItemPrefab, gridsInfo[gridIndex].location, Quaternion.identity);
            GridInfo gridInfo = gridsInfo[gridIndex];
            gridInfo.onUse = true;//사용중이라고 등록
            NetworkServer.Spawn(breedItem);//breedItem spawn
            yield return new WaitForSeconds((float)breedItemSpawnCycle);
        }
    }
    public void UnregisterGrid(Vector2 location)
    {
        for(int i = 0;i<gridsInfo.Count;i++)
        {
            GridInfo tempGrid = gridsInfo[i];
            if(tempGrid.location == location)
            {
                tempGrid.onUse = false;
            }
        }
    }
}
