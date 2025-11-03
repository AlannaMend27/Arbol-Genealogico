using Godot;
using System;
using System.Collections.Generic;
using ArbolGenealogico.scripts.Models;
using ArbolGenealogico.scripts.DataStructures;
using ArbolGenealogico.scripts.UI;

//FALTA VALIDAR NUMEROS PARA LAS COORDENADAS    
public partial class AgregarPersona : Node2D
{
	//lista global de personas creadas (temporal)
	private static List<string> cedulasExistentes = new List<string>();
	private static List<Persona> personasCreadas = new List<Persona>();

	// Prueba para visualizar la construccion el arbol en consola (futura implementacion en interfaz)
	private static VisualizadorArbol visualizador = new VisualizadorArbol();

	//campos de entrada del formulario
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
	private Button aceptarBtn;
	private Button cancelarBtn;

	//dialogo para mostrar errores
	private AcceptDialog dialogoError;

	public override void _Ready()
	{
		//buscar los nodos en la escena
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

		aceptarBtn = GetNode<Button>("aceptar");
		cancelarBtn = GetNode<Button>("cancelar");

		dialogoError = new AcceptDialog();
		dialogoError.Title = "Error";
		dialogoError.OkButtonText = "Entendido";
		AddChild(dialogoError);

		//conectar botones a sus funciones
		aceptarBtn.Pressed += OnAceptarPressed;
		cancelarBtn.Pressed += OnCancelarPressed;

		//conectar checkboxes para mostrar o ocultar campos
		vivoCheck.Pressed += OnVivoPressed;
		muertoCheck.Pressed += OnMuertoPressed;

		//configuracion inicial
		vivoCheck.ButtonPressed = true;
		label11.Visible = false;
		fechaFallecimientoInput.Visible = false;

		//configurar opciones de genero
		opcionesGenero.AddItem("No especificado");
		opcionesGenero.AddItem("Masculino");
		opcionesGenero.AddItem("Femenino");
		opcionesGenero.AddItem("Otro");
		opcionesGenero.Selected = 0;

		//configurar opciones de padres (se llenan cuando hay personas)
		ActualizarListaPadres();
	}

	private void OnAceptarPressed()
	{
		try
		{
			//validar campos obligatorios
			if (string.IsNullOrWhiteSpace(nombreInput.Text))
			{
				MostrarError("El nombre es requerido");
				return;
			}

			if (string.IsNullOrWhiteSpace(cedulaInput.Text))
			{
				MostrarError("La cédula es requerida");
				return;
			}

			//validar longitud de cedula
			if (cedulaInput.Text.Length < 9 || cedulaInput.Text.Length > 12)
			{
				MostrarError("La cédula no tiene la extensión adecuada");
				return;
			}

			//validar que la cedula sea unica
			if (cedulasExistentes.Contains(cedulaInput.Text))
			{
				MostrarError("Esta cédula ya está registrada");
				return;
			}

			//validar que el nombre no tenga numeros
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
				return;
			}

			//separar nombre del apellido
			string[] nombreCompleto = nombreInput.Text.Trim().Split(' ');
			string nombre = nombreCompleto[0];
			string apellido = nombreCompleto.Length > 1 ? string.Join(" ", nombreCompleto[1..]) : "";

			//validar que tenga al menos un apellido
			if (string.IsNullOrWhiteSpace(apellido))
			{
				MostrarError("Debe incluir al menos un apellido");
				return;
			}

			//pasar fecha
			DateTime fechaNac;
			if (!DateTime.TryParse(fechaInput.Text, out fechaNac))
			{
				MostrarError("Formato de fecha inválido.\nUse: dd/MM/yyyy\nEjemplo: 15/05/1990");
				return;
			}

			//validar que la fecha no sea futura
			if (fechaNac > DateTime.Today)
			{
				MostrarError("La fecha de nacimiento no puede ser futura");
				return;
			}

			int edad;
			if (!int.TryParse(edadInput.Text, out edad))
			{
				MostrarError("La edad debe ser un número válido");
				return;
			}

			if (edad < 0 || edad > 150)
			{
				MostrarError("La edad debe estar entre 0 y 150 años");
				return;
			}

			//validar que la edad calce con la fecha de nacimiento
			int edadCalculada = DateTime.Today.Year - fechaNac.Year;
			if (fechaNac.Date > DateTime.Today.AddYears(-edadCalculada))
				edadCalculada--;

			if (edad > edadCalculada)
			{
				MostrarError($"La edad no coincide con la fecha de nacimiento.\nEdad calculada: {edadCalculada} años");
				return;
			}

			//validar fecha de fallecimiento si esta muerto
			DateTime? fechaFallecimiento = null;
			if (muertoCheck.ButtonPressed)
			{
				DateTime fechaFall;
				if (!DateTime.TryParse(fechaFallecimientoInput.Text, out fechaFall))
				{
					MostrarError("Formato de fecha de fallecimiento inválido.\nUse: dd/MM/yyyy");
					return;
				}

				if (fechaFall <= fechaNac)
				{
					MostrarError("La fecha de fallecimiento debe ser posterior a la fecha de nacimiento");
					return;
				}

				fechaFallecimiento = fechaFall;
			}

			//convertir coordenadas de texto a numeros
			double latitud;
			if (!double.TryParse(coordYInput.Text, out latitud))
			{
				MostrarError("La coordenada Y (latitud) debe ser un número");
				return;
			}

			double longitud;
			if (!double.TryParse(coordXInput.Text, out longitud))
			{
				MostrarError("La coordenada X (longitud) debe ser un número");
				return;
			}

			//crear nueva persona con los datos
			Persona nuevaPersona = new Persona(
				nombre,
				apellido,
				cedulaInput.Text,
				fechaNac,
				edad,
				latitud,
				longitud,
				"", //¿ciudad?
				"", //¿pais?
				""  //fotita
			);

			//configurar estado y fecha de fallecimiento
			nuevaPersona.EstaVivo = vivoCheck.ButtonPressed;
			if (fechaFallecimiento.HasValue)
			{
				nuevaPersona.FechaFallecimiento = fechaFallecimiento;
			}

			//configurar genero ANTES de agregar a la lista
			switch (opcionesGenero.Selected)
			{
				case 0: nuevaPersona.GeneroPersona = Persona.Genero.NoEspecificado; break;
				case 1: nuevaPersona.GeneroPersona = Persona.Genero.Masculino; break;
				case 2: nuevaPersona.GeneroPersona = Persona.Genero.Femenino; break;
				case 3: nuevaPersona.GeneroPersona = Persona.Genero.Otro; break;
			}

			//verificar que la persona sea valida
			if (!nuevaPersona.EsValido())
			{
				var errores = nuevaPersona.ObtenerErroresValidacion();
				MostrarError(string.Join("\n", errores));
				return;
			}
			
			//agregar cedula a la lista de existentes
			cedulasExistentes.Add(cedulaInput.Text);

			//agregar persona a la lista global
			personasCreadas.Add(nuevaPersona);

			//establecer relaciones familiares
			EstablecerPadres(nuevaPersona);

			// Agregar al arbol y mostrar en consola
			visualizador.AgregarPersonaYMostrar(nuevaPersona);

			//actualizar listas SOLO si se agregó alguien masculino o femenino
			if (nuevaPersona.GeneroPersona == Persona.Genero.Masculino ||
				nuevaPersona.GeneroPersona == Persona.Genero.Femenino)
			{
				ActualizarListaPadres();
			}

			GD.Print($"✓ {nuevaPersona.NombreCompleto} agregado al árbol genealógico");
			GD.Print($"Género: {nuevaPersona.GeneroPersona}");
			GD.Print($"Total personas: {personasCreadas.Count}");

			visualizador.MostrarResumen();
			
			LimpiarCampos();
		}
		catch (Exception ex)
		{
			MostrarError($"Error inesperado:\n{ex.Message}");
		}
	}

	private void OnVivoPressed()
	{
		if (vivoCheck.ButtonPressed)
		{
			muertoCheck.ButtonPressed = false;
			label11.Visible = false;
			fechaFallecimientoInput.Visible = false;
		}
	}

	private void OnMuertoPressed()
	{
		if (muertoCheck.ButtonPressed)
		{
			vivoCheck.ButtonPressed = false;
			label11.Visible = true;
			fechaFallecimientoInput.Visible = true;
		}
	}

	private void OnCancelarPressed()
	{
		LimpiarCampos();
	}

	private void LimpiarCampos()
	{
		nombreInput.Text = "";
		cedulaInput.Text = "";
		coordXInput.Text = "";
		coordYInput.Text = "";
		fechaInput.Text = "";
		edadInput.Text = "";
		fechaFallecimientoInput.Text = "";

		//resetear opciones
		opcionesGenero.Selected = 0;
		opcionesPadre.Selected = 0;
		opcionesMadre.Selected = 0;

		//resetear a vivo por defecto
		vivoCheck.ButtonPressed = true;
		muertoCheck.ButtonPressed = false;
		label11.Visible = false;
		fechaFallecimientoInput.Visible = false;
	}

	private void MostrarError(string mensaje)
	{
		dialogoError.DialogText = mensaje;
		dialogoError.PopupCentered();
		GD.PrintErr($"Error: {mensaje}");
	}

	private void ActualizarListaPadres()
	{
		//limpiar listas actuales
		opcionesPadre.Clear();
		opcionesMadre.Clear();

		opcionesPadre.AddItem("(ninguno)");
		opcionesMadre.AddItem("(ninguno)");

		//agregar personas existentes
		foreach (var persona in personasCreadas)
		{
			string item = $"{persona.NombreCompleto} ({persona.Cedula})";

			//solo hombres pueden ser padres
			if (persona.GeneroPersona == Persona.Genero.Masculino)
			{
				opcionesPadre.AddItem(item);
			}

			//solo mujeres pueden ser madres
			if (persona.GeneroPersona == Persona.Genero.Femenino)
			{
				opcionesMadre.AddItem(item);
			}
		}

		opcionesPadre.Selected = 0;
		opcionesMadre.Selected = 0;
	}

	private void EstablecerPadres(Persona nuevaPersona)
	{
		Persona padre = null;
		Persona madre = null;

		//buscar padre seleccionado
		if (opcionesPadre.Selected > 0)
		{
			int indicePadre = 0;
			foreach (var persona in personasCreadas)
			{
				if (persona.GeneroPersona == Persona.Genero.Masculino)
				{
					indicePadre++;
					if (indicePadre == opcionesPadre.Selected)
					{
						padre = persona;
						break;
					}
				}
			}
		}

		//buscar madre seleccionada
		if (opcionesMadre.Selected > 0)
		{
			int indiceMadre = 0;
			foreach (var persona in personasCreadas)
			{
				if (persona.GeneroPersona == Persona.Genero.Femenino)
				{
					indiceMadre++;
					if (indiceMadre == opcionesMadre.Selected)
					{
						madre = persona;
						break;
					}
				}
			}
		}

		//establecer las relaciones
		if (padre != null || madre != null)
		{
			nuevaPersona.EstablecerPadres(padre, madre);
		}
	}

	public static Arbol ObtenerArbol()
	{
		return visualizador.ObtenerArbol();
	}
}
