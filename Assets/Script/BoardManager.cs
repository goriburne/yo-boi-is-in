using UnityEngine;
using System;                               //Allows us to use serializable
using System.Collections.Generic;           //Allows us to use Lists.
using Random = UnityEngine.Random;          //Tells Random to use the Unity Engine random number generator.
using UnityEngine.Tilemaps;                 //Allows us to use Ruletiles and tile
using UnityEngine.UI;                       //Allows us to use "Text"

public class BoardManager : MonoBehaviour
    {
        // Using Serializable allows us to embed a class with sub properties in the inspector.
        [Serializable]
        public class Count
        {
            public int minimum;             //Minimum value for our Count class.
            public int maximum;             //Maximum value for our Count class.


            //Assignment constructor.
            public Count(int min, int max)
            {
                minimum = min;
                maximum = max;
            }
        }


        public int radius = 4;                                         //radius in our game board.
        public int xInt;                                                //Facilitates positive negave switch
        public int scale;                                               //Log of level
        int channelIndex = 0;                                           //Direction the channel moves next
        public Count wallCount = new Count(5, 9);                       //Lower and upper limit for our random number of walls per level.
        public Count foodCount = new Count(1, 5);                       //Lower and upper limit for our random number of food items per level.
        public GameObject exit;                                         //Prefab to spawn for exit.
        public GameObject[] floorTiles;                                 //Array of floor prefabs.
        public GameObject[] wallTiles;                                  //Array of wall prefabs.
        public GameObject[] foodTiles;                                  //Array of food prefabs.
        public GameObject[] enemyTiles;                                 //Array of enemy prefabs.
        public GameObject[] outerWallTiles;                             //Array of outer tile prefabs.
        private Transform boardHolder;                                  //A variable to store a reference to the transform of our Board object.
        [SerializeField] GameObject Default;                            //An Assigned TileMap for my channel rule tile
        private List<Vector3> gridPositions = new List<Vector3>();      //A list of possible locations to place tiles.
        private List<Vector3Int> channelDirections = new List<Vector3Int>();//Adjacent hexes directions
        public RuleTile Channel;                                        //Imports the Gray Channel rule tile I created
        Tilemap chMap;                                                  //A variable timemap to contain the default map
        public Text Debug;                                              //Displays information i will ask it for on the game screen


        public void Awake()
        {
            //Set chMap to Default tilemap
            chMap = Default.GetComponent<Tilemap>();
            channelIndex = Random.Range(0, 5);
        
        }

        //Clears our list gridPositions, ch tilemap and prepares it to generate a new board.
        void InitialiseList()
        {
            //Clear our list gridPositions.
            gridPositions.Clear();
            //Clear our channel tilemap.
            chMap.RefreshAllTiles();


            //creates a grid point at origin
            gridPositions.Add(new Vector3(0f, 0f, 0f));
            for (float r = 0 ; r < radius-1; r++)
            {
                float x = 0;
                float y = 0;
                y = y + .5f * r;
                
                for (float hexDir = 0; hexDir < 6; hexDir++)
                {
                    for(float steps=0; steps<r;steps++)
                    {
                    x =x + (1 - Mathf.Floor(hexDir / 3) * 2) * (1 - hexDir % 3)*.75f;
                    y =y + (Mathf.Floor(hexDir / 3) * 2 - 1) * (2 - Mathf.Abs(1 - hexDir % 3))*.25f;
                    gridPositions.Add(new Vector3(x, y, 0f));
                    }
                }
            }
            
        channelDirections.Add(new Vector3Int(1, 0, 0));//0,1. 1,0. 1,-1. 0,-1. -1,-1. -1,0.
        channelDirections.Add(new Vector3Int(0, 1, 0));
        channelDirections.Add(new Vector3Int(-1, 1, 0));
        channelDirections.Add(new Vector3Int(-1, 0, 0));
        channelDirections.Add(new Vector3Int(-1, -1, 0));
        channelDirections.Add(new Vector3Int(0, -1, 0));
    }
       
        //Sets up the outer walls and floor (background) of the game board.
        void BoardSetup()
        {
            //Instantiate Board and set boardHolder to its transform.
            boardHolder = new GameObject("Board").transform;



            GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
            GameObject instance = Instantiate(toInstantiate, new Vector3(0, 0, 0f), Quaternion.identity) as GameObject;
            for (float r = 0; r < radius + 1; r++)
            {
                float x = 0;
                float y = 0;


                y = y + .5f * r;

                for (float hexDir = 0; hexDir < 6; hexDir++)
                {
                    for (float steps = 0; steps < r; steps++)
                    {
                        toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                        if (r == radius)
                            toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                        x = x + (1 - Mathf.Floor(hexDir / 3) * 2) * (1 - hexDir % 3) * .75f;
                        y = y + (Mathf.Floor(hexDir / 3) * 2 - 1) * (2 - Mathf.Abs(1 - hexDir % 3)) * .25f;
                        instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
                        //Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                        instance.transform.SetParent(boardHolder);
                        //Refresh the tile map to show the new positions in the tilemap.
                        chMap.RefreshAllTiles();
                        //Console.WriteLine("Radius:"+r+", Direction:"+ hexDir + ", Step:" + steps + ", X Change:" + (1 - Mathf.Floor(hexDir / 3) * 2) * (1 - hexDir % 3) * .75f + ", Y Change:" + (Mathf.Floor(hexDir / 3) * 2 - 1) * (2 - Mathf.Abs(1 - hexDir % 3)) * .25f);
                    }
                }

            }


        }
        
        void ChannelDraw()
        {
            Color cholor = Random.ColorHSV();
            Vector3 chPosition = RandomPosition();
            Vector3Int chPositionInt = new Vector3Int((int)chPosition.x, (int)chPosition.y, 0);
            chMap.SetTile(chPositionInt, Channel);
            chMap.SetColor(chPositionInt, cholor);

            for (float i = 0; i < 4+.5*scale; i++)
            {
                channelIndex =Random.Range((channelIndex + 2) % 6, (channelIndex + 4) % 6);
                int read = (channelIndex + (Mathf.Abs(chPositionInt.y) % 2) * 3) % 6;
                int tive = -1 + 2 * ((Mathf.Abs(chPositionInt.y) + 1) % 2);
                //Debug.text = "Channel index:" + channelIndex + "chPositionInt.y:" + chPositionInt.y + "Read:" + read+ "Tive:" + tive;
                chPositionInt = chPositionInt + channelDirections[read] *(tive);
                //Set channel tiles *Testing*.
                chMap.SetTile(chPositionInt, Channel);
                chMap.SetColor(chPositionInt, cholor);
                channelIndex = (channelIndex + 3) % 6;
            }
        
        }
        
        //RandomPosition returns a random position from our list gridPositions.
        Vector3 RandomPosition()
        {
            //Declare an integer randomIndex, set it's value to a random number between 0 and the count of items in our List gridPositions.
            int randomIndex = Random.Range(0, gridPositions.Count);

            //Declare a variable of type Vector3 called randomPosition, set it's value to the entry at randomIndex from our List gridPositions.
            Vector3 randomPosition = gridPositions[randomIndex];

            //Remove the entry at randomIndex from the list so that it can't be re-used.
            gridPositions.RemoveAt(randomIndex);

            //Return the randomly selected Vector3 position.
            return randomPosition;
        }


        //LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
        void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum)
        {
            //Choose a random number of objects to instantiate within the minimum and maximum limits
            int objectCount = Random.Range(minimum, maximum + 1);

            //Instantiate objects until the randomly chosen limit objectCount is reached
            for (int i = 0; i < objectCount; i++)
            {
                //Choose a position for randomPosition by getting a random position from our list of available Vector3s stored in gridPosition
                Vector3 randomPosition = RandomPosition();

                //Choose a random tile from tileArray and assign it to tileChoice
                GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];

                //Instantiate tileChoice at the position returned by RandomPosition with no change in rotation
                Instantiate(tileChoice, randomPosition, Quaternion.identity);
            }
        }


        //SetupScene initializes our level and calls the previous functions to lay out the game board
        public void SetupScene(int level)
        {
            //Find the gameobject called default and get its tile component
            chMap = GameObject.Find("Default").GetComponent<Tilemap>();

            //Remove all tiles on our ch tile map
            chMap.ClearAllTiles();

            //Scale is a variable that affects Boardsize, Ashe and Enemy Counts;
            scale = (int)Mathf.Log(level, 2f);

            //Number of radius in our game board.
            radius = 4+ scale;                                             


            //Creates the outer walls and floor.
            wallCount = new Count(3+ scale, 5 + scale) ;

            //Lower and upper limit for our random number of walls per level.
            foodCount = new Count(1+ scale, 5 + scale) ;

            //Reset our list of gridpositions.
            InitialiseList();

            //Sets up the outer walls and floor (background) of the game board.
            BoardSetup();

            for (int i=0;i<2+scale;i++)
            ChannelDraw();


            //Instantiate a random number of wall tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(wallTiles, wallCount.minimum, wallCount.maximum);

            //Instantiate a random number of food tiles based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(foodTiles, foodCount.minimum, foodCount.maximum);

            //Determine number of enemies based on current level number, based on a logarithmic progression
            int enemyCount = +scale;

            //Instantiate a random number of enemies based on minimum and maximum, at randomized positions.
            LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
            //Instantiate the exit tile in the upper right hand corner of our game board
            if (Random.Range(-1f, 1f) > ((radius+1) /(radius* 3f)))
            {
            Instantiate(exit,new Vector3((-1 + 2 * (int)Random.Range(0, 2)) * (radius - 1) * .75f,(int)Random.Range(0, radius/2) * .5f- (((radius+1) % 2))*.25f,0f),Quaternion.identity);


            }
            else
            {
            int c = (int)Random.Range(0, radius - 1);
            Instantiate(exit, new Vector3(
                (-1 + 2 * (int)Random.Range(0, 2)) * c * .75f,
                (-1 + 2 * (int)Random.Range(0, 2))* ((radius-1)*.5f-c * .25f),
                0f), Quaternion.identity);

            }

    }
    }
