using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Pokemon/Create new item")]

public class ItemBase : ScriptableObject
{
    [SerializeField] string name;

    [TextArea]
    [SerializeField] string description;
    [SerializeField] bool isRevive;
    [SerializeField] int healAmount;

    public string Name {
        get { return name; }
    }

    public string Description {
        get { return description; }
    }

    public bool IsRevive {
        get { return isRevive; }
    }

    public int HealAmount {
        get { return healAmount; }
    }
}
