using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using System.Diagnostics.CodeAnalysis;


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

public enum moveDir
{
    LEFT,
    RIGHT,
    UP,
    DOWN
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

    public static GridManager instance { get; private set; }

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
        instance = this;
        //we want to retrieve the board size from the session manager
        init_boards();

    }

    public TileData[,] getTileGrid()
    {
        return tile_grid;
    }
    public CrossData[,] getCrossGrid()
    {
        return cross_grid;
    }

    private void init_boards()
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


    [ServerRpc(RequireOwnership = false)]
    public void MoveMonadServerRpc(moveDir direction, ServerRpcParams rpcParams = default)
    {
        ulong playerId = rpcParams.Receive.SenderClientId;

        if(try_move_monad(direction, playerId, out int oldX, out int oldY, out int newX, out int newY))
        {
            MoveMonadClientRpc(playerId, oldX, oldY, newX, newY);
        }

    }

    [ClientRpc]
    private void MoveMonadClientRpc(ulong playerId, int oldX, int oldY, int newX, int newY) 
    {
        cross_grid[oldY, oldX].ownerID = NO_OWNER_ID;
        cross_grid[newY, newX].ownerID = playerId;

    }


    private bool try_move_monad(moveDir direction, ulong playerId, out int oldX, out int oldY, out int newX, out int newY)
    {
        oldX = oldY = newX = newY = -1;
        if (!IsServer)
        {
            return false;
        }

        bool player_found = false;
        for(int y = 0; y < cross_board_size; y++)
        {
            for(int x = 0; x < cross_board_size; x++)
            {
                if (cross_grid[y,x].ownerID == playerId)
                {
                    player_found = true;
                    oldX = x;
                    oldY = y;
                    break;
                }
            }
            if (player_found) { break; }
        }

        if (!player_found)
        {
            Debug.Log("Player ID not found in grid!");
            return false;
        }

        switch (direction)
        {
            case moveDir.UP:
                newY = (oldY - 1 + cross_board_size) % cross_board_size;
                break;
            case moveDir.DOWN:
                newY = (oldY + 1) % cross_board_size;
                break;
            case moveDir.LEFT:
                newX = (oldX - 1 + cross_board_size) % cross_board_size;
                break;
            case moveDir.RIGHT:
                newX = (oldX + 1) % cross_board_size;
                break;
            default:
                break;
        }
        cross_grid[oldY, oldX].ownerID = NO_OWNER_ID;
        cross_grid[newY, newX].ownerID = playerId;
        return true;



    }

    //this whole method is retarded, theres definitely a better way of doing this
    //FIX LATER
    private void try_move_monad(moveDir direction, ulong player_id)
    {
       
        if (!IsServer)
        {
            return;
        }
        //find index for current position in cross grid
        //definitely a better way of doing it than an array lookup. Should be a list of player monad positions stored separately. Can be updated along with array

        int x_pos = 0;
        int y_pos = 0;
        bool player_found = false;
        for(int i = 0; i < cross_board_size; i++)
        {
            for(int j = 0; j < cross_board_size; j++)
            {
                if (cross_grid[i,j].ownerID == player_id)
                {
                    x_pos = j; y_pos = i;
                    player_found = true;
                    
                    break;
                }
            }
            if (player_found)
            {
                break;
            }
        }

        if (!player_found)
        {
            Debug.Log("Player ID not found in grid!");
            return;
        }

        int new_x_pos = x_pos;
        int new_y_pos = y_pos;
        switch (direction)
        {

            case moveDir.UP:
                new_y_pos = (y_pos - 1 + cross_board_size) % cross_board_size;
                    break;
            case moveDir.DOWN:
                new_y_pos = (y_pos + 1) % cross_board_size;
                break;
            case moveDir.LEFT:
                new_x_pos = (x_pos - 1 + cross_board_size) % cross_board_size;
                break;
            case moveDir.RIGHT:
                new_x_pos = (x_pos + 1) % cross_board_size;
                break;
            default:
                break;
        }
        cross_grid[y_pos, x_pos].ownerID = NO_OWNER_ID;
        cross_grid[new_y_pos, new_x_pos].ownerID = player_id;

    }



}
