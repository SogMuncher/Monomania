
using System.Collections.Generic;
using UnityEngine;


public abstract class TilePiece : Piece
{
    //all pieces that move along the tilegrid will have the same function for returning valid moves(unless im mistaken)
    public override List<(int x, int y)> getValidMoves()
    {
        List<(int x, int y)> validMoves = new List<(int x, int y)>();
        if (GridManager.instance == null)
        {
            return validMoves;
        }

        TileData[,] tileGrid = GridManager.instance.getTileGrid();
        int width = tileGrid.GetLength(1);
        int height = tileGrid.GetLength(0);

        int x = Position.x; int y = Position.y;

        CheckTile((x + 1) % width, y); //right
        CheckTile((x - 1 + width) % width, y); //left
        CheckTile(x, (y - 1 + height) % height); //up
        CheckTile(x, (y + 1) % height); //down


        void CheckTile(int checkX, int checkY)
        {
            if (tileGrid[checkY, checkX].player_id == GridManager.NO_OWNER_ID)
            {
                validMoves.Add((checkX, checkY));
            }
        }


        return validMoves;
    }
}