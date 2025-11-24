using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Arbol_Core.Models;
using Arbol_Core.DataStructures;
using ArbolGenealogico.scripts.UI;

public partial class AgregarPersona : Node2D
{
	private VisualizadorArbolUI visualizadorUI;
	private static List<string> cedulasExistentes = new List<string>();
	private static List<Persona> personasCreadas = new List<Persona>();

	private static List<Persona> hombres = new List<Persona>();
	private static List<Persona> mujeres = new List<Persona>();


	// Visualiza la construcción del árbol en consola 
	private static Arbol arbol = new Arbol();

	private LineEdit nombreInput;
	private LineEdit cedulaInput;
	private LineEdit coordXInput;
	private LineEdit coordYInput;
	private LineEdit fechaInput;
	private LineEdit edadInput;
	private CheckBox vivoCheck;
	private CheckBox muertoCheck;
	private Label label11;
	private LineEdit fechaFallecimientoInput;
	private OptionButton opcionesPadre;
	private OptionButton opcionesMadre;
	private OptionButton opcionesGenero;
	private OptionButton tipoDePersona;
	private Label label12;
	private Label label13;
	private Label label16;
	private OptionButton conyugue;
	private Button aceptarBtn;
	private Button cancelarBtn;
	private Button cargarFotoBtn;
	private LineEdit rutaFotoInput;
	private FileDialog dialogoSeleccionarFoto;
	private string rutaFotoSeleccionada = "";
	private TextureRect previsualizacionFoto;

	private AcceptDialog dialogoError;
	private Button volverBtn;

	public override void _Ready()
	{
		nombreInput = GetNode<LineEdit>("nombre");
		cedulaInput = GetNode<LineEdit>("cedula");
		coordXInput = GetNode<LineEdit>("cor-x");
		coordYInput = GetNode<LineEdit>("cor-y");
		fechaInput = GetNode<LineEdit>("nacimiento");
		edadInput = GetNode<LineEdit>("Edad");
		vivoCheck = GetNode<CheckBox>("vivo");
		muertoCheck = GetNode<CheckBox>("muerto");
		label11 = GetNode<Label>("Label11");
		fechaFallecimientoInput = GetNode<LineEdit>("fechaFallecimiento");
		opcionesPadre = GetNode<OptionButton>("padre");
		opcionesMadre = GetNode<OptionButton>("madre");
		opcionesGenero = GetNode<OptionButton>("genero");
		tipoDePersona = GetNode<OptionButton>("tipodepersona");
		label12 = GetNode<Label>("Label12");
		label13 = GetNode<Label>("Label13");
		label16 = GetNode<Label>("Label16");
		conyugue = GetNode<OptionButton>("conyugue");

		aceptarBtn = GetNode<Button>("aceptar");
		cancelarBtn = GetNode<Button>("cancelar");
		volverBtn = GetNodeOrNull<Button>("volver");

		// Intentar obtener los nodos de foto (si existen en la escena)
		cargarFotoBtn = GetNodeOrNull<Button>("cargar_foto");
		rutaFotoInput = GetNodeOrNull<LineEdit>("rutaFoto");
		previsualizacionFoto = GetNodeOrNull<TextureRect>("previsualizacion_foto");

		dialogoError = new AcceptDialog();
		dialogoError.Title = "Error";
		dialogoError.OkButtonText = "Entendido";
		AddChild(dialogoError);

		//crear el FileDialog
		dialogoSeleccionarFoto = new FileDialog();
		dialogoSeleccionarFoto.FileMode = FileDialog.FileModeEnum.OpenFile;
		dialogoSeleccionarFoto.Filters = new string[] { "*.png ; Imágenes PNG", "*.jpg , *.jpeg ; Imágenes JPG" };
		dialogoSeleccionarFoto.Title = "Seleccionar foto de la persona";
		dialogoSeleccionarFoto.Access = FileDialog.AccessEnum.Filesystem;
		dialogoSeleccionarFoto.UseNativeDialog = true;

		AddChild(dialogoSeleccionarFoto);

		//conectar botones a sus funciones
		aceptarBtn.Pressed += OnAceptarPressed;
		cancelarBtn.Pressed += OnCancelarPressed;

		if (cargarFotoBtn != null)
		{
			cargarFotoBtn.Pressed += OnCargarFotoPressed;
		}

		if (volverBtn != null)
		{
			volverBtn.Pressed += OnVolverPressed;
		}

		dialogoSeleccionarFoto.FileSelected += OnFotoSeleccionada;

		//conectar checkboxes
		vivoCheck.Pressed += OnVivoPressed;
		muertoCheck.Pressed += OnMuertoPressed;

		//cambios de tipo de persona y género
		tipoDePersona.ItemSelected += OnTipoPersonaChanged;
		opcionesGenero.ItemSelected += OnGeneroChanged;

		vivoCheck.ButtonPressed = true;
		label11.Visible = false;
		fechaFallecimientoInput.Visible = false;

		opcionesGenero.AddItem("No especificado");
		opcionesGenero.AddItem("Masculino");
		opcionesGenero.AddItem("Femenino");
		opcionesGenero.Selected = 0;

		tipoDePersona.AddItem("Familiar");
		tipoDePersona.AddItem("Cónyuge");
		tipoDePersona.Selected = 0;

		ActualizarTodasLasListas();
		ConfigurarVisibilidadCampos();
		CallDeferred(nameof(InicializarVisualizador));

		// Actualiza el árbol cada vez que se entra a la escena
		CallDeferred(nameof(ActualizarVisualizacionArbol));

		//se crea la carpeta para fotos si es que no existe
		CrearCarpetaFotos();

		// se configura el tamaño de los dropdowns
		ConfigurarTamañoDropdowns();
	}

	// Crea la carpeta de fotos si no existe
	private void CrearCarpetaFotos()
	{
		string carpetaFotos = "res://fotos_personas";

		if (!DirAccess.DirExistsAbsolute(carpetaFotos))
		{
			var dir = DirAccess.Open("res://");
			if (dir != null)
			{
				var error = dir.MakeDir("fotos_personas");
				if (error == Error.Ok)
				{
					GD.Print("✓ Carpeta 'fotos_personas' creada exitosamente");
				}
				else
				{
					GD.PrintErr($"Error al crear carpeta: {error}");
				}
			}
		}
	}

	// Actualiza la visualización del árbol
	private void ActualizarVisualizacionArbol()
	{
		if (visualizadorUI != null)
		{
			visualizadorUI.ActualizarArbol(arbol);
		}
	}

	// Inicializa el visualizador del árbol
	private void InicializarVisualizador()
	{
		GD.Print("\n=== Buscando VisualizadorArbolUI ===");

		// Intentar diferentes rutas
		visualizadorUI = GetNodeOrNull<VisualizadorArbolUI>("../VisualizadorArbolUI");

		if (visualizadorUI == null)
		{
			visualizadorUI = GetNodeOrNull<VisualizadorArbolUI>("/root/Tree/VisualizadorArbolUI");
		}

		if (visualizadorUI == null)
		{
			// Buscar en toda la escena
			var root = GetTree().Root;
			visualizadorUI = BuscarVisualizadorRecursivo(root);
		}

		if (visualizadorUI == null)
		{
			GD.PrintErr("⚠ ERROR: No se encontró VisualizadorArbolUI en la escena");
			GD.PrintErr("⚠ Asegúrate de que el nodo existe y tiene el script adjunto");
		}
		else
		{
			GD.Print("✓ VisualizadorArbolUI conectado correctamente");
			GD.Print($"✓ Ruta del nodo: {visualizadorUI.GetPath()}");
		}
	}

	// Busca el visualizador recursivamente en los nodos
	private VisualizadorArbolUI BuscarVisualizadorRecursivo(Node nodo)
	{
		if (nodo is VisualizadorArbolUI visualizador)
		{
			return visualizador;
		}

		foreach (Node hijo in nodo.GetChildren())
		{
			var resultado = BuscarVisualizadorRecursivo(hijo);
			if (resultado != null)
				return resultado;
		}

		return null;
	}

	// Maneja el evento de aceptar y valida los datos
	private void OnAceptarPressed()
	{
		try
		{
			// VALIDACIONES DE CÉDULA
			if (!ValidarCedulaRequerida()) return;
			if (!ValidarLongitudCedula()) return;
			if (!ValidarCedulaUnica()) return;

			// VALIDACIONES DE FAMILIARES Y TIPO DE PERSONA
			if (!ValidarConyugeSeleccionado()) return;
			if (!ValidarFundadorUnico()) return;
			if (!ValidarPadresSeleccionados()) return;

			// VALIDACIONES DE NOMBRE
			if (!ValidarNombreRequerido()) return;
			if (!ValidarNombreSinNumeros()) return;
			if (!ValidarApellidoPresente(out string nombre, out string apellido)) return;

			// VALIDACIONES DE FORMATO DE FECHA
			if (!ValidarFormatoFechaNacimiento(out DateTime fechaNac)) return;
			if (!ValidarFechaNoFutura(fechaNac)) return;

			// VALIDACIONES DE GÉNERO
			if (!ValidarGeneroSeleccionado()) return;

			// VALIDACIONES DE EDAD
			if (!ValidarEdadNumerica(out int edad)) return;
			if (!ValidarRangoEdad(edad)) return;
			if (!ValidarEdadCoherenteConPadres(edad)) return;
			if (!ValidarEdadCoincideConFechaNacimiento(edad, fechaNac)) return;

			// VALIDACIÓN DE PADRES CONYUGUES
			if (!VerificarPadresConyugues()) return;

			// VALIDACIÓN DE FECHA DE FALLECIMIENTO
			if (!ValidarFormatoFechaFallecimiento(fechaNac, out DateTime? fechaFallecimiento)) return;

			// VALIDACIONES DE COORDENADAS
			if (!ValidarCoordenadas(out double latitud, out double longitud)) return;

			// VALIDACIONES DE FOTO
			if (!ValidarYCopiarFoto(out string rutaFotoFinal)) return;

			// CREAR LA PERSONA
			Persona nuevaPersona = CrearNuevaPersona(nombre, apellido, fechaNac, edad, latitud, longitud, rutaFotoFinal);

			// CONFIGURAR ESTADO Y FECHA DE FALLECIMIENTO
			ConfigurarEstadoPersona(nuevaPersona, fechaFallecimiento);

			// CONFIGURAR GÉNERO
			ConfigurarGenero(nuevaPersona);

			// VALIDAR PERSONA
			if (!ValidarPersonaCompleta(nuevaPersona)) return;

			// REGISTRAR Y AGREGAR PERSONA
			RegistrarPersona(nuevaPersona);

			// ESTABLECER RELACIONES
			EstablecerRelacionesFamiliares(nuevaPersona);

			// AGREGAR AL ÁRBOL Y GRAFO
			AgregarAlArbolYGrafo(nuevaPersona);

			// ACTUALIZAR UI
			ActualizarInterfaz(nuevaPersona);

			// LIMPIAR CAMPOS
			LimpiarCampos();
		}
		catch (Exception ex)
		{
			MostrarError($"Error inesperado:\n{ex.Message}");
		}
	}


	// Copia la foto al proyecto y retorna su ruta
	private string CopiarFotoAlProyecto(string rutaOrigen, string cedula)
	{
		try
		{
			// Verificar que el archivo existe
			if (!System.IO.File.Exists(rutaOrigen))
			{
				GD.PrintErr($"El archivo no existe: {rutaOrigen}");
				return "";
			}

			// Obtener extensión del archivo
			string extension = System.IO.Path.GetExtension(rutaOrigen).ToLower();

			// Crear nombre único para la foto usando la cédula
			string nombreArchivo = $"foto_{cedula}{extension}";

			// Ruta dentro del proyecto
			string carpetaDestino = ProjectSettings.GlobalizePath("res://fotos_personas");
			string rutaDestino = System.IO.Path.Combine(carpetaDestino, nombreArchivo);

			// Crear carpeta si no existe
			if (!System.IO.Directory.Exists(carpetaDestino))
			{
				System.IO.Directory.CreateDirectory(carpetaDestino);
			}

			// Copiar archivo
			System.IO.File.Copy(rutaOrigen, rutaDestino, true);

			// Retornar ruta relativa para Godot
			string rutaGodot = $"res://fotos_personas/{nombreArchivo}";

			GD.Print($"✓ Foto copiada exitosamente a: {rutaGodot}");

			return rutaGodot;
		}
		catch (Exception ex)
		{
			GD.PrintErr($"Error al copiar foto: {ex.Message}");
			return "";
		}
	}

	/// <summary>
	/// VALIDACIONES DEL FORMULARIO
	/// Validaciones de cada uno de los datos ingresados por medio del formulario
	/// </summary>


	// ==================== VALIDACIONES DE CÉDULA ====================
	
	// Valida que la cédula es requerida
	private bool ValidarCedulaRequerida()
	{
		if (string.IsNullOrWhiteSpace(cedulaInput.Text))
		{
			MostrarError("La cédula es requerida");
			return false;
		}
		return true;
	}

	// Valida la longitud de la cédula
	private bool ValidarLongitudCedula()
	{
		if (cedulaInput.Text.Length < 9 || cedulaInput.Text.Length > 12)
		{
			MostrarError("La cédula no tiene la extensión adecuada");
			return false;
		}
		return true;
	}

	// Valida que la cédula sea única
	private bool ValidarCedulaUnica()
	{
		if (cedulasExistentes.Contains(cedulaInput.Text))
		{
			MostrarError("Esta cédula ya está registrada");
			return false;
		}
		return true;
	}

	// ==================== VALIDACIONES DE FAMILIARES Y TIPO DE PERSONA ====================
	
	// Valida que el cónyuge esté seleccionado
	private bool ValidarConyugeSeleccionado()
	{
		if (tipoDePersona.Selected == 1) // 1 = Cónyuge
		{
			if (conyugue.Selected == 0) // Si no hay conyuge seleccionado
			{
				MostrarError("Debe seleccionar un cónyuge de la lista para agregar a esta persona.\n\n" +
							"Si no aparece ningún cónyuge disponible, primero debe agregar \n" +
							"al familiar con el cual desea establecer la relación conyugal.");
				return false;
			}
		}
		return true;
	}

	// Valida que solo haya un fundador en el árbol
	private bool ValidarFundadorUnico()
	{
		var fundadoresExistentes = arbol.ObtenerPersonasFundadoras();
		
		// Si ya existen fundadores
		if (fundadoresExistentes != null && fundadoresExistentes.Count > 0)
		{
			Persona fundadorExistente = fundadoresExistentes[0];
			
			// Si se intenta agregar como Familiar SIN padres, sería otro fundador
			if (tipoDePersona.Selected == 0) // 0 = Familiar
			{
				Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
				Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);
				
				// Si no tiene padres, sería un fundador adicional (error)
				if (padre == null && madre == null)
				{
					MostrarError("Ya existe un fundador en el árbol. No se puede agregar otro familiar sin padres.");
					return false;
				}
			}
			// Si es Cónyuge, debe ser del fundador
			else if (tipoDePersona.Selected == 1) // 1 = Cónyuge
			{
				// Extraer cédula del texto seleccionado en el OptionButton
				string textoSeleccionado = conyugue.GetItemText(conyugue.Selected);
				int inicioParentesis = textoSeleccionado.LastIndexOf('(');
				int finParentesis = textoSeleccionado.LastIndexOf(')');

				if (inicioParentesis > 0 && finParentesis > inicioParentesis)
				{
					string cedulaConyugue = textoSeleccionado.Substring(
						inicioParentesis + 1,
						finParentesis - inicioParentesis - 1
					).Trim();

					// Buscar en personasCreadas
					Persona conyugueSeleccionado = personasCreadas.Find(p => p.Cedula == cedulaConyugue);

					if (conyugueSeleccionado == null || conyugueSeleccionado.Cedula != fundadorExistente.Cedula)
					{
						MostrarError("Ya existe un fundador. Solo se permite agregar su cónyuge.");
						return false;
					}
				}
			}
		}
		
		return true;
	}

	// Valida que los padres estén seleccionados
	private bool ValidarPadresSeleccionados()
	{
		if (tipoDePersona.Selected == 0) 
		{
			Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
			Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);
			
			var fundadoresExistentes = arbol.ObtenerPersonasFundadoras();
			
			// Si ya hay fundadores en el árbol, esta persona DEBE tener padres
			if (fundadoresExistentes != null && fundadoresExistentes.Count > 0)
			{
				if (padre == null || madre == null)
				{
					MostrarError("Debe seleccionar tanto al padre como a la madre.\n\n" +
								"Solo la primera persona del árbol puede no tener padres.");
					return false;
				}
			}
		}
		
		return true;
	}

	// ==================== VALIDACIONES DE NOMBRE ====================
	
	// Valida que el nombre sea requerido
	private bool ValidarNombreRequerido()
	{
		if (string.IsNullOrWhiteSpace(nombreInput.Text))
		{
			MostrarError("El nombre es requerido");
			return false;
		}
		return true;
	}

	// Valida que el nombre no contenga números
	private bool ValidarNombreSinNumeros()
	{
		bool tieneNumeros = false;
		foreach (char c in nombreInput.Text)
		{
			if (char.IsDigit(c))
			{
				tieneNumeros = true;
				break;
			}
		}

		if (tieneNumeros)
		{
			MostrarError("El nombre no puede contener números");
			return false;
		}
		
		return true;
	}

	// Valida que el apellido esté presente
	private bool ValidarApellidoPresente(out string nombre, out string apellido)
	{
		string[] nombreCompleto = nombreInput.Text.Trim().Split(' ');
		nombre = nombreCompleto[0];
		apellido = nombreCompleto.Length > 1 ? string.Join(" ", nombreCompleto[1..]) : "";

		if (string.IsNullOrWhiteSpace(apellido))
		{
			MostrarError("Debe incluir al menos un apellido");
			return false;
		}
		
		return true;
	}

	// ==================== VALIDACIONES DE FECHA ====================
	
	// Valida el formato de la fecha de nacimiento
	private bool ValidarFormatoFechaNacimiento(out DateTime fechaNac)
	{
		if (!DateTime.TryParse(fechaInput.Text, out fechaNac))
		{
			MostrarError("Formato de fecha inválido.\nUse: dd/MM/yyyy\nEjemplo: 15/05/1990");
			return false;
		}
		return true;
	}

	// Valida que la fecha de nacimiento no sea futura
	private bool ValidarFechaNoFutura(DateTime fechaNac)
	{
		if (fechaNac > DateTime.Today)
		{
			MostrarError("La fecha de nacimiento no puede ser futura");
			return false;
		}
		return true;
	}

	// Valida el formato de la fecha de fallecimiento
	private bool ValidarFormatoFechaFallecimiento(DateTime fechaNac, out DateTime? fechaFallecimiento)
	{
		fechaFallecimiento = null;
		
		if (muertoCheck.ButtonPressed)
		{
			DateTime fechaFall;
			if (!DateTime.TryParse(fechaFallecimientoInput.Text, out fechaFall))
			{
				MostrarError("Formato de fecha de fallecimiento inválido.\nUse: dd/MM/yyyy");
				return false;
			}

			if (fechaFall <= fechaNac)
			{
				MostrarError("La fecha de fallecimiento debe ser posterior a la fecha de nacimiento");
				return false;
			}

			fechaFallecimiento = fechaFall;
		}
		
		return true;
	}

	// ==================== VALIDACIONES DE GÉNERO ====================
	
	// Valida que el género esté seleccionado
	private bool ValidarGeneroSeleccionado()
	{
		if (opcionesGenero.Selected == 0)
		{
			MostrarError("Debe seleccionar un género");
			return false;
		}
		return true;
	}

	// ==================== VALIDACIONES DE EDAD ====================
	
	// Valida que la edad sea un número válido
	private bool ValidarEdadNumerica(out int edad)
	{
		if (!int.TryParse(edadInput.Text, out edad))
		{
			MostrarError("La edad debe ser un número válido");
			return false;
		}
		return true;
	}

	// Valida que la edad esté dentro del rango permitido
	private bool ValidarRangoEdad(int edad)
	{
		if (edad < 0 || edad > 150)
		{
			MostrarError("La edad debe estar entre 0 y 150 años");
			return false;
		}
		return true;
	}

	// Valida que la edad sea coherente con la de los padres
	private bool ValidarEdadCoherenteConPadres(int edad)
	{
		if (tipoDePersona.Selected == 0)
		{
			Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
			Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);

			if (padre != null && edad >= padre.Edad)
			{
				MostrarError($"La edad ingresada ({edad} años) no puede ser mayor o igual que la del padre.\n" +
							$"{padre.NombreCompleto} tiene {padre.Edad} años.");
				return false;
			}

			if (madre != null && edad >= madre.Edad)
			{
				MostrarError($"Error: La edad ingresada ({edad} años) no puede ser mayor o igual que la de la madre.\n" +
							$"{madre.NombreCompleto} tiene {madre.Edad} años.");
				return false;
			}
		}
		
		return true;
	}

	// Valida que la edad coincida con la fecha de nacimiento
	private bool ValidarEdadCoincideConFechaNacimiento(int edad, DateTime fechaNac)
	{
		int edadCalculada = DateTime.Today.Year - fechaNac.Year;
		if (fechaNac.Date > DateTime.Today.AddYears(-edadCalculada))
			edadCalculada--;

		if (edad > edadCalculada || edad < edadCalculada)
		{
			MostrarError($"La edad no coincide con la fecha de nacimiento.\nEdad calculada: {edadCalculada} años");
			return false;
		}
		
		return true;
	}

	// ==================== VALIDACIONES DE COORDENADAS ====================
	
	// Valida las coordenadas ingresadas
	private bool ValidarCoordenadas(out double latitud, out double longitud)
	{
		if (!double.TryParse(coordYInput.Text, out latitud))
		{
			MostrarError("La coordenada Y (latitud) debe ser un número");
			longitud = 0;
			return false;
		}

		if (!double.TryParse(coordXInput.Text, out longitud))
		{
			MostrarError("La coordenada X (longitud) debe ser un número");
			return false;
		}
		
		return true;
	}

	// ==================== VALIDACIONES DE FOTO ====================
	
	// Valida y copia la foto al proyecto
	private bool ValidarYCopiarFoto(out string rutaFotoFinal)
	{
		rutaFotoFinal = "";
		
		// Leer la ruta de la foto
		if (rutaFotoInput != null && !string.IsNullOrWhiteSpace(rutaFotoInput.Text))
		{
			rutaFotoSeleccionada = rutaFotoInput.Text.Trim();
			GD.Print($"📸 Ruta de foto ingresada manualmente: {rutaFotoSeleccionada}");
		}
		
		// Copiar la foto al proyecto si se seleccionó
		if (!string.IsNullOrEmpty(rutaFotoSeleccionada))
		{
			rutaFotoFinal = CopiarFotoAlProyecto(rutaFotoSeleccionada, cedulaInput.Text);
			if (string.IsNullOrEmpty(rutaFotoFinal))
			{
				MostrarError("Error al copiar la foto. Verifique que el archivo existe y es una imagen válida.");
				return false;
			}
		}
		
		return true;
	}


	/// <summary>
	/// METODOS AUXILIARES
	/// Metodos auxiliares para el manejo de los datos del formulario
	/// </summary>

	// ==================== MÉTODOS AUXILIARES ====================
	
	// Crea una nueva persona con los datos ingresados
	private Persona CrearNuevaPersona(string nombre, string apellido, DateTime fechaNac, int edad, double latitud, double longitud, string rutaFotoFinal)
	{
		if (tipoDePersona.Selected == 1)
		{
			return new Persona(
				nombre,
				apellido,
				cedulaInput.Text,
				fechaNac,
				edad,
				latitud,
				longitud,
				rutaFotoFinal,
				"conyugue"
			);
		}
		else
		{
			return new Persona(
				nombre,
				apellido,
				cedulaInput.Text,
				fechaNac,
				edad,
				latitud,
				longitud,
				rutaFotoFinal,
				"familiar"
			);
		}
	}

	// Configura el estado de la persona (vivo o fallecido)
	private void ConfigurarEstadoPersona(Persona persona, DateTime? fechaFallecimiento)
	{
		persona.EstaVivo = vivoCheck.ButtonPressed;
		if (fechaFallecimiento.HasValue)
		{
			persona.FechaFallecimiento = fechaFallecimiento;
		}
	}

	// Configura el género de la persona
	private void ConfigurarGenero(Persona persona)
	{
		if (opcionesGenero.Selected == 1)
		{
			persona.GeneroPersona = Persona.Genero.Masculino;
		}
		else if (opcionesGenero.Selected == 2)
		{
			persona.GeneroPersona = Persona.Genero.Femenino;
		}
	}

	// Valida que la persona esté completa y sin errores
	private bool ValidarPersonaCompleta(Persona persona)
	{
		if (!persona.EsValido())
		{
			var errores = persona.ObtenerErroresValidacion();
			MostrarError(string.Join("\n", errores));
			return false;
		}
		return true;
	}

	// Registra una nueva persona en las listas correspondientes
	private void RegistrarPersona(Persona persona)
	{
		cedulasExistentes.Add(cedulaInput.Text);
		personasCreadas.Add(persona);

		// Filtrar por género
		if (persona.GeneroPersona == Persona.Genero.Masculino)
			hombres.Add(persona);
		else if (persona.GeneroPersona == Persona.Genero.Femenino)
			mujeres.Add(persona);
	}

	// Establece las relaciones familiares de la persona
	private void EstablecerRelacionesFamiliares(Persona persona)
	{
		if (tipoDePersona.Selected == 0)
		{
			EstablecerPadres(persona);
		}
		else
		{
			EstablecerConyuge(persona);
		}
	}

	// Agrega la persona al árbol genealógico y al grafo
	private void AgregarAlArbolYGrafo(Persona persona)
	{
		arbol.AgregarPersona(persona);

		var grafo = Grafo.ObtenerInstancia();
		grafo.AgregarNodo(persona);
		grafo.ConstruirAristas();
	}

	// Actualiza la interfaz con los datos de la nueva persona
	private void ActualizarInterfaz(Persona persona)
	{
		GD.Print("\n=== Intentando actualizar visualización ===");
		if (visualizadorUI != null)
		{
			GD.Print("Llamando a ActualizarArbol...");
			visualizadorUI.ActualizarArbol(arbol);
			GD.Print("ActualizarArbol ejecutado");
		}
		else
		{
			GD.PrintErr("⚠ ERROR: No se puede actualizar - visualizadorUI es null");
			GD.PrintErr("⚠ Verifica que VisualizadorArbolUI esté en la escena");
		}

		// Actualizar listas si se agregó alguien masculino o femenino
		if (persona.GeneroPersona == Persona.Genero.Masculino ||
			persona.GeneroPersona == Persona.Genero.Femenino)
		{
			ActualizarTodasLasListas();
		}

		GD.Print($"✓ {persona.NombreCompleto} agregado al árbol genealógico");
		GD.Print($"Género: {persona.GeneroPersona}");
		GD.Print($"Foto: {persona.RutaFotografia}");
		GD.Print($"Total personas: {personasCreadas.Count}");
	}

	// Muestra un mensaje de error en un cuadro de diálogo
	private void MostrarError(string mensaje)
	{
		dialogoError.DialogText = mensaje;
		dialogoError.PopupCentered();
		GD.PrintErr($"Error: {mensaje}");
	}

	// Actualiza todas las listas desplegables
	private void ActualizarTodasLasListas()
	{
		ActualizarListaPadres();
		ActualizarListaConyuges();
	}

	// Actualiza la lista de padres disponibles
	private void ActualizarListaPadres()
	{
		opcionesPadre.Clear();
		opcionesMadre.Clear();

		opcionesPadre.AddItem("(ninguno)");
		opcionesMadre.AddItem("(ninguno)");

		//usar listas filtradas
		foreach (var hombre in hombres)
		{
			opcionesPadre.AddItem($"{hombre.NombreCompleto} ({hombre.Cedula})");
		}	

		foreach (var mujer in mujeres)
		{
			opcionesMadre.AddItem($"{mujer.NombreCompleto} ({mujer.Cedula})");
		}

		opcionesPadre.Selected = 0;
		opcionesMadre.Selected = 0;

	}

	// Actualiza la lista de cónyuges disponibles
	private void ActualizarListaConyuges()
	{
		// Guardar la selección actual antes de limpiar
		int seleccionActual = conyugue.Selected;
		string textoSeleccionado = seleccionActual > 0 ? conyugue.GetItemText(seleccionActual) : "";
		
		// Obtener lista según género seleccionado
		List<Persona> personasDisponibles = ObtenerPersonasParaConyuge();
		
		// Verificar si la persona actualmente seleccionada sigue siendo válida
		bool seleccionSigueValida = false;
		
		if (seleccionActual > 0 && !string.IsNullOrEmpty(textoSeleccionado))
		{
			// Extraer cédula de la selección actual
			int inicioParentesis = textoSeleccionado.LastIndexOf('(');
			int finParentesis = textoSeleccionado.LastIndexOf(')');
			
			if (inicioParentesis > 0 && finParentesis > inicioParentesis)
			{
				string cedulaSeleccionada = textoSeleccionado.Substring(
					inicioParentesis + 1,
					finParentesis - inicioParentesis - 1
				).Trim();
				
				// Verificar si esa persona está en la lista de disponibles
				seleccionSigueValida = personasDisponibles.Any(p => p.Cedula == cedulaSeleccionada);
			}
		}
		
		if (!seleccionSigueValida)
		{
			conyugue.Clear();
			conyugue.AddItem("(ninguno)");
			
			foreach (var persona in personasDisponibles)
			{
				if (persona.Conyuge == null)
				{
					conyugue.AddItem($"{persona.NombreCompleto} ({persona.Cedula})");
				}
			}
			
			conyugue.Selected = 0;
		}
		// Si la selección sigue válida, no hacemos nada (mantiene la selección actual)
	}

	// Obtiene las personas disponibles para ser cónyuges
	private List<Persona> ObtenerPersonasParaConyuge()
	{
		int generoSeleccionado = opcionesGenero.Selected;

		if (generoSeleccionado == 1)
		{
			return mujeres;
		}
		else if (generoSeleccionado == 2)
		{
			return hombres;
		}
		else
		{
			return personasCreadas;
		}
	}

	// Verifica que los padres seleccionados sean cónyuges entre sí
	private Boolean VerificarPadresConyugues()
	{
		Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
		Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);

		//si ambos padres están seleccionados, validar que sean cónyuges
		if (padre != null && madre != null)
		{
			if (padre.Conyuge != madre || madre.Conyuge != padre)
			{
				MostrarError("El padre y la madre seleccionados deben ser cónyuges entre sí.\n\n" +
							"Por favor, seleccione padres que estén casados o deje uno de los campos vacío.");
				return false;
			}
			return true;
		}
		return true;
	}

	// Establece los padres de una nueva persona
	private void EstablecerPadres(Persona nuevaPersona)
	{
		Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
		Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);

		if (padre != null || madre != null)
		{
			nuevaPersona.EstablecerPadres(padre, madre);
		}
	}

	// Establece el cónyuge de una nueva persona
	private void EstablecerConyuge(Persona nuevaPersona)
	{
		string textoSeleccionado = conyugue.GetItemText(conyugue.Selected);

		int inicioParentesis = textoSeleccionado.LastIndexOf('(');
		int finParentesis = textoSeleccionado.LastIndexOf(')');

		string cedulaConyugue = textoSeleccionado.Substring(
			inicioParentesis + 1,
			finParentesis - inicioParentesis - 1
		).Trim();

		//encontrar conyugue por medio de lista que contiene a todas las personas creadas
		Persona conyugeSeleccionado = personasCreadas.Find(p => p.Cedula == cedulaConyugue);

		if (conyugeSeleccionado == null)
		{
			GD.PrintErr($"Error: No se encontró persona con cédula {cedulaConyugue}");
			return;
		}

		nuevaPersona.Conyuge = conyugeSeleccionado;
		conyugeSeleccionado.Conyuge = nuevaPersona;
	}

	// Busca una persona en la lista según el índice seleccionado
	private Persona BuscarPersonaEnLista(List<Persona> lista, int indiceSeleccionado)
	{
		if (indiceSeleccionado <= 0 || indiceSeleccionado > lista.Count)
			return null;

		return lista[indiceSeleccionado - 1]; //-1 porque el índice 0 es "(ninguno)"
	}


	/// <summary>
	/// GESTIÓN DE CONTROLES DE INTERFAZ
	/// </summary>

	// Maneja el evento de marcar como vivo
	private void OnVivoPressed()
	{
		if (vivoCheck.ButtonPressed)
		{
			muertoCheck.ButtonPressed = false;
			label11.Visible = false;
			fechaFallecimientoInput.Visible = false;
		}
	}

	// Maneja el evento de marcar como fallecido
	private void OnMuertoPressed()
	{
		if (muertoCheck.ButtonPressed)
		{
			vivoCheck.ButtonPressed = false;
			label11.Visible = true;
			fechaFallecimientoInput.Visible = true;
		}
	}

	// Maneja el cambio de tipo de persona en el formulario
	private void OnTipoPersonaChanged(long index)
	{
		ConfigurarVisibilidadCampos();
		ActualizarListaConyuges();
	}

	// Maneja el cambio de género en el formulario
	private void OnGeneroChanged(long index)
	{
		ActualizarListaConyuges();
	}

	private void ConfigurarVisibilidadCampos()
	{
		bool esFamiliar = tipoDePersona.Selected == 0; //0 es familiar y 1 es el cónyuge

		//mostrar u ocultar campos de padres
		label12.Visible = esFamiliar;
		opcionesMadre.Visible = esFamiliar;
		label13.Visible = esFamiliar;
		opcionesPadre.Visible = esFamiliar;

		//mostrar u ocultar campos de cónyuge
		label16.Visible = !esFamiliar;
		conyugue.Visible = !esFamiliar;
	}

	private void ConfigurarTamañoDropdowns()
	{
		// Configurar tamaño fijo para los botones
		ConfigurarDrop(opcionesPadre, new Vector2(170, 28));
		ConfigurarDrop(opcionesMadre, new Vector2(170, 28));
		ConfigurarDrop(opcionesGenero, new Vector2(163, 35));
		ConfigurarDrop(tipoDePersona, new Vector2(120, 35));
		ConfigurarDrop(conyugue, new Vector2(170, 28));	
	}

	private void ConfigurarDrop(OptionButton drop, Vector2 size)
	{
		drop.CustomMinimumSize = size;
		drop.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
		drop.ClipContents = true;

		// Forzar clipping del texto interno
		var label = drop.GetChild(0) as Label;
		if (label != null)
			label.ClipText = true;

		// Forzar tamaño final después del layout
		drop.CallDeferred("set_size", size);

		// Limitar el tamaño del popup
		var popup = drop.GetPopup();
		popup.MaxSize = new Vector2I(400, 250);
	}


	// Maneja el evento de cancelar y limpia los campos
	private void OnCancelarPressed()
	{
		LimpiarCampos();
	}

	// Maneja el evento de volver al menú principal
	private void OnVolverPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}

	// Limpia todos los campos del formulario
	private void LimpiarCampos()
	{
		nombreInput.Text = "";
		cedulaInput.Text = "";
		coordXInput.Text = "";
		coordYInput.Text = "";
		fechaInput.Text = "";
		edadInput.Text = "";
		fechaFallecimientoInput.Text = "";

		rutaFotoSeleccionada = "";
		if (rutaFotoInput != null)
			rutaFotoInput.Text = "";

		if (previsualizacionFoto != null)
		{
			previsualizacionFoto.Texture = null;
		}

		opcionesGenero.Selected = 0;
		opcionesPadre.Selected = 0;
		opcionesMadre.Selected = 0;
		conyugue.Selected = 0;
		tipoDePersona.Selected = 0;

		vivoCheck.ButtonPressed = true;
		muertoCheck.ButtonPressed = false;
		label11.Visible = false;
		fechaFallecimientoInput.Visible = false;

		ConfigurarVisibilidadCampos();
	}

	// Maneja el evento de cargar una foto desde el sistema
	private void OnCargarFotoPressed()
	{
		if (dialogoSeleccionarFoto != null)
		{
			// Configurar la ruta inicial a la carpeta Downloads del usuario
			string downloadsPath = System.IO.Path.Combine(
				System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile),
                "Downloads"
			);

			// Verificar que la carpeta existe
			if (System.IO.Directory.Exists(downloadsPath))
			{
				dialogoSeleccionarFoto.CurrentDir = downloadsPath;
			}

			dialogoSeleccionarFoto.PopupCentered();
		}
	}

	// Maneja la selección de una foto desde el cuadro de diálogo
	private void OnFotoSeleccionada(string ruta)
	{
		GD.Print($"📸 Ruta recibida del FileDialog: {ruta}");

		//la ruta ya viene como absoluta del sistema si se usa Access = Filesystem
		rutaFotoSeleccionada = ruta;

		if (rutaFotoInput != null)
		{
			rutaFotoInput.Text = System.IO.Path.GetFileName(rutaFotoSeleccionada);
		}

		// Mostrar previsualización si existe el nodo
		if (previsualizacionFoto != null)
		{
			try
			{
				var image = Image.LoadFromFile(rutaFotoSeleccionada);
				if (image != null)
				{
					var texture = ImageTexture.CreateFromImage(image);
					previsualizacionFoto.Texture = texture;
					GD.Print("✓ Previsualización cargada");
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"Error al cargar previsualización: {ex.Message}");
			}
		}
	}
}
