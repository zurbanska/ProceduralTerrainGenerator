using UnityEngine;
using UnityEngine.UIElements;

public class UIControler : MonoBehaviour
{
    public Button generateTerrainButton;
    public Button messageButton;
    public Label messageLabel;

    [SerializeField] private TerrainManager terrainManager;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        generateTerrainButton = root.Q<Button>("generate-terrain-button");
        messageButton = root.Q<Button>("show-message-button");
        messageLabel = root.Q<Label>("message-label");

        generateTerrainButton.clicked += GenerateTerrainButtonPressed;
        messageButton.clicked += MessageButtonPressed;
    }

    void GenerateTerrainButtonPressed()
    {
        terrainManager.GenerateChunks();
    }

    void MessageButtonPressed()
    {
        if (messageLabel.style.visibility == Visibility.Visible)
        {
            messageLabel.style.visibility = Visibility.Hidden;
        } else messageLabel.style.visibility = Visibility.Visible;

    }
}
