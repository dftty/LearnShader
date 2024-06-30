public class LightOffCmd : ICommand
{
    public Light light;

    public LightOffCmd(Light light)
    {
        this.light = light;
    }

    public void Execute()
    {
        light.Off();
    }

    public void Undo()
    {
        light.On();
    }
}