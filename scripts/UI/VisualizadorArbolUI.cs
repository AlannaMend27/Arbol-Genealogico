using Godot;
using System;
using System.Collections.Generic;
using ArbolGenealogico.scripts.Models;
using ArbolGenealogico.scripts.DataStructures;

namespace ArbolGenealogico.scripts.UI
{
	public partial class VisualizadorArbolUI : Node2D
	{
		// Escena del nodo persona (debe crearse previamente)
		[Export] public PackedScene nodoPersonaScene;
		
		// Referencia al árbol genealógico
		private Arbol arbol;
		
		// Configuración de espaciado
		private const float ESPACIO_HORIZONTAL = 200f;
		private const float ESPACIO_VERTICAL = 150f;
		private const float MARGEN_INICIAL_X = 400f; // Más espacio desde la izquierda
		private const float MARGEN_INICIAL_Y = 150f; // Más espacio desde arriba
		
		// Contenedor para los nodos visuales
		private Node2D contenedorNodos;
		
		// Diccionario para mantener referencia a los nodos creados
		private Dictionary<string, NodoPersonaVisual> nodosVisuales;
		
		// Camera2D para navegación
		private Camera2D camera;
		
		public override void _Ready()
		{
			// Crear un CanvasLayer para separar el árbol de la UI
			var canvasLayer = new CanvasLayer();
			canvasLayer.Layer = -1; // Detrás de la UI principal
			AddChild(canvasLayer);
			
			// Inicializar contenedores dentro del CanvasLayer
			contenedorNodos = new Node2D();
			contenedorNodos.Name = "ContenedorNodos";
			canvasLayer.AddChild(contenedorNodos);
			
			nodosVisuales = new Dictionary<string, NodoPersonaVisual>();
			
			// Configurar cámara dentro del CanvasLayer
			camera = new Camera2D();
			camera.Enabled = true;
			canvasLayer.AddChild(camera);
			
			// Obtener el árbol desde AgregarPersona
			arbol = AgregarPersona.ObtenerArbol();
			
			// Conectar al EventBus si existe
			if (GetNodeOrNull("/root/EventBus") != null)
			{
				var eventBus = GetNode<EventBus>("/root/EventBus");
				eventBus.PersonaAgregada += OnPersonaAgregada;
			}
			
			// Generar visualización inicial
			GenerarVisualizacion();
		}
		
		private void OnPersonaAgregada()
		{
			ActualizarVisualizacion();
		}
		
		public override void _Process(double delta)
		{
			// Controles de cámara
			float velocidadCamara = 500f * (float)delta;
			
			if (Input.IsActionPressed("ui_right"))
				camera.Position += new Vector2(velocidadCamara, 0);
			if (Input.IsActionPressed("ui_left"))
				camera.Position += new Vector2(-velocidadCamara, 0);
			if (Input.IsActionPressed("ui_down"))
				camera.Position += new Vector2(0, velocidadCamara);
			if (Input.IsActionPressed("ui_up"))
				camera.Position += new Vector2(0, -velocidadCamara);
		}
		
		public override void _Input(InputEvent @event)
		{
			// Zoom con scroll
			if (@event is InputEventMouseButton mouseEvent)
			{
				if (mouseEvent.ButtonIndex == MouseButton.WheelUp)
				{
					camera.Zoom *= 1.1f;
				}
				else if (mouseEvent.ButtonIndex == MouseButton.WheelDown)
				{
					camera.Zoom *= 0.9f;
				}
			}
		}
		
		// Generar toda la visualización del árbol
		public void GenerarVisualizacion()
		{
			GD.Print("=== INICIANDO GENERACIÓN DE VISUALIZACIÓN ===");
			
			// Limpiar visualización anterior
			LimpiarVisualizacion();
			
			if (arbol == null)
			{
				GD.PrintErr("ERROR: El árbol es NULL");
				return;
			}
			
			GD.Print($"Árbol tiene {arbol.CantidadMiembros} miembros");
			
			if (arbol.CantidadMiembros == 0)
			{
				GD.Print("No hay personas para visualizar - árbol vacío");
				return;
			}
			
			// Obtener fundadores
			var fundadores = arbol.ObtenerPersonasFundadoras();
			
			GD.Print($"Fundadores encontrados: {fundadores.Count}");
			
			if (fundadores.Count == 0)
			{
				GD.PrintErr("No hay fundadores en el árbol");
				return;
			}
			
			// Calcular posiciones para cada generación
			var posicionesPorGeneracion = CalcularPosiciones();
			
			// Crear nodos visuales para todas las personas
			GD.Print("Creando nodos visuales...");
			foreach (var persona in arbol.ObtenerTodasLasPersonas())
			{
				CrearNodoVisual(persona, posicionesPorGeneracion);
				GD.Print($"  - Nodo creado para: {persona.NombreCompleto} en posición {posicionesPorGeneracion[persona.Cedula]}");
			}
			
			// Dibujar conexiones entre padres e hijos
			GD.Print("Dibujando conexiones...");
			DibujarConexiones();
			
			GD.Print($"✓ Visualización generada: {nodosVisuales.Count} nodos");
			GD.Print("=== FIN GENERACIÓN ===");
		}
		
		// Calcular posiciones para todas las personas
		private Dictionary<string, Vector2> CalcularPosiciones()
		{
			var posiciones = new Dictionary<string, Vector2>();
			var todasLasPersonas = arbol.ObtenerTodasLasPersonas();
			
			// Agrupar por generación
			var porGeneracion = new Dictionary<int, List<Persona>>();
			int maxGeneracion = 0;
			
			foreach (var persona in todasLasPersonas)
			{
				if (!porGeneracion.ContainsKey(persona.Generacion))
				{
					porGeneracion[persona.Generacion] = new List<Persona>();
				}
				porGeneracion[persona.Generacion].Add(persona);
				
				if (persona.Generacion > maxGeneracion)
					maxGeneracion = persona.Generacion;
			}
			
			// Calcular posiciones para cada generación
			for (int gen = 0; gen <= maxGeneracion; gen++)
			{
				if (!porGeneracion.ContainsKey(gen))
					continue;
				
				var personasGen = porGeneracion[gen];
				float yPos = MARGEN_INICIAL_Y + (gen * ESPACIO_VERTICAL);
				
				// Calcular ancho total necesario
				float anchoTotal = (personasGen.Count - 1) * ESPACIO_HORIZONTAL;
				float xInicial = MARGEN_INICIAL_X - (anchoTotal / 2f);
				
				for (int i = 0; i < personasGen.Count; i++)
				{
					float xPos = xInicial + (i * ESPACIO_HORIZONTAL);
					posiciones[personasGen[i].Cedula] = new Vector2(xPos, yPos);
				}
			}
			
			return posiciones;
		}
		
		// Crear un nodo visual para una persona
		private void CrearNodoVisual(Persona persona, Dictionary<string, Vector2> posiciones)
		{
			if (nodosVisuales.ContainsKey(persona.Cedula))
			{
				GD.Print($"    Nodo ya existe para {persona.NombreCompleto}");
				return;
			}
			
			GD.Print($"    Creando nodo para {persona.NombreCompleto}...");
			
			// Si no hay escena empaquetada, crear un nodo simple
			NodoPersonaVisual nodoVisual;
			
			if (nodoPersonaScene != null)
			{
				nodoVisual = nodoPersonaScene.Instantiate<NodoPersonaVisual>();
				GD.Print("      Usando escena empaquetada");
			}
			else
			{
				nodoVisual = new NodoPersonaVisual();
				GD.Print("      Usando nodo programático");
			}
			
			// Configurar el nodo visual
			nodoVisual.ConfigurarPersona(persona);
			
			// Posicionar el nodo
			if (posiciones.ContainsKey(persona.Cedula))
			{
				nodoVisual.Position = posiciones[persona.Cedula];
				GD.Print($"      Posición: {nodoVisual.Position}");
			}
			else
			{
				GD.PrintErr($"      ERROR: No hay posición para {persona.Cedula}");
			}
			
			contenedorNodos.AddChild(nodoVisual);
			nodosVisuales[persona.Cedula] = nodoVisual;
			GD.Print($"      ✓ Nodo agregado al contenedor");
		}
		
		// Dibujar las conexiones entre padres e hijos
		private void DibujarConexiones()
		{
			foreach (var persona in arbol.ObtenerTodasLasPersonas())
			{
				// Dibujar líneas desde los hijos hacia los padres
				if (persona.Padre != null)
				{
					DibujarLinea(persona.Cedula, persona.Padre.Cedula);
				}
				
				if (persona.Madre != null)
				{
					DibujarLinea(persona.Cedula, persona.Madre.Cedula);
				}
			}
		}
		
		// Dibujar una línea entre dos personas
		private void DibujarLinea(string cedulaHijo, string cedulaPadre)
		{
			if (!nodosVisuales.ContainsKey(cedulaHijo) || !nodosVisuales.ContainsKey(cedulaPadre))
				return;
			
			var nodoHijo = nodosVisuales[cedulaHijo];
			var nodoPadre = nodosVisuales[cedulaPadre];
			
			var linea = new Line2D();
			linea.AddPoint(nodoHijo.Position);
			linea.AddPoint(nodoPadre.Position);
			linea.DefaultColor = new Color(0.5f, 0.5f, 0.5f);
			linea.Width = 2f;
			linea.ZIndex = -1;
			
			contenedorNodos.AddChild(linea);
		}
		
		// Limpiar toda la visualización
		private void LimpiarVisualizacion()
		{
			foreach (var nodo in contenedorNodos.GetChildren())
			{
				nodo.QueueFree();
			}
			
			nodosVisuales.Clear();
		}
		
		// Actualizar la visualización (llamar después de agregar personas)
		public void ActualizarVisualizacion()
		{
			arbol = AgregarPersona.ObtenerArbol();
			GenerarVisualizacion();
		}
		
		// Método alternativo para actualizar desde fuera
		public void Refrescar()
		{
			ActualizarVisualizacion();
		}
		
		// Activar/desactivar cámara del árbol
		public void ActivarCamara(bool activar)
		{
			if (camera != null)
			{
				camera.Enabled = activar;
			}
		}
	}
	
	// Clase para representar visualmente un nodo de persona
	public partial class NodoPersonaVisual : Node2D
	{
		private Persona persona;
		private ColorRect fondo;
		private Label labelNombre;
		private Label labelInfo;
		
		public NodoPersonaVisual()
		{
			// Crear fondo del nodo
			fondo = new ColorRect();
			fondo.Size = new Vector2(150, 80);
			fondo.Position = new Vector2(-75, -40);
			fondo.Color = new Color(0.2f, 0.3f, 0.5f);
			AddChild(fondo);
			
			// Crear label para el nombre
			labelNombre = new Label();
			labelNombre.Position = new Vector2(-70, -30);
			labelNombre.AddThemeColorOverride("font_color", Colors.White);
			AddChild(labelNombre);
			
			// Crear label para información adicional
			labelInfo = new Label();
			labelInfo.Position = new Vector2(-70, -10);
			labelInfo.AddThemeColorOverride("font_color", Colors.LightGray);
			AddChild(labelInfo);
		}
		
		public void ConfigurarPersona(Persona p)
		{
			persona = p;
			
			// Configurar textos
			labelNombre.Text = persona.NombreCompleto;
			labelInfo.Text = $"{persona.Edad} años\nGen: {persona.Generacion}";
			
			// Cambiar color según género
			switch (persona.GeneroPersona)
			{
				case Persona.Genero.Masculino:
					fondo.Color = new Color(0.3f, 0.5f, 0.8f);
					break;
				case Persona.Genero.Femenino:
					fondo.Color = new Color(0.8f, 0.3f, 0.5f);
					break;
				default:
					fondo.Color = new Color(0.5f, 0.5f, 0.5f);
					break;
			}
			
			// Si está fallecido, oscurecer
			if (!persona.EstaVivo)
			{
				fondo.Color = fondo.Color.Darkened(0.5f);
			}
		}
		
		public override void _Ready()
		{
			// Detectar clics en el nodo
			var area = new Area2D();
			var collision = new CollisionShape2D();
			var shape = new RectangleShape2D();
			shape.Size = new Vector2(150, 80);
			collision.Shape = shape;
			area.AddChild(collision);
			AddChild(area);
			
			area.InputEvent += OnInputEvent;
		}
		
		private void OnInputEvent(Node viewport, InputEvent @event, long shapeIdx)
		{
			if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
			{
				if (mouseEvent.ButtonIndex == MouseButton.Left)
				{
					MostrarInformacion();
				}
			}
		}
		
		private void MostrarInformacion()
		{
			GD.Print($"\n=== {persona.NombreCompleto} ===");
			GD.Print($"Cédula: {persona.Cedula}");
			GD.Print($"Edad: {persona.Edad} años");
			GD.Print($"Género: {persona.GeneroPersona}");
			GD.Print($"Estado: {(persona.EstaVivo ? "Vivo" : "Fallecido")}");
			GD.Print($"Generación: {persona.Generacion}");
			
			if (persona.Padre != null)
				GD.Print($"Padre: {persona.Padre.NombreCompleto}");
			if (persona.Madre != null)
				GD.Print($"Madre: {persona.Madre.NombreCompleto}");
			if (persona.Hijos.Count > 0)
				GD.Print($"Hijos: {persona.Hijos.Count}");
		}
	}
}