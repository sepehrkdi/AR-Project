using UnityEngine; 
using Vuforia; 

[RequireComponent(typeof(ObserverBehaviour))] 
public class AutoCharacterPlacer : MonoBehaviour 
{ 
   [Header("Prefab & Raycast Settings")] 
   public GameObject characterPrefab;           // Assign in Inspector 
   //public float raycastHeight = 20f;         // How high above the painting to start the ray 
   //public float raycastDistance = 30f;         // How far down to raycast for ground 
   //public LayerMask groundLayer = ~0;          // Layer mask for what counts as "ground" 

   private ObserverBehaviour observer; 
   private GameObject spawnedCharacter; 
   private bool isCurrentlyTracked = false; 

   void Awake() 
   { 
       observer = GetComponent<ObserverBehaviour>(); 
   } 

   void OnEnable() 
   { 
       observer.OnTargetStatusChanged += OnStatusChanged; 
   } 

   void OnDisable() 
   { 
       observer.OnTargetStatusChanged -= OnStatusChanged; 
   } 

   //private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status) 
   //{ 
   //    bool isTracked = status.Status == Status.TRACKED; 

   //    if (isTracked && spawnedCharacter == null) 
   //    { 
   //        SpawnCharacter(); 
   //    } 
   //    else if (!isTracked && spawnedCharacter != null) 
   //    { 
   //        Destroy(spawnedCharacter); 
   //        spawnedCharacter = null; 
   //    } 
   //} 
   private void OnStatusChanged(ObserverBehaviour behaviour, TargetStatus status) 
   { 
       // Only TRACKED counts as in view  everything else is treated as lost 
       bool isNowTracked = status.Status == Status.TRACKED; 

       if (isNowTracked && spawnedCharacter == null) 
       { 
           SpawnCharacter(); 
       } 
       else if (!isNowTracked && spawnedCharacter != null) 
       { 
           Destroy(spawnedCharacter); 
           spawnedCharacter = null; 
       } 
   } 

   //private void SpawnCharacter() 
   //{ 
   //    // Step 1: Start ray from above the ImageTarget 
   //    Vector3 origin = transform.position + Vector3.up * raycastHeight; 

   //    // Step 2: RaycastAll downward to get all hits 
   //    RaycastHit[] allHits = Physics.RaycastAll(origin, Vector3.down, raycastDistance); 

   //    // Step 3: Find the first hit that is NOT part of this ImageTarget 
   //    RaycastHit? groundHit = null; 
   //    foreach (var hit in allHits) 
   //    { 
   //        // Skip anything that's part of the ImageTarget itself 
   //        if (hit.collider.transform.IsChildOf(transform)) 
   //            continue; 

   //        groundHit = hit; 
   //        break; 
   //    } 

   //    // Step 4: If we didn't hit anything valid, exit 
   //    if (!groundHit.HasValue) 
   //        return; 

   //    // Step 5: Use the valid ground hit to place the character 
   //    Vector3 spawnPos = groundHit.Value.point; 

   //    // 2) Face the user: compute horizontal-only look direction toward the main camera 
   //    Vector3 toCamera = Camera.main.transform.position - spawnPos; 
   //    toCamera.y = 0; 
   //    Quaternion spawnRot = toCamera.sqrMagnitude > 0.001f 
   //        ? Quaternion.LookRotation(toCamera) 
   //        : Quaternion.identity; 

   //    // 3) Instantiate 
   //    spawnedCharacter = Instantiate(characterPrefab, spawnPos, spawnRot); 

   //    // 4) Assign description 
   //    var speech = spawnedCharacter.GetComponent<CharacterSpeech>(); 
   //    if (speech != null) 
   //        speech.SetDescription(GetPaintingDescription(gameObject.name)); 
   //} 
   private void SpawnCharacter() 
   { 
       // 1) Ground position: use GroundPlaneStage's Y 
       float groundY = GameObject.Find("Ground Plane Stage").transform.position.y; 

       // 2) Place character beside the image target, but on the ground 
       Vector3 imagePos = transform.position; 

       // To control where "beside the painting" really is. 
       //Vector3 spawnPos = new Vector3(imagePos.x + 1.0f, groundY, imagePos.z);  // Offset in X or Z as needed 
       Vector3 toCamera = Camera.main.transform.position - imagePos; 
       toCamera.y = 0; 
       toCamera.Normalize(); // direction from image target to camera 

       // Distance offset toward camera 
       float distanceForward = 0.5f; 
       Vector3 offset = toCamera * distanceForward; 

       // Vertical offset to push the character downward if needed 
       float verticalOffset = -5.2f; 

       Vector3 spawnPos = new Vector3( 
           imagePos.x + offset.x, 
           GameObject.Find("Ground Plane Stage").transform.position.y + verticalOffset, 
           imagePos.z + offset.z 
       ); 

       // 3) Face the camera 
       //Vector3 toCamera = Camera.main.transform.position - spawnPos; 
       //toCamera.y = 0; 
       Quaternion spawnRot = toCamera.sqrMagnitude > 0.001f 
           ? Quaternion.LookRotation(toCamera) 
           : Quaternion.identity; 

       // 4) Instantiate 
       spawnedCharacter = Instantiate(characterPrefab, spawnPos, spawnRot); 
       //spawnedCharacter.transform.SetParent(GameObject.Find("Ground Plane Stage").transform); 

       // 5) Set painting description 
       var speech = spawnedCharacter.GetComponent<CharacterSpeech>(); 
       if (speech != null) 
           speech.SetDescription(GetPaintingDescription(gameObject.name)); 
   } 
   void Update() 
   { 
       if (!isCurrentlyTracked) return; 

       // Ensure Ground Plane Stage exists 
       GameObject groundStage = GameObject.Find("Ground Plane Stage"); 
       if (groundStage == null) return; 

       float groundY = groundStage.transform.position.y; 
       Vector3 imagePos = transform.position; 

       // Character should appear beside the painting at ground level 
       //Vector3 spawnPos = new Vector3(imagePos.x + 0.2f, groundY - 0.3f, imagePos.z - 0.3f); 
       Vector3 toCamera = Camera.main.transform.position - imagePos; 
       toCamera.y = 0; 
       toCamera.Normalize(); 

       float distanceForward = 0.5f; 
       Vector3 offset = toCamera * distanceForward; 
       float verticalOffset = -0.5f; 

       Vector3 spawnPos = new Vector3( 
           imagePos.x + offset.x, 
           groundY + verticalOffset, 
           imagePos.z + offset.z 
       ); 

       // Make it face the camera 
       //Vector3 toCamera = Camera.main.transform.position - spawnPos; 
       //toCamera.y = 0; 
       Quaternion spawnRot = toCamera.sqrMagnitude > 0.001f 
           ? Quaternion.LookRotation(toCamera) 
           : Quaternion.identity; 

       if (spawnedCharacter == null) 
       { 
           spawnedCharacter = Instantiate(characterPrefab, spawnPos, spawnRot); 
           spawnedCharacter.transform.SetParent(groundStage.transform, true);  // Optional: keep it grounded 

           var speech = spawnedCharacter.GetComponent<CharacterSpeech>(); 
           if (speech != null) 
               speech.SetDescription(GetPaintingDescription(gameObject.name)); 
       } 
       else 
       { 
           // Update position and rotation every frame 
           spawnedCharacter.transform.position = spawnPos; 
           spawnedCharacter.transform.rotation = spawnRot; 
       } 
   } 


   private string GetPaintingDescription(string imageTargetName) 
   { 
       switch (imageTargetName) 
       { 
           case "bhj": 
               return "Behjat Sadr (19242009) was a pioneering Iranian modernist who created this abstract work during her time in Italy. She broke from tradition by using industrial paints on floor-laid canvases, scraping them to form bark-like textures and cosmic black grooves, blending organic rhythm with bold innovation."; 
           case "hezar": 
               return "Sani ol-Molks One Thousand and One Nights (MS 11) blends Persian miniature with European realism, creating a rich, theatrical vision of the mythical East. Commissioned in the Qajar era, it served as a powerful statement of Iranian cultural pridewhere each image reclaims the narrative: We are the storytellers."; 
           case "mirrorHall": 
               return "Kamal-ol-Molks Mirror Hall is a realist masterpiece depicting the opulent Hall of Mirrors in Tehrans Golestan Palace. With stunning detail, it reflects power and politicsfeaturing a solitary Naser al-Din Shah, subtly overshadowed by the grandeur around him, hinting at the monarchys decline."; 
           case "samaa": 
               return "Kamal ud-Din Behzads Dance of Sufi Dervishes transforms a 15th-century garden into a scene of spiritual ecstasy. With calligraphic grace, the dervishes whirl and sway in divine trance. Every figure is unique, every movement sacredminiature art elevated to pure joy."; 
           case "vessel": 
               return "Iran Darroudis Our Veins, The Earths Veins is a surrealist protest against industrial ruin. Painted during Irans oil boom, it depicts the earth as a bleeding bodyits veins entangled with pipelines and human ambition. Beauty meets anguish in a prophetic cry: what we take from the earth, we take from ourselves."; 
           case "Angel": 
               return "Mahmoud Farshchians Joyful Spring is a mystical explosion of Persian miniature on canvas. A divine dancer dissolves into blossoms and flame, symbolizing spiritual renewal. Farshchian blends tradition with cosmic energythis is not just spring, its Nowruz for the soul."; 
           default: 
               return "Unknown painting."; 
       } 
   } 
} 