using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarController : MonoBehaviour
{

    struct AlienBlip
    {
        public GameObject radarElement;
        public GameObject alien;
    }

    [SerializeField] private GameObject radarBlip;
    [SerializeField] private float radarEdgeDistance = 100.0f;

    private GameObject player;
    private List<AlienBlip> alienBlips = new List<AlienBlip>();

    private float resX = 512.0f;
    private float resY = 512.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void BootUpRadar()
    {
        player = FindObjectOfType<CapsuleController>().gameObject;
        var aliens = FindObjectsOfType<AlienController>();

        foreach (var alien in aliens)
        {
            AlienBlip blip;
            blip.radarElement = Instantiate(radarBlip, transform, false);
            blip.alien = alien.gameObject;
            alienBlips.Add(blip);
        }
    }

    public void ShutDownRadar()
    {
        player = null;
        foreach (var blip in alienBlips)
        {
            Destroy(blip.radarElement);
        }
        alienBlips.Clear();
    }

    public void DisableBlip(GameObject finishedAlien)
    {
        foreach (var blip in alienBlips)
        {
            if (blip.alien == finishedAlien)
            {
                blip.radarElement.SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPos = Vector3.zero;
        if (player)
            playerPos = player.transform.position;

        foreach (var blip in alienBlips)
        {
            Vector3 playerDistance = blip.alien.transform.position - playerPos;

            Vector3 newPosition = playerDistance / radarEdgeDistance;
            newPosition.x = Mathf.Clamp(newPosition.x, -0.5f, 0.5f);
            newPosition.y = Mathf.Clamp(newPosition.z, -0.5f, 0.5f);
            newPosition.z = 0.0f;

            blip.radarElement.GetComponent<RectTransform>().localPosition = newPosition * resX;
        }
    }
}
