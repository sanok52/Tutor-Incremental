using System.Collections;

public interface IOutSpikerView
{
    void Break();
    public bool IsPlay(OutDialogElement element);
    public IEnumerator Play(OutDialogElement element);
}
