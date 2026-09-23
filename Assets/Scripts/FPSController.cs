using UnityEngine;
using TMPro;

[System.Serializable]
public class Gun
{
    public string gunName;
    public GameObject gunObject;      // ตัวโมเดลปืน
    public Transform firePoint;       // ปากกระบอกปืนของปืนนี้โดยเฉพาะ
    public int maxClip;
    public int currentClip;
    public int reserveAmmo;
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
    public Gun[] guns = new Gun[2]; // Slot 0: ปืนหลัก, Slot 1: ปืนรอง
    public int currentGunIndex = 0;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    [Header("UI (Optional)")]
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

        UpdateWeaponVisibility();
        UpdateAmmoUI();
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
            // คำนวณหาจุดกึ่งกลางจอที่กล้องกำลังเล็งไป
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            RaycastHit hit;
            Vector3 targetPoint;

            // ถ้ายิงโดนสิ่งของในระยะ 100 เมตร ให้เล็งไปที่จุดนั้น ถ้าไม่โดนอะไรเลยให้พุ่งไปข้างหน้า 100 เมตร
            if (Physics.Raycast(ray, out hit, 100f))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = ray.GetPoint(100f);
            }

            // คำนวณทิศทางจากปากกระบอกปืนไปยังจุดเล็งกลางจอ
            Vector3 shootDirection = (targetPoint - gun.firePoint.position).normalized;

            // เสกกระสุนออกจากปลายปากกระบอกปืน
            GameObject bullet = Instantiate(gun.bulletPrefab, gun.firePoint.position, Quaternion.LookRotation(shootDirection));
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddForce(shootDirection * gun.bulletForce, ForceMode.Impulse);
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