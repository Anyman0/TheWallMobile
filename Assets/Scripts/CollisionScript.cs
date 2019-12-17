using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CollisionScript : MonoBehaviour
{

    GameScript gs;
   
    [HideInInspector]
    public int cDropped;
    [HideInInspector]
    public float sTime;
    GameObject statPanel;
   
    private void Awake()
    {
        gs = GameObject.Find("Environment").GetComponent<GameScript>();             
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("DANG! Hit object: " + collision.transform.name);

        if (collision.gameObject.tag == "GoblinClimber" && this.transform.tag != "WallTop")
        {
            gs.climbersDropped++;
            
        }

        if(collision.gameObject.layer != 8) // Layer check DOES NOT WORK. TODO: Oil needs to go through the barrel and keep moving. Now it gets destroyed on impact with barrel where it gets instantiated.
        {            
            Destroy(collision.gameObject);
        }
        
        else if(collision.gameObject.layer == 8)
        {
            Debug.Log("Layer is: " + collision.gameObject.layer + " , so do nothing..");
        }


        if(this.transform.tag == "WallTop" && collision.transform.tag == "GoblinClimber")
        {
            Debug.Log("Climber reached the top... Game Over!");           
            gs.isMenuVisible = true;
            gs.MenuView.SetActive(true);
            var seconds = Mathf.Round(gs.surviveTimer + gs.addedTimePerWave);
            GameObject.FindGameObjectWithTag("PauseButton").SetActive(false);
            GameObject.FindGameObjectWithTag("GameStatsPanel").SetActive(false);
            GameObject.Find("ClimberText").GetComponent<Text>().text = "You managed to drop a total of: " + gs.climbersDropped + " Climbers.";
            GameObject.Find("MainSurvivedText").GetComponent<Text>().text = "You survived for a total of: " + seconds + " Seconds.";
        }

        //Destroy(this.gameObject); // Should we destroy the arrow in collision? 
        //RemoveProjectile();
       
    }

}
