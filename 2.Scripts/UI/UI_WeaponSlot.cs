using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UI_WeaponSlot : MonoBehaviour
{
    public Image weaponIcon;
    public GameObject ammoParent;
    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI totalAmmoText;

    [SerializeField] private float colorTransitionDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.15f;

    private Tweener colorTween;

    private void Awake()
    {

    }

    public void UpdateWeaponSlot(Weapon myWeapon, bool activeWeapon)
    {
        colorTween?.Kill();

        if (myWeapon == null)
        {
            colorTween = weaponIcon.DOFade(0f, fadeOutDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    ammoParent.SetActive(false);
                });
        }
        else
        {
            ammoParent.SetActive(true);
            weaponIcon.sprite = myWeapon.weaponData.weaponIcon;

            Color targetColor = activeWeapon ? Color.white : new Color(0, 0, 0);

            if (weaponIcon.color.a < 0.1f)
            {
                weaponIcon.color = new Color(targetColor.r, targetColor.g, targetColor.b, 0f);
            }

            colorTween = weaponIcon.DOColor(targetColor, colorTransitionDuration)
                .SetEase(Ease.OutQuad);

            currentAmmoText.text = myWeapon.bulletsInMagazine.ToString();
            totalAmmoText.text = "/ "+myWeapon.totalReserveAmmo.ToString();
        }
    }

    private void OnDestroy()
    {
        colorTween?.Kill();
    }
}
