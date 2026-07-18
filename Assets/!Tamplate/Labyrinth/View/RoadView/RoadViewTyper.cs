using System.Collections;
using System.Linq;
using UnityEngine;

public class RoadViewTyper : RoadViewMono
{
    [SerializeField] private TextTyper textTyper;
    [SerializeField] private bool drawPlayer = true;

    public override void View(RoomViewInformation info)
    {
       StartCoroutine(textTyper.ClearAndTypeText(GetDirectionsText(info.transDirections, info), 150));
    }

    private string GetDirectionsText(MoveDirection[] direction, RoomViewInformation info)
    {
        bool isFwd = direction.Contains(MoveDirection.Fwd);
        bool isRight = direction.Contains(MoveDirection.Right);
        bool isLeft = direction.Contains(MoveDirection.Left);
        bool isBack = direction.Contains(MoveDirection.Back);

        string b = "###"; //base
        string ss = $"<color=#000000>#</color>"; //space
        string s = $"<color=#000000>{b}</color>"; //space
        string w = $"<color=grey>{b}</color>"; //wall
        string e = $"<color=#FF00C1>{b}</color>"; //exit / enter

        string lockable = $"<color=#ffa500>{b}</color>";
        string unlockable = $"<color=#00C8FF>{b}</color>";

        string p1 = drawPlayer ? $"<color=#00FF00>/\\</color>{ss}" : s;
        string p2 = drawPlayer ? $"{ss}<color=#00FF00> | </color>{ss}" : s;

        bool IsLockable(MoveDirection dir) =>
            info.localbleDirections != null && info.localbleDirections.Contains(dir);

        bool IsUnlockable(MoveDirection dir) =>
            info.unlockableDirections != null && info.unlockableDirections.Contains(dir);

        bool HasAny(MoveDirection dir, bool isOpenDirFlag) =>
            isOpenDirFlag || IsLockable(dir) || IsUnlockable(dir) ||
            (info.Node.isEnterNode && info.toExitDirecrion == dir);

        string Colorize(MoveDirection dir, string normal, bool isOpenDirFlag)
        {
            // выход
            if (info.Node.isEnterNode && info.toExitDirecrion == dir)
                return e;

            // двери
            if (IsLockable(dir))
                return lockable;

            if (IsUnlockable(dir))
                return unlockable;

            // обычный проход
            if (isOpenDirFlag)
                return normal;

            // иначе пусть вызывающий решает, что рисовать (стена/пустота)
            return normal;
        }

        // флаг "что-то есть в этом направлении" (проход/дверь/выход)
        bool hasFwd = HasAny(MoveDirection.Fwd, isFwd);
        bool hasLeft = HasAny(MoveDirection.Left, isLeft);
        bool hasRight = HasAny(MoveDirection.Right, isRight);
        bool hasBack = HasAny(MoveDirection.Back, isBack);

        // линия вперёд
        string line1 = hasFwd
            ? $"{w}{Colorize(MoveDirection.Fwd, s, isFwd)}{w}"
            : $"{w}{w}{w}";

        // линия с боками (лево/право)
        string leftMid = hasLeft ? Colorize(MoveDirection.Left, s, isLeft) : w;
        string rightMid = hasRight ? Colorize(MoveDirection.Right, s, isRight) : w;
        string line2 = $"{leftMid}{s}{rightMid}";

        // линия с игроком — тут тоже красим двери/выход
        string leftPlayer = hasLeft ? Colorize(MoveDirection.Left, w, isLeft) : w;
        string rightPlayer = hasRight ? Colorize(MoveDirection.Right, w, isRight) : w;
        string line3 = $"{leftPlayer}{p1}{rightPlayer}";

        // линия назад
        string line4 = hasBack
            ? $"{w}{Colorize(MoveDirection.Back, s, isBack)}{w}"
            : $"{w}{w}{w}";

        return $"{line1}\n{line1}\n{line2}\n{line3}\n{line4}\n{line4}";
    }

    public override void ClearView()
    {
        StartCoroutine(textTyper.PlayClearText());
        base.ClearView();
    }
}