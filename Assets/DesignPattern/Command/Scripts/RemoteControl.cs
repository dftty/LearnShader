using System.Collections.Generic;

public class RemoteControl
{
    private ICommand[] onCommands;
    private ICommand[] offCommands;

    public RemoteControl()
    {
        onCommands = new ICommand[7];
        offCommands = new ICommand[7];

        ICommand noCommand = new NoCmd();
        for (int i = 0; i < 7; i++)
        {
            onCommands[i] = noCommand;
            offCommands[i] = noCommand;
        }
    }

    public void SetCommand(int slot, ICommand onCommand, ICommand offCommand)
    {
        onCommands[slot] = onCommand;
        offCommands[slot] = offCommand;
    }

    public void OnButtonWasPushed(int slot)
    {
        if (slot < 0 || slot >= onCommands.Length)
        {
            return;
        }

        onCommands[slot].Execute();
    }

    public void OffButtonWasPushed(int slot)
    {
        if (slot < 0 || slot >= offCommands.Length)
        {
            return;
        }

        offCommands[slot].Execute();
    }
}