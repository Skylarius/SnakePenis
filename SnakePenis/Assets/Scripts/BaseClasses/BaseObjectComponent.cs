using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObjectComponent : MonoBehaviour
{
    // Used to mark as active in game
    private bool activeSelfInGame = false;
    
    public void SetActiveInGame(bool condition)
    {
        activeSelfInGame = condition;
    }

    public bool IsActiveInGame()
    {
        return activeSelfInGame;
    }

    private void OnEnable()
    {
        activeSelfInGame = true;
    }

    private void OnDisable()
    {
        activeSelfInGame = false;
    }
}
