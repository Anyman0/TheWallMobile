using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameScript : MonoBehaviour
{

    // Wall material for the level
    private Material loadedWall;

    // Climber attributes
    private GameObject climber;
    private double climberSpawnTimer;
    private double climberSpawn;
    private double climbST = 0;
    private float climberSpeedInWave;

    // Boolean to check if Menu is set to active
    [HideInInspector]
    public bool isMenuVisible;

    // Game attributes
    private List<Vector3> spawnLocations;
    private Touch touch;
    private Vector2 touchPosition;
    public bool IsInvoked;
    private Vector3 location;
    private Quaternion zRotation;
    public GameObject MenuView;
    private List<GameObject> climbersInPlay;
    private GameObject rumbleParticle;
    private GameObject waveViewObject;
    private bool isWaveView;
    private bool increase;
    private bool goToWaveView = true;
    private Image waveViewImage;
    private float colorA;
    private int[] waveCounts;
    private int waves = 1;
    [HideInInspector]
    public int addedTimePerWave;
    private int prevAddedTime;


    // Weapon attributes
    // Bow
    private Quaternion rotationY;
    private Quaternion rotationYDown;
    private float rotateSpeedModifier = 0.01f;
    private Quaternion bowCenter; // Basic rotation value of the weapon
    private float xRotation; // This is used to count the weapon rotation from bowCenter
    private float weaponInputCenter; // Basic position value of the weapon
    private float weaponYPosition; // This is used to calculate the position of touch/mouse from bowCenter   
    private AudioSource bowShoot;
    // Barrels
    public List<GameObject> projectiles;
    public List<GameObject> barrels;
    private GameObject firedProjectile;
    private Transform hitBarrel;
    private float barrelCenter; // Basic position value of the barrel
    private float barrelX; // This is used to calculate the posiition of touch/mouse from barrelCenter
    private Vector3 barrelPosition;
    private float UpOrDown = 0f;
    private float oilDropCooldown; // This is value used to set cooldown for dropping oil 
    private bool canDropOil;
    private AudioSource oilDrop;
    // Scythe
    private Quaternion scytheOrigin;
    private Vector3 chainOrigin;
    private GameObject chain; // For setting it active, since we can't find the object when it's set to inactive
    private bool canWeDrop;
    private bool returnBool;
    private AudioSource rumbleAudio;

    // Player attributes
    public int climbersDropped;
    private float timeSurvived;
    private Text climbersDroppedText;
    private Text timeSurvivedText;
    [HideInInspector]
    public float surviveTimer;

    // Get scripts
    MainMenuScript.Level lvl;
    CollisionScript cs;
    private MainMenuScript mms;   

    private void Awake() // Awake() works as the initializer here 
    {
        //DontDestroyOnLoad(this); // Scene stays active after swapping to MenuView. 
        lvl = new MainMenuScript.Level();
        mms = GameObject.FindGameObjectWithTag("MainMenu").GetComponent<MainMenuScript>();        
        climberSpawn = 5;
        spawnLocations = new List<Vector3>();

        // As we are NOT destroying the Menu on load, we hide it here instead. // TODO: Is this a performance issue?
        MenuView = GameObject.FindGameObjectWithTag("MainMenu");
        GameObject.FindGameObjectWithTag("MainMenu").SetActive(false);

        // Initialize texts
        timeSurvivedText = GameObject.FindGameObjectWithTag("SurvivedText").GetComponent<Text>();
        climbersDroppedText = GameObject.FindGameObjectWithTag("ClimbersText").GetComponent<Text>();

        // Get Scythe and Chain origin positions
        scytheOrigin = GameObject.FindGameObjectWithTag("Scythe").transform.rotation;

        GameObject.FindGameObjectWithTag("PauseButton").SetActive(true);
        GameObject.FindGameObjectWithTag("GameStatsPanel").SetActive(true);

        // Get Rumble
        rumbleAudio = GameObject.FindGameObjectWithTag("Scythe").GetComponent<AudioSource>();
        rumbleParticle = GameObject.Find("Rumble");
        rumbleParticle.SetActive(false);

        // Get and hide WaveView
        waveViewImage = GameObject.FindGameObjectWithTag("WaveView").GetComponent<Image>();
        waveViewImage.CrossFadeAlpha(0, 0.1f, true);
        colorA = waveViewImage.color.a;
        colorA = 0;
        waveViewImage.SetNativeSize();
        waveViewObject = GameObject.FindGameObjectWithTag("WaveView");
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadLevel(0);
        AddSpawnLocations();

        // Here we make WaveCounts include values from 10 to 200. 
        waveCounts = new int[20];
        int v = 10;
        for(int i = 0; i <= 19; i++)
        {

            waveCounts[i] = v;
            v += 10;          
        }
        
    }

    // TODO: Spawntime of climbers should be determined by time survived. Need a timer for this 
    void FixedUpdate() 
    {
        //prevAddedTime = addedTimePerWave;
               
        if (waves == 1)
        {
            climberSpawn = 5;
            climberSpeedInWave = 1;
            addedTimePerWave = 40;
        }
        else if(waves == 2)
        {
            climberSpawn = 4;
            climberSpeedInWave = 1;
            addedTimePerWave = 80;
        }
        else if(waves == 3)
        {
            climberSpawn = 4;
            climberSpeedInWave = 2;
            addedTimePerWave = 120;
        }
        else if(waves == 4)
        {
            climberSpawn = 3;
            climberSpeedInWave = 3;
            addedTimePerWave = 160;
        }
        else if(waves == 5)
        {
            climberSpawn = 2;
            climberSpeedInWave = 3;
            addedTimePerWave = 200;
        }
            
       
        if(!isMenuVisible)
        {
            if(!isWaveView)
            {
                surviveTimer += 0.015f;
                climberSpawnTimer += 0.015f;               
            }
                       
            // Debug.Log("ClimbST: " + climbST + ". And climberSpawnTimer: " + climberSpawnTimer + ". And climberSpawn: " + climberSpawn + ". SurviveTimer: " + surviveTimer);
            //Debug.Log("Value of climbST: " + climbST + ". And the value of climberSpawnTimer: " + climberSpawnTimer + ". And surviveTimer: " + surviveTimer);
            if(surviveTimer < 10f) // At level 1. Start
            {
                //climberSpawn = 5;
                if(climbST <= climberSpawnTimer)
                {
                    climbST = climberSpawnTimer + climberSpawn;
                    var climber = mms.levels[0].climbers[0];
                    var climbSpeedMultiplier = climberSpeedInWave; //mms.levels[0].climberSpeed;
                    var weapon = mms.levels[0].weapons[0];
                    var barrel = barrels[0];
                    var existingWeapon = GameObject.FindGameObjectWithTag("Weapon");
                    var existingBarrel = GameObject.FindGameObjectWithTag("Barrel");

                    AddClimber(climber, climbSpeedMultiplier);
                    if(existingWeapon == null) AddWeapon(weapon);
                    if (existingBarrel == null) AddBarrel(barrel);
                    LoadLevel(waves);
                }

            }

            else if(surviveTimer >= waveCounts[0] && surviveTimer < waveCounts[1]) // At level 2
            {
                                                              
                if(!isWaveView)
                {
                    
                    if (climbST <= climberSpawnTimer)
                    {
                        climbST = climberSpawnTimer + climberSpawn;
                        var climber = mms.levels[1].climbers[0];
                        var climbSpeedMultiplier = climberSpeedInWave;//mms.levels[1].climberSpeed;

                        AddClimber(climber, climbSpeedMultiplier);                       
                    }
                }
                
            }
            
            else if(surviveTimer > waveCounts[1] && surviveTimer < waveCounts[2]) // At level 3
            {
                
                if(climbST <= climberSpawnTimer)
                {
                    climbST = climberSpawnTimer + climberSpawn - 1;
                    var climber = mms.levels[2].climbers[0];
                    var climbSpeedMultiplier = climberSpeedInWave;//mms.levels[2].climberSpeed;
                    AddClimber(climber, climbSpeedMultiplier);
                    
                }
            }

            else if(surviveTimer > waveCounts[2] && surviveTimer < waveCounts[3]) // At level 4
            {
                
                if(climbST <= climberSpawnTimer)
                {
                    climbST = climberSpawnTimer + climberSpawn;
                    var climber = mms.levels[2].climbers[0];
                    var climbSpeedMultiplier = climberSpeedInWave;
                    AddClimber(climber, climbSpeedMultiplier);
                    
                }
            }
            
            if(GameObject.FindGameObjectWithTag("Climbers").transform.childCount == 0 && surviveTimer >= waveCounts[3])
            {                
                waves++;
                surviveTimer = 0;
                climberSpawnTimer = 0;
                climbST = 0;
                oilDropCooldown = 0;
                GameObject.FindGameObjectWithTag("WaveText").GetComponent<Text>().text = "NEXT WAVE..!";
                GameObject.FindGameObjectWithTag("WaveText").GetComponent<Text>().fontSize = 10;
                WaveView();
            }

                               
             
            // IF user has chosen Keyboard&Mouse inputs for testing
            if(mms.ChosenInput == MainMenuScript.chosenInput.KeyboardMouse)
            {
                
                if (Input.GetButton("Fire1"))
                {
                    var weapon = GameObject.FindGameObjectWithTag("Weapon");
                    var barrel = GameObject.FindGameObjectWithTag("Barrel");
                    var mousePos = Input.mousePosition;
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit))
                    {
                        
                        if (hit.transform.tag == "Weapon")
                        {

                            if (UpOrDown == 0)
                            {
                                rotationY = Quaternion.Euler(0f, 0f, -mousePos.y * rotateSpeedModifier);
                                rotationYDown = Quaternion.Euler(0f, 0f, mousePos.y * rotateSpeedModifier);
                            }

                            //Debug.Log("mousePos.y: " + mousePos + ". And rotationY: " + rotationY + ". And rotationYDown: " + rotationYDown + ". WepCenter " + weaponInputCenter + ". And ray: " + ray);
                            UpOrDown = mousePos.y / 100;

                            if (UpOrDown > 0.6f)
                            {
                                //xRotation = (float)Math.Round(Mathf.Abs(bowCenter - hit.transform.rotation.x), 2);
                                weaponYPosition = (float)Math.Round(Mathf.Abs(weaponInputCenter - mousePos.y), 2);
                                mousePos.y = weaponYPosition;
                                weapon.transform.rotation = rotationY * weapon.transform.rotation;
                                //Debug.Log("mousePos.y: " + mousePos.y / 100 + ". And rotationY: " + rotationY);                          
                            }
                            else if (UpOrDown <= 0.4f)
                            {
                                weaponYPosition = (float)Math.Round(Mathf.Abs(weaponInputCenter - mousePos.y), 2);
                                mousePos.y = weaponYPosition;
                                weapon.transform.rotation = rotationYDown * weapon.transform.rotation;
                                //Debug.Log("mousePos.y: " + mousePos.y / 100 + ". And rotationYDown: " + rotationYDown);
                            }

                             
                            rotationY = Quaternion.Euler(0f, 0f, -mousePos.y * rotateSpeedModifier);
                            rotationYDown = Quaternion.Euler(0f, 0f, mousePos.y * rotateSpeedModifier);
                            //Debug.Log("mousePos.Y:  " + mousePos.y + " And UpOrDown: " + UpOrDown);

                        }

                        else if (hit.transform.tag == "Barrel")
                        {

                            /*Debug.Log(hit.transform.name + " And the center is: " + barrelCenter + ". And the center in game: " + hit.transform.position.x);
                            barrelX =  (float)Math.Round(Mathf.Abs(hit.transform.position.x - mousePos.x), 2);
                            var animator = barrel.GetComponent<Animator>();
                            var LeftOrRight = mousePos.x / 100;
                            if(LeftOrRight > 4.15)
                            {
                                animator.SetFloat("X", barrelX);
                            }
                            else if(LeftOrRight <= 4.15)
                            {
                                animator.SetFloat("X", barrelX * -1);
                            }
                            // Movement now done in DragScript which is attached to the barrel
                            // TODO: Maybe do it with animation instead? Makes it smoother?

                            Debug.Log(barrelX / 10 + ". And mouseX: " + mousePos.x + ". Moving left or right: " + LeftOrRight); // 54.8 -- 30.8  -- 10.8 */
                        }
                    }

                }

                if (Input.GetKeyDown(KeyCode.Space))
                {
                   
                    foreach (var projectile in projectiles)
                    {
                        if (projectile.tag == "Arrow")
                        {
                            Debug.Log(projectile);
                            firedProjectile = projectile;
                        }

                    }

                    // Restricts shooting to one arrow at a time
                    var arrow = GameObject.FindGameObjectWithTag("Arrow");
                    if (arrow == null)
                    {
                        FireProjectiles(firedProjectile);
                        bowShoot = GameObject.FindGameObjectWithTag("Weapon").GetComponent<AudioSource>();
                        bowShoot.Play();
                    }


                }

                if (Input.GetKeyDown(KeyCode.P) && canDropOil == true)
                {
                    foreach (var projectile in projectiles)
                    {
                        if (projectile.name == "Oil")
                        {                             
                            Debug.Log(projectile);
                            firedProjectile = projectile;
                        }
                    }
                    var oil = GameObject.Find("Oil");
                    if (oil == null)
                    {
                        FireProjectiles(firedProjectile);
                        oilDrop = GameObject.FindGameObjectWithTag("Barrel").GetComponent<AudioSource>();
                        oilDrop.Play();
                    }

                    var barrelAnim = GameObject.FindGameObjectWithTag("Barrel").GetComponent<Animation>();
                    if (!barrelAnim.isPlaying)
                    {
                        barrelAnim.Play("BarrelRotation", PlayMode.StopSameLayer);
                    }

                    oilDropCooldown = surviveTimer;
                    canDropOil = false;
                    // Could still give below a chance. Maybe try and change the oil attributes?
                    //spawner.Spawn(); // Spawns oil from barrel! <<<<<<<------------ This is related to the waterspawner script. In that the oil-object cant collide.
                }

                if (Input.GetKeyDown(KeyCode.L))
                {
                    // TODO: Write a method to drop the scythe and then return to the start-state
                    /*var chain = GameObject.FindGameObjectWithTag("Chain");
                    if (chain != null) ReturnScythe();
                    else if (chain == null) DropScythe();*/

                    if (!canWeDrop) canWeDrop = true;
                    rumbleAudio.Play();
                }
            }

            // IF user has chosen TouchInput for mobile
            if(mms.ChosenInput == MainMenuScript.chosenInput.TouchInput)
            {

                // IS supposed to be touch version of rotation an object
                if (Input.touchCount > 0)
                {
                    touch = Input.GetTouch(0);
                    var weapon = GameObject.FindGameObjectWithTag("Weapon");
                    touchPosition = Input.GetTouch(0).position;
                    RaycastHit hit;
                    Ray ray = Camera.main.ScreenPointToRay(touchPosition);
                    
                    if (Physics.Raycast(ray, out hit))
                    {
                        
                        if (hit.transform.tag == "Weapon")
                        {
                            
                            if (UpOrDown == 0)
                            {
                                rotationY = Quaternion.Euler(0f, 0f, -touchPosition.y * rotateSpeedModifier);
                                rotationYDown = Quaternion.Euler(0f, 0f, touchPosition.y * rotateSpeedModifier);
                            }

                            UpOrDown = touchPosition.y / 100;
                            Debug.Log(UpOrDown + " touchPositionY: " + touchPosition.y);
                            if (UpOrDown > 0.6f)
                            {
                                //xRotation = (float)Math.Round(Mathf.Abs(bowCenter - hit.transform.rotation.x), 2);
                                weaponYPosition = (float)Math.Round(Mathf.Abs(weaponInputCenter - touchPosition.y), 2);
                                touchPosition.y = weaponYPosition;
                                weapon.transform.rotation = rotationY * weapon.transform.rotation;
                                //Debug.Log("mousePos.y: " + mousePos.y / 100 + ". And rotationY: " + rotationY);                          
                            }
                            else if (UpOrDown <= 0.4f)
                            {
                                weaponYPosition = (float)Math.Round(Mathf.Abs(weaponInputCenter - touchPosition.y), 2);
                                touchPosition.y = weaponYPosition;
                                weapon.transform.rotation = rotationYDown * weapon.transform.rotation;
                                //Debug.Log("mousePos.y: " + mousePos.y / 100 + ". And rotationYDown: " + rotationYDown);
                            }

                            // TODO: Put logic here. What happens when player holds finger(or in this testing-case mouse) on object. 
                            // Rotation. 
                            rotationY = Quaternion.Euler(0f, 0f, -touchPosition.y * rotateSpeedModifier);
                            rotationYDown = Quaternion.Euler(0f, 0f, touchPosition.y * rotateSpeedModifier);
                            //Debug.Log("mousePos.Y:  " + mousePos.y + " And UpOrDown: " + UpOrDown);


                            // Below works, but trying to implement same working solution from mouseInput above
                            /*rotationY = Quaternion.Euler(0f, -touch.deltaPosition.x * rotateSpeedModifier, 0f);
                            weapon.transform.rotation = rotationY * weapon.transform.rotation;*/
                        }

                        // This is where we fire arrow if player releases finger on bow
                        if(touch.phase == TouchPhase.Ended && hit.transform.tag == "Weapon")
                        {
                            foreach (var projectile in projectiles)
                            {
                                if (projectile.tag == "Arrow")
                                {
                                    Debug.Log(projectile);
                                    firedProjectile = projectile;
                                }

                            }

                            // Restricts shooting to one arrow at a time
                            var arrow = GameObject.FindGameObjectWithTag("Arrow");
                            if (arrow == null)
                            {
                                FireProjectiles(firedProjectile);
                                bowShoot = GameObject.FindGameObjectWithTag("Weapon").GetComponent<AudioSource>();
                                bowShoot.Play();
                            }
                        }

                        // This is where we drop the oil if player releases finger on barrel
                        if(touch.phase == TouchPhase.Ended && hit.transform.tag == "Barrel" && canDropOil )
                        {
                            foreach (var projectile in projectiles)
                            {
                                if (projectile.name == "Oil")
                                {
                                    Debug.Log(projectile);
                                    firedProjectile = projectile;
                                }
                            }
                            var oil = GameObject.Find("Oil");
                            if (oil == null)
                            {
                                FireProjectiles(firedProjectile);
                                oilDrop = GameObject.FindGameObjectWithTag("Barrel").GetComponent<AudioSource>();
                                oilDrop.Play();
                            }

                            var barrelAnim = GameObject.FindGameObjectWithTag("Barrel").GetComponent<Animation>();
                            if (!barrelAnim.isPlaying)
                            {
                                barrelAnim.Play("BarrelRotation", PlayMode.StopSameLayer);
                            }

                            oilDropCooldown = surviveTimer;
                            canDropOil = false;
                        }

                        if(Input.touchCount == 2)
                        {
                            Debug.Log("Two fingers on screen. Dropping scythe!");
                            if (!canWeDrop) canWeDrop = true;
                            rumbleAudio.Play();
                        }
                    }
                    
                }

            }
                       
            // Below code drops the scythe and then returns it to its origin.         
            if(canWeDrop)
            {
                if(!returnBool)
                {
                    DropScythe();
                }
                
                var scythePos = GameObject.FindGameObjectWithTag("Scythe");
                
                if(scythePos.transform.rotation.z > 0.9f)
                {
                    Debug.Log("At over 0.7f. Actual value is here: " + scythePos.transform.rotation);
                    returnBool = true;
                    rumbleParticle.SetActive(true);
                }

                if(returnBool)
                {
                    ReturnScythe();
                    if(scythePos.transform.rotation.z <= -0.7f)
                    {
                        scythePos.transform.rotation = scytheOrigin;
                        returnBool = false;
                        canWeDrop = false;
                        
                    }
                }
            }

            if(!rumbleAudio.isPlaying)
            {
                rumbleParticle.SetActive(false);
            }

        }
        
        climbersDroppedText.text = "Climbers Dropped: " + climbersDropped;          
        timeSurvivedText.text = "Time Survived: \n" + (int)surviveTimer; 
        if(increase) InvokeRepeating("fontSize", 1, 3);

        //Debug.Log(GameObject.FindGameObjectWithTag("SurvivedText") + ". And the actual text in it: " + timeSurvivedText.text);

        // Set a cooldown for dropping oil. TODO: Make it visible to the player somehow. Maybe change opacity of the barrel?
        if (surviveTimer - oilDropCooldown > 10 && canDropOil == false)
        {
            canDropOil = true;            
        }
    }


    // This method is called to load the "Start" - settings. Timer starts from 0.
    public void LoadLevel(int level)
    {
        try
        {
            if(level == 0)
            {
                loadedWall = mms.walls[level];
                GameObject.FindGameObjectWithTag("Wall").GetComponent<MeshRenderer>().material = loadedWall;
            }
            else if(level == 1)
            {
                loadedWall = mms.walls[level];
                GameObject.FindGameObjectWithTag("Wall").GetComponent<MeshRenderer>().material = loadedWall;
            }
            else if(level == 2)
            {
                loadedWall = mms.walls[level];
                GameObject.FindGameObjectWithTag("Wall").GetComponent<MeshRenderer>().material = loadedWall;
            }
                                   
        }

        catch
        {
            Debug.Log("Whoopsie! Couldn't load Wall O.O");
        }
                          
    }

    // Fill the list of spawn locations 
    public void AddSpawnLocations()
    {
        if(spawnLocations.Count == 0)
        {
            for(int i = 410; i < 425; i++)
            {                
                spawnLocations.Add(new Vector3(i, 397f, -28.2f));
            }
        }
    }

    // Method to drop the scythe
    public void DropScythe()
    {
        var scythe = GameObject.FindGameObjectWithTag("Scythe");
        var scytheRotation = Quaternion.Euler(0, 0, scythe.transform.rotation.z + 1f);
        scythe.transform.rotation = scytheRotation * scythe.transform.rotation;
    }

    // Method to return the scythe to the origin position
    public void ReturnScythe()
    {
        var scythe = GameObject.FindGameObjectWithTag("Scythe");
        var scytheRotation = Quaternion.Euler(0, 0, scythe.transform.rotation.z + -1f);
        scythe.transform.rotation = scytheRotation * scythe.transform.rotation;
    }

    // Method to add climber in game. Getting a random spawnposition from a list of spawnpositions
    public void AddClimber(GameObject climber, float cMultiply)
    {        
        var spawnLocs = spawnLocations.ToArray();       
        var climbersObject = GameObject.FindGameObjectWithTag("Climbers");        
        var newClimber = Instantiate(climber, spawnLocations[UnityEngine.Random.Range(0,15)], Quaternion.identity, climbersObject.transform);       
        var animator = newClimber.GetComponent<Animator>();
        animator.SetFloat("ClimbSpeedMultiplier", cMultiply);       
    }

    
    // Method to add players chosen weapon in game.
    public void AddWeapon(GameObject weapon)
    {
       
        var newWeapon = Instantiate(weapon);
        bowCenter = newWeapon.transform.rotation;
        weaponInputCenter = 50f; // TODO: Calculate this position from the object instead of hardcoding it.
    }

    // Method to fire projectiles from players chosen weapon
    public void FireProjectiles(GameObject projectile)
    {
        if(projectile.tag == "Arrow")
        {
            var weapon = GameObject.FindGameObjectWithTag("Weapon");
            location = weapon.transform.position;
            zRotation = projectile.transform.rotation * weapon.transform.rotation;            
        }
        else if(projectile.name == "Oil")
        {
            var oilBarrel = GameObject.FindGameObjectWithTag("Barrel");
            var oilLocationOffset = new Vector3(0f, -2f, -1f);
            location = oilBarrel.transform.position + oilLocationOffset;
            zRotation = projectile.transform.rotation; // * oilBarrel.transform.rotation;
            //DropOil(projectile);
            StartCoroutine(DropOil(projectile, 1.3f));
        }

        if (projectile.name != "Oil")
        {
            var newProjectile = Instantiate(projectile, location, new Quaternion(zRotation.x, zRotation.y, zRotation.z, zRotation.w));
            // Arrow anim
            if (newProjectile.tag == "Arrow")
            {
                newProjectile.GetComponent<Animator>().Play("ShotArrowAnimation", 0);
            }
            
        }

        Invoke("RemoveProjectile", 4f); // This value should come from a given parameter

    }
    
    IEnumerator DropOil(GameObject projectile, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        var newOil = Instantiate(projectile, location, new Quaternion(zRotation.x, zRotation.y, zRotation.z, zRotation.w));
    }

    // Method to remove a fired projectile from game after a given time
    public void RemoveProjectile()
    {
        GameObject projectile = null;
        var arrow = GameObject.FindGameObjectWithTag("Arrow");
        var oil = GameObject.FindGameObjectWithTag("Oil");
        if (arrow != null) projectile = arrow;
        if (oil != null) projectile = oil;
        Destroy(projectile);        
    }


    // Method to spawn the players chosen barrel
    public void AddBarrel(GameObject Barrel)
    {
        
        var newBarrel = Instantiate(Barrel);
        barrelCenter = newBarrel.transform.position.x;
    }

    // This method is called if player pauses the game 
    public void PauseButton()
    {
        var climbersParent = GameObject.FindGameObjectWithTag("Climbers");
        var pauseButton = GameObject.FindGameObjectWithTag("PauseButton");
        Debug.Log("WTF");
        if(!MenuView.activeSelf)
        {
           isMenuVisible = true;
           MenuView.SetActive(true);           
           pauseButton.GetComponentInChildren<Text>().text = "Continue";
           

           foreach (Transform child in climbersParent.transform)
           {
                var cSpeed = child.GetComponent<Animator>().GetFloat("ClimbSpeedMultiplier");
                child.GetComponent<ClimberAttributesScript>().climbSpeed = cSpeed;
                child.GetComponent<Animator>().SetFloat("ClimbSpeedMultiplier", 0);
           } 

        }
        else if(MenuView.activeSelf)
        {
            isMenuVisible = false;
            MenuView.SetActive(false);           
            pauseButton.GetComponentInChildren<Text>().text = "Pause";
            

            foreach (Transform child in climbersParent.transform)
            {                   
                child.GetComponent<Animator>().SetFloat("ClimbSpeedMultiplier", child.GetComponent<ClimberAttributesScript>().climbSpeed);                
            }
        }
        
    }

    // This shows user the change of waves
    public void WaveView()
    {
            isWaveView = true;
            var waveText = GameObject.FindGameObjectWithTag("WaveText");
            //GameObject.FindGameObjectWithTag("WaveText").GetComponent<Text>().text = "NEXT WAVE..!";
            Debug.Log(colorA);
            if (colorA == 0)
            {
                Debug.Log(colorA);
                waveViewImage.CrossFadeAlpha(1, 1.5f, true);
                colorA = waveViewImage.color.a;
                Debug.Log(colorA);
            }

            if (colorA == 1)
            {
                increase = true;
                StartCoroutine(crossFadeOut());
            }
      
    }

    IEnumerator crossFadeOut()
    {
        yield return new WaitForSeconds(3);
        waveViewImage.CrossFadeAlpha(0, 1.5f, false);
        colorA = 0;      
        isWaveView = false;
        increase = false;
        goToWaveView = false;
    }

    void fontSize()
    {
        GameObject.FindGameObjectWithTag("WaveText").GetComponent<Text>().fontSize += 1;
    }
  

}
