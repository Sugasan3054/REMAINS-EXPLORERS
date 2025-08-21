using System;

[Serializable]
public class Role
{
    public int id;
    public string name;
    public string description;
    public int abilityUses;
    public int usedAbilities = 0;
    public bool isDisabled = false;

    public Role(int id, string name, string description, int abilityUses = 1)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.abilityUses = abilityUses;
    }

    // �R�s�[�p�̃R���X�g���N�^
    public Role(Role other)
    {
        this.id = other.id;
        this.name = other.name;
        this.description = other.description;
        this.abilityUses = other.abilityUses;
        this.usedAbilities = 0;
        this.isDisabled = false;
    }
}