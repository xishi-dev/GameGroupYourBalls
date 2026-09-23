using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public enum AmmoType
    {
        Primary,   
        Secondary  
    }

    [Header("Ammo Type")]
    public AmmoType ammoType = AmmoType.Primary;

    [Header("Animation")]
    public float rotationSpeed = 60f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.15f;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime), Space.World);
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            FPSController player = other.GetComponent<FPSController>();
            if (player != null)
            {
                int targetSlot = (ammoType == AmmoType.Primary) ? 0 : 1;
                bool pickedUp = player.RefillReserveAmmo(targetSlot);

                if (pickedUp)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
