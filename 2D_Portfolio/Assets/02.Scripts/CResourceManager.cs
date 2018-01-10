using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CResourceManager : SingleTon<CResourceManager>
{
    private static CResourceManager _instance = null;

    private Dictionary<string, Sprite> m_weaponSprite = new Dictionary<string, Sprite>();
    private Dictionary<string, Sprite> m_characterillurSprite = new Dictionary<string, Sprite>();
    private Dictionary<string, RuntimeAnimatorController> m_characterAnimator = new Dictionary<string, RuntimeAnimatorController>();
      
    void Awake()
    {
        if(_instance != null)
        {
            Destroy(this);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
        LoadWeaponSprite("Sprites/Items/Weapon");
        LoadCharacterillurSprite("Sprites/Characters/illustrate/Weapon");
        LoadCharacterAnimator("Animator");
    }
  
    void Start ()
    {
        

        //Debug.Log(GetAnimator("w060002").name);
        //Debug.Log(GetWeaponSprite("w060002").name);
        
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void LoadWeaponSprite(string weaponSpritePath)
    {
        //"Sprite/Items/Weapon"
        Sprite[] tSp = Resources.LoadAll<Sprite>(weaponSpritePath);
        for(int i = 0; i < tSp.Length; i++)
        {
            Sprite tSp1 = tSp[i];
            m_weaponSprite[tSp1.name] = tSp1;
        }
    }

    public void LoadCharacterillurSprite(string illurSpritePath)
    {
        Sprite[] tSp = Resources.LoadAll<Sprite>(illurSpritePath);
        for (int i = 0; i < tSp.Length; i++)
        {
            Sprite tSp1 = tSp[i];
            m_characterillurSprite[tSp1.name] = tSp1;
        }
    }
    public void LoadCharacterAnimator(string animPath)
    {
        RuntimeAnimatorController[] tAnim = Resources.LoadAll<RuntimeAnimatorController>(animPath);
        for(int i = 0; i < tAnim.Length; i++)
        {
            RuntimeAnimatorController tAnim1 = tAnim[i];
            m_characterAnimator[tAnim1.name] = tAnim1;
        }
    }

    public RuntimeAnimatorController GetAnimator(string itemcode)
    {
        return m_characterAnimator[itemcode];
    }

    public Sprite GetWeaponSprite(string itemcode)
    {
        return m_weaponSprite[itemcode];
    }

    public Sprite GetillurSprite(string itemcode)
    {
        return m_characterillurSprite[itemcode];
    }
}
