using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;


//probably better datastructure out there, could player be ref'd directly from playerlist? doesnt seem to like pointers.

public enum PieceType
{
    //no need to include monad here as will never be used on a tile piece, can be idenfitied by position on cross grid by playerid
    None,
    MASS,
    MEDIA,
    MILITANT,
    MARTYR,
    MASTER
}

public struct TileData {
    public PieceType piece_type;
    public ulong player_id;

}

public struct CrossData
{
    public ulong ownerID;
}

public class GridManager : NetworkBehaviour
{

    public const ulong NO_OWNER_ID = ulong.MaxValue; //going to use this to indicate default no ownership state of a grid entry 

    [SerializeField] SessionManager SessionManager;
    //2 grids, one for storing the tile square board state, and the other for the cross sections/corners
    //probably each index can be populated by combining player id with piece type
    private TileData[,] tile_grid;
    private CrossData[,] cross_grid; //cross grid will require special logic as an edge being occupied also means its corresponding edge on the other side is also occupied

    private int board_size;
    private int cross_board_size;



    private void Awake()
    {
        //we want to retrieve the board size from the session manager
        init_board();



        


    }

    private void init_board()
    {
        //we can set up the board here, write logic for initial piece setup, etc...

        //setting up default board state here
        if (SessionManager != null)
        {
            board_size = SessionManager.BoardSize;
        }
        else
        {
            board_size = 3;
        }

        tile_grid = new TileData[board_size, board_size];
        for (int i = 0; i < board_size; i++)
        {
            for (int j = 0; j < board_size; j++)
            {
                tile_grid[i, j] = new TileData { piece_type = PieceType.MASS, player_id = NO_OWNER_ID };
            }
        }
        cross_board_size = board_size + 1;
        cross_grid = new CrossData[cross_board_size, cross_board_size]; //cross section grid will simply be one unit larger
        for (int i = 0; i < cross_board_size; i++)
        {
            for (int j = 0; j < cross_board_size; j++)
            {
                cross_grid[i, j] = new CrossData { ownerID = NO_OWNER_ID };
            }
        }

    }

    //implement RPC code below


}
