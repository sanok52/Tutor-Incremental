using UnityEngine;

public class LabyrinthView3DGizmos : LabyrinthView
{
    [SerializeField] private float nodeRadius = 0.5f;
    [SerializeField] private float unitDistance = 0.5f;

    private void OnDrawGizmos()
    {
        if (Data == null)
            return;

        foreach (var node in Data.labyrinthNodes)
        {
            Vector3 position = GetWorldPosition(node.position);
            Gizmos.DrawWireSphere(position, nodeRadius);
        }

        foreach (var trans in Data.labyrinthTransitions)
        {
            (LabyrinthNode, LabyrinthNode) nodes = Data.GetNodes(trans);
            Gizmos.DrawLine(GetWorldPosition(nodes.Item1.position), GetWorldPosition(nodes.Item2.position));
        }
    }

    public override Vector3 GetWorldPosition(Vector2Int position)
    {
        return transform.position + new Vector3(position.x, 0, -position.y) * unitDistance;
    }
}