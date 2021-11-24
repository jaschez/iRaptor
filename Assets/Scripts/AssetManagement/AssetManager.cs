using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This attribute makes the asset show up in the editor's Assets -> Create menu
// and when right-clicking in your Project window and choosing Create
[CreateAssetMenu(fileName = "New public Asset", menuName = "Custom/Asset Manager")]
public class AssetManager : ScriptableObject
{

    // You could also expose individual named fields, or provide a public method
    // to retrieve a material based on an enum key or other input...
    public Material whiteMat;

}
