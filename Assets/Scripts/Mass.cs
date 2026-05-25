
using UnityEngine;
using System.Collections.Generic;


public class Mass : TilePiece
{
    private void OnMouseDown()
    {
        var moves = getValidMoves();
        if (!isActive) { setActive(true); } // piece is made "active" when clicked

        //implement further handling for moving a mass piece, then make rpc request to change the grid state

    }

}
