using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIVisualizer : MonoBehaviour
{

    static UIVisualizer instance;

    PlayerModule player;

    public BuffUIManager buffUIManager;

    public TextMeshModifier textNotifier;

    public UIGadgetVisualizer gadgetVisualizer;

    public Transform canvasParent;

    public GameObject healthBarGO;
    public GameObject popUpPrefab;
    public GameObject enemyHealthPrefab;

    public Text cuText;

    public Animator powerUpPart;
    public Animator sceneTransitioner;

    public StillPopUp cuPopUp;
    private Minimap minimap;

    Slider healthBarComponent;
    RectTransform healthBarTransform;

    Text healthText;

    int currentHealth;

    float originalHealthbarWidth;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    public void InitUI()
    {
        player = (PlayerModule)PlayerModule.GetInstance();
        player.OnEntityEvent += OnPlayerEvent;

        healthBarComponent = healthBarGO.GetComponent<Slider>();
        healthBarTransform = healthBarGO.GetComponent<RectTransform>();
        healthText = healthBarGO.GetComponentInChildren<Text>();

        originalHealthbarWidth = healthBarTransform.sizeDelta.x;

        healthBarComponent.value = 0;
        currentHealth = player.GetHP();

        healthText.text = currentHealth + "/" + player.GetMaxHP();
        healthBarTransform.sizeDelta = new Vector2(0, healthBarTransform.sizeDelta.y);

        cuText.text = player.GetCarbonUnits().ToString();

        PopUpMessage.ResetInfoPopupOffset();

        gadgetVisualizer.Init(player.GetGadgetUnits(), player.GetMaxGadgetUnits());

        minimap = Minimap.GetInstance();
        minimap.Generate();

        StartCoroutine("UpdateHealthBar");
        StartCoroutine("UpdateMaxHealthBar");
    }

    private void OnDisable()
    {
        player.OnEntityEvent -= OnPlayerEvent;
    }

    void OnPlayerEvent(Entity sender, Entity.EntityEvent eventType, int param)
    {

        if (eventType == Entity.EntityEvent.Heal)
        {
            Debug.Log("Curado en " + param);

            if (sender.GetEntityType() == EntityType.Player)
            {
                PopUp(PopUpType.Info, "Heal +" + param.ToString(), sender.transform, Color.green, 1f, 20);
            }

            PopUp(PopUpType.Good, param.ToString(), sender.transform, .7f, 20);

            StartCoroutine("UpdateHealthBar");
        }

        if (eventType == PlayerModule.PlayerEvent.AddedCU)
        {
            cuText.text = player.GetCarbonUnits().ToString();
            cuPopUp.UpdateValue(param);
        }

        if (eventType == PlayerModule.PlayerEvent.AddedGadgetUse)
        {
            string msg;

            if (player.GetGadgetUnits() < player.GetMaxGadgetUnits())
            {
                msg = "+1 ENERGY CAPSULE";
            }
            else
            {
                msg = "FULL!";
            }

            PopUp(PopUpType.Info, msg, player.transform, Color.yellow, 1);

            CamManager.GetInstance().ShakeQuake(2, 2f, false);
            gadgetVisualizer.AddUse();
        }

        if (eventType == PlayerModule.PlayerEvent.RechargedGadgetUse)
        {

            if (player.GetGadgetUnits() == player.GetMaxGadgetUnits())
            {
                CamManager.GetInstance().ShakeQuake(4, 2f, false);
            }

            gadgetVisualizer.AddUse();
        }

        if (eventType == PlayerModule.PlayerEvent.SpentGadgetUse)
        {

            gadgetVisualizer.SpendUse();
        }

        if (eventType == Entity.EntityEvent.Damage)
        {

            //Indicar daño producido
            PopUp(PopUpType.Bad, param.ToString(), sender.transform, .6f, 25);
            SoundManager.Play(Sounds.PlayerImpact, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

            CamManager.GetInstance().ShakeQuake(10, 1.5f, false);
            CamManager.GetInstance().ShockGame(.1f);
            StartCoroutine("UpdateHealthBar");
        }

        if (eventType == Entity.EntityEvent.Death)
        {
            //Indicar daño producido

            CamManager.GetInstance().ShakeQuake(20, 3f, true);
        }

        if (eventType == PlayerModule.PlayerEvent.BuffActivation)
        {
            buffUIManager.AddUIBuff((PowerUpDrop.PowerUpType)param, 20);
            StartCoroutine(AnimatePowerUp());

            textNotifier.NotifyMessage("NEW MODULE INSTALLED");
            PopUp(PopUpType.Info, ((PowerUpDrop.PowerUpType)param).ToString(), sender.transform, 1.5f, 30);
            Debug.Log("Buff activado: " + ((PowerUpDrop.PowerUpType)param).ToString());
        }

        if (eventType == PlayerModule.PlayerEvent.BuffExtension)
        {
            buffUIManager.AddUIBuff((PowerUpDrop.PowerUpType)param, 20);
            StartCoroutine(AnimatePowerUp());

            textNotifier.NotifyMessage("MODULE USE EXTENDED");
            PopUp(PopUpType.Info, ((PowerUpDrop.PowerUpType)param).ToString(), sender.transform, 1.5f, 30);
            Debug.Log("Buff extendido: " + ((PowerUpDrop.PowerUpType)param).ToString());
        }

        if (eventType == PlayerModule.PlayerEvent.BuffUse)
        {
            buffUIManager.UseBuffs();
        }

        if (eventType == PlayerModule.PlayerEvent.EndBuff)
        {

            buffUIManager.DeleteUIBuff((PowerUpDrop.PowerUpType)param);

            Debug.Log("Buff eliminado: " + ((PowerUpDrop.PowerUpType)param).ToString());
        }
    }

    public void PopUpImportantMessage(string str)
    {
        textNotifier.NotifyMessage(str);
    }

    public void TransitionScene()
    {
        sceneTransitioner.SetTrigger("upgrade");
    }

    public void PopUp(PopUpType type, string msg, Transform target, float lifeTime = .3f, int size = 25, float movement = 6f, int blinks = 3){
        GameObject go = Instantiate(popUpPrefab, canvasParent);

        go.transform.SetParent(canvasParent);
        PopUpMessage popUpComp = go.GetComponent<PopUpMessage>();

        popUpComp.Init(type, msg, target, lifeTime, size, movement, blinks);
    }

    public void PopUp(PopUpType type, string msg, Transform target, Color otherCol, float lifeTime = .3f, int size = 25, float movement = 6f, int blinks = 3)
    {
        GameObject go = Instantiate(popUpPrefab, canvasParent);

        go.transform.SetParent(canvasParent);
        PopUpMessage popUpComp = go.GetComponent<PopUpMessage>();

        popUpComp.Init(type, msg, otherCol, target, lifeTime, size, movement, blinks);
    }

    public void CreateEnemyHealthUI(EnemyModule enemy)
    {
        EnemyHealthUI enemyUI = Instantiate(enemyHealthPrefab, canvasParent).GetComponent<EnemyHealthUI>();
        enemyUI.AttachTarget(enemy);
    }

    IEnumerator UpdateHealthBar()
    {
        currentHealth = player.GetHP();

        int sign = healthBarComponent.value < currentHealth ? 1 : -1;

        healthBarComponent.maxValue = player.GetMaxHP();

        healthText.text = currentHealth + "/" + player.GetMaxHP();

        while ( sign * (currentHealth - healthBarComponent.value) > 0)
        {
            healthBarComponent.value = Mathf.Lerp(healthBarComponent.value, currentHealth + 1 * sign, Time.deltaTime * 2);
            yield return null;
        }

        healthBarComponent.value = currentHealth;

        yield return null;
    }

    IEnumerator UpdateMaxHealthBar()
    {
        //Realizamos este cálculo con el objetivo de que la barra no exceda su ancho por la derecha
        float barWidth = Mathf.Log(player.GetMaxHP(), 25) * originalHealthbarWidth;

        int sign = healthBarTransform.sizeDelta.x < barWidth ? 1 : -1;

        Vector2 healthSize = healthBarTransform.sizeDelta;

        healthText.text = currentHealth + "/" + player.GetMaxHP();

        while (sign * (barWidth - healthBarTransform.rect.width) > 0)
        {
            healthSize.x = Mathf.Lerp(healthSize.x, barWidth + 1 * sign, Time.deltaTime * 3);

            healthBarTransform.sizeDelta = healthSize;

            yield return null;
        }

        healthSize.x = barWidth;
        healthBarTransform.sizeDelta = healthSize;

        yield return null;
    }

    IEnumerator AnimatePowerUp()
    {
        powerUpPart.SetTrigger("powerup");

        powerUpPart.transform.position = player.transform.position;

        while (!powerUpPart.GetAnimatorTransitionInfo(0).IsName("exit"))
        {
            powerUpPart.transform.position = player.transform.position;
            yield return null;
        }
    }

    public static UIVisualizer GetInstance()
    {
        return instance;
    }
}
