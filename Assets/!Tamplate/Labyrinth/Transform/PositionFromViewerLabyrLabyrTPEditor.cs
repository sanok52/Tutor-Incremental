#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PositionFromViewerLabyrLabyrTP))]
public class PositionFromViewerLabyrLabyrTPEditor : Editor
{
    private PositionFromViewerLabyrLabyrTP _target;

    private void OnEnable()
    {
        _target = (PositionFromViewerLabyrLabyrTP)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        //if (Application.isPlaying)
        //   return;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Управление в редакторе", EditorStyles.boldLabel);

        // Гарантируем, что LabyrTransform существует и инициализирован
        if (_target.LabyrTransform == null)
        {
            _target.SetTransformNotSubscrube(new LabyrinthObjectTransform());
        }

        // Проверяем, что данные лабиринта не null
        if (_target.LabyrinthView == null || _target.LabyrinthView.Data == null)
        {
            EditorGUILayout.HelpBox("LabyrinthView или Data не назначены!", MessageType.Warning);
            return;
        }

        // Если data у LabyrTransform не инициализирована, делаем это сейчас
        if (_target.LabyrTransform.Data == null && !Application.isPlaying)
        {
            string startNodeId = _target.LabyrinthView.Data.labyrinthNodes.Length > 0
                ? _target.LabyrinthView.Data.labyrinthNodes[0].ID
                : null;
            if (string.IsNullOrEmpty(startNodeId))
            {
                EditorGUILayout.HelpBox("В данных лабиринта нет узлов!", MessageType.Error);
                return;
            }
            _target.LabyrTransform.Init(_target.LabyrinthView.Data, startNodeId, Vector2Int.up);
        }

        // ---- Движение ----
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("↑ Вперёд"))
        {
            _target.LabyrTransform.Translate(MoveDirection.Fwd, _target.IsRotateMove);

            _target.UpdatePositionWork(_target.LabyrTransform.GetPosition());
            _target.UpdateDirectionWork(_target.LabyrTransform.GetDirection());
        }
        if (GUILayout.Button("↓ Назад"))
        {
            _target.LabyrTransform.Translate(MoveDirection.Back, _target.IsRotateMove);

            _target.UpdatePositionWork(_target.LabyrTransform.GetPosition());
            _target.UpdateDirectionWork(_target.LabyrTransform.GetDirection());
        }
        if (GUILayout.Button("← Влево"))
        {
            _target.LabyrTransform.Translate(MoveDirection.Left, _target.IsRotateMove);

            _target.UpdatePositionWork(_target.LabyrTransform.GetPosition());
            _target.UpdateDirectionWork(_target.LabyrTransform.GetDirection());
        }
        if (GUILayout.Button("→ Вправо"))
        {
            _target.LabyrTransform.Translate(MoveDirection.Right, _target.IsRotateMove);

            _target.UpdatePositionWork(_target.LabyrTransform.GetPosition());
            _target.UpdateDirectionWork(_target.LabyrTransform.GetDirection());
        }
        EditorGUILayout.EndHorizontal();

        // ---- Поворот ----
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("↺ Поворот влево"))
        {
            _target.LabyrTransform.Rotate(MoveDirection.Left);

            _target.UpdatePositionWork(_target.LabyrTransform.GetPosition());
            _target.UpdateDirectionWork(_target.LabyrTransform.GetDirection());
        }
        if (GUILayout.Button("↻ Поворот вправо"))
        {
            _target.LabyrTransform.Rotate(MoveDirection.Right);

            _target.UpdatePositionWork(_target.LabyrTransform.GetPosition());
            _target.UpdateDirectionWork(_target.LabyrTransform.GetDirection());
        }
        EditorGUILayout.EndHorizontal();

        // ---- Мгновенное обновление (без анимации) ----
        if (GUILayout.Button("Обновить позицию и поворот (мгновенно)"))
        {
            // Вызовы методов, которые уже используют LabyrTransform.GetPosition() и GetDirection()
            _target.UpdatePositionWork(_target.LabyrTransform.GetPosition());
            _target.UpdateDirectionWork(_target.LabyrTransform.GetDirection());
        }

        // ---- ----
        if (GUILayout.Button("Обновить данные"))
        {
            string startNodeId = _target.LabyrinthView.Data.labyrinthNodes.Length > 0
                ? _target.LabyrinthView.Data.labyrinthNodes[0].ID
                : null;
            if (string.IsNullOrEmpty(startNodeId))
            {
                EditorGUILayout.HelpBox("В данных лабиринта нет узлов!", MessageType.Error);
                return;
            }
            _target.LabyrTransform.Init(_target.LabyrinthView.Data, startNodeId, Vector2Int.up);
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(_target);
        }
    }
}
#endif