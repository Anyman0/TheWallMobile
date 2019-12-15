using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{

    GameScript gs;
    private void Awake()
    {
        gs = GameObject.Find("Environment").GetComponent<GameScript>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("DANG! Hit object: " + collision.transform.name);

        if (collision.gameObject.tag == "GoblinClimber")
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
        //Destroy(this.gameObject); // Should we destroy the arrow in collision? 
        //RemoveProjectile();
       
    }

}
