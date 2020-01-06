#if UNITY_EDITOR
using UnityEngine;

[ExecuteInEditMode]
public class e_LevelButtons : MonoBehaviour
{
    public GameObject ButtonPrefab; //prefab
    public menu_MenuController MenuController;
    public int ButtonsInRow = 5;
    public Vector2 Offset = new Vector2(60,60);

}
#endif