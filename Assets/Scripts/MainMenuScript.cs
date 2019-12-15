using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenuScript : MonoBehaviour
{


    private Button playButton;
    public Material[] walls;

    // With this we can define each level in the inspector
    [System.Serializable]
    public class Level 
    {        
        public GameObject[] climbers;
        public GameObject[] weapons;       
        public float climberSpeed;
    }

    public Level[] levels;

    private void Awake()
    {
        DontDestroyOnLoad(this);        
    }


    // Start is called before the first frame update
    void Start()
    {        
        playButton =  GameObject.FindGameObjectWithTag("PlayButton").GetComponent<Button>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("GameScene");
    }
}
