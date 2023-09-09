using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCube : MonoBehaviour {
#region Synopsis
    //Synopsis
    //Complete script for the cube spawner for Week 3 of Interactive Scripting
    //Gives the ability to create cubes in a certain area.


    //Requirements:
    // Change the Color of each spawned object
    // Change how many objects are spawned
    // Change the time between spawns
    // Change the size of the spawn area
    // Add physics to the spawned object
    // Reset the spawn logic on a key press
#endregion

#region Variables
    //Editable Variables
    [SerializeField] List<GameObject> spawnableObjects; //Which object or objects do you want to spawn?
    [SerializeField] int amountOfObjectsToSpawn; //Total number of objects wanting to spawn
    
    [Range(0.1f, 5f)] //A slider for ease of use to set the time between object spawning
    [SerializeField] float timeBetweenSpawns;

    [Header("Spawn Ranges")] // Here is where you set your spawn range for x and z. y is always 2
    [SerializeField] float spawnRangeX;  
    [SerializeField] float spawnRangeZ;

    [Header("Spawn Scales")] //This is where you set your spawn scale for the objects if you want it randomized
    [SerializeField] float spawnScaleX; 
    [SerializeField] float spawnScaleY; 
    [SerializeField] float spawnScaleZ;

    [Header("Optional Choices")]
    [SerializeField] bool usePhysics; //If you want your objects to have rigidbodies
    [SerializeField] bool spawnInfinite; //If you don't want to have an end to the spawning
    [SerializeField] bool randomColor; //If you want your objects to spawn with a random color
    [SerializeField] bool defaultScale; //If you want your objects to spawn with their prefabs default scale

    //Private variables
    int objectsSpawned; //A counter for the amount of objects that have been spawned
    bool canReset; //A condition that will stop the spawning process when turned on
    Vector3 spawnableObjectScale; //A variable for setting the object scale to a random value
    List<GameObject> objectsToDelete = new List<GameObject>(); //A list of spawned objects so we can delete them when we restart
#endregion

    private void Start() {
        if(spawnableObjects.Count == 0) { //We are checking if there are any objects in the spawn list 
            Debug.LogError("No objects to spawn"); //If there aren't any objects to spawn, throw an error
        } else {
            GetRandomSpawnObject(); //Start the spawning process
        }

    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.R)) { //This section will check if we can wipe the created objects and restart or not.
            if(canReset || spawnInfinite) { //If it is infinitely spawning, you can restart at any time
                Restart();
            }
        }
    }

    private void GetRandomSpawnObject() { //Pick a random object from the list to spawn
        var objectToSpawn = Random.Range(0, spawnableObjects.Count);
        
        SpawnObject(objectToSpawn); //Start the spawn sequence for that object
    }

    private void SpawnObject(int objectToSpawn) {
        if(!canReset || spawnInfinite) { //This checks to see if we are to the max number of spawnobjects or if infinite spawning is true
            GameObject prefabToSpawn = spawnableObjects[objectToSpawn]; //Using the objectToSpawn from the GetRandomSpawnObject function,
                                                                        //I find what that correlates to in the spawnableObjects
            Vector3 spawnPosition = GetRandomSpawnPosition(); //Set a random spawn position for the object that is spawning

            GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity); //Spawn the random object
            ColorSpawnedObject(spawnedObject); //Set the color of the newly spawned object

            if(!defaultScale) { //If the uniformScalingObjects bool is false, it will spawn with a custom scale randomly generated
                GetRandomSpawnScale();
                spawnedObject.transform.localScale = spawnableObjectScale;
            }
            
            Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
            if(usePhysics && rb == null) { //If we want physics for the object, we can have the spawner add a rigidbody
                rb = spawnedObject.AddComponent<Rigidbody>();
            }

            objectsToDelete.Add(spawnedObject); //This is a list for for wiping the board when we restart
            if(!spawnInfinite) { //This checks if we are spawning infinetley or if we have a limit
                objectsSpawned++;
                ObjectCounter(); //If we have a limit set, we run a check to see if we have reached the limit
            } else { //If infinite spawning is activated, we skip the check and start the countdown for the next spawn
                StartCoroutine(SpawnTimer());
            }
        }
    }

    private Vector3 GetRandomSpawnPosition() { //This generates a random position between the -spawnrange and +spawn range. Always spawn on Y 2
        float randomX = Random.Range(-spawnRangeX, spawnRangeX);
        float randomZ = Random.Range(-spawnRangeZ, spawnRangeZ);

        return new Vector3(randomX, 2,randomZ); //This is the random position that we give to the object when we spawn it
    }

    private Vector3 GetRandomSpawnScale() { //If we are getting a random scale for the object, this generates the random scale
        float randomX = Random.Range(0, spawnScaleX);
        float randomY = Random.Range(0, spawnScaleY);
        float randomZ = Random.Range(0, spawnScaleZ);

        spawnableObjectScale = new Vector3(randomX,randomY,randomZ);
        return spawnableObjectScale; //This is what we input to set the random scale of the spawned object
    }

    private void ColorSpawnedObject(GameObject spawnedObject) {
        if(randomColor) { //If random color is checked, we set the color to a random color, otherwise it will use the prefabs default
            spawnedObject.GetComponent<MeshRenderer>().material.color = Random.ColorHSV();
        }
    }

    private void ObjectCounter() { //This will run after an object is spawned to see if we have reached the maximum number of spawnable objects
        if(objectsSpawned >= amountOfObjectsToSpawn) { //If we have reached the maximum, we stop the spawning and send a message saying we reached the max
            canReset = true;
            Debug.Log("Reached the max amount of spawnable objects: " + amountOfObjectsToSpawn);
        } else { //If we haven't reached the maximum, we start the timer to spawn another one
            StartCoroutine(SpawnTimer());
        }
    }

    IEnumerator SpawnTimer() { //This is a simple timer that will start the spawn process after the time between spawns has been met
        yield return new WaitForSeconds(timeBetweenSpawns);
        GetRandomSpawnObject();
    }

    private void Restart() { //When we want to restart the whole spawning process, we can call this function
        objectsSpawned = 0; //sets the object spawn count to 0
        canReset = false; //allows for spawning to start happening again

        foreach(GameObject gameObject in objectsToDelete) { //This gets all of the gameobjects from the objects to delete list
            if(gameObject != null) {
                Destroy(gameObject); //We want to clear the board when we restart so this will delete all of the spawned objects
            }
        }

        objectsToDelete.Clear(); //This clears the list so its not bloated with non-existing objects

        if(!spawnInfinite) { //If we have spawn infinite enabled, then this would cause an additional spawn when not needed
            ObjectCounter(); //This starts the spawning process all over again
        }
    }

    private void OnDrawGizmosSelected() { //This is a simple visual for the editor so you can see how large the spawn range is
        Gizmos.color = Color.green;
        
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(spawnRangeX * 2, 0, spawnRangeZ * 2));
    }
}
