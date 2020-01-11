

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;        //Allows us to use SceneManager
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

//Player inherits from MovingObject, our base class for objects that can move, Enemy also inherits from this.
public class Player : MovingObject
{
    public float restartLevelDelay = 1f;        //Delay time in seconds to restart level.
    public int pointsPerFood = 10;                //Number of points to add to player food points when picking up a food object.
    public int pointsPerSoda = 20;                //Number of points to add to player food points when picking up a soda object.
    public int wallDamage = 1;                    //How much damage a player does to a wall when chopping it.
    public Text foodText;                           //What the energy is displayed as
    public Text angle;                           //What the energy is displayed as
    public AudioClip moveSound1;                    //Footstep sound effects
    public AudioClip moveSound2;                
    public AudioClip eatSound1;                     //eating soundeffects
    public AudioClip eatSound2;
    public AudioClip drinkSound1;                   //drinking soundeffects
    public AudioClip drinkSound2;
    public AudioClip GameOverSound;                 //gameover soundeffects
    public AudioClip doe;
    public AudioClip ree;
    public AudioClip mee;
    public AudioClip channelStart;
    

    private Animator animator;                    //Used to store a reference to the Player's animator component.
    private int food;                            //Used to store player food points total during level.

    [SerializeField] GameObject Ashe;               //Connect the Ashe tilemap
    [SerializeField] GameObject worldLight;         //Connect the World Light
    Tilemap asheMap;                                //The variable to operate with our Ashe Tile map
    private List<Vector3Int> channeledTiles = new List<Vector3Int>();      //A list of possible locations to place tiles.
    [SerializeField] Tile lightTile;                //Connect the tile that appears while the charater is channeling
    Vector3 tail = new Vector3(0f, 0f, 0f);
    Vector3 head = new Vector3(0f, 0f, 0f);
    float angleBetween;
    // Make a game object
    GameObject lightGameObject;
    // Add the light component
    Light lightComp ;
    private Transform lightHolder;                                    //A variable to store a reference to the transform of our Board object.
    bool channelIsIntended;

    public void Awake()
    {
        //Connect the Game object with the variable
        asheMap = Ashe.GetComponent<Tilemap>();
    }

    //Start overrides the Start function of MovingObject
    protected override void Start()
    {
        //Get a component reference to the Player's animator component
        animator = GetComponent<Animator>();

        foodText.text = "Food:" + food;
        //Get the current food point total stored in GameManager.instance between levels.
        food = GameManager.instance.playerFoodPoints;

        //Call the Start function of the MovingObject base class.
        base.Start();
    }


    //This function is called when the behaviour becomes disabled or inactive.
    private void OnDisable()
    {
        //When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
        GameManager.instance.playerFoodPoints = food;
    }


    private void Update()
    {
        //If it's not the player's turn, exit the function.
        if (!GameManager.instance.playersTurn) return;

        float horizontal = 0;      //Used to store the horizontal move direction.
        float vertical = 0;        //Used to store the vertical move direction.
        


        //Controls for movenent the map
        if (Input.GetKeyDown(KeyCode.W))
        {
            horizontal = 0f;
            vertical = 0.5f;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            horizontal = 0.75f;
            vertical = 0.25f;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            horizontal = 0.75f;
            vertical = -0.25f;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            horizontal = 0;
            vertical = -0.5f;
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            horizontal = -0.75f;
            vertical = -0.25f;
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            horizontal = -0.75f;
            vertical = 0.25f;
        }

        //Find the position of the Player on the cell by its global postion
        Vector3Int cellPosition = asheMap.WorldToCell(transform.localPosition);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            channeledTiles.Add(cellPosition);
            // Make a game object
            lightGameObject = new GameObject("TheLight");


            // Add the light component
            lightComp = lightGameObject.AddComponent<Light>();

            // Set color, intensity,range and tag of the light
            lightComp.color = Color.blue;
            lightComp.intensity = 10;
            lightComp.range = 1;
            lightComp.tag = "Light";

            // Set the position of the light (or any transform property)
            lightGameObject.transform.position = transform.localPosition + new Vector3(0, 0, -.3f);

            //The player Very Obviously means to channel
            channelIsIntended = true;

            SoundManager.instance.musicSource.volume = .005f;
            SoundManager.instance.PlaySingle(channelStart);
        }
            if (Input.GetKey(KeyCode.Space)&& (channeledTiles.Count<4)&& (channelIsIntended))
        {   //Instantiate Board and set boardHolder to its transform.
            
            asheMap.SetTile(cellPosition, lightTile);
            worldLight.SetActive(false);
            if (horizontal != 0 || vertical != 0)
            {
                channeledTiles.Add(cellPosition);
                // Make a game object
                lightGameObject = new GameObject("TheLight");


                // Add the light component
                lightComp = lightGameObject.AddComponent<Light>();

                // Set color, intensity,range and tag of the light
                lightComp.color = Color.blue;
                lightComp.intensity = 10;
                lightComp.range = 1;
                lightComp.tag = "Light";

                // Set the position of the light (or any transform property)
                lightGameObject.transform.position = transform.localPosition + new Vector3(horizontal, vertical, -.3f);
                
            }
        }
        else
        {
            //Reset the channeled tiles and world light
            channeledTiles.Clear();
            asheMap.ClearAllTiles();
            worldLight.SetActive(true);
            SoundManager.instance.musicSource.volume = .15f;


            // Function for clearing the channel light
            GameObject[] respawns; respawns = GameObject.FindGameObjectsWithTag("Light");
            for (int i = 0; i < respawns.Length; i++)
            {
                GameObject.Destroy(respawns[i]);
            }
            channelIsIntended = false;
        }
        SoundManager.instance.efxSource.volume = 1f;
        if (channeledTiles.Count == 2)
        {
            SoundManager.instance.efxSource.volume = .3f;
        }
        else if (channeledTiles.Count == 3)
        {
            SoundManager.instance.efxSource.volume = 1f;
        }
        //Check if we have a non-zero value for horizontal or vertical
        if (horizontal != 0 || vertical != 0)
        {
            head = new Vector3(horizontal, vertical, 0);
            // angleBetween is approximately equal to 0.9548
            angleBetween = Vector3.Angle(tail, head);
            angle.text = "Angle:" + Mathf.Ceil(angleBetween);
            tail = head;
            //Call AttemptMove passing in the generic parameter Wall, since that is what Player may interact with if they encounter one (by attacking it)
            //Pass in horizontal and vertical as parameters to specify the direction to move Player in.\
            animator.SetTrigger("movingT");
            AttemptMove<Wall>(horizontal, vertical);
            animator.SetFloat("moveX", horizontal);
            animator.SetFloat("moveY", vertical);
            
        }
        

    }
        

    //AttemptMove overrides the AttemptMove function in the base class MovingObject
    //AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
    protected override void AttemptMove<T>(float xDir, float yDir)
    {
        //Every time player moves, subtract from food points total.
        food--;
        foodText.text = "Food:" + food;
        //Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.
        base.AttemptMove<T>(xDir, yDir);

        //Hit allows us to reference the result of the Linecast done in Move.
        RaycastHit2D hit;

        //If Move returns true, meaning Player was able to move into an empty space.
        if (Move(xDir, yDir, out hit))
        {
            if (channeledTiles.Count < 4&& 0 < channeledTiles.Count)
            {



                if (angleBetween == 0f || angleBetween == 180f)
                {
                    SoundManager.instance.PlaySingle(doe);
                }
                else if (angleBetween <90f)
                {
                    SoundManager.instance.PlaySingle(ree);
                }
                else if (angleBetween > 90f )
                {
                    SoundManager.instance.PlaySingle(mee);
                }
            }
            else
            {
                //Call RandomizeSfx of SoundManager to play the move sound, passing in two audio clips to choose from.
                SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
            }
        }

        //Since the player has moved and lost food points, check if the game has ended.
        CheckIfGameOver();

        //Set the playersTurn boolean of GameManager to false now that players turn is over.
        GameManager.instance.playersTurn = false;
    }


    //OnCantMove overrides the abstract function OnCantMove in MovingObject.
    //It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
    protected override void OnCantMove<T>(T component)
    {
        //Set hitWall to equal the component passed in as a parameter.
        Wall hitWall = component as Wall;

        //Call the DamageWall function of the Wall we are hitting.
        hitWall.DamageWall(wallDamage);

        //Set the attack trigger of the player's animation controller in order to play the player's attack animation.
        animator.SetTrigger("PlayerChop");
    }


    //OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Check if the tag of the trigger collided with is Exit.
        if (other.tag == "Exit")
        {
            //Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
            Invoke("Restart", restartLevelDelay);

            //Disable the player object since level is over.
            enabled = false;
        }

        //Check if the tag of the trigger collided with is Food.
        else if (other.tag == "Food")
        {
            //Add pointsPerFood to the players current food total.
            food += pointsPerFood;
            foodText.text = "Food:" + food;
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            //Disable the food object the player collided with.
            other.gameObject.SetActive(false);
        }

        //Check if the tag of the trigger collided with is Soda.
        else if (other.tag == "Soda")
        {
            //Add pointsPerSoda to players food points total
            food += pointsPerSoda;
            foodText.text = "Food:" + food;
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            //Disable the soda object the player collided with.
            other.gameObject.SetActive(false);
        }
    }


    //Restart reloads the scene when called.
    private void Restart()
    {
        //Load the last scene loaded, in this case Main, the only scene in the game.
        SceneManager.LoadScene(0);
    }


    //LoseFood is called when an enemy attacks the player.
    //It takes a parameter loss which specifies how many points to lose.
    public void LoseFood(int loss)
    {
        //Set the trigger for the player animator to transition to the PlayerHurt animation.
        animator.SetTrigger("PlayerHurt");

        //Subtract lost food points from the players total.
        food -= loss;
        foodText.text = "Food:" + food;

        //Check to see if game has ended.
        CheckIfGameOver();
    }


    //CheckIfGameOver checks if the player is out of food points and if so, ends the game.
    private void CheckIfGameOver()
    {
        //Check if food point total is less than or equal to zero.
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(GameOverSound);
            SoundManager.instance.musicSource.Stop();
            //Call the GameOver function of GameManager.
            GameManager.instance.GameOver();
        }
    }
}


