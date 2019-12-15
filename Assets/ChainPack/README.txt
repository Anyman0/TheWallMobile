How to break the chain:

There is a Chain static method

public static void CutMe (GameObject gameObject)

just pass this method any object and if the object is a link in the chain, the chain is broken at the site of the chain link.
For example flying arrow can break the chain like this (the code on the arrows)

public void OnCollisionEnter2D (Collision2D collision)
{
     Chain.CutMe (collision.collider.gameObject);
}

-------------------------------------------------------

How to manage the chain in GAME MODE:

You can only change the speed roulette and pick off a chain of objects A and B. For this purpose, all manipulations with the chain in the GAME MODE, set using

GetComponent <Chain> ().chainController;

This controller is designed to not disrupt class Chain, as it contains a lot of public methods.