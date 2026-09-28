using UnityEngine;

public class AttachGunToHand : MonoBehaviour
{
    public GameObject gun;

    [Header("Gun Position")]
    public Vector3 gunPosition;

    [Header("Gun Rotation")]
    public Vector3 gunRotation;

    [Header("Gun Scale")]
    public float gunScale = 0.01f;

    void Start()
    {
        Animator animator = GetComponent<Animator>();

        Transform rightHand =
            animator.GetBoneTransform(HumanBodyBones.RightHand);

        if (rightHand == null)
        {
            Debug.LogError("หา Right Hand ไม่เจอ!");
            return;
        }

        gun.transform.SetParent(rightHand);

        gun.transform.localPosition = gunPosition;
        gun.transform.localRotation = Quaternion.Euler(gunRotation);
        gun.transform.localScale = Vector3.one * gunScale;
    }
}