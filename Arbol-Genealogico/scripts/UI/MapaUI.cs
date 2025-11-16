using Godot;
using System;
using System.Collections.Generic;
using Arbol_Core.DataStructures;
using Arbol_Core.Models;

public partial class MapaUI : Node2D
{
    [Export]
    public NodePath MapaTexturePath = "MapaTextureRect";

    private const int MARCADOR_SIZE = 48; // tamaño en px
    private const int LINE_WIDTH = 3;

    private TextureRect mapaTexture;
    private Grafo grafo;
    private Button volverBtn;

    // se guardan marcadores por cédula
    private Dictionary<string, Control> marcadores = new Dictionary<string, Control>();

    private Node2D dibujoContainer;

    public override void _Ready()
    {
        mapaTexture = GetNodeOrNull<TextureRect>(MapaTexturePath);
        if (mapaTexture == null)
        {
            GD.PrintErr("MapaUI: no se encontró TextureRect en la ruta '" + MapaTexturePath + "'. Crea un TextureRect llamado 'MapaTextureRect' o ajusta el export NodePath.");
            return;
        }

        grafo = Grafo.ObtenerInstancia();

        dibujoContainer = new Node2D();
        dibujoContainer.Name = "DibujoContainer";
        mapaTexture.AddChild(dibujoContainer);

        volverBtn = GetNodeOrNull<Button>("volver");
        if (volverBtn != null)
        {
            volverBtn.Pressed += OnVolverPressed;
        }

        ActualizarLabelsInfo();
        ReconstruirMarcadores();
    }

    private void ActualizarLabelsInfo()
    {
        var (distancia, lejos, cerca) = grafo.ObtenerValoresUI();

        var labelDistancia = GetNodeOrNull<Label>("promedio");
        if (labelDistancia != null)
            labelDistancia.Text = $"Distancia promedio: {distancia}";

        var labelLejos = GetNodeOrNull<Label>("lejos");
        if (labelLejos != null)
            labelLejos.Text = lejos != string.Empty ? $"Más lejos: {lejos}" : "Más lejos: N/A";

        var labelCerca = GetNodeOrNull<Label>("cerca");
        if (labelCerca != null)
            labelCerca.Text = cerca != string.Empty ? $"Más cerca: {cerca}" : "Más cerca: N/A";
    }

    // reconstruye todos los marcadores (llamar después de agregar personas)
    public void ReconstruirMarcadores()
    {
        // limpiar marcadores anteriores y dibujos
        foreach (var kv in marcadores)
        {
            if (kv.Value != null && kv.Value.IsInsideTree())
                kv.Value.QueueFree();
        }
        marcadores.Clear();
        ClearDibujos();

        var personas = grafo.ObtenerTodasLasPersonas();
        foreach (var p in personas)
        {
            CrearMarcador(p);
        }
    }

    private void CrearMarcador(Persona persona)
    {
        if (persona == null) return;

        // contiene la foto o emoji
        Control marcador = new Control();
        marcador.Name = persona.Cedula;
        marcador.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        marcador.CustomMinimumSize = new Vector2(MARCADOR_SIZE, MARCADOR_SIZE);
        marcador.Size = new Vector2(MARCADOR_SIZE, MARCADOR_SIZE);

        // centrar el marcador en la coordenada
        var posicion = MapearCoordenadaAPunto(new Vector2((float)persona.Longitud, (float)persona.Latitud));
        marcador.Position = posicion - marcador.Size / 2;

        if (!string.IsNullOrEmpty(persona.RutaFotografia))
        {
            var textureRect = new TextureRect();
            textureRect.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            textureRect.Position = Vector2.Zero;
            textureRect.Size = new Vector2(MARCADOR_SIZE, MARCADOR_SIZE);
            textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
            textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;

            bool fotoOk = false;
            try
            {
                var image = Image.LoadFromFile(persona.RutaFotografia);
                if (image != null)
                {
                    var texture = ImageTexture.CreateFromImage(image);
                    textureRect.Texture = texture;
                    var shader = new Shader();
                    shader.Code = @"
shader_type canvas_item;

void fragment() {
    vec2 uv = UV - vec2(0.5);
    float dist = length(uv);
    
    if (dist > 0.5) {
        COLOR = vec4(0.0);
    } else {
        COLOR = texture(TEXTURE, UV);
    }
}";
                    var material = new ShaderMaterial();
                    material.Shader = shader;
                    textureRect.Material = material;

                    fotoOk = true;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error cargando foto {persona.RutaFotografia}: {ex.Message}");
            }

            if (fotoOk)
            {
                marcador.AddChild(textureRect);
                marcador.TooltipText = $"{persona.NombreCompleto}\n{persona.Cedula}\n(Foto cargada)";
            }
            else
            {
                var label = new Label();
                label.Text = "📷";
                label.SetAnchorsPreset(Control.LayoutPreset.Center);
                marcador.AddChild(label);
                marcador.TooltipText = $"{persona.NombreCompleto}\n{persona.Cedula}\n(Error al cargar foto)";
            }
        }
        else
        {
            // Mostrar emoji según género
            var label = new Label();
            label.Text = persona.GeneroPersona == Persona.Genero.Masculino ? "🔵" : "🔴";
            label.SetAnchorsPreset(Control.LayoutPreset.Center);
            label.AddThemeFontSizeOverride("font_size", 32);
            marcador.AddChild(label);
            marcador.TooltipText = persona.NombreCompleto + "\n" + persona.Cedula;
        }

        // Crear botón invisible para capturar clics
        var btn = new Button();
        btn.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        btn.Position = Vector2.Zero;
        btn.Size = new Vector2(MARCADOR_SIZE, MARCADOR_SIZE);
        btn.Modulate = new Color(1, 1, 1, 0); 
        btn.Pressed += () => OnMarcadorPressed(persona.Cedula);
        marcador.AddChild(btn);

        mapaTexture.AddChild(marcador);
        marcadores[persona.Cedula] = marcador;
    }

    private Vector2 MapearCoordenadaAPunto(Vector2 coord)
    {
        return coord;
    }

    private void OnMarcadorPressed(string cedula)
    {
        if (!marcadores.ContainsKey(cedula)) return;

        ClearDibujos();

        var origen = marcadores[cedula];
        var origenCentro = origen.Position + origen.Size / 2;

        foreach (var kv in marcadores)
        {
            if (kv.Key == cedula) continue;

            var destino = kv.Value;
            var destinoCentro = destino.Position + destino.Size / 2;

            // dibujar linea
            var linea = new Line2D();
            linea.DefaultColor = Colors.Yellow;
            linea.Width = LINE_WIDTH;
            linea.Antialiased = true;
            linea.ZIndex = 5;
            linea.AddPoint(origenCentro);
            linea.AddPoint(destinoCentro);
            dibujoContainer.AddChild(linea);

            // etiqueta de distancia (en píxeles)
            float distancia = origenCentro.DistanceTo(destinoCentro);
            var etiqueta = new Label();
            etiqueta.Text = $"{distancia:F1} px";
            etiqueta.AddThemeFontSizeOverride("font_size", 10);
            etiqueta.AddThemeColorOverride("font_color", Colors.White);
            etiqueta.AddThemeColorOverride("font_outline_color", Colors.Black);
            etiqueta.AddThemeConstantOverride("outline_size", 2);
            etiqueta.Position = (origenCentro + destinoCentro) / 2 - new Vector2(20, 10);
            etiqueta.ZIndex = 10;
            dibujoContainer.AddChild(etiqueta);
        }
    }

    private void ClearDibujos()
    {
        foreach (var child in dibujoContainer.GetChildren())
        {
            if (child is Node n && n.IsInsideTree())
                n.QueueFree();
        }
    }

    // para refrescar desde código
    public void Refresh()
    {
        ReconstruirMarcadores();
    }

    private void OnVolverPressed()
    {
        GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
    }
}
