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
    public float fireRate = 0.15f;    // มีค่าเริ่มต้นแน่นอน ไม่เป็น 0
    public bool isAutomatic = true;
    public float bulletForce = 35f;
    public GameObject bulletPrefab;
}

[RequireComponent(typeof(CharacterController))]
public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 7f;
    public float gravity = -9.81f;

    [Header("Weapons & Ammo")]
    public Gun[] guns = new Gun[2];
    public int currentGunIndex = 0;
    private float nextTimeToFire = 0f;
    private bool isReloading = false;

    [Header("UI")]
    public TextMeshProUGUI ammoText;

    private CharacterController controller;
    private Vector3 velocity;
    public Camera mainCam; // ให้ใส่กล้องใน Inspector ได้โดยตรงเพื่อความแม่นยำ

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // ดึงกล้องหลัก ถ้ายังไม่ได้ลากใส่
        if (mainCam == null)
        {
            mainCam = Camera.main;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SetupDefaultGuns();
        UpdateWeaponVisibility();
        UpdateAmmoUI();
    }

    void SetupDefaultGuns()
    {
        // ป้องกันค่า fireRate กลายเป็น 0
        if (guns[0] != null && guns[0].fireRate <= 0.01f) guns[0].fireRate = 0.15f;
        if (guns[1] != null && guns[1].fireRate <= 0.01f) guns[1].fireRate = 0.3f;
    }

    void Update()
    {
        HandleMovement();
        HandleRotationTowardsMouse();
        HandleWeaponSwitch();
        HandleShootingInput();
        HandleReloadInput();
    }

    void HandleMovement()
    {
        if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(moveX, 0f, moveZ).normalized;
        controller.Move(move * moveSpeed * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleRotationTowardsMouse()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null) return;

        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, transform.position.y, 0f));

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 lookDirection = hitPoint - transform.position;
            lookDirection.y = 0f; 

            if (lookDirection.sqrMagnitude > 0.05f)
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
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
                // ถ้าค่า fireRate น้อยเกินไป ป้องกันไม่ให้บั๊ก
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

        if (gun.bulletPrefab != null && gun.firePoint != null)
        {
            Vector3 shootDirection = transform.forward;

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
}
