public class OutSpikerViewSpecific : OutSpikerView
{
    public string IDSpiker;

    public override bool IsPlay(OutDialogElement element) => element.IDSpiker == IDSpiker;
}