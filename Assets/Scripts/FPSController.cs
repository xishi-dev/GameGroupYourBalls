using UnityEngine;
using TMPro;

[System.Serializable]
public class Gun
{
    public string gunName;
    public GameObject gunObject;     
    public Transform firePoint;       
    public int maxClip;               
    public int currentClip;           
    public int reserveAmmo;          
    public int maxReserveAmmo;        
    public float fireRate;            
    public bool isAutomatic;         
    public float bulletForce;       
    public GameObject bulletPrefab;   
}

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look Settings")]
    public Transform playerCamera;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Weapons & Ammo")]
    public Gun[] guns = new Gun[2]; 
    public int currentGunIndex = 0;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    private CharacterController controller;
    private Vector3 velocity;
    private float verticalRotation = 0f;
    private bool isCursorLocked = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        LockCursor(true);

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        SetupDefaultGuns();
        UpdateWeaponVisibility();
        UpdateAmmoUI();
    }

    void SetupDefaultGuns()
    {
        if (guns[0] == null || string.IsNullOrEmpty(guns[0].gunName))
        {
            guns[0] = new Gun
            {
                gunName = "Primary Rifle",
                maxClip = 30,
                currentClip = 30,
                reserveAmmo = 60,
                maxReserveAmmo = 60,
                fireRate = 0.12f,
                isAutomatic = true,
                bulletForce = 35f
            };
        }

        if (guns[1] == null || string.IsNullOrEmpty(guns[1].gunName))
        {
            guns[1] = new Gun
            {
                gunName = "Secondary Pistol",
                maxClip = 12,
                currentClip = 12,
                reserveAmmo = 36,
                maxReserveAmmo = 36,
                fireRate = 0.25f,
                isAutomatic = false,
                bulletForce = 30f
            };
        }
    }

    void Update()
    {
        HandleCursorLock();

        if (isCursorLocked)
        {
            HandleMouseLook();
            HandleWeaponSwitch();
            HandleShootingInput();
            HandleReloadInput();
        }

        HandleMovement();
    }

    void HandleWeaponSwitch()
    {
        int previousGun = currentGunIndex;

        if (Input.GetKeyDown(KeyCode.Alpha1)) currentGunIndex = 0;
        if (Input.GetKeyDown(KeyCode.Alpha2)) currentGunIndex = 1;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) currentGunIndex = (currentGunIndex + 1) % guns.Length;
        else if (scroll < 0f) currentGunIndex = (currentGunIndex - 1 + guns.Length) % guns.Length;

        if (previousGun != currentGunIndex)
        {
            isReloading = false;
            UpdateWeaponVisibility();
            UpdateAmmoUI();
        }
    }

    void UpdateWeaponVisibility()
    {
        for (int i = 0; i < guns.Length; i++)
        {
            if (guns[i].gunObject != null)
            {
                guns[i].gunObject.SetActive(i == currentGunIndex);
            }
        }
    }

    void HandleShootingInput()
    {
        if (isReloading) return;

        Gun currentGun = guns[currentGunIndex];
        bool shootTriggered = currentGun.isAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (shootTriggered && Time.time >= nextTimeToFire)
        {
            if (currentGun.currentClip > 0)
            {
                nextTimeToFire = Time.time + currentGun.fireRate;
                Shoot(currentGun);
            }
            else
            {
                Reload();
            }
        }
    }

    void Shoot(Gun gun)
    {
        gun.currentClip--;
        UpdateAmmoUI();

        if (gun.bulletPrefab != null && gun.firePoint != null)
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            RaycastHit hit;
            Vector3 targetPoint;

            if (Physics.Raycast(ray, out hit, 100f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(100f);
            }

            Vector3 shootDirection = (targetPoint - gun.firePoint.position).normalized;

            GameObject bullet = Instantiate(gun.bulletPrefab, gun.firePoint.position, Quaternion.LookRotation(shootDirection));
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = shootDirection * gun.bulletForce;
            }
        }
    }

    void HandleReloadInput()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isReloading)
        {
            Reload();
        }
    }

    void Reload()
    {
        Gun gun = guns[currentGunIndex];
        if (gun.currentClip == gun.maxClip || gun.reserveAmmo <= 0) return;

        isReloading = true;
        Invoke(nameof(FinishReload), 1.2f);
    }

    void FinishReload()
    {
        Gun gun = guns[currentGunIndex];
        int neededAmmo = gun.maxClip - gun.currentClip;
        int ammoToLoad = Mathf.Min(neededAmmo, gun.reserveAmmo);

        gun.currentClip += ammoToLoad;
        gun.reserveAmmo -= ammoToLoad;

        isReloading = false;
        UpdateAmmoUI();
    }

    public bool RefillReserveAmmo(int gunIndex)
    {
        if (gunIndex < 0 || gunIndex >= guns.Length) return false;

        Gun targetGun = guns[gunIndex];
        if (targetGun == null) return false;

        if (targetGun.reserveAmmo >= targetGun.maxReserveAmmo)
        {
            return false;
        }

        targetGun.reserveAmmo = targetGun.maxReserveAmmo;
        UpdateAmmoUI();
        return true;
    }

    void UpdateAmmoUI()
    {
        if (ammoText != null)
        {
            Gun gun = guns[currentGunIndex];
            ammoText.text = $"[{gun.gunName}]\n{gun.currentClip} / {gun.reserveAmmo}";
        }
    }

    void HandleCursorLock()
    {
        if (Input.GetMouseButtonDown(0) && !isCursorLocked) LockCursor(true);
        if (Input.GetKeyDown(KeyCode.Escape)) LockCursor(false);
    }

    void LockCursor(bool lockState)
    {
        isCursorLocked = lockState;
        Cursor.lockState = lockState ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !lockState;
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        if (playerCamera != null)
        {
            playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
        }
    }

    void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;
        controller.Move(move * walkSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}