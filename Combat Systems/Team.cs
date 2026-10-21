using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Team 
{
    public string teamName = "new team";
    public Color teamColor = Color.white;
    public Team()
    {
        teamName = "No Allegiance";
        teamColor = Color.lightGray / 2;
    }
    public Team(string newName, Color newColor)
    {
        teamName = newName;
        teamColor = newColor;
    }

    public bool CompareAllied(Team other)
    {
        return teamName.ToLower() == other.teamName.ToLower();
    }

    public bool CompareEnemy(Team other)
    {
        return teamName.ToLower() != other.teamName.ToLower();
    }
    public override string ToString()
    {
        return $"<color=#{teamColor.ToHexString()}>{teamName}</color>";
    }
    // Override == operator
    public static bool operator ==(Team a, Team b)
    {
        // Handle null comparisons safely
        if (ReferenceEquals(a, b))
            return true;

        if (a is null || b is null)
            return false;

        return a.CompareAllied(b);
    }
     
    public static bool operator !=(Team a, Team b)
    {
        return !(a == b);
    }

    // Override Equals so collections use the same logic
    public override bool Equals(object obj)
    {
        return this == obj as Team;
    }

    // Recommended when overriding Equals
    public override int GetHashCode()
    {
        // Stable hash based on team ID (or whatever uniquely identifies your teams)
        return teamName.GetHashCode();
    }
}
