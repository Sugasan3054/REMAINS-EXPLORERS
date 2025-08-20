using System.Collections.Generic;
using System;

[Serializable]
public class Role
{
    public int id;
    public string name;
    public string description;
    public int abilityUses = 1;
    public int usedAbilities = 0;
    public bool isDisabled = false; // ���s���ɂ���Ĕ\�͂����������ꂽ��

    public Role(int id, string name, string description, int abilityUses = 1)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.abilityUses = abilityUses;
    }
}