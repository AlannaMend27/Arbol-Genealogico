using Godot;
using System;
using System.Collections.Generic;
using ArbolGenealogico.scripts.Models;
using ArbolGenealogico.scripts.DataStructures;
using ArbolGenealogico.scripts.UI;

//FALTA VALIDAR NUMEROS PARA LAS COORDENADAS    
public partial class AgregarPersona : Node2D
{
    private static List<string> cedulasExistentes = new List<string>();
    private static List<Persona> personasCreadas = new List<Persona>();

    private static List<Persona> hombres = new List<Persona>();
    private static List<Persona> mujeres = new List<Persona>();
    

    // Prueba para visualizar la construccion el arbol en consola (futura implementacion en interfaz)
    private static VisualizadorArbol visualizador = new VisualizadorArbol();

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

    private AcceptDialog dialogoError;

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

        dialogoError = new AcceptDialog();
        dialogoError.Title = "Error";
        dialogoError.OkButtonText = "Entendido";
        AddChild(dialogoError);

        //conectar botones a sus funciones
        aceptarBtn.Pressed += OnAceptarPressed;
        cancelarBtn.Pressed += OnCancelarPressed;

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
    }

    private void OnAceptarPressed()
    {
        try
        {
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
            if (cedulaInput.Text.Length < 9 || cedulaInput.Text.Length > 12)
            {
                MostrarError("La cédula no tiene la extensión adecuada");
                return;
            }

            //cedula unica
            if (cedulasExistentes.Contains(cedulaInput.Text))
            {
                MostrarError("Esta cédula ya está registrada");
                return;
            }

            if (string.IsNullOrWhiteSpace(cedulaInput.Text))
            {
                MostrarError("La cédula es requerida");
                return;
            }

            // Validar que si es Cónyuge, debe seleccionar a alguien
            if (tipoDePersona.Selected == 1) // 1 = Cónyuge
            {
                if (conyugue.Selected == 0) // Si no hay conyuge seleccionado
                {
                    MostrarError("Debe seleccionar un cónyuge de la lista para agregar a esta persona.\n\n" +
                    "Si no aparece ningún cónyuge disponible, primero debe agregar \n" +
                    "al familiar con el cual desea establecer la relación conyugal.");
                    return;
                }
            }

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
            DateTime fechaNac;
            if (!DateTime.TryParse(fechaInput.Text, out fechaNac))
            {
                MostrarError("Formato de fecha inválido.\nUse: dd/MM/yyyy\nEjemplo: 15/05/1990");
                return;
            }
            if (fechaNac > DateTime.Today)
            {
                MostrarError("La fecha de nacimiento no puede ser futura");
                return;
            }

            //Verificar que se ingreso genero
            if (opcionesGenero.Selected == 0)
            {
                MostrarError("Debe seleccionar un género");
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

            // Validad que la edad sea coherente con la de los padres (En caso de que la persona sea familiar)
            if (tipoDePersona.Selected == 0)
            {
                Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
                Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);

                if (padre != null && edad >= padre.Edad)
                {
                    MostrarError($"La edad ingresada ({edad} años) no puede ser mayor o igual que la del padre.\n" +
                                $"{padre.NombreCompleto} tiene {padre.Edad} años.");
                    return;
                }

                if (madre != null && edad >= madre.Edad)
                {
                    MostrarError($"Error: La edad ingresada ({edad} años) no puede ser mayor o igual que la de la madre.\n" +
                                $"{madre.NombreCompleto} tiene {madre.Edad} años.");
                    return;
                }
            }

            int edadCalculada = DateTime.Today.Year - fechaNac.Year;
            if (fechaNac.Date > DateTime.Today.AddYears(-edadCalculada))
                edadCalculada--;

            if (edad > edadCalculada)
            {
                MostrarError($"La edad no coincide con la fecha de nacimiento.\nEdad calculada: {edadCalculada} años");
                return;
            }

            //Verificar que ambos padres sean conyugues
            if ( !VerificarPadresConyugues())
            {
                return;
            }

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

            //Crear la persona dependiendo de su tipo
            Persona nuevaPersona;

            if (tipoDePersona.Selected == 1)
            {
                nuevaPersona = new Persona(
                nombre,
                apellido,
                cedulaInput.Text,
                fechaNac,
                edad,
                latitud,
                longitud,
                "", //foto
                "conyugue"
                );
            }
            else
            {
                nuevaPersona = new Persona(
                nombre,
                apellido,
                cedulaInput.Text,
                fechaNac,
                edad,
                latitud,
                longitud,
                "", //foto
                "familiar"
                );
            }
            

            //configurar estado y fecha de fallecimiento
            nuevaPersona.EstaVivo = vivoCheck.ButtonPressed;
            if (fechaFallecimiento.HasValue)
            {
                nuevaPersona.FechaFallecimiento = fechaFallecimiento;
            }

            if (opcionesGenero.Selected == 1)
            {
                nuevaPersona.GeneroPersona = Persona.Genero.Masculino;
            }
            else if (opcionesGenero.Selected == 2)
            {
                nuevaPersona.GeneroPersona = Persona.Genero.Femenino;
            }

            if (!nuevaPersona.EsValido())
            {
                var errores = nuevaPersona.ObtenerErroresValidacion();
                MostrarError(string.Join("\n", errores));
                return;
            }

            cedulasExistentes.Add(cedulaInput.Text);
            personasCreadas.Add(nuevaPersona);

            //filtrar por género
            if (nuevaPersona.GeneroPersona == Persona.Genero.Masculino)
                hombres.Add(nuevaPersona);
            else if (nuevaPersona.GeneroPersona == Persona.Genero.Femenino)
                mujeres.Add(nuevaPersona);

            //establecer relaciones familiares
            if (tipoDePersona.Selected == 0)
            {
                EstablecerPadres(nuevaPersona);
            }
            else
            {
                EstablecerConyuge(nuevaPersona);
            }

            // Agregar al arbol y mostrar en consola
            visualizador.AgregarPersonaYMostrar(nuevaPersona);

            //actualizar listas si se agregó alguien masculino o femenino
            if (nuevaPersona.GeneroPersona == Persona.Genero.Masculino ||
                nuevaPersona.GeneroPersona == Persona.Genero.Femenino)
            {
                ActualizarTodasLasListas();
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

    private void OnTipoPersonaChanged(long index)
    {
        ConfigurarVisibilidadCampos();
        ActualizarListaConyuges();
    }

    private void OnGeneroChanged(long index)
    {
        ActualizarListaConyuges();
    }

    private void ConfigurarVisibilidadCampos()
    {
        bool esFamiliar = tipoDePersona.Selected == 0; //0 es familiar y 1 es el cónyugue

        //mostrar u ocultar campos de padres
        label12.Visible = esFamiliar;
        opcionesMadre.Visible = esFamiliar;
        label13.Visible = esFamiliar;
        opcionesPadre.Visible = esFamiliar;

        //mostrar u ocultar campos de cónyuge
        label16.Visible = !esFamiliar;
        conyugue.Visible = !esFamiliar;
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

    private void MostrarError(string mensaje)
    {
        dialogoError.DialogText = mensaje;
        dialogoError.PopupCentered();
        GD.PrintErr($"Error: {mensaje}");
    }

    private void ActualizarTodasLasListas()
    {
        ActualizarListaPadres();
        ActualizarListaConyuges();
    }

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

    private void ActualizarListaConyuges()
    {
        conyugue.Clear();
        conyugue.AddItem("(ninguno)");

        //obtener lista según género seleccionado
        List<Persona> personasDisponibles = ObtenerPersonasParaConyuge();

        //agregar solo personas que NO tienen cónyuge
        foreach (var persona in personasDisponibles)
        {
            if (persona.Conyuge == null)
            {
                conyugue.AddItem($"{persona.NombreCompleto} ({persona.Cedula})");
            }
        }

        conyugue.Selected = 0;
    }

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

    private Boolean VerificarPadresConyugues()
    {
        Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
        Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);

        // Si ambos padres están seleccionados, validar que sean cónyuges
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

    private void EstablecerPadres(Persona nuevaPersona)
    {
        Persona padre = BuscarPersonaEnLista(hombres, opcionesPadre.Selected);
        Persona madre = BuscarPersonaEnLista(mujeres, opcionesMadre.Selected);

        if (padre != null || madre != null)
        {
            nuevaPersona.EstablecerPadres(padre, madre);
        }
    }

    private void EstablecerConyuge(Persona nuevaPersona)
    {
        string textoSeleccionado = conyugue.GetItemText(conyugue.Selected);
        
        int inicioParentesis = textoSeleccionado.LastIndexOf('(');
        int finParentesis = textoSeleccionado.LastIndexOf(')');

        string cedulaConyugue = textoSeleccionado.Substring(
            inicioParentesis + 1, 
            finParentesis - inicioParentesis - 1
        ).Trim();

        // Encontrar conyugue por medio de lista que contiene a todas las personas creadas
        Persona conyugeSeleccionado = personasCreadas.Find(p => p.Cedula == cedulaConyugue);

        if (conyugeSeleccionado == null)
        {
            GD.PrintErr($"Error: No se encontró persona con cédula {cedulaConyugue}");
            return;
        }

        nuevaPersona.Conyuge = conyugeSeleccionado;
        conyugeSeleccionado.Conyuge = nuevaPersona;
    }

    private Persona BuscarPersonaEnLista(List<Persona> lista, int indiceSeleccionado)
    {
        if (indiceSeleccionado <= 0 || indiceSeleccionado > lista.Count)
            return null;

        return lista[indiceSeleccionado - 1]; //-1 porque el índice 0 es "(ninguno)"
    }

    public static Arbol ObtenerArbol()
    {
        return visualizador.ObtenerArbol();
    }
}