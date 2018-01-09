using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace UnityEngine.UI
{
    public abstract class LoopScrollDataSource
    {
        public abstract void ProvideData(Transform transform, int idx);
        public abstract void LoadWeaponInvenData(Transform transform , List<WeaponInventory> weaponInven);
    }

	public class LoopScrollSendIndexSource : LoopScrollDataSource
    {
		public static readonly LoopScrollSendIndexSource Instance = new LoopScrollSendIndexSource();

		LoopScrollSendIndexSource(){}

        public override void LoadWeaponInvenData(Transform transform, List<WeaponInventory> weaponInven)
        {
            //transform.SendMessage("LoadItemData",  weaponInven);

        }

        public override void ProvideData(Transform transform, int idx)
        {
            //TODO : 여기다가 데이타 드리븐 연결
            if(CInventoryManager.EINVENTORY_CATEGORY.Weapon == CInventoryManager.GetInstance.m_eINVENTORY_CATEGORY)
            {
                transform.SendMessage("LoadWeaponInvenData", idx);
            }
            else if (CInventoryManager.EINVENTORY_CATEGORY.Potion == CInventoryManager.GetInstance.m_eINVENTORY_CATEGORY)
            {
                transform.SendMessage("LoadPotionInvenData", idx);
            }

        }
    }

	public class LoopScrollArraySource<T> : LoopScrollDataSource
    {
        T[] objectsToFill;

		public LoopScrollArraySource(T[] objectsToFill)
        {
            this.objectsToFill = objectsToFill;
        }

        public override void LoadWeaponInvenData(Transform transform, List<WeaponInventory> weaponInven)
        {            
        }

        public override void ProvideData(Transform transform, int idx)
        {
            transform.SendMessage("LoadWeaponInvenData", objectsToFill[idx]);
        }
    }
}