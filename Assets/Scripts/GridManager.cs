using UnityEngine;
using Unity.Netcode;
using UnityEngine.Rendering;
using System.Diagnostics.CodeAnalysis;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Splines.ExtrusionShapes;


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

    public event Action<int, int, CrossData> OnCrossChanged;
    public event Action<int, int, TileData> OnTileChanged;



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

    private void SetCross(int y, int x, CrossData crossData)
    {
        cross_grid[y,x] = crossData;
        OnCrossChanged?.Invoke(x,y, crossData);
    }

    private void SetTile(int y, int x, TileData tileData)
    {
        tile_grid[y,x] = tileData;
        OnTileChanged?.Invoke(x, y, tileData);
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
    
    //this function will be called from elsewhere, clients will make a request to move a piece at a passed position, and in a passed direction
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void MoveTilePieceServerRpc(int posX, int posY, moveDir direction, RpcParams rpcParams = default)
    {
        ulong playerId = rpcParams.Receive.SenderClientId;
        if (tile_grid[posY, posX].player_id != playerId)
        {
            Debug.Log("Piece at specified position does not belong to calling player!");
            return;
        }
        PieceType pieceType = tile_grid[posY, posX].piece_type;

        if(try_move_tile_piece(posX, posY, direction, out int oldX, out int oldY, out int newX, out int newY))
        {
            moveTilePieceClientRpc(playerId, pieceType,oldX, oldY, newX, newY);
        }
    }
    //this function updates the tilegrid for every client assuming the original host making the request was successful
    [ClientRpc]
    private void moveTilePieceClientRpc(ulong playerId, PieceType pieceType, int oldX, int oldY, int newX, int newY)
    {
        SetTile(oldY, oldX, new TileData { player_id = NO_OWNER_ID, piece_type = PieceType.None });
        SetTile(newY, newX, new TileData { player_id = playerId, piece_type = pieceType });
    }

    //this function double checks that the request is valid.
    private bool try_move_tile_piece(int x,int y, moveDir direction, out int oldX, out int oldY, out int newX, out int newY)
    {
        oldX = x; 
        oldY = y;
        newX = newY = -1;

        if (!IsServer)
        {
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
        if (tile_grid[newY, newX].player_id == NO_OWNER_ID)
        {
            return true; // the space we've requested to move to is empty
        }

        return false;
    }

    //function is called from outside the class, player makes a request to move their monad piece in a specified direction
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void MoveMonadServerRpc(moveDir direction, RpcParams rpcParams = default)
    {
        ulong playerId = rpcParams.Receive.SenderClientId;

        if(try_move_monad(direction, playerId, out int oldX, out int oldY, out int newX, out int newY))
        {
            MoveMonadClientRpc(playerId, oldX, oldY, newX, newY);
        }

    }
    //here data is updated for all clients assuming the request was successful
    [ClientRpc]
    private void MoveMonadClientRpc(ulong playerId, int oldX, int oldY, int newX, int newY) 
    {
        SetCross(oldY, oldX, new CrossData { ownerID = NO_OWNER_ID });
        SetCross(newY, newX, new CrossData { ownerID = playerId });

    }
    
    //function checks that a requested move is valid
    //Given interactivity will extend from clicks the calling player should already know where their monad piece is. no need to search for it. Change at some point!!!
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
        
        SetCross(oldY, oldX, new CrossData { ownerID = NO_OWNER_ID });
        SetCross(newY, newX, new CrossData { ownerID = playerId });
        return true;



    }


}
