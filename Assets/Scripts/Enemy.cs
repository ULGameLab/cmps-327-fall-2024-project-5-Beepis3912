using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// FSM States for the enemy
public enum EnemyState { STATIC, CHASE, REST, MOVING, DEFAULT };

public enum EnemyBehavior {EnemyBehavior1, EnemyBehavior2, EnemyBehavior3 };

public class Enemy : MonoBehaviour
{
    //pathfinding
    protected PathFinder pathFinder;
    public GenerateMap mapGenerator;
    protected Queue<Tile> path;
    protected GameObject playerGameObject;

    public Tile currentTile;
    protected Tile targetTile;
    public Vector3 velocity;

    //properties
    public float speed = 1.0f;
    public float visionDistance = 5;
    public int maxCounter = 5;
    protected int playerCloseCounter;

    protected EnemyState state = EnemyState.DEFAULT;
    protected Material material;

    public EnemyBehavior behavior = EnemyBehavior.EnemyBehavior1;


    //added for assitance in enemy behavior
    List<Player> playerList;
    Player closePlayer;

    // Start is called before the first frame update
    void Start()
    {
        path = new Queue<Tile>();
        pathFinder = new PathFinder();
        playerGameObject = GameObject.FindWithTag("Player");
        playerCloseCounter = maxCounter;
        material = GetComponent<MeshRenderer>().material;

        //added for assitance in enemy behavior
        playerList = new List<Player>((Player[])GameObject.FindObjectsByType(typeof(Player), FindObjectsSortMode.None));
    }

    // Update is called once per frame
    void Update()
    {
        if (mapGenerator.state == MapState.DESTROYED) return;

        // Stop Moving the enemy if the player has reached the goal
        if (playerGameObject.GetComponent<Player>().IsGoalReached() || playerGameObject.GetComponent<Player>().IsPlayerDead())
        {
            //Debug.Log("Enemy stopped since the player has reached the goal or the player is dead");
            return;
        }

        switch(behavior)
        {
            case EnemyBehavior.EnemyBehavior1:
                HandleEnemyBehavior1();
                break;
            case EnemyBehavior.EnemyBehavior2:
                HandleEnemyBehavior2();
                break;
            case EnemyBehavior.EnemyBehavior3:
                HandleEnemyBehavior3();
                break;
            default:
                break;
        }

    }

    public void Reset()
    {
        Debug.Log("enemy reset");
        path.Clear();
        state = EnemyState.DEFAULT;
        currentTile = FindWalkableTile();
        transform.position = currentTile.transform.position;
    }

    Tile FindWalkableTile()
    {
        Tile newTarget = null;
        int randomIndex = 0;
        while (newTarget == null || !newTarget.mapTile.Walkable)
        {
            randomIndex = (int)(Random.value * mapGenerator.width * mapGenerator.height - 1);
            newTarget = GameObject.Find("MapGenerator").transform.GetChild(randomIndex).GetComponent<Tile>();
        }
        return newTarget;
    }

    // Dumb Enemy: Keeps Walking in Random direction, Will not chase player
    private void HandleEnemyBehavior1()
    {
        switch (state)
        {
            case EnemyState.DEFAULT: // generate random path 
                
                //Changed the color to white to differentiate from other enemies
                material.color = Color.white;
                
                if (path.Count <= 0) path = pathFinder.RandomPath(currentTile, 20);

                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;
                
                //if target reached
                if (Vector3.Distance(transform.position, targetTile.gameObject.transform.position) <= 0.05f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;
                }

                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }

    // TODO: Enemy chases the player when it is nearby
    private void HandleEnemyBehavior2()
    {
        //enemy moves randomly when player is not nearby
        switch (state)
        {
            case EnemyState.DEFAULT:
                //changed color to blue to differintiate
                material.color = Color.blue;

                if (path.Count <= 0)
                    path = pathFinder.RandomPath(currentTile, 20);
                if(path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                Debug.Log("Moving");
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

                //Debug.Log(Vector3.Distance(transform.position, targetTile.transform.position));

                //if target reached
                if(Vector3.Distance(transform.position, targetTile.transform.position) <= 0.5f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;

                    //decrease playerCloseCounter
                    playerCloseCounter--;

                    ///*
                    //if counter less than zero check for player
                    if (playerCloseCounter <= 0)
                    {
                        Debug.Log("First if");
                        foreach (Player player in playerList)
                        {
                            Debug.Log("foreach");
                            if (Vector3.Distance(player.gameObject.transform.position, transform.position) <= visionDistance)
                            {
                                Debug.Log("second if");
                                closePlayer = player;

                                //if player close reset counter
                                playerCloseCounter = maxCounter;
                                Debug.Log("counter max");
                                break;
                            }
                            
                        }
                        if (playerCloseCounter > 0)
                        {
                            Debug.Log("move to chase");
                            state = EnemyState.CHASE;
                        }

                        else
                            state = EnemyState.DEFAULT;
                    }
                    
                    //*/
                }

                break;
              
            case EnemyState.CHASE:
                Debug.Log("Chase ");
                if (Vector3.Distance(playerGameObject.transform.position, transform.position) <= visionDistance)
                {
                    //Debug.Log("Chase if");
                    //  targetTile = playerGameObject.transform.
                    targetTile = playerGameObject.GetComponent<Player>().currentTile;
                        //Debug.Log("second Chase if");
                        path = pathFinder.FindPathAStar(currentTile, playerGameObject.GetComponent<Player>().currentTile);

                    //offset by two
                    //
                    //if(playerGameObject.GetComponent<Player>().pathFinder.DoneList.Count > 2)
                    

                        //Debug.Log("third Chase if");
                        targetTile = path.Dequeue();
                        state = EnemyState.MOVING;
                }
                break;
            default:
                state = EnemyState.DEFAULT;
                break;
        }   
                
    }

    // TODO: Third behavior (Describe what it does)
    private void HandleEnemyBehavior3()
    {
        //enemy moves randomly when player is not nearby
        switch (state)
        {
            case EnemyState.DEFAULT:
                //changed color to black to differintiate
                material.color = Color.black;

                if (path.Count <= 0)
                    path = pathFinder.RandomPath(currentTile, 20);
                if (path.Count > 0)
                {
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;

            case EnemyState.MOVING:
                Debug.Log("Moving");
                //move
                velocity = targetTile.gameObject.transform.position - transform.position;
                transform.position = transform.position + (velocity.normalized * speed) * Time.deltaTime;

                //Debug.Log(Vector3.Distance(transform.position, targetTile.transform.position));

                //if target reached
                if (Vector3.Distance(transform.position, targetTile.transform.position) <= 0.5f)
                {
                    currentTile = targetTile;
                    state = EnemyState.DEFAULT;

                    //decrease playerCloseCounter
                    playerCloseCounter--;

                    ///*
                    //if counter less than zero check for player
                    if (playerCloseCounter <= 0)
                    {
                        Debug.Log("First if");
                        foreach (Player player in playerList)
                        {
                            Debug.Log("foreach");
                            if (Vector3.Distance(player.gameObject.transform.position, transform.position) <= visionDistance)
                            {
                                Debug.Log("second if");
                                closePlayer = player;

                                //if player close reset counter
                                playerCloseCounter = maxCounter;
                                Debug.Log("counter max");
                                break;
                            }

                        }
                        if (playerCloseCounter > 0)
                        {
                            Debug.Log("move to chase");
                            state = EnemyState.CHASE;
                        }

                        else
                            state = EnemyState.DEFAULT;
                    }

                    //*/
                }

                break;

            case EnemyState.CHASE:
                Debug.Log("Chase ");
                if (Vector3.Distance(playerGameObject.transform.position, transform.position) <= 2 * visionDistance)
                {
                    material.color = Color.cyan;
                    //Debug.Log("Chase if");
                    //  targetTile = playerGameObject.transform.
                    targetTile = playerGameObject.GetComponent<Player>().currentTile;
                    //Debug.Log("second Chase if");
                    int adjacentSize = playerGameObject.GetComponent<Player>().currentTile.Adjacents.Count;
                    path = pathFinder.FindPathAStar(currentTile, playerGameObject.GetComponent<Player>().currentTile.Adjacents[Random.Range(0, adjacentSize - 1)]);

                    //offset by two
                    //
                    //if(playerGameObject.GetComponent<Player>().pathFinder.DoneList.Count > 2)


                    //Debug.Log("third Chase if");
                    targetTile = path.Dequeue();
                    state = EnemyState.MOVING;
                }
                break;


            default:
                state = EnemyState.DEFAULT;
                break;
        }
    }
}
