using UnityEngine;
using Unity.Netcode;
using System;




public class PlayerConsoleInputTest : MonoBehaviour
{
    //this class is going to be solely for testing purposes.
    //players will be able to retrived board state as a simple grid and make input requests

    private string player_command = "";

    private void OnGUI()
    {
        GUI.Box(new Rect(10, 10, 320, 100), "RPC Test");
        player_command = GUI.TextField(new Rect(20, 40, 200, 25), player_command);

        if (GUI.Button(new Rect(230, 40, 80, 25), "Run")){
            RunCommand(player_command);
            player_command = "";

        }
        DrawCrossGrid();
    }
    private void RunCommand(string command)
    {
        string cmd = command.Trim().ToLower();
        if (PlayerConsoleInputRPCHandler.instance == null)
        {
            Debug.Log("NO RpcInputTestHandler Found");
            return;
        }

        switch (cmd)
        {
            //write  commands here, theyll call rpcinputtesthandler.instance.functionname
            case "move up":
                GridManager.instance.MoveMonadServerRpc(moveDir.UP);
                break;
            case "move down":
                GridManager.instance.MoveMonadServerRpc(moveDir.DOWN);
                break;
            case "move left":
                GridManager.instance.MoveMonadServerRpc(moveDir.LEFT);
                break;
            case "move right":
                GridManager.instance.MoveMonadServerRpc(moveDir.RIGHT);
                break;
            default:
                Debug.Log($"Unknown Command: {cmd}");
                break;
        }
    }

    private void DrawCrossGrid()
    {
        if (GridManager.instance == null)
        {
            GUI.Label(new Rect(10, 130, 200, 20), "No GridManager");
            return;
        }
        int cellSize = 30;
        int startX = 10;
        int startY = 160;

        var grid = GridManager.instance.getCrossGrid();
        for(int y = 0; y <grid.GetLength(0); y++)
        {
            for(int x = 0; x < grid.GetLength(1); x++)
            {
                var tile = grid[x, y];
                string val = ((int)tile.ownerID).ToString();

                GUI.Box(
                    new Rect(startX + x * cellSize, startY + y * cellSize, cellSize, cellSize), val
                    );

            }
        }

    }


}
