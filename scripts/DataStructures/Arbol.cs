using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ArbolGenealogico.scripts.Models;

namespace ArbolGenealogico.scripts.DataStructures
{
	public partial class Arbol
	{
		// Diccionario para acceso rápido a personas por cédula
		private Dictionary<string, Persona> personasPorCedula;
		
		// Lista de todas las personas en el árbol
		private List<Persona> todasLasPersonas;
		
		// Propiedad para saber cuántas personas hay
		public int CantidadMiembros => todasLasPersonas.Count;
		
		// Constructor
		public Arbol()
		{
			personasPorCedula = new Dictionary<string, Persona>();
			todasLasPersonas = new List<Persona>();
		}

		// Agregar una persona al árbol
		// La persona ya debe tener asignados su Padre y Madre (pueden ser null si es fundador)
		public bool AgregarPersona(Persona nuevaPersona)
		{
			// Validación básica
			if (nuevaPersona == null)
			{
				GD.PrintErr("Error: Ingrese los datos de una persona");
				return false;
			}

			if (!nuevaPersona.EsValido())
			{
				GD.PrintErr("Error: verifique que ha colocado todos los datos de una persona");
				return false;
			}

			if (personasPorCedula.ContainsKey(nuevaPersona.Cedula))
			{
				GD.PrintErr($"Error: Ya existe una persona con la cédula {nuevaPersona.Cedula}");
				return false;
			}

			//Agregar generacion correspondiente
			if (nuevaPersona.TipoPersona == "familiar")
			{
				ActualizarGeneracion(nuevaPersona);
			}
            else
            {
                ActualizarGeneracionConyugue(nuevaPersona);
            }

			// Agregar al árbol
			personasPorCedula[nuevaPersona.Cedula] = nuevaPersona;
			todasLasPersonas.Add(nuevaPersona);

			// Si tiene padre, agregarlo como hijo del padre
			if (nuevaPersona.Padre != null)
			{
				if (!nuevaPersona.Padre.Hijos.Contains(nuevaPersona))
				{
					nuevaPersona.Padre.AgregarHijo(nuevaPersona);
				}
			}

			// Si tiene madre, agregarlo como hijo de la madre
			if (nuevaPersona.Madre != null)
			{
				if (!nuevaPersona.Madre.Hijos.Contains(nuevaPersona))
				{
					nuevaPersona.Madre.AgregarHijo(nuevaPersona);
				}
			}

			GD.Print($"Persona agregada: {nuevaPersona.NombreCompleto}");
			return true;
		}

		//Actualzar la generacion de la persona de acuerdo a sus padres
		private void ActualizarGeneracion(Persona persona)
		{
			if (persona == null)
				return;

			// Si no tiene padres, es fundador (generación 0)
			if (persona.Padre == null && persona.Madre == null)
			{
				persona.Generacion = 0;
				return;
			}

			// Calcular generación basándose en los padres
			int generacionPadre = persona.Padre?.Generacion ?? -1;
			int generacionMadre = persona.Madre?.Generacion ?? -1;

			// La generación es la mayor de los padres + 1
			persona.Generacion = Math.Max(generacionPadre, generacionMadre) + 1;
		}	
		
		//Actualizar la generacion de la persona de acuerdo a su conyugue (para tipo de persona conyugue)
		private void ActualizarGeneracionConyugue(Persona persona)
        {
			if (persona == null)
			{
				return;
			}

			if(persona.Conyuge == null)
            {
				return;
            }

			persona.Generacion = persona.Conyuge.Generacion; 
        }
		
		// Buscar una persona por su cédula
		public Persona BuscarPorCedula(string cedula)
		{
			if (string.IsNullOrEmpty(cedula))
				return null;
				
			personasPorCedula.TryGetValue(cedula, out Persona persona);
			return persona;
		}
		
		// Obtener todas las personas del árbol
		public List<Persona> ObtenerTodasLasPersonas()
		{
			return new List<Persona>(todasLasPersonas);
		}

		// Obtener los hermanos de una persona
		public List<Persona> ObtenerHermanos(Persona persona)
		{
			if (persona == null)
				return new List<Persona>();

			return persona.ObtenerHermanos();
		}
		
		// Obtener las personas fundadoras (generación 0)
		public List<Persona> ObtenerPersonasFundadoras()
		{
			var fundadores = new List<Persona>();
			
			foreach (var persona in todasLasPersonas)
			{
				if (persona.Generacion == 0)
				{
					fundadores.Add(persona);
				}
			}
			
			return fundadores;
		}
		
		// Limpiar todo el árbol
		public void LimpiarArbol()
		{
			personasPorCedula.Clear();
			todasLasPersonas.Clear();
			GD.Print("Árbol genealógico limpiado");
		}
	}
}