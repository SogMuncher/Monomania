
using System.Collections.Generic;
using UnityEngine;


public abstract class Piece : MonoBehaviour
{
    public bool isActive { get; private set; }
    public ulong OwnerId { get; private set; }

    public (int x, int y) Position { get; private set;} //this is just used for rendering purposes. authoritative position is in the GRIDMANAGER
    //function returns a list of the valid places a piece can move from its current position
    public abstract List<(int x, int y)> getValidMoves();

    public void Initialize(ulong ownerId, int x, int y)
    {
        OwnerId = ownerId;
        Position = (x, y);
        isActive = false;
    }

    public void setPosition(int x, int y)
    {
        Position = (x, y);
    }
    public void setActive(bool active)
    {
        isActive = active;
    }

    protected virtual void OnMouseDown()
    {
        var moves = getValidMoves();
        if (!isActive) { setActive(true); } // piece is made "active" when clicked

        //further functionality can be implemented by children extending the base class function

    }


}

