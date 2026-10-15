using UnityEngine;

[System.Serializable]
public class LeaderTraits
{
    public Passive passive;
    public int maxMoves = 3;
    public Enum_HexTile_Type[] weakLandTypes = new Enum_HexTile_Type[] 
    { Enum_HexTile_Type.mountain, Enum_HexTile_Type.water };
    public Enum_HexTile_Type[] noWalkLandTypes = new Enum_HexTile_Type[] 
    { Enum_HexTile_Type.empty, Enum_HexTile_Type.wall };

    public bool ContainsUnfavoredType(Enum_HexTile_Type type)
    {
        foreach(var item in weakLandTypes)
            if(item == type) return true; ;

        return false;
    }
    public bool ContainsUnfavoredType(HexTile tile)
    {
        if (!tile) return false;

        return ContainsUnfavoredType(tile.type);
    }

    public bool ContainsNoWalkType(Enum_HexTile_Type type)
    {
        foreach(var item in noWalkLandTypes)
            if(item == type) return true; ;

        return false;
    }
    public bool ContainsNoWalkType(HexTile tile)
    {
        if (!tile) return false;

        return ContainsNoWalkType(tile.type);
    }
}
