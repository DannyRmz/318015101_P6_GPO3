using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWay : MonoBehaviour
{
    public WayCreator rutaActual;
    public int currentWayPointID;
    public float rotSpeed;
    public float speed;
    public float reachDistance=0.1f;
    public int way=0;
    public int clickCount = 0; 
    public float shakeIntensity = 5f; 
    public float extraSpeed = 0.5f;
    private bool isFalling = false;
    Vector3 last_position;
    Vector3 current_position;
    public ParticleSystem takeOffParticles;
    public GameObject explosionEffect;
    private bool hasTakenOff = false;
    private Rigidbody rb;
    public Material dissolveMaterial;
    private float dissolveAmount = 0f;
    private bool isDissolving = false;
    public List<Renderer> allRenderers;
    private bool hasLanded = false;
    public RutaSelector rutaManager;
    private int indiceActual = 0;
    public float velocidad = 5f;
    public float precision = 0.5f;


    void Start()
    {
        last_position=transform.position;
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rutaActual = rutaManager.ObtenerRutaAleatoria();
    }

    void Update()
    {
        if (rutaActual == null || rutaActual.path_objs.Count == 0) return;

        Vector3 destino = rutaActual.path_objs[indiceActual].position;
        transform.position = Vector3.MoveTowards(transform.position, destino, velocidad * Time.deltaTime);

        if (Vector3.Distance(transform.position, destino) < precision)
        {
            indiceActual++;
            if (indiceActual >= rutaActual.path_objs.Count)
            {
                indiceActual = 0;
            }
        }     

        Vector3 down =  Vector3.down;

        if (!hasTakenOff && isTakingOffCondition())
        {
            takeOffParticles.Play();
            hasTakenOff = true;
        }

        DetectClick();
        if(Input.touchCount>1)
        { 
            way=1;
            currentWayPointID=0;
            speed=0.7f;
            
        }

        if (!isFalling)
        {
            float distance = Vector3.Distance(rutaActual.path_objs[currentWayPointID].position, transform.position);
            transform.position = Vector3.MoveTowards(transform.position, rutaActual.path_objs[currentWayPointID].position, Time.deltaTime * speed);

            Vector3 direction = rutaActual.path_objs[currentWayPointID].position - transform.position;
            if (direction.magnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                Quaternion correction = Quaternion.Euler(-90, 0, 0); 
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation * correction, Time.deltaTime * rotSpeed);
            }

            if (distance <= reachDistance)
            {
                currentWayPointID++;
            }

            if (currentWayPointID >= rutaActual.path_objs.Count)
            {
                currentWayPointID = 0;
                if (way == 1)
                {
                    way = 0;
                    speed = 0.2f;
                }
            }
        }

        if (isFalling && IsGrounded() & !isDissolving)
        {
            hasLanded = true;
            rb.linearVelocity = Vector3.zero; 
            StartCoroutine(StopPhysicsAndDissolve());
        }

        if (isFalling && !IsGrounded() && !hasLanded)
        {
            transform.Rotate(Vector3.forward * Time.deltaTime * 500f);
        }


    }

    void DetectClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == transform)
                {
                    clickCount++;

                    if (clickCount < 3)
                    {
                        StartCoroutine(ShakePlane());
                        speed += extraSpeed;
                    }
                    else
                    {
                        rb.isKinematic = false;
                        rb.useGravity = true;
                        isFalling = true;
                        //rb.AddForce(Vector3.down * 200f, ForceMode.VelocityChange);
                        rb.AddForce(Vector3.down * 500f, ForceMode.Impulse);
                        rb.centerOfMass = new Vector3(0, -0.5f, 0);

                        Vector3 randomTorque = new Vector3(
                        Random.Range(-200f, 200f),
                        Random.Range(-100f, 100f),
                        Random.Range(-200f, 200f));
                        rb.AddTorque(randomTorque);
                        rb.AddTorque(new Vector3(500f, 250f, 500f), ForceMode.Impulse);
                    }
                }
            }
        }
    }


    IEnumerator ShakePlane()
    {
        float duration = 1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-shakeIntensity, shakeIntensity) * 0.03f;
            float y = Random.Range(-shakeIntensity, shakeIntensity) * 0.03f;
            transform.position += new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator StopPhysicsAndDissolve()
    {
        yield return new WaitForSeconds(0.01f);
        rb.isKinematic = true;
        isDissolving = true;
        StartCoroutine(DissolveEffect());
    }

    IEnumerator DissolveEffect()
    {
        float dissolveAmount = 0f;

        while (dissolveAmount < 0.8f)
        {
            dissolveAmount += Time.deltaTime * 0.5f;

            foreach (Renderer rend in allRenderers)
            {
                Material mat = rend.material;
                mat.SetFloat("_DesolveStrenght", dissolveAmount);
            }

            yield return null;
        }

        if (explosionEffect != null)
        {
            explosionEffect.transform.position = transform.position;
            explosionEffect.SetActive(true);
            
            ParticleSystem ps = explosionEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }
    }

   bool IsGrounded()
    {
        Vector3 rayOrigin = transform.position + Vector3.down * 1.5f;
        Vector3 rayDirection = Vector3.down;
        float rayDistance = 300.0f;

        Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red);

        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, rayDistance))
        {
            return true;
        }

        return false;
    }

    bool isTakingOffCondition()
    {
        return currentWayPointID == 1 && transform.position.y > 1.0f;
    }
}
