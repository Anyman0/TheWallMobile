namespace Water2D {
	using UnityEngine;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine.UI;
	using DynamicLight2D;

	public struct microSpawn{
		public Vector3 pos;
		public int amount;
		public Vector2 initVel;

		public microSpawn(Vector3 pos, int amount, Vector2 initVel)
		{
			this.pos = pos;
			this.amount = amount;
			this.initVel = initVel;
		}
	}

	public class Water2D_Spawner : MonoBehaviour
	{

		public static Water2D_Spawner instance;

		void Awake()
		{
			if(instance == null)
				instance = this;

		}

		/*[Title("Water 2D", 20f, 20)]*/

		/*[Space(25f)]*/

		/// <summary>
		/// Drops objects array.
		/// </summary>
		public GameObject [] WaterDropsObjects;

		/// <summary>
		/// The size of each drop.
		/// </summary>
		[Range (0f,2f)]	public float size = .45f;

		/// <summary>
		/// The life time of each particle.
		/// </summary>
		[Range (0f,100f)] public float LifeTime = 5f;

		/// <summary>
		/// The delay between particles emission.
		/// </summary>
		[Range (0f,.3f)] public float DelayBetweenParticles = 0.05f;

		/// <summary>
		/// The water material.
		/// </summary>

		[Header("Material & color")]
		public Material WaterMaterial;
		public Color FillColor = new Color(0f,0/0f,1f);
		public Color StrokeColor = new Color(4/255f,156/255f,1f);



		[Separator()]

		[Header("Speed & direction")]
		/// <summary>
		/// The initial speed of particles after spawn.
		/// </summary>
		public Vector2 initSpeed = new Vector2(1f,-1.8f);


		[Separator()]

		[Header("Apply setup changes over lifetime")]
		/// <summary>
		/// The dynamic changes can be apply ?.
		/// </summary>
		public bool DynamicChanges = true;

        [Space(20f)]
        [Header("Runtime actions")]

        [ButtonAttribute("Start!", "Water2D.Water2D_Spawner", "RunSpawner")]public bool btn_0;
		static void RunSpawner()
		{
            instance.Spawn();

        }
        [ButtonAttribute("Stop and restore", "Water2D.Water2D_Spawner", "StopSpawner")] public bool btn_1;
        static void StopSpawner()
        {
            instance.Restore();

        }
        /*
        [Separator()]

        [ButtonAttribute("Help?", "Water2D.Water2D_Spawner", "askHelp")] public bool btn;
        static void askHelp()
        {
            string email = "info@2ddlpro.com";
            string subject = "Water 2D Help!";
            Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + "");
        }
        */


        bool _dynamic = true;
		public bool Dynamic {get{return  _dynamic;} 
			set{
				_dynamic = value;
			}
		}

		//bool alreadySpawned = false;



		public int AllBallsCount{ get; private set;}
		public bool IsWaterInScene{ get; private set;}

		//int usableDropsCount;
		int DefaultCount;


		// MICRO SPWNS
		// Used to make spawn in other positions with same properties (use same array of particles)
		List<microSpawn> microSpawns;

		bool _breakLoop = false;

		GameObject _parent;



		void Start()
		{
			//Application.targetFrameRate = 60;

			_parent = new GameObject ("_metaBalls");
			_parent.hideFlags = HideFlags.HideInHierarchy;
			WaterDropsObjects [0].transform.SetParent (_parent.transform);
			WaterDropsObjects [0].transform.localScale = new Vector3 (size, size, 1f);
			WaterDropsObjects [0].GetComponent<MetaballParticleClass>().Active = false;





            for (int i = 1; i < WaterDropsObjects.Length; i++) {
				WaterDropsObjects[i] = Instantiate(WaterDropsObjects[0], gameObject.transform.position, new Quaternion(0,0,0,0)) as GameObject;
				WaterDropsObjects [i].GetComponent<MetaballParticleClass>().Active = false;
				WaterDropsObjects [i].transform.SetParent (_parent.transform);
				WaterDropsObjects [i].transform.localScale = new Vector3 (size, size, 1f);
                WaterDropsObjects[i].layer = WaterDropsObjects[0].layer;
                //WaterDropsObjects[i].SetActive(false);
            }

            WaterDropsObjects[0].SetActive(false);

            AllBallsCount = WaterDropsObjects.Length;


			microSpawns = new List<microSpawn>(5); // Up to 5 microspwawn


            //instance.Spawn();
        }

		public void RunMicroSpawn(Vector3 pos, int amount, Vector2 initVel)
		{
			addMicroSpawn (pos, amount, initVel);
			executeMicroSpawns ();
		}

		public void addMicroSpawn(Vector3 pos, int amount, Vector2 initVel)
		{
			microSpawns.Add( new microSpawn (pos, amount, initVel));
		}



		public void Spawn(){
			Spawn (DefaultCount);
		}

		public void Spawn(int count){
			executeMicroSpawns ();
            if (DelayBetweenParticles == 0f)
            {
                SpawnAll();
            }
            else {
                StartCoroutine(loop(gameObject.transform.position, initSpeed, count));
            }
			
		}

        public void SpawnAll() {
            SpawnAllParticles(gameObject.transform.position, initSpeed, DefaultCount);
        }

		public void Spawn(int count, Vector3 pos){
			executeMicroSpawns ();
			StartCoroutine (loop(pos, initSpeed, count));
		}

		public void Spawn(int count, Vector3 pos, Vector2 InitVelocity, float delay = 0f){
			executeMicroSpawns ();
			StartCoroutine (loop(pos, InitVelocity, count, delay));
		}

		void executeMicroSpawns()
		{
			if (microSpawns == null)
				return;

			if (microSpawns.Count > 0 && microSpawns.Capacity > 0) {
				for (int i = 0; i < microSpawns.Count; i++) {
					//Spawn (microSpawns [i].amount, microSpawns [i].pos, microSpawns [i].initVel);
					DynamicChanges = false;
					StartCoroutine (loop(microSpawns [i].pos, microSpawns [i].initVel, microSpawns [i].amount ,0f));
				}

				microSpawns.Clear ();
			}
		}

		public void Restore()
		{

			IsWaterInScene = false;
			_breakLoop = true;

			microSpawns.Clear ();


			for (int i = 0; i < WaterDropsObjects.Length; i++) {
				if (WaterDropsObjects [i].GetComponent<MetaballParticleClass> ().Active == true) {
					WaterDropsObjects [i].GetComponent<MetaballParticleClass> ().Active = false;
				}
				WaterDropsObjects [i].GetComponent<MetaballParticleClass> ().witinTarget = false;			
			}




			gameObject.transform.localEulerAngles = Vector3.zero;
			initSpeed = new Vector2 (0, -2f);

			DefaultCount = AllBallsCount;
			//usableDropsCount = DefaultCount;
			//Dynamic = false;
		}

		IEnumerator loop(Vector3 _pos, Vector2 _initSpeed, int count = -1, float delay = 0f, bool waitBetweenDropSpawn = true){
			yield return new WaitForSeconds (delay);

			_breakLoop = false;

			IsWaterInScene = true;

			int auxCount = 0;
			while (true) {
				for (int i = 0; i < WaterDropsObjects.Length; i++) {

					if (_breakLoop)
						yield break;

					MetaballParticleClass MetaBall = WaterDropsObjects [i].GetComponent<MetaballParticleClass> ();

					if (MetaBall.Active == true)
						continue;

					MetaBall.LifeTime = LifeTime;
					WaterDropsObjects [i].transform.position = transform.position;
					MetaBall.Active = true;
					MetaBall.witinTarget = false;

					if (_initSpeed == Vector2.zero)
						_initSpeed = initSpeed;

					if (DynamicChanges) {
						_initSpeed = initSpeed;
						MetaBall.transform.localScale = new Vector3 (size, size, 1f);
						SetWaterColor (FillColor, StrokeColor);
					}

					WaterDropsObjects [i].GetComponent<Rigidbody2D> ().velocity = _initSpeed;


					// Count limiter
					if (count > -1) {
						auxCount++;
						if (auxCount >= count && !Dynamic) {
							yield break;
						} 
					}

					if(waitBetweenDropSpawn)
						yield return new WaitForSeconds (DelayBetweenParticles);

				}
				yield return new WaitForEndOfFrame ();
				//alreadySpawned = true;

				if (!Dynamic)
					yield break;

			}
		}


        void SpawnAllParticles(Vector3 _pos, Vector2 _initSpeed, int count = -1, float delay = 0f, bool waitBetweenDropSpawn = true)
        {
           

            IsWaterInScene = true;

            int auxCount = 0;
           // while (true)
            //{
                for (int i = 0; i < WaterDropsObjects.Length; i++)
                {


                    MetaballParticleClass MetaBall = WaterDropsObjects[i].GetComponent<MetaballParticleClass>();

                    if (MetaBall.Active == true)
                        continue;

                    MetaBall.LifeTime = LifeTime;
                    WaterDropsObjects[i].transform.position = transform.position;
                    MetaBall.Active = true;
                    MetaBall.witinTarget = false;

                    if (_initSpeed == Vector2.zero)
                        _initSpeed = initSpeed;

                    if (DynamicChanges)
                    {
                        _initSpeed = initSpeed;
                        MetaBall.transform.localScale = new Vector3(size, size, 1f);
                        SetWaterColor(FillColor, StrokeColor);
                    }

                    WaterDropsObjects[i].GetComponent<Rigidbody2D>().velocity = _initSpeed;


                    // Count limiter
                    if (count > -1)
                    {
                        auxCount++;
                        if (auxCount >= count && !Dynamic)
                        {
                            break;
                        }
                    }

                    

                }
               
                //alreadySpawned = true;

             
           // }
        }

        public void SetWaterColor(Color fill, Color stroke)
		{
			WaterMaterial.SetColor ("_Color", fill);
			WaterMaterial.SetColor ("_StrokeColor", stroke);

		}

	}

}