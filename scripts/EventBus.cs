using Godot;

public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; }
    
    [Signal]
    public delegate void PersonaAgregadaEventHandler();
    
    public override void _EnterTree()
    {
        Instance = this;
    }
    
    public override void _Ready()
    {
        GD.Print("EventBus inicializado correctamente");
    }
    
    public void NotificarPersonaAgregada()
    {
        EmitSignal(SignalName.PersonaAgregada);
        GD.Print("Señal PersonaAgregada emitida");
    }
}