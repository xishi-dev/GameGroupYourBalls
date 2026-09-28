using UnityEngine;
using TMPro;

[System.Serializable]
public class Gun
{
    public string gunName;
    public GameObject gunObject;
    public Transform firePoint;
    public int maxClip = 30;
    public int currentClip = 30;
    public int reserveAmmo = 60;
    public int maxReserveAmmo = 60;
    public float fireRate = 0.15f;
    public bool isAutomatic = true;
    public float bulletForce = 35f;
    public GameObject bulletPrefab;
}

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4.5f;         
    public float runSpeed = 8.5f;           
    public float gravity = -9.81f;

    [Header("Mouse Look Settings (FPS)")]
    public Transform playerCamera;         
    public float mouseSensitivity = 1.5f;   
    public float maxLookAngle = 80f;

    [Header("Animation")]
    public Animator animator;

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

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerCamera == null && Camera.main != null)
        {
            playerCamera = Camera.main.transform;
        }

        LockCursor(true);

        SetupDefaultGuns();
        UpdateWeaponVisibility();
        UpdateAmmoUI();
    }

    void SetupDefaultGuns()
    {
        if (guns != null)
        {
            if (guns.Length > 0 && guns[0] != null && guns[0].fireRate <= 0.01f)
                guns[0].fireRate = 0.15f;

            if (guns.Length > 1 && guns[1] != null && guns[1].fireRate <= 0.01f)
                guns[1].fireRate = 0.3f;
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
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        bool isMoving = (Mathf.Abs(moveX) > 0.01f || Mathf.Abs(moveZ) > 0.01f);
        bool isRunning = isMoving && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && moveZ > 0.1f;

        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        Vector3 move = (transform.right * moveX + transform.forward * moveZ).normalized;
        controller.Move(move * currentSpeed * Time.deltaTime);
        if (animator != null)
        {
            animator.SetBool("IsMoving", isMoving);
            animator.SetBool("IsRunning", isRunning);
            animator.SetFloat("Forward", moveZ); 
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleWeaponSwitch()
    {
        int previousGun = currentGunIndex;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            currentGunIndex = 0;

        if (Input.GetKeyDown(KeyCode.Alpha2))
            currentGunIndex = 1;

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            currentGunIndex = (currentGunIndex + 1) % guns.Length;
        }
        else if (scroll < 0f)
        {
            currentGunIndex = (currentGunIndex - 1 + guns.Length) % guns.Length;
        }

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
        if (isReloading)
            return;

        Gun currentGun = guns[currentGunIndex];

        bool shootTriggered = currentGun.isAutomatic
            ? Input.GetButton("Fire1")
            : Input.GetButtonDown("Fire1");

        if (shootTriggered && Time.time >= nextTimeToFire)
        {
            if (currentGun.currentClip > 0)
            {
                float rate = (currentGun.fireRate <= 0.01f) ? 0.15f : currentGun.fireRate;
                nextTimeToFire = Time.time + rate;
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

        if (animator != null)
        {
            animator.SetTrigger("Shoot");
        }

        if (gun.bulletPrefab != null && gun.firePoint != null)
        {
            Vector3 shootDirection;
            if (playerCamera != null)
            {
                Ray ray = new Ray(playerCamera.position, playerCamera.forward);
                Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit, 100f) ? hit.point : ray.GetPoint(100f);
                shootDirection = (targetPoint - gun.firePoint.position).normalized;
            }
            else
            {
                shootDirection = transform.forward;
            }

            GameObject bullet = Instantiate(
                gun.bulletPrefab,
                gun.firePoint.position,
                Quaternion.LookRotation(shootDirection)
            );

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

        if (gun.currentClip == gun.maxClip || gun.reserveAmmo <= 0)
            return;

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
        if (gunIndex < 0 || gunIndex >= guns.Length)
            return false;

        Gun targetGun = guns[gunIndex];
        if (targetGun == null)
            return false;

        if (targetGun.reserveAmmo >= targetGun.maxReserveAmmo)
            return false;

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
}