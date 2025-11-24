using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Arbol_Core.Models;

namespace ArbolGenealogico.scripts.UI
{
    public partial class VisualizadorArbolUI : Control
    {
        // Configuración visual
        private const float ESPACIO_HORIZONTAL = 220f;
        private const float ESPACIO_VERTICAL = 200f;
        private const float RADIO_NODO = 80f; 
        private const float MARGEN_INICIAL = 100f;

        private Arbol_Core.DataStructures.Arbol arbol;
        private Dictionary<string, Vector2> posicionesNodos;
        private ScrollContainer scrollContainer;
        private Control contenedorCanvas;

        public override void _Ready()
        {
            posicionesNodos = new Dictionary<string, Vector2>();
            ConfigurarUI();
        }

        private void ConfigurarUI()
        {
            // Panel de fondo
            var panelFondo = new Panel();
            var styleFondo = new StyleBoxFlat();
            styleFondo.BgColor = new Color(0.95f, 0.95f, 0.98f);
            styleFondo.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            styleFondo.SetBorderWidthAll(2);
            panelFondo.AddThemeStyleboxOverride("panel", styleFondo);
            panelFondo.SetAnchorsPreset(LayoutPreset.FullRect);
            AddChild(panelFondo);

            // Panel superior con título
            var panelSuperior = new Panel();
            var styleSuperior = new StyleBoxFlat();
            styleSuperior.BgColor = new Color(0.2f, 0.3f, 0.5f);
            panelSuperior.AddThemeStyleboxOverride("panel", styleSuperior);
            panelSuperior.SetAnchorsPreset(LayoutPreset.TopWide);
            panelSuperior.CustomMinimumSize = new Vector2(0, 50);
            AddChild(panelSuperior);

            // Título
            var lblTitulo = new Label();
            lblTitulo.Text = "🌳 ÁRBOL GENEALÓGICO";
            lblTitulo.Position = new Vector2(20, 12);
            lblTitulo.AddThemeFontSizeOverride("font_size", 20);
            lblTitulo.AddThemeColorOverride("font_color", Colors.White);
            panelSuperior.AddChild(lblTitulo);

            // Leyenda
            CrearLeyenda(panelSuperior);

            // ScrollContainer para el árbol
            scrollContainer = new ScrollContainer();
            scrollContainer.SetAnchorsPreset(LayoutPreset.FullRect);
            scrollContainer.SetAnchorAndOffset(Side.Top, 0, 55);
            scrollContainer.FollowFocus = true;
            AddChild(scrollContainer);

            // Contenedor canvas para dibujar
            contenedorCanvas = new Control();
            contenedorCanvas.CustomMinimumSize = new Vector2(3000, 2000);
            scrollContainer.AddChild(contenedorCanvas);
        }

        private void CrearLeyenda(Panel panelPadre)
        {
            float xPos = 500;

            // Leyenda Hombre
            var lblHombre = new Label();
            lblHombre.Text = "🔵 Hombre";
            lblHombre.Position = new Vector2(xPos, 20);
            lblHombre.AddThemeFontSizeOverride("font_size", 11);
            lblHombre.AddThemeColorOverride("font_color", Colors.White);
            panelPadre.AddChild(lblHombre);

            // Leyenda Mujer
            var lblMujer = new Label();
            lblMujer.Text = "🔴 Mujer";
            lblMujer.Position = new Vector2(xPos + 100, 20);
            lblMujer.AddThemeFontSizeOverride("font_size", 11);
            lblMujer.AddThemeColorOverride("font_color", Colors.White);
            panelPadre.AddChild(lblMujer);

            // Leyenda Cónyuge
            var lblConyuge = new Label();
            lblConyuge.Text = "❤️ Cónyuge";
            lblConyuge.Position = new Vector2(xPos + 180, 20);
            lblConyuge.AddThemeFontSizeOverride("font_size", 11);
            lblConyuge.AddThemeColorOverride("font_color", Colors.White);
            panelPadre.AddChild(lblConyuge);
        }

        public void ActualizarArbol(Arbol_Core.DataStructures.Arbol arbolActual)
        {
            if (arbolActual == null)
                return;

            arbol = arbolActual;

            if (arbol.CantidadMiembros == 0)
                return;

            // Limpiar contenedor
            foreach (var child in contenedorCanvas.GetChildren())
            {
                if (child is Node childNode)
                    childNode.QueueFree();
            }

            posicionesNodos.Clear();

            // Calcular posiciones
            CalcularPosicionesNodos();

            // Dibujar en orden: conexiones primero, luego nodos
            DibujarConexiones();
            DibujarNodos();

            // Ajustar tamaño del canvas
            AjustarTamañoCanvas();
        }

        private void CalcularPosicionesNodos()
        {
            var todasLasPersonas = arbol.ObtenerTodasLasPersonas();

            // Agrupar por generación
            var porGeneracion = new Dictionary<int, List<Persona>>();
            foreach (var persona in todasLasPersonas)
            {
                if (!porGeneracion.ContainsKey(persona.Generacion))
                {
                    porGeneracion[persona.Generacion] = new List<Persona>();
                }
                porGeneracion[persona.Generacion].Add(persona);
            }

            var generacionesOrdenadas = porGeneracion.Keys.OrderBy(x => x).ToList();

            // Calcular posiciones por generación
            foreach (var gen in generacionesOrdenadas)
            {
                var personasGen = porGeneracion[gen];
                float yPos = MARGEN_INICIAL + (gen * ESPACIO_VERTICAL);

                // Agrupar por familias (hermanos y sus cónyuges)
                var familias = AgruparPorFamilia(personasGen);

                float xOffset = MARGEN_INICIAL;

                foreach (var familia in familias)
                {
                    // Calcular cuánto espacio necesita esta familia
                    int espaciosNecesarios = 0;
                    foreach (var persona in familia)
                    {
                        espaciosNecesarios++;
                        if (persona.Conyuge != null && !posicionesNodos.ContainsKey(persona.Conyuge.Cedula))
                        {
                            espaciosNecesarios++;
                        }
                    }

                    // Posicionar cada miembro de la familia
                    int espacioActual = 0;
                    foreach (var persona in familia)
                    {
                        if (posicionesNodos.ContainsKey(persona.Cedula))
                            continue;

                        float xPos = xOffset + (espacioActual * ESPACIO_HORIZONTAL);
                        posicionesNodos[persona.Cedula] = new Vector2(xPos, yPos);
                        espacioActual++;

                        // Posicionar cónyuge al lado
                        if (persona.Conyuge != null && !posicionesNodos.ContainsKey(persona.Conyuge.Cedula))
                        {
                            float xPosConyuge = xOffset + (espacioActual * ESPACIO_HORIZONTAL);
                            posicionesNodos[persona.Conyuge.Cedula] = new Vector2(xPosConyuge, yPos);
                            espacioActual++;
                        }
                    }

                    xOffset += (espaciosNecesarios * ESPACIO_HORIZONTAL) + 80;
                }
            }
        }

        private List<List<Persona>> AgruparPorFamilia(List<Persona> personas)
        {
            var familias = new List<List<Persona>>();
            var procesadas = new HashSet<string>();

            foreach (var persona in personas)
            {
                if (procesadas.Contains(persona.Cedula))
                    continue;

                var familia = new List<Persona> { persona };
                procesadas.Add(persona.Cedula);

                // Agregar hermanos
                var hermanos = persona.ObtenerHermanos();
                foreach (var hermano in hermanos)
                {
                    if (personas.Contains(hermano) && !procesadas.Contains(hermano.Cedula))
                    {
                        familia.Add(hermano);
                        procesadas.Add(hermano.Cedula);
                    }
                }

                familias.Add(familia);
            }

            return familias;
        }

        private void DibujarConexiones()
        {
            var todasLasPersonas = arbol.ObtenerTodasLasPersonas();
            var conexionesDibujadas = new HashSet<string>();

            foreach (var persona in todasLasPersonas)
            {
                // Conexión con padre
                if (persona.Padre != null && posicionesNodos.ContainsKey(persona.Padre.Cedula))
                {
                    DibujarLineaParental(persona.Cedula, persona.Padre.Cedula, new Color(0.0f, 0.0f, 0.0f), 4);
                }

                // Conexión con madre
                if (persona.Madre != null && posicionesNodos.ContainsKey(persona.Madre.Cedula))
                {
                    DibujarLineaParental(persona.Cedula, persona.Madre.Cedula, new Color(0.0f, 0.0f, 0.0f), 4);
                }

                // Conexión conyugal
                if (persona.Conyuge != null && posicionesNodos.ContainsKey(persona.Conyuge.Cedula))
                {
                    string key = string.Compare(persona.Cedula, persona.Conyuge.Cedula) < 0
                        ? $"{persona.Cedula}-{persona.Conyuge.Cedula}"
                        : $"{persona.Conyuge.Cedula}-{persona.Cedula}";

                    if (!conexionesDibujadas.Contains(key))
                    {
                        DibujarLineaConyugal(persona.Cedula, persona.Conyuge.Cedula);
                        conexionesDibujadas.Add(key);
                    }
                }
            }
        }

        private void DibujarLineaParental(string cedulaHijo, string cedulaPadre, Color color, float grosor)
        {
            if (!posicionesNodos.ContainsKey(cedulaHijo) || !posicionesNodos.ContainsKey(cedulaPadre))
                return;

            var posHijo = posicionesNodos[cedulaHijo] + new Vector2(RADIO_NODO, 0);
            var posPadre = posicionesNodos[cedulaPadre] + new Vector2(RADIO_NODO, RADIO_NODO * 2);

            var line = new Line2D();
            line.DefaultColor = color;
            line.Width = 8;
            line.ZIndex = 10;
            line.Antialiased = true;

            // Línea con curva suave
            float puntoMedioY = (posHijo.Y + posPadre.Y) / 2;

            line.AddPoint(posPadre);
            line.AddPoint(new Vector2(posPadre.X, puntoMedioY));
            line.AddPoint(new Vector2(posHijo.X, puntoMedioY));
            line.AddPoint(posHijo);

            contenedorCanvas.AddChild(line);
        }

        private void DibujarLineaConyugal(string cedula1, string cedula2)
        {
            if (!posicionesNodos.ContainsKey(cedula1) || !posicionesNodos.ContainsKey(cedula2))
                return;

            var pos1 = posicionesNodos[cedula1] + new Vector2(RADIO_NODO * 2, RADIO_NODO);
            var pos2 = posicionesNodos[cedula2] + new Vector2(0, RADIO_NODO);

            var line = new Line2D();
            line.DefaultColor = new Color(1.0f, 0.0f, 0.0f);
            line.Width = 10;
            line.ZIndex = 10;
            line.Antialiased = true;

            line.AddPoint(pos1);
            line.AddPoint(pos2);

            contenedorCanvas.AddChild(line);

            //añadir corazón en el medio
            var puntoMedio = (pos1 + pos2) / 2;
            var lblCorazon = new Label();
            lblCorazon.Text = "❤";
            lblCorazon.Position = puntoMedio - new Vector2(10, 10);
            lblCorazon.AddThemeFontSizeOverride("font_size", 16);
            lblCorazon.ZIndex = 10;
            contenedorCanvas.AddChild(lblCorazon);
        }

        private void DibujarNodos()
        {
            var todasLasPersonas = arbol.ObtenerTodasLasPersonas();

            foreach (var persona in todasLasPersonas)
            {
                if (!posicionesNodos.ContainsKey(persona.Cedula))
                    continue;

                CrearNodoPersona(persona);
            }
        }

        private void CrearNodoPersona(Persona persona)
        {
            var pos = posicionesNodos[persona.Cedula];

            var panel = new Panel();
            panel.Position = pos;
            panel.CustomMinimumSize = new Vector2(RADIO_NODO * 2, RADIO_NODO * 2);
            panel.ZIndex = 5;

            var styleBox = new StyleBoxFlat();

            Color colorBase;
            if (persona.GeneroPersona == Persona.Genero.Masculino)
                colorBase = new Color(0.5f, 0.7f, 1.0f);
            else if (persona.GeneroPersona == Persona.Genero.Femenino)
                colorBase = new Color(1.0f, 0.5f, 0.7f);
            else
                colorBase = new Color(0.7f, 0.7f, 0.7f);

            if (!persona.EstaVivo)
                colorBase = colorBase.Darkened(0.5f);

            styleBox.BgColor = colorBase;
            styleBox.BorderColor = persona.EstaVivo ? Colors.Black : new Color(0.3f, 0.3f, 0.3f);
            styleBox.SetBorderWidthAll(3);

            //radio circular = radio del nodo
            styleBox.CornerRadiusTopLeft = (int)RADIO_NODO;
            styleBox.CornerRadiusTopRight = (int)RADIO_NODO;
            styleBox.CornerRadiusBottomLeft = (int)RADIO_NODO;
            styleBox.CornerRadiusBottomRight = (int)RADIO_NODO;

            styleBox.ShadowColor = new Color(0, 0, 0, 0.3f);
            styleBox.ShadowSize = 4;
            styleBox.ShadowOffset = new Vector2(2, 2);

            panel.AddThemeStyleboxOverride("panel", styleBox);

            // VBox para contenido
            var vbox = new VBoxContainer();
            vbox.SetAnchorsPreset(LayoutPreset.FullRect);
            vbox.SetAnchorAndOffset(Side.Left, 0, 4);
            vbox.SetAnchorAndOffset(Side.Right, 1, -4);
            vbox.SetAnchorAndOffset(Side.Top, 0, 4);
            vbox.SetAnchorAndOffset(Side.Bottom, 1, -4);
            vbox.AddThemeConstantOverride("separation", 0);  
            panel.AddChild(vbox);

            //intentar cargar foto de la persona
            bool fotoMostrada = false;
            if (!string.IsNullOrEmpty(persona.RutaFotografia))
            {
                fotoMostrada = MostrarFotoPersona(vbox, persona);
            }

            //si no se muestra la foto, se una foto por default
            if (!fotoMostrada)
            {
                fotoMostrada = MostrarFotoPersona(vbox, persona);
            }

            //ícono de estado
            var lblEstado = new Label();
            lblEstado.Text = persona.EstaVivo ? "●" : "✝";
            lblEstado.HorizontalAlignment = HorizontalAlignment.Center;
            lblEstado.AddThemeFontSizeOverride("font_size", fotoMostrada ? 10 : 14);
            lblEstado.AddThemeColorOverride("font_color", persona.EstaVivo ? Colors.LimeGreen : Colors.DarkGray);
            vbox.AddChild(lblEstado);

            // === PANEL CONTENEDOR DEL NOMBRE ===
            var panelNombre = new PanelContainer();
            panelNombre.CustomMinimumSize = new Vector2(RADIO_NODO * 2 - 8, 22);

            // Crear un estilo para el fondo azul (#123258)
            var styleBoxNombre = new StyleBoxFlat(); // Nuevo nombre para no chocar con los tuyos
            styleBoxNombre.BgColor = new Color("#334d80");
            styleBoxNombre.CornerRadiusTopLeft = 4;
            styleBoxNombre.CornerRadiusTopRight = 4;
            styleBoxNombre.CornerRadiusBottomLeft = 4;
            styleBoxNombre.CornerRadiusBottomRight = 4;

            panelNombre.AddThemeStyleboxOverride("panel", styleBoxNombre);

            // === LABEL DEL NOMBRE ===
            var lblNombreCompleto = new Label();
            lblNombreCompleto.Text = $"{persona.Nombre} {persona.Apellido}";
            lblNombreCompleto.HorizontalAlignment = HorizontalAlignment.Center;
            lblNombreCompleto.VerticalAlignment = VerticalAlignment.Center;
            lblNombreCompleto.AutowrapMode = TextServer.AutowrapMode.Word;

            lblNombreCompleto.AddThemeColorOverride("font_color", Colors.White);
            lblNombreCompleto.AddThemeFontSizeOverride("font_size", 12);

            // Agregar label dentro del panel azul
            panelNombre.AddChild(lblNombreCompleto);

            // Agregar ese panel al vbox del nodo
            vbox.AddChild(panelNombre);

            // Tu línea original (esta sí se queda)
            contenedorCanvas.AddChild(panel);

            //botón de información
            var btnInfo = new Button();
            btnInfo.Text = "ℹ️";
            btnInfo.Position = pos + new Vector2(RADIO_NODO * 2 - 35, 5);
            btnInfo.CustomMinimumSize = new Vector2(30, 30);
            btnInfo.ZIndex = 6;
            btnInfo.Pressed += () => MostrarDetallesPersona(persona);
            contenedorCanvas.AddChild(btnInfo);
        }

        private bool MostrarFotoPersona(VBoxContainer vbox, Persona persona)
        {
            try
            {
                GD.Print($"📸 Intentando cargar foto: {persona.RutaFotografia}");

                Texture2D texture = null;

                // si hay ruta fotografica, intenta cargar la foto

                if (persona.RutaFotografia != "")
                {
                    // Verificar si existe en res://
                    if (ResourceLoader.Exists(persona.RutaFotografia))
                    {
                        texture = GD.Load<Texture2D>(persona.RutaFotografia);
                        GD.Print($"✓ Foto cargada con GD.Load: {persona.RutaFotografia}");
                    }
                    // Si no existe en res://, intentar cargar desde sistema de archivos
                    else
                    {
                        string rutaAbsoluta = ProjectSettings.GlobalizePath(persona.RutaFotografia);
                        if (System.IO.File.Exists(rutaAbsoluta))
                        {
                            var image = Image.LoadFromFile(rutaAbsoluta);
                            if (image != null)
                            {
                                texture = ImageTexture.CreateFromImage(image);
                                GD.Print($"✓ Foto cargada con Image.LoadFromFile: {rutaAbsoluta}");
                            }
                        }
                    }
                }

                // si no hay ruta fotografica asignada a la persona, se carga imagen por default
                else
                {
                    Image image = null;
                    if (persona.GeneroPersona == Persona.Genero.Masculino)
                    {
                        image = Image.LoadFromFile("res://fotos_default/personaHombre.jpg");
                    }
                    else
                    {
                        image = Image.LoadFromFile("res://fotos_default/personaMujer.jpg");
                    }

                    texture = ImageTexture.CreateFromImage(image);
                }

                if (texture != null)
                {
                    //crear el contenedor horizontal para centrar la foto
                    var hboxFoto = new HBoxContainer();
                    hboxFoto.Alignment = BoxContainer.AlignmentMode.Center;
                    vbox.AddChild(hboxFoto);

                    var textureRect = new TextureRect();
                    textureRect.Texture = texture;
                    textureRect.CustomMinimumSize = new Vector2(120, 120);
                    textureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
                    textureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

                    //hacer la imagen circular usando shader
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

                    hboxFoto.AddChild(textureRect);

                    GD.Print($"✓ Foto mostrada exitosamente para {persona.NombreCompleto}");
                    return true;
                }
                else
                {
                    GD.PrintErr($"⚠ No se pudo cargar textura para: {persona.RutaFotografia}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"❌ Error al cargar foto para {persona.NombreCompleto}: {ex.Message}");
                return false;
            }
        }

        private void MostrarDetallesPersona(Persona persona)
        {
            var dialogo = new AcceptDialog();
            dialogo.Title = $"📋 Información de {persona.NombreCompleto}";

            string detalles = $"╔════════════════════════════════╗\n";
            detalles += $"    DATOS PERSONALES              \n";
            detalles += $"╚════════════════════════════════╝\n\n";
            detalles += $"📌 Cédula: {persona.Cedula}\n";
            detalles += $"🎂 Edad: {persona.Edad} años\n";
            detalles += $"📅 Nacimiento: {persona.FechaNacimiento:dd/MM/yyyy}\n";
            detalles += $"⚥ Género: {persona.GeneroPersona}\n";
            detalles += $"💚 Estado: {(persona.EstaVivo ? "Vivo ●" : "Fallecido ✝")}\n";

            if (!persona.EstaVivo && persona.FechaFallecimiento.HasValue)
            {
                detalles += $"⚰ Fallecimiento: {persona.FechaFallecimiento.Value:dd/MM/yyyy}\n";
            }

            detalles += $"👤 Tipo: {(persona.TipoPersona == "familiar" ? "Familiar" : "Cónyuge")}\n";
            detalles += $"🔢 Generación: {persona.Generacion}\n";

            if (!string.IsNullOrEmpty(persona.RutaFotografia))
            {
                detalles += $"📸 Foto: {System.IO.Path.GetFileName(persona.RutaFotografia)}\n";
            }

            detalles += $"\n╔════════════════════════════════╗\n";
            detalles += $"    UBICACIÓN                     \n";
            detalles += $"╚════════════════════════════════╝\n\n";
            detalles += $"🌍 Latitud: {persona.Latitud:F4}\n";
            detalles += $"🌍 Longitud: {persona.Longitud:F4}\n\n";

            detalles += $"╔════════════════════════════════╗\n";
            detalles += $"    RELACIONES FAMILIARES         \n";
            detalles += $"╚════════════════════════════════╝\n\n";
            detalles += $"👨 Padre: {persona.Padre?.NombreCompleto ?? "N/A"}\n";
            detalles += $"👩 Madre: {persona.Madre?.NombreCompleto ?? "N/A"}\n";
            detalles += $"💑 Cónyuge: {persona.Conyuge?.NombreCompleto ?? "N/A"}\n";
            detalles += $"👶 Hijos: {persona.Hijos.Count}\n";

            if (persona.Hijos.Count > 0)
            {
                detalles += $"\n   Nombres:\n";
                foreach (var hijo in persona.Hijos)
                {
                    detalles += $"   • {hijo.NombreCompleto} ({hijo.Edad} años)\n";
                }
            }

            var hermanos = persona.ObtenerHermanos();
            if (hermanos.Count > 0)
            {
                detalles += $"\n👫 Hermanos: {hermanos.Count}\n";
                foreach (var hermano in hermanos)
                {
                    detalles += $"   • {hermano.NombreCompleto} ({hermano.Edad} años)\n";
                }
            }

            dialogo.DialogText = detalles;
            dialogo.Size = new Vector2I(500, 600);

            AddChild(dialogo);
            dialogo.PopupCentered();
        }

        private void AjustarTamañoCanvas()
        {
            if (posicionesNodos.Count == 0)
                return;

            float maxX = 0;
            float maxY = 0;

            foreach (var pos in posicionesNodos.Values)
            {
                maxX = Math.Max(maxX, pos.X + RADIO_NODO * 2);
                maxY = Math.Max(maxY, pos.Y + RADIO_NODO * 2);
            }

            contenedorCanvas.CustomMinimumSize = new Vector2(
                Math.Max(maxX + 200, 3000),
                Math.Max(maxY + 200, 2000)
            );
        }
    }
}