using UnityEngine;

public class RealTimeLabyrinthEditor : MonoBehaviour
{
    public LabyrinthDataSO dataSO;
    public LabyrinthData data;
    public LabyrinthView labyrinthView;    

    private void OnValidate()
    {
        if((data == null && dataSO == null) ||  labyrinthView == null) 
            return;

        labyrinthView.UpdateView(dataSO ? dataSO.Data : data);
    }
} 