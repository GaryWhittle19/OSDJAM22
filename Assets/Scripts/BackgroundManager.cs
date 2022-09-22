using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    [SerializeField] float parallaxFactor;
    [SerializeField] GameObject player;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if ( !player ) { return; };

        Rigidbody rb = player.GetComponent<Rigidbody>();
        Vector2 playerVelocity = new Vector2(rb.velocity.x, rb.velocity.z);

        MeshRenderer mr = GetComponent<MeshRenderer>();

        Material mat = mr.material;

        Vector2 offset = mat.mainTextureOffset;

        offset.x += Time.deltaTime * playerVelocity.x * parallaxFactor;
        offset.y += Time.deltaTime * playerVelocity.y * parallaxFactor;

        mat.mainTextureOffset = offset;
    }
}
