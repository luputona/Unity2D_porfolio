using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System.Text;

public interface IItemData
{
    void Awake();

    void Start();
   
    IEnumerator LoadData();
    

    void ConstructData();
    void LoadLocalData();
    void ConstructLocalData();

}
