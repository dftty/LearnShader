public class LightOnCmd : ICommand
{
    public Light light;

    public LightOnCmd(Light light)
    {
        this.light = light;
    }

    public void Execute()
    {
        light.On();
    }

    public void Undo()
    {
        light.Off();
    }
}