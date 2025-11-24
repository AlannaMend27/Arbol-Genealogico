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

	/// <summary>
	/// Inicializa todos los componentes de la interfaz y configura los eventos del formulario
	/// </summary>
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

	/// <summary>
	/// Crea la carpeta de fotos dentro del proyecto si no existe
	/// </summary>
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
					// Carpeta creada correctamente (sin log en consola)
				}
				else
				{
				}
			}
		}
	}

	/// <summary>
	/// Actualiza la visualización del árbol genealógico en la interfaz
	/// </summary>
	private void ActualizarVisualizacionArbol()
	{
		if (visualizadorUI != null)
		{
			visualizadorUI.ActualizarArbol(arbol);
		}
	}

	/// <summary>
	/// Inicializa el visualizador del árbol genealógico buscándolo en la escena
	/// </summary>
	private void InicializarVisualizador()
	{
		

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
			// Visualizador no encontrado: dejar que la UI maneje el error si es necesario
		}
		else
		{
			// Visualizador conectado
		}
	}

	/// <summary>
	/// Busca recursivamente el visualizador del árbol en el árbol de nodos
	/// </summary>
	/// <param name="nodo">Nodo desde donde iniciar la búsqueda</param>
	/// <returns>El visualizador encontrado o null si no existe</returns>
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

	/// <summary>
	/// Maneja el evento de clic en el botón aceptar, validando y guardando los datos de la persona
	/// </summary>
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


	/// <summary>
	/// Copia la foto seleccionada al directorio del proyecto con un nombre único basado en la cédula
	/// </summary>
	/// <param name="rutaOrigen">Ruta del archivo de foto original</param>
	/// <param name="cedula">Cédula de la persona para nombrar el archivo</param>
	/// <returns>Ruta relativa de Godot de la foto copiada o cadena vacía si hay error</returns>
	private string CopiarFotoAlProyecto(string rutaOrigen, string cedula)
	{
		try
		{
			// Verificar que el archivo existe
			if (!System.IO.File.Exists(rutaOrigen))
			{
				return "";
			}

			// Obtener extensión del archivo
			string extension = System.IO.Path.GetExtension(rutaOrigen).ToLower();

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

			return rutaGodot;
		}
		catch (Exception)
		{
			return "";
		}
	}

	/// <summary>
	/// Validaciones de cada uno de los datos ingresados por medio del formulario
	/// </summary>


	// ==================== VALIDACIONES DE CÉDULA ====================
	
	/// <summary>
	/// Valida que el campo de cédula no esté vacío
	/// </summary>
	/// <returns>True si la cédula está ingresada, false en caso contrario</returns>
	private bool ValidarCedulaRequerida()
	{
		if (string.IsNullOrWhiteSpace(cedulaInput.Text))
		{
			MostrarError("La cédula es requerida");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Valida que la longitud de la cédula esté dentro del rango permitido
	/// </summary>
	/// <returns>True si la longitud es válida, false en caso contrario</returns>
	private bool ValidarLongitudCedula()
	{
		if (cedulaInput.Text.Length < 9 || cedulaInput.Text.Length > 12)
		{
			MostrarError("La cédula no tiene la extensión adecuada");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Valida que la cédula ingresada no esté ya registrada en el sistema
	/// </summary>
	/// <returns>True si la cédula es única, false si ya existe</returns>
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
	
	/// <summary>
	/// Valida que se haya seleccionado un cónyuge cuando el tipo de persona es cónyuge
	/// </summary>
	/// <returns>True si la validación es correcta, false en caso contrario</returns>
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

	/// <summary>
	/// Valida que solo exista un fundador en el árbol genealógico
	/// </summary>
	/// <returns>True si la validación es correcta, false si se intenta agregar un segundo fundador</returns>
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
				string textoSeleccionado = conyugue.GetItemText(conyugue.Selected);
				string cedulaConyugue = ExtraerCedulaDeTexto(textoSeleccionado);

				if (!string.IsNullOrEmpty(cedulaConyugue))
				{
					Persona conyugueSeleccionado = personasCreadas.Find(p => p.Cedula == cedulaConyugue);
					
					if (conyugueSeleccionado == null)
					{
						MostrarError("Ya existe un fundador. Solo se permite agregar su cónyuge.");
						return false;
					}
				}

			}
		}
		
		return true;
	}

	/// <summary>
	/// Valida que ambos padres estén seleccionados cuando ya existen fundadores en el árbol
	/// </summary>
	/// <returns>True si la validación es correcta, false en caso contrario</returns>
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
	
	/// <summary>
	/// Valida que el campo de nombre no esté vacío
	/// </summary>
	/// <returns>True si el nombre está ingresado, false en caso contrario</returns>
	private bool ValidarNombreRequerido()
	{
		if (string.IsNullOrWhiteSpace(nombreInput.Text))
		{
			MostrarError("El nombre es requerido");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Valida que el nombre no contenga caracteres numéricos
	/// </summary>
	/// <returns>True si el nombre es válido, false si contiene números</returns>
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

	/// <summary>
	/// Valida que el nombre incluya al menos un apellido y extrae nombre y apellido
	/// </summary>
	/// <param name="nombre">Variable de salida con el nombre de la persona</param>
	/// <param name="apellido">Variable de salida con el apellido de la persona</param>
	/// <returns>True si tiene apellido, false en caso contrario</returns>
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
	
	/// <summary>
	/// Valida que el formato de la fecha de nacimiento sea correcto
	/// </summary>
	/// <param name="fechaNac">Variable de salida con la fecha de nacimiento parseada</param>
	/// <returns>True si el formato es válido, false en caso contrario</returns>
	private bool ValidarFormatoFechaNacimiento(out DateTime fechaNac)
	{
		if (!DateTime.TryParse(fechaInput.Text, out fechaNac))
		{
			MostrarError("Formato de fecha inválido.\nUse: dd/MM/yyyy\nEjemplo: 15/05/1990");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Valida que la fecha de nacimiento no sea una fecha futura
	/// </summary>
	/// <param name="fechaNac">Fecha de nacimiento a validar</param>
	/// <returns>True si la fecha es válida, false si es futura</returns>
	private bool ValidarFechaNoFutura(DateTime fechaNac)
	{
		if (fechaNac > DateTime.Today)
		{
			MostrarError("La fecha de nacimiento no puede ser futura");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Valida el formato de la fecha de fallecimiento y que sea posterior al nacimiento
	/// </summary>
	/// <param name="fechaNac">Fecha de nacimiento de la persona</param>
	/// <param name="fechaFallecimiento">Variable de salida con la fecha de fallecimiento parseada</param>
	/// <returns>True si la validación es correcta, false en caso contrario</returns>
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
	
	/// <summary>
	/// Valida que se haya seleccionado un género para la persona
	/// </summary>
	/// <returns>True si hay un género seleccionado, false en caso contrario</returns>
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
	
	/// <summary>
	/// Valida que la edad ingresada sea un número válido
	/// </summary>
	/// <param name="edad">Variable de salida con la edad parseada</param>
	/// <returns>True si la edad es un número válido, false en caso contrario</returns>
	private bool ValidarEdadNumerica(out int edad)
	{
		if (!int.TryParse(edadInput.Text, out edad))
		{
			MostrarError("La edad debe ser un número válido");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Valida que la edad esté dentro del rango permitido (0-150 años)
	/// </summary>
	/// <param name="edad">Edad a validar</param>
	/// <returns>True si la edad está en el rango, false en caso contrario</returns>
	private bool ValidarRangoEdad(int edad)
	{
		if (edad < 0 || edad > 150)
		{
			MostrarError("La edad debe estar entre 0 y 150 años");
			return false;
		}
		return true;
	}

	/// <summary>
	/// Valida que la edad de la persona sea menor que la edad de sus padres
	/// </summary>
	/// <param name="edad">Edad a validar</param>
	/// <returns>True si la edad es coherente, false en caso contrario</returns>
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

	/// <summary>
	/// Valida que la edad ingresada coincida con la edad calculada a partir de la fecha de nacimiento
	/// </summary>
	/// <param name="edad">Edad ingresada manualmente</param>
	/// <param name="fechaNac">Fecha de nacimiento para calcular la edad</param>
	/// <returns>True si la edad coincide, false en caso contrario</returns>
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
	
	/// <summary>
	/// Valida que las coordenadas ingresadas sean números válidos
	/// </summary>
	/// <param name="latitud">Variable de salida con la latitud parseada</param>
	/// <param name="longitud">Variable de salida con la longitud parseada</param>
	/// <returns>True si ambas coordenadas son válidas, false en caso contrario</returns>
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
	
	/// <summary>
	/// Valida que la foto exista y la copia al directorio del proyecto
	/// </summary>
	/// <param name="rutaFotoFinal">Variable de salida con la ruta final de la foto en el proyecto</param>
	/// <returns>True si la validación y copia son exitosas, false en caso contrario</returns>
	private bool ValidarYCopiarFoto(out string rutaFotoFinal)
	{
		rutaFotoFinal = "";
		
		// Leer la ruta de la foto
		if (rutaFotoInput != null && !string.IsNullOrWhiteSpace(rutaFotoInput.Text))
		{
			rutaFotoSeleccionada = rutaFotoInput.Text.Trim();
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
	
	/// <summary>
	/// Crea una nueva instancia de Persona con los datos proporcionados
	/// </summary>
	/// <param name="nombre">Nombre de la persona</param>
	/// <param name="apellido">Apellido de la persona</param>
	/// <param name="fechaNac">Fecha de nacimiento</param>
	/// <param name="edad">Edad actual</param>
	/// <param name="latitud">Coordenada de latitud</param>
	/// <param name="longitud">Coordenada de longitud</param>
	/// <param name="rutaFotoFinal">Ruta de la fotografía</param>
	/// <returns>Nueva instancia de Persona</returns>
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

	/// <summary>
	/// Extrae la cédula del texto de un item del OptionButton
	/// </summary>
	/// <param name="textoItem">Texto del item en formato "Nombre (Cédula)"</param>
	/// <returns>La cédula extraída o cadena vacía si no se encuentra</returns>
	private string ExtraerCedulaDeTexto(string textoItem)
	{
		int inicioParentesis = textoItem.LastIndexOf('(');
		int finParentesis = textoItem.LastIndexOf(')');
		
		if (inicioParentesis > 0 && finParentesis > inicioParentesis)
		{
			return textoItem.Substring(
				inicioParentesis + 1,
				finParentesis - inicioParentesis - 1
			).Trim();
		}
		
		return "";
	}

	/// <summary>
	/// Configura el estado vital de la persona y su fecha de fallecimiento si aplica
	/// </summary>
	/// <param name="persona">Persona a configurar</param>
	/// <param name="fechaFallecimiento">Fecha de fallecimiento opcional</param>
	private void ConfigurarEstadoPersona(Persona persona, DateTime? fechaFallecimiento)
	{
		persona.EstaVivo = vivoCheck.ButtonPressed;
		if (fechaFallecimiento.HasValue)
		{
			persona.FechaFallecimiento = fechaFallecimiento;
		}
	}

	/// <summary>
	/// Configura el género de la persona según la selección del formulario
	/// </summary>
	/// <param name="persona">Persona a configurar</param>
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

	/// <summary>
	/// Valida que la persona cumpla con todos los requisitos necesarios
	/// </summary>
	/// <param name="persona">Persona a validar</param>
	/// <returns>True si la persona es válida, false en caso contrario</returns>
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

	/// <summary>
	/// Registra una nueva persona en las listas del sistema
	/// </summary>
	/// <param name="persona">Persona a registrar</param>
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

	/// <summary>
	/// Establece las relaciones familiares de la persona (padres o cónyuge)
	/// </summary>
	/// <param name="persona">Persona a la que se le establecerán las relaciones</param>
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

	/// <summary>
	/// Agrega la persona al árbol genealógico y al grafo de relaciones
	/// </summary>
	/// <param name="persona">Persona a agregar</param>
	private void AgregarAlArbolYGrafo(Persona persona)
	{
		arbol.AgregarPersona(persona);

		var grafo = Grafo.ObtenerInstancia();
		grafo.AgregarNodo(persona);
		grafo.ConstruirAristas();
	}

	/// <summary>
	/// Actualiza la interfaz con la nueva persona agregada
	/// </summary>
	/// <param name="persona">Persona que fue agregada</param>
	private void ActualizarInterfaz(Persona persona)
	{
		if (visualizadorUI != null)
		{
			visualizadorUI.ActualizarArbol(arbol);
		}

		// Actualizar listas si se agregó alguien masculino o femenino
		if (persona.GeneroPersona == Persona.Genero.Masculino ||
			persona.GeneroPersona == Persona.Genero.Femenino)
		{
			ActualizarTodasLasListas();
		}
	}

	/// <summary>
	/// Muestra un mensaje de error en un cuadro de diálogo
	/// </summary>
	/// <param name="mensaje">Mensaje de error a mostrar</param>
	private void MostrarError(string mensaje)
	{
		dialogoError.DialogText = mensaje;
		dialogoError.PopupCentered();
	}

	/// <summary>
	/// Actualiza todas las listas desplegables del formulario
	/// </summary>
	private void ActualizarTodasLasListas()
	{
		ActualizarListaPadres();
		ActualizarListaConyuges();
	}

	/// <summary>
	/// Actualiza las listas desplegables de padre y madre disponibles
	/// </summary>
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

	/// <summary>
	/// Actualiza la lista desplegable de cónyuges disponibles según el género seleccionado
	/// </summary>
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
			string cedulaSeleccionada = ExtraerCedulaDeTexto(textoSeleccionado);
			
			if (!string.IsNullOrEmpty(cedulaSeleccionada))
			{
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

	/// <summary>
	/// Obtiene la lista de personas disponibles para ser cónyuges según el género seleccionado
	/// </summary>
	/// <returns>Lista de personas disponibles como cónyuges</returns>
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

	/// <summary>
	/// Verifica que los padres seleccionados sean cónyuges entre sí
	/// </summary>
	/// <returns>True si los padres son cónyuges o si no aplica la validación, false en caso contrario</returns>
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

	/// <summary>
	/// Establece las relaciones de padre y madre para una persona
	/// </summary>
	/// <param name="nuevaPersona">Persona a la que se le asignarán los padres</param>
	private void EstablecerPadres(Persona nuevaPersona)
	{
		Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
		Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);

		if (padre != null || madre != null)
		{
			nuevaPersona.EstablecerPadres(padre, madre);
		}
	}

	/// <summary>
	/// Establece la relación de cónyuge entre dos personas
	/// </summary>
	/// <param name="nuevaPersona">Persona a la que se le asignará el cónyuge</param>
	private void EstablecerConyuge(Persona nuevaPersona)
	{
		string textoSeleccionado = conyugue.GetItemText(conyugue.Selected);
		string cedulaConyugue = ExtraerCedulaDeTexto(textoSeleccionado);

		if (string.IsNullOrEmpty(cedulaConyugue))
		{
			return;
		}

		//encontrar conyugue por medio de lista que contiene a todas las personas creadas
		Persona conyugeSeleccionado = personasCreadas.Find(p => p.Cedula == cedulaConyugue);

		if (conyugeSeleccionado == null)
		{
			return;
		}

		nuevaPersona.Conyuge = conyugeSeleccionado;
		conyugeSeleccionado.Conyuge = nuevaPersona;
	}

	/// <summary>
	/// Busca una persona en una lista según el índice seleccionado en un OptionButton
	/// </summary>
	/// <param name="lista">Lista donde buscar la persona</param>
	/// <param name="indiceSeleccionado">Índice seleccionado en el OptionButton</param>
	/// <returns>La persona encontrada o null si no existe</returns>
	private Persona BuscarPersonaEnLista(List<Persona> lista, int indiceSeleccionado)
	{
		if (indiceSeleccionado <= 0 || indiceSeleccionado > lista.Count)
			return null;

		return lista[indiceSeleccionado - 1]; //-1 porque el índice 0 es "(ninguno)"
	}


	/// <summary>
	/// GESTIÓN DE CONTROLES DE INTERFAZ
	/// </summary>

	/// <summary>
	/// Maneja el evento cuando se marca el checkbox de vivo
	/// </summary>
	private void OnVivoPressed()
	{
		if (vivoCheck.ButtonPressed)
		{
			muertoCheck.ButtonPressed = false;
			label11.Visible = false;
			fechaFallecimientoInput.Visible = false;
		}
	}

	/// <summary>
	/// Maneja el evento cuando se marca el checkbox de fallecido
	/// </summary>
	private void OnMuertoPressed()
	{
		if (muertoCheck.ButtonPressed)
		{
			vivoCheck.ButtonPressed = false;
			label11.Visible = true;
			fechaFallecimientoInput.Visible = true;
		}
	}

	/// <summary>
	/// Maneja el cambio de selección en el tipo de persona (familiar o cónyuge)
	/// </summary>
	/// <param name="index">Índice de la opción seleccionada</param>
	private void OnTipoPersonaChanged(long index)
	{
		ConfigurarVisibilidadCampos();
		ActualizarListaConyuges();
	}

	/// <summary>
	/// Maneja el cambio de selección en el género de la persona
	/// </summary>
	/// <param name="index">Índice de la opción seleccionada</param>
	private void OnGeneroChanged(long index)
	{
		ActualizarListaConyuges();
	}

	/// <summary>
	/// Configura la visibilidad de los campos según el tipo de persona seleccionado
	/// </summary>
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

	/// <summary>
	/// Configura el tamaño de todos los dropdowns del formulario
	/// </summary>
	private void ConfigurarTamañoDropdowns()
	{
		// Configurar tamaño fijo para los botones
		ConfigurarDrop(opcionesPadre, new Vector2(170, 28));
		ConfigurarDrop(opcionesMadre, new Vector2(170, 28));
		ConfigurarDrop(opcionesGenero, new Vector2(163, 35));
		ConfigurarDrop(tipoDePersona, new Vector2(120, 35));
		ConfigurarDrop(conyugue, new Vector2(170, 28));	
	}

	/// <summary>
	/// Configura el tamaño y comportamiento de un OptionButton específico
	/// </summary>
	/// <param name="drop">OptionButton a configurar</param>
	/// <param name="size">Tamaño deseado para el dropdown</param>
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


	/// <summary>
	/// Maneja el evento de clic en el botón cancelar y limpia todos los campos
	/// </summary>
	private void OnCancelarPressed()
	{
		LimpiarCampos();
	}

	/// <summary>
	/// Maneja el evento de clic en el botón volver y regresa al menú principal
	/// </summary>
	private void OnVolverPressed()
	{
		GetTree().ChangeSceneToFile("res://scenes/MainMenu.tscn");
	}

	/// <summary>
	/// Limpia todos los campos del formulario y restablece los valores por defecto
	/// </summary>
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

	/// <summary>
	/// Maneja el evento de clic en el botón de cargar foto, abriendo el diálogo de selección de archivos
	/// </summary>
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

	/// <summary>
	/// Maneja la selección de una foto desde el diálogo de archivos
	/// </summary>
	/// <param name="ruta">Ruta del archivo de foto seleccionado</param>
	private void OnFotoSeleccionada(string ruta)
	{
		// la ruta ya viene como absoluta del sistema si se usa Access = Filesystem
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
				}
			}
			catch (Exception)
			{
				// no mostrar logs en consola
			}
		}
	}
}