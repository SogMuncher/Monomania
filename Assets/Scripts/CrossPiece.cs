
using System.Collections.Generic;


public abstract class CrossPiece : Piece
{
    
    public override List<(int x, int y)> getValidMoves()
    {
        List<(int x, int y)> validMoves = new List<(int x, int y)>();
        if (GridManager.instance == null)
        {
            return validMoves;
        }
        
        CrossData[,] crossGrid = GridManager.instance.getCrossGrid();
        int width = crossGrid.GetLength(1);
        int height = crossGrid.GetLength(0);

        int x = Position.x; int y = Position.y;

        CheckCross((x + 1) % width, y); //right
        CheckCross((x - 1 + width) % width, y); //left
        CheckCross(x, (y - 1 + height) % height); //up
        CheckCross(x, (y + 1) % height); //down


        void CheckCross(int checkX, int checkY)
        {
            if (crossGrid[checkY, checkX].ownerID == GridManager.NO_OWNER_ID)
            {
                validMoves.Add((checkX, checkY));
            }
        }


        return validMoves;
    }
}