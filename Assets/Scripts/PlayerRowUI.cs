using TMPro;
using UnityEngine;


// Pretty straightforward, just a utility function for setting the label/name of a player row prefab

public class PlayerRowUI : MonoBehaviour
{

    [SerializeField] private TMP_Text nameLabel;
    public void set_name(string name)
    {
        if (nameLabel)
        {
            nameLabel.text = name;
        }
    }

}
