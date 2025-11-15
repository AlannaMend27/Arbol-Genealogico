using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using Arbol_Core.Models;
using Arbol_Core.DataStructures;

namespace ArbolGenealogico.scripts.UI
{
	public partial class VisualizadorArbol
	{
		// Instancia del árbol genealógico
		private Arbol_Core.DataStructures.Arbol arbol;
		
		// Constructor
		public VisualizadorArbol()
		{
			arbol = new Arbol_Core.DataStructures.Arbol();
		}
		
		// Agregar una persona al árbol y mostrar el estado
		public void AgregarPersonaYMostrar(Persona persona)
		{
			GD.Print("\n========================================");
			GD.Print($"Agregando persona: {persona.NombreCompleto}");
			GD.Print("========================================");
			
			bool exito = arbol.AgregarPersona(persona);
			
			if (exito)
			{
				GD.Print($"✓ {persona.NombreCompleto} agregado exitosamente");
				MostrarEstadoActual();
			}
			else
			{
				GD.PrintErr($"✗ Error al agregar a {persona.NombreCompleto}");
			}
		}
		
		// Mostrar el estado actual del árbol completo
		public void MostrarEstadoActual()
		{
			GD.Print("\n--- ESTADO DEL ÁRBOL ---");
			GD.Print($"Total de personas: {arbol.CantidadMiembros}");
			
			if (arbol.CantidadMiembros == 0)
			{
				GD.Print("El árbol está vacío");
				return;
			}
			
			GD.Print("\n--- ESTRUCTURA DEL ÁRBOL ---");
			MostrarArbolVisual();
			
			GD.Print("\n--- DETALLES DE CADA PERSONA ---");
			MostrarDetallesPersonas();
		}
		
		// Mostrar el árbol de forma visual en consola
		private void MostrarArbolVisual()
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
			
			// Mostrar por generación
			var generacionesOrdenadas = new List<int>(porGeneracion.Keys);
			generacionesOrdenadas.Sort();
			
			foreach (var gen in generacionesOrdenadas)
			{
				GD.Print($"\nGeneración {gen}:");
				foreach (var persona in porGeneracion[gen])
				{
					string prefijo = new string(' ', gen * 4);
					string icono = persona.EstaVivo ? "➜" : "✝";
					string info = $"{prefijo}{icono} {persona.NombreCompleto} ({persona.Cedula})";
					
					if (persona.Conyuge != null)
					{
						info += $" ❤ {persona.Conyuge.NombreCompleto}";
					}
					
					GD.Print(info);
					
					// Mostrar hijos
					if (persona.Hijos.Count > 0)
					{
						string prefijoHijos = new string(' ', (gen + 1) * 4);
						GD.Print($"{prefijoHijos}└─ Hijos: {persona.Hijos.Count}");
					}
				}
			}
		}
		
		// Mostrar detalles de todas las personas
		private void MostrarDetallesPersonas()
		{
			var todasLasPersonas = arbol.ObtenerTodasLasPersonas();
			
			foreach (var persona in todasLasPersonas)
			{
				GD.Print($"\n☆ {persona.NombreCompleto}");
				GD.Print($"   Cédula: {persona.Cedula}");
				GD.Print($"   Género: {persona.GeneroPersona}");
				GD.Print($"   Edad: {persona.Edad} años");
				GD.Print($"   Estado: {(persona.EstaVivo ? "Vivo" : "Fallecido")}");
				GD.Print($"   Generación: {persona.Generacion}");
				GD.Print($"   Ubicación: ({persona.Latitud}, {persona.Longitud})");
				
				// Mostrar relaciones familiares
				if (persona.Padre != null)
					GD.Print($"   Padre: {persona.Padre.NombreCompleto}");
				
				if (persona.Madre != null)
					GD.Print($"   Madre: {persona.Madre.NombreCompleto}");
				
				if (persona.Conyuge != null)
					GD.Print($"   Cónyuge: {persona.Conyuge.NombreCompleto}");
				
				if (persona.Hijos.Count > 0)
				{
					GD.Print($"   Hijos ({persona.Hijos.Count}):");
					foreach (var hijo in persona.Hijos)
					{
						GD.Print($"      - {hijo.NombreCompleto}");
					}
				}
				
				var hermanos = persona.ObtenerHermanos();
				if (hermanos.Count > 0)
				{
					GD.Print($"   Hermanos ({hermanos.Count}):");
					foreach (var hermano in hermanos)
					{
						GD.Print($"      - {hermano.NombreCompleto}");
					}
				}
			}
		}
		
		// Mostrar un resumen
		public void MostrarResumen()
		{
			GD.Print("\n┌─────────────────────────────────────┐");
			GD.Print("│      RESUMEN DEL ÁRBOL              │");
			GD.Print("└─────────────────────────────────────┘");
			
			var todasLasPersonas = arbol.ObtenerTodasLasPersonas();
			
			int vivos = 0;
			int fallecidos = 0;
			int hombres = 0;
			int mujeres = 0;
			
			foreach (var persona in todasLasPersonas)
			{
				if (persona.EstaVivo) vivos++;
				else fallecidos++;
				
				if (persona.GeneroPersona == Persona.Genero.Masculino) hombres++;
				if (persona.GeneroPersona == Persona.Genero.Femenino) mujeres++;
			}
			
			GD.Print($"Total personas: {arbol.CantidadMiembros}");
			GD.Print($"Vivos: {vivos} | Fallecidos: {fallecidos}");
			GD.Print($"Hombres: {hombres} | Mujeres: {mujeres}");
			
			// Mostrar fundadores
			var fundadores = arbol.ObtenerPersonasFundadoras();
			if (fundadores.Count > 0)
			{
				GD.Print("\nFundadores:");
				foreach (var fundador in fundadores)
				{
					GD.Print($"  - {fundador.NombreCompleto}");
				}
			}
		}
		
		// Obtener el árbol
		public Arbol_Core.DataStructures.Arbol ObtenerArbol()
		{
			return arbol;
		}
	}
}