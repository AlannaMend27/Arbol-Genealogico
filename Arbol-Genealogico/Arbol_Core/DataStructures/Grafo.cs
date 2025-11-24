using System;
using System.Collections.Generic;
using System.Linq;
using Arbol_Core.Models;

namespace Arbol_Core.DataStructures
{
	// Clase que representa un nodo en el grafo, asociado a una persona
	public class NodoGrafo
	{
		public Persona Persona { get; set; }
		public Dictionary<string, AristaGrafo> Aristas { get; set; }

		/// <summary>
		/// Inicializa un nuevo nodo del grafo asociado a una persona
		/// </summary>
		/// <param name="persona">La persona que representará este nodo</param>
		public NodoGrafo(Persona persona)
		{
			Persona = persona;
			Aristas = new Dictionary<string, AristaGrafo>();
		}
	}

	// Clase que representa una arista en el grafo, conectando dos personas
	public class AristaGrafo
	{
		public Persona PersonaDestino { get; set; }
		public double Distancia { get; set; }

		/// <summary>
		/// Inicializa una nueva arista entre dos personas con una distancia específica
		/// </summary>
		/// <param name="destino">La persona destino de esta arista</param>
		/// <param name="distancia">La distancia entre las dos personas</param>
		public AristaGrafo(Persona destino, double distancia)
		{
			PersonaDestino = destino;
			Distancia = distancia;
		}
	}

	// Clase que representa el grafo, conectando todas las personas entre sí
	public class Grafo
	{
		private Dictionary<string, NodoGrafo> nodos;
		private static Grafo instancia;
		public int CantidadNodos => nodos.Count;

		/// <summary>
		/// Inicializa una nueva instancia del grafo vacío
		/// </summary>
		public Grafo()
		{
			nodos = new Dictionary<string, NodoGrafo>();
		}

		/// <summary>
		/// Obtiene la instancia única del grafo utilizando el patrón singleton
		/// </summary>
		/// <returns>La instancia única del grafo</returns>
		public static Grafo ObtenerInstancia()
		{
			if (instancia == null)
			{
				instancia = new Grafo();
			}
			return instancia;
		}

		/// <summary>
		/// Agrega un nodo al grafo asociado a una persona
		/// </summary>
		/// <param name="persona">La persona a agregar como nodo en el grafo</param>
		public void AgregarNodo(Persona persona)
		{
			if (persona == null || nodos.ContainsKey(persona.Cedula))
				return;

			nodos[persona.Cedula] = new NodoGrafo(persona);
		}

		/// <summary>
		/// Construye las aristas entre todos los nodos del grafo calculando las distancias entre cada par de personas
		/// </summary>
		public void ConstruirAristas()
		{
			var listaPersonas = nodos.Values.Select(n => n.Persona).ToList();

			// para cada nodo se crean aristas hacia todos los demás
			foreach (var nodo in nodos.Values)
			{
				nodo.Aristas.Clear();
				foreach (var otraPersona in listaPersonas)
				{
					if (nodo.Persona.Cedula != otraPersona.Cedula)
					{
						double distancia = nodo.Persona.CalcularDistancia(otraPersona);
						nodo.Aristas[otraPersona.Cedula] = new AristaGrafo(otraPersona, distancia);
					}
				}
			}
		}

		/// <summary>
		/// Obtiene la distancia entre dos personas específicas del grafo
		/// </summary>
		/// <param name="cedula1">Cédula de la primera persona</param>
		/// <param name="cedula2">Cédula de la segunda persona</param>
		/// <returns>La distancia entre ambas personas o -1 si alguna no existe</returns>
		public double ObtenerDistancia(string cedula1, string cedula2)
		{
			if (!nodos.ContainsKey(cedula1) || !nodos.ContainsKey(cedula2))
				return -1;

			var nodo = nodos[cedula1];
			if (nodo.Aristas.ContainsKey(cedula2))
				return nodo.Aristas[cedula2].Distancia;

			return -1;
		}

		/// <summary>
		/// Obtiene un diccionario con las distancias desde una persona hacia todas las demás
		/// </summary>
		/// <param name="cedula">Cédula de la persona origen</param>
		/// <returns>Diccionario con cada persona y su distancia desde el origen</returns>
		public Dictionary<Persona, double> ObtenerDistanciasDesde(string cedula)
		{
			var distancias = new Dictionary<Persona, double>();

			if (!nodos.ContainsKey(cedula))
				return distancias;

			var nodo = nodos[cedula];
			foreach (var arista in nodo.Aristas.Values)
			{
				distancias[arista.PersonaDestino] = arista.Distancia;
			}

			return distancias;
		}

		/// <summary>
		/// Obtiene el par de personas que se encuentran más lejanas entre sí en el grafo
		/// </summary>
		/// <returns>Tupla con las dos personas más lejanas y la distancia entre ellas</returns>
		public (Persona persona1, Persona persona2, double distancia) ObtenerParMasLejano()
		{
			Persona p1 = null, p2 = null;
			double maxDistancia = -1;

			var lista = nodos.Values.Select(n => n.Persona).ToList();
			for (int i = 0; i < lista.Count; i++)
			{
				for (int j = i + 1; j < lista.Count; j++)
				{
					double distancia = lista[i].CalcularDistancia(lista[j]);
					if (distancia > maxDistancia)
					{
						maxDistancia = distancia;
						p1 = lista[i];
						p2 = lista[j];
					}
				}
			}

			return (p1, p2, maxDistancia);
		}

		/// <summary>
		/// Obtiene el par de personas que se encuentran más cercanas entre sí en el grafo
		/// </summary>
		/// <returns>Tupla con las dos personas más cercanas y la distancia entre ellas</returns>
		public (Persona persona1, Persona persona2, double distancia) ObtenerParMasCercano()
		{
			Persona p1 = null, p2 = null;
			double minDistancia = double.MaxValue;

			var lista = nodos.Values.Select(n => n.Persona).ToList();
			for (int i = 0; i < lista.Count; i++)
			{
				for (int j = i + 1; j < lista.Count; j++)
				{
					double distancia = lista[i].CalcularDistancia(lista[j]);
					if (distancia < minDistancia)
					{
						minDistancia = distancia;
						p1 = lista[i];
						p2 = lista[j];
					}
				}
			}

			return (p1, p2, minDistancia == double.MaxValue ? 0 : minDistancia);
		}

		/// <summary>
		/// Calcula la distancia promedio entre todas las personas del grafo
		/// </summary>
		/// <returns>La distancia promedio o 0 si hay menos de dos personas</returns>
		public double CalcularDistanciaPromedio()
		{
			if (nodos.Count < 2)
				return 0;

			// no duplicar A<->B - calcular distancia entre coordenadas (píxeles)
			var lista = nodos.Values.Select(n => n.Persona).ToList();
			double suma = 0;
			int pares = 0;

			for (int i = 0; i < lista.Count; i++)
			{
				for (int j = i + 1; j < lista.Count; j++)
				{
					//llamar al metodo persona
					double distancia = lista[i].CalcularDistancia(lista[j]);
					
					if (distancia >= 0) // validar que sea válida
					{
						suma += distancia;
						pares++;
					}
				}
			}
			return pares > 0 ? (suma / pares) : 0;
		}
		

		/// <summary>
		/// Obtiene valores formateados de estadísticas del grafo para mostrar en la interfaz de usuario
		/// </summary>
		/// <returns>Tupla con la distancia promedio, el par más lejano y el par más cercano como cadenas formateadas</returns>
		public (string distancia, string lejos, string cerca) ObtenerValoresUI()
		{
			var masLejanos = ObtenerParMasLejano();
			var masCercanos = ObtenerParMasCercano();

			string distanciaTexto = $"{CalcularDistanciaPromedio():F2} km";

			// Si no hay pares no devolvemos "N/A" — devolvemos cadena vacía
			string lejosTexto = (masLejanos.persona1 != null && masLejanos.persona2 != null)
				? $"{masLejanos.persona1.NombreCompleto} ↔ {masLejanos.persona2.NombreCompleto}"
				: string.Empty;

			string cercaTexto = (masCercanos.persona1 != null && masCercanos.persona2 != null)
				? $"{masCercanos.persona1.NombreCompleto} ↔ {masCercanos.persona2.NombreCompleto}"
				: string.Empty;

			return (distanciaTexto, lejosTexto, cercaTexto);
		}

		/// <summary>
		/// Obtiene una lista con todas las personas presentes en el grafo
		/// </summary>
		/// <returns>Lista de todas las personas</returns>
		public List<Persona> ObtenerTodasLasPersonas()
		{
			return nodos.Values.Select(n => n.Persona).ToList();
		}

		/// <summary>
		/// Verifica si una persona existe en el grafo mediante su cédula
		/// </summary>
		/// <param name="cedula">Cédula de la persona a verificar</param>
		/// <returns>True si la persona existe, false en caso contrario</returns>
		public bool ExistePersona(string cedula)
		{
			return nodos.ContainsKey(cedula);
		}

		/// <summary>
		/// Obtiene una persona específica del grafo por su cédula
		/// </summary>
		/// <param name="cedula">Cédula de la persona a obtener</param>
		/// <returns>La persona encontrada o null si no existe</returns>
		public Persona ObtenerPersona(string cedula)
		{
			if (nodos.ContainsKey(cedula))
				return nodos[cedula].Persona;
			return null;
		}

		/// <summary>
		/// Elimina todos los nodos del grafo, dejándolo vacío
		/// </summary>
		public void Limpiar()
		{
			nodos.Clear();
		}

	}
}