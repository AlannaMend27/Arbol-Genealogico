using Godot;
using System;
using System.Collections.Generic;
using Arbol_Core.DataStructures;
using Arbol_Core.Models;

public partial class MapaUI : Node2D
{
	[Export]
	public NodePath MapaTexturePath = "ViewportClip/MapaContainer/MapaTextureRect";

	private const int MARCADOR_SIZE = 68;
	private const int LINE_WIDTH = 3;

	// Configuración de zoom
	[Export] public float ZoomMin = 0.5f;
	[Export] public float ZoomMax = 3.0f;
	[Export] public float ZoomSpeed = 0.15f;
	[Export] public float ZoomSmoothness = 15.0f;
	[Export] public float PanSpeed = 500.0f;
	[Export] public bool InstantZoom = true; // Zoom sin suavizado (solo para debug)

	private TextureRect mapaTexture;
	private Grafo grafo;
	private Button volverBtn;

	// Nodos para zoom
	private Node2D _mapaContainer;
	private Control _viewportClip;
	
	private float _currentZoom = 1.0f;
	private float _targetZoom = 1.0f;
	private Vector2 _panOffset = Vector2.Zero;
	private Vector2 _dragStart;
	private bool _isDragging = false;
	private Vector2 _initialPanOffset = Vector2.Zero; // Guardar el offset inicial

	// Panel de datos personales
	private Panel panelInfo;
	private Label labelNombre;
	private Label labelCedula;
	private Label labelEdad;
	private Label labelPadre;
	private Label labelMadre;

	// Marcadores por cédula
	private Dictionary<string, Control> marcadores = new Dictionary<string, Control>();
	private Node2D dibujoContainer;

	// Área del mapa en la pantalla (área visible donde se puede hacer zoom)
	private Vector2 _mapaContentSize = new Vector2(1152, 684);

	/// <summary>
	/// Inicializa la interfaz del mapa, configura el zoom, carga el grafo y crea los marcadores de las personas
	/// </summary>
	public override void _Ready()
	{
		ConfigurarEstructuraZoom();
		
		mapaTexture = GetNodeOrNull<TextureRect>(MapaTexturePath);
		if (mapaTexture == null)
		{
			// MapaTexture no encontrado
			return;
		}

		// Compensar la posición negativa del MapaTextureRect
		// Y ajustar para centrar mejor el mapa visible
		Vector2 baseOffset = -mapaTexture.Position; // (205, 56)
		
		// Calcular el centro: queremos que el centro del mapa esté en el centro del viewport
		Vector2 mapCenter = mapaTexture.Size / 2; // Centro del mapa
		Vector2 viewportCenter = _viewportClip.Size / 2; // Centro del viewport
		
		// Offset para centrar + ajuste manual hacia la izquierda
		_panOffset = viewportCenter - mapCenter + baseOffset;
		_panOffset.X = 200; // Mover 200px más a la izquierda
		_initialPanOffset = _panOffset;
		
		// Offset inicial calculado y aplicado

		grafo = Grafo.ObtenerInstancia();

		// Crear dibujoContainer dentro de mapaTexture
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

		// panel de informacion de la pesona seleccionada
		panelInfo = GetNodeOrNull<Panel>("PanelInfoPersona");
		if (panelInfo != null)
		{
			labelNombre = panelInfo.GetNodeOrNull<Label>("VBoxContainer/nombre");
			labelCedula = panelInfo.GetNodeOrNull<Label>("VBoxContainer/cedula");
			labelEdad = panelInfo.GetNodeOrNull<Label>("VBoxContainer/edad");
			labelPadre = panelInfo.GetNodeOrNull<Label>("VBoxContainer/padre");
			labelMadre = panelInfo.GetNodeOrNull<Label>("VBoxContainer/madre");
			panelInfo.Visible = false;
		}
	}

	/// <summary>
	/// Configura la estructura de nodos necesaria para el sistema de zoom y clipping del mapa
	/// </summary>
	private void ConfigurarEstructuraZoom()
	{
		_viewportClip = GetNodeOrNull<Control>("ViewportClip");
		if (_viewportClip == null)
		{
			_viewportClip = new Control();
			_viewportClip.Name = "ViewportClip";
			_viewportClip.ClipContents = true;

			// Posición fija donde queremos que esté el área visible del mapa
			_viewportClip.Position = new Vector2(0, 95);
			_viewportClip.Size = new Vector2(1150, 680); // Agrandado para ver más mapa
			AddChild(_viewportClip);
			MoveChild(_viewportClip, 0);
		}

		_mapaContainer = GetNodeOrNull<Node2D>("ViewportClip/MapaContainer");
		if (_mapaContainer == null)
		{
			_mapaContainer = new Node2D();
			_mapaContainer.Name = "MapaContainer";
			_viewportClip.AddChild(_mapaContainer);
		}

		// Mover MapaTextureRect al contenedor si no está ahí
		var mapaTextureRect = GetNodeOrNull<TextureRect>("MapaTextureRect");
		if (mapaTextureRect != null && mapaTextureRect.GetParent() != _mapaContainer)
		{
			mapaTextureRect.GetParent().RemoveChild(mapaTextureRect);
			_mapaContainer.AddChild(mapaTextureRect);
			
				// ViewportClip y MapaTextureRect reubicados
		}
	}

	/// <summary>
	/// Procesa las entradas de zoom y paneo del mapa en cada frame
	/// </summary>
	/// <param name="delta">Tiempo transcurrido desde el último frame</param>
	public override void _Process(double delta)
	{
		HandleZoomInput();
		HandlePanInput(delta);
		SmoothZoom(delta);
		ApplyTransform();
	}

	/// <summary>
	/// Maneja los eventos de entrada del usuario para interacción con el mapa
	/// </summary>
	/// <param name="event">Evento de entrada a procesar</param>
	public override void _Input(InputEvent @event)
	{
		HandleMouseInteraction(@event);
	}

	/// <summary>
	/// Procesa las entradas de teclado para controlar el zoom del mapa
	/// </summary>
	private void HandleZoomInput()
	{
		if (Input.IsActionJustPressed("ui_page_up"))
			ZoomIn();
		else if (Input.IsActionJustPressed("ui_page_down"))
			ZoomOut();
	}

	/// <summary>
	/// Maneja las interacciones del mouse para zoom con rueda y arrastre del mapa
	/// </summary>
	/// <param name="event">Evento de entrada del mouse</param>
	private void HandleMouseInteraction(InputEvent @event)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			Vector2 mousePos = mouseButton.Position;
			
			// Verificar si el mouse está sobre el ViewportClip usando su área global
			Rect2 viewportGlobalArea = new Rect2(
				_viewportClip.GlobalPosition,
				_viewportClip.Size
			);
			
			if (!viewportGlobalArea.HasPoint(mousePos))
				return;

			if (mouseButton.ButtonIndex == MouseButton.WheelUp && mouseButton.Pressed)
			{
				ZoomTowardsMouse(mousePos, true);
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelDown && mouseButton.Pressed)
			{
				ZoomTowardsMouse(mousePos, false);
			}
			else if (mouseButton.ButtonIndex == MouseButton.Middle || 
					 mouseButton.ButtonIndex == MouseButton.Right)
			{
				if (mouseButton.Pressed)
				{
					_isDragging = true;
					_dragStart = mousePos;
				}
				else
				{
					_isDragging = false;
				}
			}
		}

		if (@event is InputEventMouseMotion mouseMotion && _isDragging)
		{
			Vector2 mousePos = mouseMotion.Position;
			Vector2 dragDelta = mousePos - _dragStart;
			_panOffset += dragDelta;
			_dragStart = mousePos;
			ClampPan();
		}
	}

	/// <summary>
	/// Procesa las entradas de teclado para mover el mapa en las cuatro direcciones
	/// </summary>
	/// <param name="delta">Tiempo transcurrido desde el último frame</param>
	private void HandlePanInput(double delta)
	{
		Vector2 panDirection = Vector2.Zero;

		if (Input.IsActionPressed("ui_left"))
			panDirection.X += 1;
		if (Input.IsActionPressed("ui_right"))
			panDirection.X -= 1;
		if (Input.IsActionPressed("ui_up"))
			panDirection.Y += 1;
		if (Input.IsActionPressed("ui_down"))
			panDirection.Y -= 1;

		if (panDirection != Vector2.Zero)
		{
			panDirection = panDirection.Normalized();
			_panOffset += panDirection * PanSpeed * (float)delta;
			ClampPan();
		}
	}

	/// <summary>
	/// Realiza zoom hacia la posición del mouse manteniendo el punto bajo el cursor fijo
	/// </summary>
	/// <param name="mouseScreenPos">Posición del mouse en la pantalla</param>
	/// <param name="zoomIn">True para acercar, false para alejar</param>
	private void ZoomTowardsMouse(Vector2 mouseScreenPos, bool zoomIn)
	{
		// Posición del mouse relativa al ViewportClip
		Vector2 mouseRelativeToClip = mouseScreenPos - _viewportClip.GlobalPosition;
		
		// Guardar el zoom anterior ANTES de cambiarlo
		float oldTargetZoom = _targetZoom;
		
		// Cambiar el zoom objetivo
		if (zoomIn)
			ZoomIn();
		else
			ZoomOut();
		
		// Punto del mapa bajo el mouse en el espacio del mapa
		// Convertir la posición del mouse a coordenadas del mapa
		Vector2 pointInMap = (mouseRelativeToClip - _panOffset) / oldTargetZoom;
		
		// Calcular nuevo offset para mantener ese punto bajo el mouse
		_panOffset = mouseRelativeToClip - pointInMap * _targetZoom;
		
		// Si queremos zoom instantáneo (para debug)
		if (InstantZoom)
		{
			_currentZoom = _targetZoom;
		}
		
		ClampPan();
	}

	/// <summary>
	/// Aumenta el nivel de zoom del mapa dentro de los límites establecidos
	/// </summary>
	private void ZoomIn()
	{
		_targetZoom += ZoomSpeed;
		_targetZoom = Mathf.Clamp(_targetZoom, ZoomMin, ZoomMax);
	}

	/// <summary>
	/// Disminuye el nivel de zoom del mapa dentro de los límites establecidos
	/// </summary>
	private void ZoomOut()
	{
		_targetZoom -= ZoomSpeed;
		_targetZoom = Mathf.Clamp(_targetZoom, ZoomMin, ZoomMax);
	}

	/// <summary>
	/// Aplica interpolación suave al zoom actual hacia el zoom objetivo
	/// </summary>
	/// <param name="delta">Tiempo transcurrido desde el último frame</param>
	private void SmoothZoom(double delta)
	{
		_currentZoom = Mathf.Lerp(_currentZoom, _targetZoom, ZoomSmoothness * (float)delta);
	}

	/// <summary>
	/// Aplica la transformación de escala y posición al contenedor del mapa
	/// </summary>
	private void ApplyTransform()
	{
		if (_mapaContainer == null) return;

		_mapaContainer.Scale = new Vector2(_currentZoom, _currentZoom);
		_mapaContainer.Position = _panOffset;
	}

	/// <summary>
	/// Limita el desplazamiento del mapa para evitar que se salga del área visible
	/// </summary>
	private void ClampPan()
	{
		var mapSize = _mapaContentSize * _targetZoom; // Volver a usar targetZoom
		var viewportSize = _viewportClip.Size;

		// Permitir un margen más generoso - solo evitar que se vaya completamente fuera
		float margin = 800f; // Margen aumentado a 200px para más libertad
		
		// Calcular límites permitiendo ver toda el área del mapa
		float minX = viewportSize.X - mapSize.X - margin;
		float maxX = margin;
		float minY = viewportSize.Y - mapSize.Y - margin;
		float maxY = margin;

		// Si el mapa es más pequeño que el viewport (zoom out), centrarlo
		if (mapSize.X < viewportSize.X)
		{
			minX = maxX = (viewportSize.X - mapSize.X) / 2;
		}
		if (mapSize.Y < viewportSize.Y)
		{
			minY = maxY = (viewportSize.Y - mapSize.Y) / 2;
		}

		_panOffset.X = Mathf.Clamp(_panOffset.X, minX, maxX);
		_panOffset.Y = Mathf.Clamp(_panOffset.Y, minY, maxY);
	}

	/// <summary>
	/// Restablece el zoom a su valor inicial y centra el mapa en su posición original
	/// </summary>
	public void ResetZoom()
	{
		_targetZoom = 1.0f;
		_currentZoom = 1.0f;
		_panOffset = _initialPanOffset; // Volver al offset inicial en lugar de (0,0)
	}

	/// <summary>
	/// Actualiza las etiquetas de la interfaz con las estadísticas del grafo (distancia promedio, par más cercano y más lejano)
	/// </summary>
	private void ActualizarLabelsInfo()
	{
		var (distancia, lejos, cerca) = grafo.ObtenerValoresUI();

		var labelDistancia = GetNodeOrNull<Label>("promedio");
		if (labelDistancia != null)
			labelDistancia.Text = $"{distancia}";

		var labelLejos = GetNodeOrNull<Label>("lejos");
		if (labelLejos != null)
			labelLejos.Text = lejos != string.Empty ? $"{lejos}" : "N/A";

		var labelCerca = GetNodeOrNull<Label>("cerca");
		if (labelCerca != null)
			labelCerca.Text = cerca != string.Empty ? $"{cerca}" : "N/A";
	}

	/// <summary>
	/// Elimina todos los marcadores existentes y los vuelve a crear con las personas actuales del grafo
	/// </summary>
	public void ReconstruirMarcadores()
	{
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

	/// <summary>
	/// Crea un marcador visual en el mapa para una persona específica con su foto y nombre
	/// </summary>
	/// <param name="persona">La persona para la cual crear el marcador</param>
	private void CrearMarcador(Persona persona)
	{
		if (persona == null) return;

		Control marcador = new Control();
		marcador.Name = persona.Cedula;
		marcador.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		marcador.CustomMinimumSize = new Vector2(MARCADOR_SIZE, MARCADOR_SIZE);
		marcador.Size = new Vector2(MARCADOR_SIZE, MARCADOR_SIZE);

		var posicion = MapearCoordenadaAPunto(new Vector2((float)persona.Longitud, (float)persona.Latitud));
		marcador.Position = posicion - marcador.Size / 2 + new Vector2(2, 0);

		// Determinar la ruta de la foto a cargar
		string rutaFoto = persona.RutaFotografia;
		if (string.IsNullOrEmpty(rutaFoto))
		{
			// Usar foto por defecto según el género
			if (persona.GeneroPersona == Persona.Genero.Masculino)
			{
				rutaFoto = "res://fotos_default/personaHombre.jpg";
			}
			else
			{
				rutaFoto = "res://fotos_default/personaMujer.jpg";
			}
		}

		// Intentar cargar la foto
		bool fotoOk = CargarFotoEnMarcador(marcador, rutaFoto);
		
		// Configurar tooltip según el resultado
		if (fotoOk)
		{
			marcador.TooltipText = $"{persona.NombreCompleto}\n{persona.Cedula}";
		}
		else
		{
			var label = new Label();
			label.Text = "📷";
			label.SetAnchorsPreset(Control.LayoutPreset.Center);
			marcador.AddChild(label);
			marcador.TooltipText = $"{persona.NombreCompleto}\n{persona.Cedula}\n(Error al cargar foto)";
		}

		// Label del nombre de la persona
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

		// Botón invisible para detectar clics
		var btn = new Button();
		btn.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
		btn.Position = Vector2.Zero;
		btn.Size = new Vector2(MARCADOR_SIZE, MARCADOR_SIZE);
		btn.Modulate = new Color(1, 1, 1, 0);
		btn.Pressed += () => OnMarcadorPressed(persona.Cedula);
		marcador.AddChild(btn);

		// Agregar al mapa
		mapaTexture.AddChild(marcador);
		marcadores[persona.Cedula] = marcador;
	}

	/// <summary>
	/// Carga una imagen en un marcador aplicando un shader circular
	/// </summary>
	/// <param name="marcador">El control del marcador donde se agregará la imagen</param>
	/// <param name="rutaFoto">Ruta del archivo de imagen a cargar</param>
	/// <returns>True si la foto se cargó exitosamente, false en caso contrario</returns>
	private bool CargarFotoEnMarcador(Control marcador, string rutaFoto)
	{
		try
		{
			var image = Image.LoadFromFile(rutaFoto);
			if (image == null) return false;

			var textureRect = new TextureRect();
			textureRect.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
			textureRect.Position = Vector2.Zero;
			textureRect.Size = new Vector2(MARCADOR_SIZE, MARCADOR_SIZE);
			textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered;
			textureRect.Texture = ImageTexture.CreateFromImage(image);

			// Aplicar shader circular
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

			marcador.AddChild(textureRect);
			return true;
		}
		catch (Exception ex)
		{
			// Error al cargar foto (silenciado)
			return false;
		}
	}

	/// <summary>
	/// Muestra la información detallada de una persona en el panel de información de la interfaz
	/// </summary>
	/// <param name="cedula">Cédula de la persona cuya información se mostrará</param>
	private void MostrarInfoPersona(string cedula)
	{
		var persona = grafo.ObtenerPersona(cedula);
		if (persona == null || panelInfo == null) return;
		
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
		
		panelInfo.Visible = true;
	}

	/// <summary>
	/// Convierte coordenadas geográficas (longitud, latitud) a coordenadas de píxeles en el mapa
	/// </summary>
	/// <param name="coord">Vector con longitud (X) y latitud (Y)</param>
	/// <returns>Posición en píxeles en el mapa</returns>
	private Vector2 MapearCoordenadaAPunto(Vector2 coord)
	{
		float longitud = coord.X;
		float latitud = coord.Y;

		float x = 3.596f * longitud + 505.5f;
		float y = -5.044f * latitud + 402.1f;
		
		return new Vector2(x, y);
	}

	/// <summary>
	/// Maneja el evento de clic en un marcador, mostrando líneas y distancias hacia todas las demás personas
	/// </summary>
	/// <param name="cedula">Cédula de la persona cuyo marcador fue clickeado</param>
	private void OnMarcadorPressed(string cedula)
	{
		if (!marcadores.ContainsKey(cedula)) return;

		// mostrar la informacion de la persona
		ClearDibujos();
		MostrarInfoPersona(cedula);

		var personaOrigen = grafo.ObtenerPersona(cedula);
		if (personaOrigen == null) return;

		var origen = marcadores[cedula];
		var origenCentro = origen.Position + origen.Size / 2;

		foreach (var kv in marcadores)
		{
			if (kv.Key == cedula) continue;

			// cacular centro del marcador destino
			var destino = kv.Value;
			var destinoCentro = destino.Position + destino.Size / 2;

			// crear y dibujar linea amarilla
			var linea = new Line2D();
			linea.DefaultColor = Colors.Yellow;
			linea.Width = LINE_WIDTH;
			linea.Antialiased = true;
			linea.ZIndex = 5;
			linea.AddPoint(origenCentro);
			linea.AddPoint(destinoCentro);
			dibujoContainer.AddChild(linea);

			// calcular distancia
			var personaDestino = grafo.ObtenerPersona(kv.Key);
			double distanciaKm = personaOrigen.CalcularDistancia(personaDestino);

			// creacion de contenedor para etiqueta
			var contenedor = new PanelContainer();
			contenedor.Position = (origenCentro + destinoCentro) / 2 - new Vector2(30, 15);
			contenedor.ZIndex = 10;

			// darle formato al contenedor
			var styleBox = new StyleBoxFlat();
			styleBox.BgColor = Colors.White;
			styleBox.SetCornerRadiusAll(3);
			styleBox.ContentMarginLeft = 5;
			styleBox.ContentMarginRight = 5;
			styleBox.ContentMarginTop = 2;
			styleBox.ContentMarginBottom = 2;
			contenedor.AddThemeStyleboxOverride("panel", styleBox);

			// muestra la distancia a la que se encuentra la personaen el contendeor
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

	/// <summary>
	/// Elimina todas las líneas y etiquetas de distancia dibujadas en el mapa
	/// </summary>
	private void ClearDibujos()
	{
		foreach (var child in dibujoContainer.GetChildren())
		{
			if (child is Node n && n.IsInsideTree())
				n.QueueFree();
		}
	}

	/// <summary>
	/// Actualiza la visualización del mapa reconstruyendo todos los marcadores
	/// </summary>
	public void Refresh()
	{
		ReconstruirMarcadores();
	}

	/// <summary>
	/// Maneja el evento del botón volver, regresando a la escena del menú principal
	/// </summary>
	private void OnVolverPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}
}