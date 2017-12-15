using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CItemData : SingleTon<CItemData>
{
    protected abstract void Awake();
    protected abstract void Start();

    protected abstract IEnumerator LoadData();
    protected abstract void ConstructData();

}
