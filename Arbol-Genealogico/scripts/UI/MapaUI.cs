using Godot;
using System;
using System.Collections.Generic;
using Arbol_Core.DataStructures;
using Arbol_Core.Models;

public partial class MapaUI : Node2D
{
	[Export]
	public NodePath MapaTexturePath = "MapaTextureRect";

	private const int MARCADOR_SIZE = 68; // tamaño en px
	private const int LINE_WIDTH = 3;

	private TextureRect mapaTexture;
	private Grafo grafo;
	private Button volverBtn;

	// panel de datos personales
	private Panel panelInfo;
	private Label labelNombre;
	private Label labelCedula;
	private Label labelEdad;
	private Label labelPadre;
	private Label labelMadre;


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

		// paneles de datos
		panelInfo = GetNodeOrNull<Panel>("PanelInfoPersona");
		if (panelInfo != null)
		{
			labelNombre = panelInfo.GetNodeOrNull<Label>("VBoxContainer/nombre");
			labelCedula = panelInfo.GetNodeOrNull<Label>("VBoxContainer/cedula");
			labelEdad = panelInfo.GetNodeOrNull<Label>("VBoxContainer/edad");
			labelPadre = panelInfo.GetNodeOrNull<Label>("VBoxContainer/padre");
			labelMadre = panelInfo.GetNodeOrNull<Label>("VBoxContainer/madre");
			
			// Ocultar el panel al inicio
			panelInfo.Visible = false;
		}
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
		marcador.Position = posicion - marcador.Size / 2 + new Vector2(2, 0);

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

		// recuadro con el nombre que se muestra debajo de la foto
		var labelNombre = new Label();
		labelNombre.Text = persona.Nombre; 
		labelNombre.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		labelNombre.Position = new Vector2(0, MARCADOR_SIZE); 
		labelNombre.Size = new Vector2(MARCADOR_SIZE, 20);
		labelNombre.HorizontalAlignment = HorizontalAlignment.Center;
		labelNombre.VerticalAlignment = VerticalAlignment.Center;
		labelNombre.AddThemeFontSizeOverride("font_size", 15);
		labelNombre.AddThemeColorOverride("font_color", Colors.White);
		labelNombre.AddThemeColorOverride("font_outline_color", Colors.Black);
		labelNombre.AddThemeConstantOverride("outline_size", 1);
		marcador.AddChild(labelNombre);

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

		// muestra la informacion d ela persona en interfaz
		private void MostrarInfoPersona(string cedula)
		{
			var persona = grafo.ObtenerPersona(cedula);
			if (persona == null || panelInfo == null) return;
			
			// Actualizar labels con la información
			if (labelNombre != null)
				labelNombre.Text = $" Nombre: {persona.Nombre}";
			
			if (labelCedula != null)
				labelCedula.Text = $" Cedula: {persona.Cedula}";
			
			if (labelEdad != null)
				labelEdad.Text = $" Edad: {persona.Edad} años";
			
			if (labelPadre != null)
			{
				if (persona.Padre != null)
					labelPadre.Text = $" Padre: {persona.Padre.Nombre}";
				else
					labelPadre.Text = " Padre: N/A";
			}
			
			if (labelMadre != null)
			{
				if (persona.Madre != null)
					labelMadre.Text = $" Madre: {persona.Madre.Nombre}";
				else
					labelMadre.Text = " Madre: N/A";
			}
			
			// Mostrar el panel
			panelInfo.Visible = true;
		}

	//Convierte una coordenada geográfica a una posición en píxeles
	private Vector2 MapearCoordenadaAPunto(Vector2 coord)
	{
	float longitud = coord.X;
	float latitud = coord.Y;

	float x = 3.596f * longitud + 505.5f;
	float y = -5.044f * latitud + 402.1f;
	
	return new Vector2(x, y);
	}

	private void OnMarcadorPressed(string cedula)
	{
		if (!marcadores.ContainsKey(cedula)) return;

		ClearDibujos();

		MostrarInfoPersona(cedula);

		var personaOrigen = grafo.ObtenerPersona(cedula);
		if (personaOrigen == null) return;

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

			// calcular distancia real en kilometros
			var personaDestino = grafo.ObtenerPersona(kv.Key);
			double distanciaKm = personaOrigen.CalcularDistancia(personaDestino);

			// Crear un contenedor con fondo blanco
			var contenedor = new PanelContainer();
			contenedor.Position = (origenCentro + destinoCentro) / 2 - new Vector2(30, 15);
			contenedor.ZIndex = 10;

			// Crear el StyleBox para el fondo blanco
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = Colors.White;
			styleBox.SetCornerRadiusAll(3);
			styleBox.ContentMarginLeft = 5;
			styleBox.ContentMarginRight = 5;
			styleBox.ContentMarginTop = 2;
			styleBox.ContentMarginBottom = 2;
			contenedor.AddThemeStyleboxOverride("panel", styleBox);

			// Crear la etiqueta con texto negro
			var etiqueta = new Label();
			etiqueta.Text = $"{distanciaKm:F1} km";
			etiqueta.AddThemeFontSizeOverride("font_size", 10);
			etiqueta.AddThemeColorOverride("font_color", Colors.Black);
			etiqueta.HorizontalAlignment = HorizontalAlignment.Center;
			etiqueta.VerticalAlignment = VerticalAlignment.Center;

			contenedor.AddChild(etiqueta);
			dibujoContainer.AddChild(contenedor);
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
