using UnityEngine;

public class RoadViewMono : MonoBehaviour, IRoadView
{
    public virtual void View(RoomViewInformation info)
    { }
    public virtual void ClearView()
    { }
}