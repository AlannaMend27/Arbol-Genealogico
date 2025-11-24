using System;
using System.Collections.Generic;
using System.Linq;
using Arbol_Core.Models;

namespace Arbol_Core.DataStructures
{
	/// <summary>
	/// Clase que representa la estructura del árbol genealógico y gestiona las relaciones familiares
	/// </summary>
	public partial class Arbol
	{
		// Diccionario para acceso rápido a personas por cédula
		private Dictionary<string, Persona> personasPorCedula;
		
		// Lista de todas las personas en el árbol
		private List<Persona> todasLasPersonas;
		
		// Propiedad para saber cuántas personas hay
		public int CantidadMiembros => todasLasPersonas.Count;
		
		/// <summary>
		/// Constructor de la clase
		/// </summary>
		public Arbol()
		{
			personasPorCedula = new Dictionary<string, Persona>();
			todasLasPersonas = new List<Persona>();
		}

		/// <summary>
		/// Agrega una persona al árbol genealógico con sus relaciones familiares
		/// </summary>
		/// <param name="nuevaPersona">La persona a agregar al árbol</param>
		/// <returns>True si la persona se agregó exitosamente, false si ya existe o es inválida</returns>
		public bool AgregarPersona(Persona nuevaPersona)
		{
			// Validación básica
			if (nuevaPersona == null)
			{
				return false;
			}

			if (!nuevaPersona.EsValido())
			{
				return false;
			}

			if (personasPorCedula.ContainsKey(nuevaPersona.Cedula))
			{
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

			return true;
		}


		/// <summary>
		/// Actualiza la generación de una persona basándose en la generación de sus padres
		/// </summary>
		/// <param name="persona">La persona cuya generación se actualizará</param>
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
		
		/// <summary>
		/// Actualiza la generación de un cónyuge para que coincida con la generación de su pareja
		/// </summary>
		/// <param name="persona">La persona cuya generación se actualizará basándose en su cónyuge</param>
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

		/// <summary>
		/// Busca una persona en el árbol genealógico por su número de cédula
		/// </summary>
		/// <param name="cedula">El número de cédula de la persona a buscar</param>
		/// <returns>La persona encontrada o null si no existe en el árbol</returns>		
		public Persona BuscarPorCedula(string cedula)
		{
			if (string.IsNullOrEmpty(cedula))
				return null;
				
			personasPorCedula.TryGetValue(cedula, out Persona persona);
			return persona;
		}
		
		/// <summary>
		/// Obtiene una copia de la lista con todas las personas registradas en el árbol genealógico
		/// </summary>
		/// <returns>Lista con todas las personas del árbol</returns>
		public List<Persona> ObtenerTodasLasPersonas()
		{
			return new List<Persona>(todasLasPersonas);
		}
		
		/// <summary>
		/// Obtiene una lista de las personas fundadoras del árbol genealógico (generación 0)
		/// </summary>
		/// <returns>Lista con las personas de la generación 0</returns>
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
		
		/// <summary>
		/// Elimina todas las personas del árbol genealógico, dejándolo vacío
		/// </summary>
		public void LimpiarArbol()
		{
			personasPorCedula.Clear();
			todasLasPersonas.Clear();
		}
	}
}