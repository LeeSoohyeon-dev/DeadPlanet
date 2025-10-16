using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponSlot : MonoBehaviour
{
    public Image weaponIcon;
    public GameObject ammoParent;
    public TextMeshProUGUI currentAmmoText;
    public TextMeshProUGUI totalAmmoText;

    private void Awake()
    {

    }

    public void UpdateWeaponSlot(Weapon myWeapon, bool activeWeapon)
    {
        if (myWeapon == null)
        {
            weaponIcon.color = new Color(1, 1, 1, 0f);
            ammoParent.SetActive(false);
        }
        else
        {
            ammoParent.SetActive(true);

            Color newColor = activeWeapon ? Color.white : new Color(0, 0, 0);

            weaponIcon.color = newColor;
            weaponIcon.sprite = myWeapon.weaponData.weaponIcon;

            currentAmmoText.text = myWeapon.bulletsInMagazine.ToString();
            totalAmmoText.text = "/ "+myWeapon.totalReserveAmmo.ToString();
        }

    }
}
