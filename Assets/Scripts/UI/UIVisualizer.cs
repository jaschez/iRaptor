using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UIVisualizer : MonoBehaviour
{

    static UIVisualizer instance;

    PlayerModule player;

    public InventoryUIManager UIItemManager;

    public TextMeshModifier textNotifier;

    public UIGadgetVisualizer gadgetVisualizer;

    public GameOverUI gameOverUI;

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

        healthBarComponent.value = 1;
        currentHealth = player.GetHP();

        healthText.text = currentHealth + "/" + player.GetMaxHP();

        cuText.text = player.GetCarbonUnits().ToString();

        PopUpMessage.ResetInfoPopupOffset();

        gadgetVisualizer.Init(player.GetGadgetUnits(), player.GetMaxGadgetUnits());

        minimap = Minimap.GetInstance();
        minimap.Generate();

        UpdateHealthBar(false);
        UpdateMaxHealthBar();

        TransitionSystem.GetInstance().SetTransitionColor(Color.black);
        TransitionSystem.GetInstance().Apply(TransitionSystem.Transition.FadeIn, .3f);
    }

    void OnPlayerEvent(Entity sender, Entity.EntityEvent eventType, object param)
    {

        if (eventType == Entity.EntityEvent.Heal)
        {
            Debug.Log("Curado en " + param);

            if (sender.GetEntityType() == EntityType.Player)
            {
                PopUp(PopUpType.Info, "Heal +" + param.ToString(), sender.transform, Color.green, 1f, 20);
            }

            PopUp(PopUpType.Good, param.ToString(), sender.transform, .7f, 20);

            UpdateHealthBar();
        }

        if (eventType == PlayerModule.PlayerEvent.AddedCU)
        {
            cuText.text = player.GetCarbonUnits().ToString();
            cuPopUp.UpdateValue((int)param);
        }

        if (eventType == PlayerModule.PlayerEvent.SpentCU)
        {
            cuText.text = player.GetCarbonUnits().ToString();
            PopUp(PopUpType.Info, "Chest purchased", player.transform, Color.cyan, 1);
        }

        if (eventType == PlayerModule.PlayerEvent.InsufficientCU)
        {
            cuText.text = player.GetCarbonUnits().ToString();
            PopUp(PopUpType.Info, "INSUFFICIENT CU", player.transform, Color.white, 1, 25, 6, 0);
        }

        if (eventType == PlayerModule.PlayerEvent.AddedGadgetPortion)
        {
            gadgetVisualizer.SetPortion((float)param);
        }

        if (eventType == PlayerModule.PlayerEvent.AddedGadgetUse)
        {
            string msg;

            if (player.GetGadgetUnits() < player.GetMaxGadgetUnits())
            {
                msg = "+1 AMMO";
            }
            else
            {
                msg = "FULL";
            }

            PopUp(PopUpType.Info, msg, player.transform, Color.yellow, .3f, 25, 6, 2);

            CamManager.GetInstance().ShakeQuake(2, 2f, false);
            gadgetVisualizer.AddUse();
        }

        if (eventType == PlayerModule.PlayerEvent.RechargedGadgetUse)
        {

            if (player.GetGadgetUnits() == player.GetMaxGadgetUnits())
            {
                CamManager.GetInstance().ShakeQuake(4, 2f, false);
            }

            for (int i = 0; i < (int)param; i++) {
                gadgetVisualizer.AddUse();
            }
        }

        if (eventType == PlayerModule.PlayerEvent.SpentGadgetUse)
        {
            gadgetVisualizer.SpendUse();
        }

        if (eventType == Entity.EntityEvent.Damage)
        {

            //Indicar daño producido
            PopUp(PopUpType.Bad, param.ToString(), sender.transform, .6f, 25);
            SoundManager.Play(Sound.PlayerImpact, CamManager.GetInstance().transform.position, CamManager.GetInstance().transform);

            CamManager.GetInstance().ShakeQuake(10, 1.5f, false);
            CamManager.GetInstance().ShockGame(.1f);
            UpdateHealthBar();
        }

        if (eventType == Entity.EntityEvent.Death)
        {
            //Indicar daño producido

            CamManager.GetInstance().ShakeQuake(20, 3f, true);

            if (sender.GetEntityType() == EntityType.Player)
            {
                gameOverUI.Show();
            }
        }

        if (eventType == PlayerModule.PlayerEvent.ItemPicked)
        {
            ItemData item = (ItemData)param;

            UIItemManager.AddUIItem(item);
            StartCoroutine(AnimatePowerUp());

            textNotifier.NotifyMessage(item.Description.ToUpper());
            PopUp(PopUpType.Info, item.Name.ToUpper(), sender.transform, 1.5f, 30);
            Debug.Log("Item recogido: " + item.Name);
        }
    }

    public void PopUpImportantMessage(string str)
    {
        textNotifier.NotifyMessage(str);
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

    void UpdateHealthBar(bool animated = true)
    {
        currentHealth = player.GetHP();

        healthBarComponent.maxValue = player.GetMaxHP();

        healthText.text = currentHealth + "/" + player.GetMaxHP();

        if (animated) {
            healthBarComponent.DOValue(currentHealth, .5f).SetEase(Ease.OutElastic);
        }
        else
        {
            healthBarComponent.value = currentHealth;
        }
    }

    void UpdateMaxHealthBar()
    {
        //Realizamos este cálculo con el objetivo de que la barra no exceda su ancho por la derecha
        float barWidth = Mathf.Log(player.GetMaxHP(), 25) * originalHealthbarWidth;

        Vector2 healthSize = healthBarTransform.sizeDelta;

        healthSize.x = barWidth;
        healthBarTransform.sizeDelta = healthSize;
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
