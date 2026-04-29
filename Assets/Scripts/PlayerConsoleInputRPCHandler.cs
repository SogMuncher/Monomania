using UnityEngine;

public class PlayerConsoleInputRPCHandler : MonoBehaviour
{
    public static PlayerConsoleInputRPCHandler instance;

    public void Awake()
    {
        instance = this;
    }

    // [ServerRpc(RequireOwnership =false)]
    //write methods here for player to make requests to the server which will alter the grid state


}
