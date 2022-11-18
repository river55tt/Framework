using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI LocalPlayerUI;

    public Transform healthBar;
    public Text healthText;
    public Transform powerBar;
    public Text powerText;
    public Transform jetpackFuel;

    public Transform cursor;
    public Transform reticle;
    public Text cursorcontrols;
    public Text cursordescription;

    public EntityProperties playerStats;
    public PlayerMovement playerMovement;

    public GameObject buildMenu;
    // Start is called before the first frame update
    public float maxhealth;
    public float maxpower;

    public void SetStats(float health, float power)
    {
        healthBar.localScale = new Vector3 (health / maxhealth,1,1);
        healthText.text = Mathf.FloorToInt(health) + "/" + Mathf.FloorToInt(maxhealth);

        powerBar.localScale = new Vector3(power / maxpower, 1, 1);
        powerText.text = Mathf.FloorToInt(power) + "/" + Mathf.FloorToInt(maxpower);
    }
    public void SetJetpack(float jetpack)
    {
        jetpackFuel.localScale = new Vector3(1, jetpack, 1);
    }

    void Start()
    {
        if (LocalPlayerUI == null)
        {
            LocalPlayerUI = this;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            gameObject.SetActive(false);
            //StartCoroutine("InitializeMatch");
            //TerrainSystem.TerrainManager.StartMatch();
        }

    }
}
