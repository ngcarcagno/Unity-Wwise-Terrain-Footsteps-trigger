using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WwiseTerrainFootSteps : MonoBehaviour
{
    /* This is a script that attached to the player controller
    lets you the detect the surfaces where the player is walking when they
    where created using terrain object and call to the respective Wwise Event
    and Switch. Fully customizable so it can be adapted to any Wwise session.
    The script is in progress so any advice will be useful. */

    //Variables to CheckIfGrounded
    bool isGrounded;
    bool isOnTerrain;
    RaycastHit hit;

    //Variables to CheckTerrain
    Transform playerTransform;
    Terrain terrainObject;
    int posX;
    int posZ;
    float valor1;
    float valor2;
    float valor3;
    float valor4;

    //Variables to TriggerFootsteps
    CharacterController character;
    float currentSpeed;
    bool walking;
    float distanceCovered;
    float airTime;
    [Header("Name of Wwise Event")]
    public string fsEventName;
    [Header("Name of Wwise Switch Group")]
    public string fsSwitchGroup;
    [Header("Name of Wwise Switch States (Surfaces)")]
    public string fsSwitchState1;
    public string fsSwitchState2;
    public string fsSwitchState3;
    public string fsSwitchState4;
    [Header("Rate of footsteps per unit")]
    public float fsRate = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        terrainObject = Terrain.activeTerrain;
        playerTransform = gameObject.GetComponent<Transform>();
        character = gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = PlayerGrounded();
        isOnTerrain = CheckOnTerrain();
        currentSpeed = GetPlayerSpeed();
        walking = CheckIfWalking();
        PlaySoundIfFalling();

        if (walking)
        {
            distanceCovered += (currentSpeed * Time.deltaTime) * fsRate;
            if (distanceCovered > 1)
            {
                TriggerFootstep();
                distanceCovered = 0;
            }
        }
    }

    // Functions to CheckIfGrounded
    bool PlayerGrounded() // NOTE that the Player Controller needs a collider for this function to work
    {
        return Physics.Raycast(transform.position, Vector3.down, out hit, GetComponent<Collider>().bounds.extents.y + 0.5f);
    }

    bool CheckOnTerrain()
    {
        if (hit.collider != null && hit.collider.tag == "Terrain")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    //Functions to CheckTerrainTexture
    void UpdatePosition()
    {
        Vector3 terrainPosition = playerTransform.position - terrainObject.transform.position;
        Vector3 mapPosition = new Vector3(terrainPosition.x / terrainObject.terrainData.size.x, 0, terrainPosition.z / terrainObject.terrainData.size.z);
        float xCoord = mapPosition.x * terrainObject.terrainData.alphamapWidth;
        float zCoord = mapPosition.z * terrainObject.terrainData.alphamapWidth;
        posX = (int)xCoord;
        posZ = (int)zCoord;
    }

    void CheckTexture()
    {
        float[,,] splatMap = terrainObject.terrainData.GetAlphamaps(posX, posZ, 1, 1);
        valor1 = splatMap[0, 0, 0];
        valor2 = splatMap[0, 0, 1];
    }

    public void GetTerrainTexture()
    {
        UpdatePosition();
        CheckTexture();
    }

    //Functions to TriggerFootsteps
    float GetPlayerSpeed()
    {
        float speed = character.velocity.magnitude;
        return speed;
    }

    bool CheckIfWalking()
    {
        if (currentSpeed > 0 && isGrounded)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void TriggerFootstep()
    {
        if (isOnTerrain)
        {
            GetTerrainTexture();

            if (valor1 > 0 && fsSwitchState1 != "")
            {
                AkSoundEngine.SetSwitch(fsSwitchGroup, fsSwitchState1, gameObject);
                AkSoundEngine.PostEvent(fsEventName, gameObject);
            }
            if (valor2 > 0 && fsSwitchState2 != "")
            {
                AkSoundEngine.SetSwitch(fsSwitchGroup, fsSwitchState2, gameObject);
                AkSoundEngine.PostEvent(fsEventName, gameObject);
            }
            if (valor3 > 0 && fsSwitchState3 != "")
            {
                AkSoundEngine.SetSwitch(fsSwitchGroup, fsSwitchState3, gameObject);
                AkSoundEngine.PostEvent(fsEventName, gameObject);
            }
            if (valor4 > 0 && fsSwitchState4 != "")
            {
                AkSoundEngine.SetSwitch(fsSwitchGroup, fsSwitchState4, gameObject);
                AkSoundEngine.PostEvent(fsEventName, gameObject);
            }
        }
        else
        {
            AkSoundEngine.PostEvent(fsEventName, gameObject);
        }
    }

    void PlaySoundIfFalling()
    {
        if (!isGrounded)
        {
            airTime += Time.deltaTime;
        }
        else
        {
            if (airTime > 0.25f)
            {
                TriggerFootstep();
                airTime = 0;
            }
        }
    }
}
