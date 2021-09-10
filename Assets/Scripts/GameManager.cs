using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }return _instance;
        }
        
    }

    #region Public var
    //Fungsi [Range (min, max)] untuk menjaga value agar tetap berada pada range.
    [Range (0f, 1f)]
    public float AutoCollectPercentage = 0.1f;
    public ResourceConfig[] ResourceConfigs;  
    public Sprite[] ResourcesSprites;  

    public Transform ResourceParent;
    public ResourceControl ResourcePrefab;
    public TapText TapTextPrefab;

    public Transform CoinIcon;
    public Text GoldInfo;
    public Text AutoCollectInfo;
    #endregion

    private List<ResourceControl> _activeResources = new List<ResourceControl>();
    private List<TapText> _tapTextPool = new List<TapText>();
    private float _collectSecond;

    public double TotalGold { get; private set; }
    
    // Start is called before the first frame update
    private void Start()
    {
        AddAllResources();
    }

    // Update is called once per frame
    private void Update()
    {
        //Fungsi untuk eksekusi collect per second
        _collectSecond += Time.unscaledDeltaTime;
        if (_collectSecond >= 1f)
        {
            CollectPerSecond();
            _collectSecond = 0f;
        }

        CheckResourceCost();

        CoinIcon.transform.localScale = Vector3.LerpUnclamped(CoinIcon.transform.localScale, Vector3.one *2f, 0.15f);
        CoinIcon.transform.Rotate (0f, 0f, Time.deltaTime * -100f);
    }

    private void AddAllResources()
    {
        bool showResources = true;
        foreach (ResourceConfig config in ResourceConfigs)
        {
            GameObject obj = Instantiate (ResourcePrefab.gameObject, ResourceParent, false);
            ResourceControl resource = obj.GetComponent<ResourceControl>();

            resource.SetConfig(config);
            obj.gameObject.SetActive(showResources);

            if (showResources && !resource.isUnlocked)
            {
                showResources = false;
            }
            _activeResources.Add(resource);
        }
    }

    public void ShowNextResource()
    {
        foreach(ResourceControl resource in _activeResources)
        {
            if (!resource.gameObject.activeSelf)
            {
                resource.gameObject.SetActive(true);
                break;
            }
        }
    }

    private void CheckResourceCost()
    {
        foreach (ResourceControl resource in _activeResources)
        {
            bool isBuyable = false;
            if (resource.isUnlocked)
            {
                isBuyable = TotalGold >= resource.GetUpgradeCost();
            }else
            {
                isBuyable = TotalGold >= resource.GetUnlockCost();
            }
            resource.ResourceImage.sprite = ResourcesSprites[isBuyable ? 1 : 0];
        }
    }

    private void CollectPerSecond()
    {
        double output = 0;
        foreach (ResourceControl resource in _activeResources)
        {
            if (resource.isUnlocked)
            {
               output +=resource.GetOutput(); 
            }
            
        }

        output *= AutoCollectPercentage;

        //Fungsi ToString untuk memberikan 1 angka dibelakang koma
        AutoCollectInfo.text = $"Auto Collect: {output.ToString("F1")} / second";
        AddGold(output);
        Debug.Log("Duit Masuk" + output.ToString("0"));
    }

    public void AddGold(double value)
    {
        TotalGold += value;
        GoldInfo.text = $"Gold: {TotalGold.ToString("0")}";
    }

    public void CollectByTap (Vector3 tapPosition, Transform parent)
    {
        Debug.Log("Masuk sini");
        double output = 0;
        foreach (ResourceControl resource in _activeResources)
        {
            if (resource.isUnlocked)
            {
                output += resource.GetOutput();
            }
            
        }

        TapText tapText = GetOrCreateTapText();
        tapText.transform.SetParent (parent, false);
        tapText.transform.position = tapPosition;

        tapText.Text.text = $"+{output.ToString ("0")}";
        tapText.gameObject.SetActive(true);
        CoinIcon.transform.localScale = Vector3.one * 1.75f;

        AddGold(output);
        
    }

    private TapText GetOrCreateTapText()
    {
        TapText tapText = _tapTextPool.Find (t => !t.gameObject.activeSelf);
        if (tapText == null)
        {
            tapText = Instantiate (TapTextPrefab).GetComponent<TapText>();
            _tapTextPool.Add(tapText);
        }
        return tapText;
    }
    
}

[System.Serializable]
public struct ResourceConfig
{
    public string Name;
    public double UnlockCost;
    public double UpgradeCost;
    public double Output;
}
