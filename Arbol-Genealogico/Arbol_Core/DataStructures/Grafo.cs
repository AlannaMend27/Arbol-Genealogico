using System;
using System.Collections.Generic;
using System.Linq;
using Arbol_Core.Models;

namespace Arbol_Core.DataStructures
{
	// nodo del grafo que representa a una persona y sus conexiones
	public class NodoGrafo
	{
		public Persona Persona { get; set; }
		public Dictionary<string, AristaGrafo> Aristas { get; set; }

		public NodoGrafo(Persona persona)
		{
			Persona = persona;
			Aristas = new Dictionary<string, AristaGrafo>();
		}
	}

	// arista del grafo que conecta dos personas con su distancia
	public class AristaGrafo
	{
		public Persona PersonaDestino { get; set; }
		public double Distancia { get; set; }

		public AristaGrafo(Persona destino, double distancia)
		{
			PersonaDestino = destino;
			Distancia = distancia;
		}
	}

	// conecta a todas las personas entre sí
	public class Grafo
	{
		private Dictionary<string, NodoGrafo> nodos;
		private static Grafo instancia;
		public int CantidadNodos => nodos.Count;

		public Grafo()
		{
			nodos = new Dictionary<string, NodoGrafo>();
		}

		public static Grafo ObtenerInstancia()
		{
			if (instancia == null)
			{
				instancia = new Grafo();
			}
			return instancia;
		}

		public void AgregarNodo(Persona persona)
		{
			if (persona == null || nodos.ContainsKey(persona.Cedula))
				return;

			nodos[persona.Cedula] = new NodoGrafo(persona);
		}

		// conectar todos los nodos entre sí
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

		// distancia entre dos personas específicas
		public double ObtenerDistancia(string cedula1, string cedula2)
		{
			if (!nodos.ContainsKey(cedula1) || !nodos.ContainsKey(cedula2))
				return -1;

			var nodo = nodos[cedula1];
			if (nodo.Aristas.ContainsKey(cedula2))
				return nodo.Aristas[cedula2].Distancia;

			return -1;
		}

		// distancias desde una persona hacia las demás
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

		// más lejos 
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

		// más cerca uno del otro
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

		// distancia promedio (en píxeles, igual a como se calcula en MapaUI)
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
		

		// mostrar en ui
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

		// obtener todas las personas del grafo
		public List<Persona> ObtenerTodasLasPersonas()
		{
			return nodos.Values.Select(n => n.Persona).ToList();
		}

		// verificar si una persona existe en el grafo
		public bool ExistePersona(string cedula)
		{
			return nodos.ContainsKey(cedula);
		}

		// obtener una persona específica por cédula
		public Persona ObtenerPersona(string cedula)
		{
			if (nodos.ContainsKey(cedula))
				return nodos[cedula].Persona;
			return null;
		}

		public void Limpiar()
		{
			nodos.Clear();
		}

	}
}
