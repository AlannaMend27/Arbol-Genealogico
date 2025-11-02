using Godot;
using System;
using ArbolGenealogico.scripts.Models;

namespace ArbolGenealogico.scripts.Models
{
	public partial class testPersona : Node
	{
		public override void _Ready()
		{
			GD.Print("===== INICIANDO PRUEBAS DE CLASE PERSONA =====\n");
			
			PruebaCrearPersonas();
			PruebaRelacionesFamiliares();
			PruebaDistancias();
			PruebaValidaciones();
			PruebaMetodosAdicionales();
			
			GD.Print("\n===== PRUEBAS COMPLETADAS =====");
		}
		
		private void PruebaCrearPersonas()
		{
			GD.Print("--- Prueba 1: Crear Personas ---");
			
			var juan = new Persona(
				"Juan", "Pérez", "123456789",
				new DateTime(1980, 5, 15), 43,
				9.9281, -84.0907,
				"San José", "Costa Rica",
                "res://fotos/juan.jpg"
			);
			juan.GeneroPersona = Persona.Genero.Masculino;
			
			GD.Print($"Persona creada: {juan.NombreCompleto}");
			GD.Print($"Cédula: {juan.Cedula}");
			GD.Print($"Fecha de Nacimiento: {juan.FechaNacimiento.ToShortDateString()}");
			GD.Print($"Edad: {juan.Edad} años");
			GD.Print($"Ubicación: {juan.Ciudad}, {juan.Pais}");
			GD.Print($"Coordenadas: ({juan.Latitud}, {juan.Longitud})");
			GD.Print($"Estado: {(juan.EstaVivo ? "Vivo" : "Fallecido")}");
			GD.Print($"Generación: {juan.Generacion}");
			GD.Print($"Ruta foto: {juan.RutaFotografia}\n");
		}
		
		private void PruebaRelacionesFamiliares()
		{
			GD.Print("--- Prueba 2: Relaciones Familiares ---");
			
			// Crear abuelos (Generación 0)
			var abueloPaterno = new Persona(
				"Carlos", "Pérez", "111111",
				new DateTime(1950, 3, 10), 73,
				9.9, -84.1,
				"San José", "Costa Rica",
                "res://fotos/abuelo.jpg"
			);
			abueloPaterno.GeneroPersona = Persona.Genero.Masculino;
			abueloPaterno.Generacion = 0;
			
			var abuelaPaterna = new Persona(
				"Ana", "Gómez", "222222",
				new DateTime(1952, 7, 20), 71,
				9.9, -84.1,
				"San José", "Costa Rica",
                "res://fotos/abuela.jpg"
			);
			abuelaPaterna.GeneroPersona = Persona.Genero.Femenino;
			abuelaPaterna.Generacion = 0;
			
			// Crear padres (Generación 1)
			var padre = new Persona(
				"Juan", "Pérez", "333333",
				new DateTime(1980, 5, 15), 43,
				10.0, -84.2,
				"Heredia", "Costa Rica",
                "res://fotos/padre.jpg"
			);
			padre.GeneroPersona = Persona.Genero.Masculino;
			padre.EstablecerPadres(abueloPaterno, abuelaPaterna);
			
			var madre = new Persona(
				"María", "López", "444444",
				new DateTime(1982, 9, 25), 41,
				10.0, -84.2,
				"Heredia", "Costa Rica",
                "res://fotos/madre.jpg"
			);
			madre.GeneroPersona = Persona.Genero.Femenino;
			madre.Generacion = 1;
			
			// Crear hijos (Generación 2)
			var hijo1 = new Persona(
				"Pedro", "Pérez", "555555",
				new DateTime(2005, 1, 10), 18,
				10.1, -84.3,
				"Alajuela", "Costa Rica",
                "res://fotos/pedro.jpg"
			);
			hijo1.GeneroPersona = Persona.Genero.Masculino;
			
			var hijo2 = new Persona(
				"Sofía", "Pérez", "666666",
				new DateTime(2008, 6, 15), 15,
				10.1, -84.3,
				"Alajuela", "Costa Rica",
                "res://fotos/sofia.jpg"
			);
			hijo2.GeneroPersona = Persona.Genero.Femenino;
			
			// Establecer relaciones padre-hijos
			padre.AgregarHijo(hijo1);
			madre.AgregarHijo(hijo1);
			
			padre.AgregarHijo(hijo2);
			madre.AgregarHijo(hijo2);
			
			// Mostrar árbol genealógico
			GD.Print("Árbol Genealógico:");
			GD.Print("\nGeneración 0:");
			GD.Print($"  {abueloPaterno.NombreCompleto} (Gen {abueloPaterno.Generacion})");
			GD.Print($"  {abuelaPaterna.NombreCompleto} (Gen {abuelaPaterna.Generacion})");
			
			GD.Print("\nGeneración 1:");
			GD.Print($"  └─ {padre.NombreCompleto} (Gen {padre.Generacion})");
			GD.Print($"     Ciudad: {padre.Ciudad}");
			GD.Print($"  └─ {madre.NombreCompleto} (Gen {madre.Generacion})");
			GD.Print($"     Ciudad: {madre.Ciudad}");
			
			GD.Print("\nGeneración 2:");
			GD.Print($"     Hijos de {padre.Nombre}:");
			foreach (var hijo in padre.Hijos)
			{
				GD.Print($"     └─ {hijo.NombreCompleto} (Gen {hijo.Generacion}, {hijo.Edad} años)");
				GD.Print($"        Ciudad: {hijo.Ciudad}");
			}
			
			// Probar métodos de obtener relaciones
			GD.Print($"\n--- Relaciones de {hijo1.Nombre} ---");
			
			GD.Print("Hermanos:");
			var hermanos = hijo1.ObtenerHermanos();
			if (hermanos.Count > 0)
			{
				foreach (var hermano in hermanos)
				{
					GD.Print($"  • {hermano.NombreCompleto}");
				}
			}
			else
			{
				GD.Print("  (Sin hermanos)");
			}
			
			GD.Print("\nPadres:");
			if (hijo1.Padre != null)
				GD.Print($"  • Padre: {hijo1.Padre.NombreCompleto}");
			if (hijo1.Madre != null)
				GD.Print($"  • Madre: {hijo1.Madre.NombreCompleto}");
			
			GD.Print("\nAbuelos:");
			var abuelos = hijo1.ObtenerAbuelos();
			if (abuelos.Count > 0)
			{
				foreach (var abuelo in abuelos)
				{
					GD.Print($"  • {abuelo.NombreCompleto}");
				}
			}
			else
			{
				GD.Print("  (Sin abuelos registrados)");
			}
			
			GD.Print($"\n--- Relaciones de {abueloPaterno.Nombre} ---");
			GD.Print("Nietos:");
			var nietos = abueloPaterno.ObtenerNietos();
			if (nietos.Count > 0)
			{
				foreach (var nieto in nietos)
				{
					GD.Print($"  • {nieto.NombreCompleto}");
				}
			}
			else
			{
				GD.Print("  (Sin nietos registrados)");
			}
			GD.Print("");
		}
		
		private void PruebaDistancias()
		{
			GD.Print("--- Prueba 3: Cálculo de Distancias ---");
			
			// Persona 1: San José, Costa Rica
			var persona1 = new Persona(
				"Luis", "Rodríguez", "777777",
				new DateTime(1990, 4, 12), 33,
				9.9281, -84.0907,
				"San José", "Costa Rica",
                ""
			);
			
			// Persona 2: Nueva York, USA
			var persona2 = new Persona(
				"Andrea", "Martínez", "888888",
				new DateTime(1992, 11, 8), 31,
				40.7128, -74.0060,
				"Nueva York", "Estados Unidos",
                ""
			);
			
			// Persona 3: Heredia, Costa Rica (cerca de San José)
			var persona3 = new Persona(
				"Carlos", "Jiménez", "999999",
				new DateTime(1985, 2, 3), 38,
				10.0000, -84.1000,
				"Heredia", "Costa Rica",
                ""
			);
			
			// Calcular distancias
			double distancia1_2 = persona1.CalcularDistancia(persona2);
			double distancia1_3 = persona1.CalcularDistancia(persona3);
			double distancia2_3 = persona2.CalcularDistancia(persona3);
			
			GD.Print($"{persona1.NombreCompleto} ({persona1.Ciudad}, {persona1.Pais})");
			GD.Print($"  ↔ {persona2.NombreCompleto} ({persona2.Ciudad}, {persona2.Pais})");
			GD.Print($"  Distancia: {distancia1_2:F2} km\n");
			
			GD.Print($"{persona1.NombreCompleto} ({persona1.Ciudad}, {persona1.Pais})");
			GD.Print($"  ↔ {persona3.NombreCompleto} ({persona3.Ciudad}, {persona3.Pais})");
			GD.Print($"  Distancia: {distancia1_3:F2} km\n");
			
			GD.Print($"{persona2.NombreCompleto} ({persona2.Ciudad}, {persona2.Pais})");
			GD.Print($"  ↔ {persona3.NombreCompleto} ({persona3.Ciudad}, {persona3.Pais})");
			GD.Print($"  Distancia: {distancia2_3:F2} km\n");
			
			// Validar coordenadas
			GD.Print("Validación de coordenadas:");
			GD.Print($"  {persona1.Nombre}: {(persona1.TieneCoordenadasValidas() ? "✓ Válidas" : "✗ Inválidas")}");
			GD.Print($"  {persona2.Nombre}: {(persona2.TieneCoordenadasValidas() ? "✓ Válidas" : "✗ Inválidas")}");
			GD.Print($"  {persona3.Nombre}: {(persona3.TieneCoordenadasValidas() ? "✓ Válidas" : "✗ Inválidas")}");
			GD.Print("");
		}
		
		private void PruebaValidaciones()
		{
			GD.Print("--- Prueba 4: Validaciones ---\n");
			
			// Test 1: Persona completamente válida
			GD.Print("Test 1: Persona con todos los datos correctos");
			var personaValida = new Persona(
				"Roberto", "Sánchez", "101010",
				new DateTime(1995, 8, 20), 28,
				9.9281, -84.0907,
				"San José", "Costa Rica",
                "res://fotos/roberto.jpg"
			);
			
			var erroresValida = personaValida.ObtenerErroresValidacion();
			GD.Print($"  ¿Es válida? {personaValida.EsValido()}");
			GD.Print($"  Cantidad de errores: {erroresValida.Count}");
			if (erroresValida.Count == 0)
				GD.Print("  ✓ Sin errores - Persona completamente válida\n");
			
			// Test 2: Persona sin nombre
			GD.Print("Test 2: Persona sin nombre");
			var personaSinNombre = new Persona(
				"", "García", "202020",
				new DateTime(1990, 1, 1), 33,
				10.0, -84.0,
				"Heredia", "Costa Rica",
                ""
			);
			
			var erroresSinNombre = personaSinNombre.ObtenerErroresValidacion();
			GD.Print($"  Cantidad de errores: {erroresSinNombre.Count}");
			foreach (var error in erroresSinNombre)
			{
				GD.Print($"    ✗ {error}");
			}
			GD.Print("");
			
			// Test 3: Persona con coordenadas inválidas
			GD.Print("Test 3: Persona con coordenadas inválidas");
			var personaCoordInvalidas = new Persona(
				"Ana", "Martínez", "303030",
				new DateTime(1988, 6, 15), 35,
				200, -300,  // Coordenadas fuera de rango
				"Ciudad", "País",
                ""
			);
			
			var erroresCoord = personaCoordInvalidas.ObtenerErroresValidacion();
			GD.Print($"  Coordenadas: Lat={personaCoordInvalidas.Latitud}, Lon={personaCoordInvalidas.Longitud}");
			GD.Print($"  Cantidad de errores: {erroresCoord.Count}");
			foreach (var error in erroresCoord)
			{
				GD.Print($"    ✗ {error}");
			}
			GD.Print("");
			
			// Test 4: Persona con fecha de nacimiento futura
			GD.Print("Test 4: Persona con fecha de nacimiento futura");
			var personaFechaFutura = new Persona(
				"Carlos", "López", "404040",
				new DateTime(2030, 12, 31), 0,
				15.0, -90.0,
				"Ciudad", "País",
                ""
			);
			
			var erroresFecha = personaFechaFutura.ObtenerErroresValidacion();
			GD.Print($"  Fecha de nacimiento: {personaFechaFutura.FechaNacimiento.ToShortDateString()}");
			GD.Print($"  Fecha actual: {DateTime.Today.ToShortDateString()}");
			GD.Print($"  Cantidad de errores: {erroresFecha.Count}");
			foreach (var error in erroresFecha)
			{
				GD.Print($"    ✗ {error}");
			}
			GD.Print("");
			
			// Test 5: Persona sin múltiples datos
			GD.Print("Test 5: Persona con múltiples errores");
			var personaMultiplesErrores = new Persona(
				"", "",  "   ",  // Nombre, apellido y cédula vacíos
				default(DateTime), 0,  // Fecha por defecto
				250, -200,  // Coordenadas inválidas
				"", "",
                ""
			);
			
			var erroresMultiples = personaMultiplesErrores.ObtenerErroresValidacion();
			GD.Print($"  Cantidad de errores: {erroresMultiples.Count}");
			foreach (var error in erroresMultiples)
			{
				GD.Print($"    ✗ {error}");
			}
			GD.Print("");
		}
		
		private void PruebaMetodosAdicionales()
		{
			GD.Print("--- Prueba 5: Métodos Adicionales ---");
			
			// Crear familia
			var padre = new Persona(
				"Miguel", "Fernández", "100100",
				new DateTime(1978, 3, 5), 45,
				40.4168, -3.7038,
				"Madrid", "España",
                "res://fotos/miguel.jpg"
			);
			padre.GeneroPersona = Persona.Genero.Masculino;
			padre.Generacion = 0;
			
			var hijo = new Persona(
				"Lucas", "Fernández", "200200",
				new DateTime(2003, 8, 12), 20,
				41.3851, 2.1734,
				"Barcelona", "España",
                "res://fotos/lucas.jpg"
			);
			hijo.GeneroPersona = Persona.Genero.Masculino;
			
			// Probar ToString
			GD.Print("\nMétodo ToString():");
			GD.Print($"  {padre.ToString()}");
			GD.Print($"  {hijo.ToString()}");
			
			// Agregar hijo
			GD.Print($"\nAgregar hijo:");
			GD.Print($"  Hijos de {padre.Nombre} antes: {padre.Hijos.Count}");
			padre.AgregarHijo(hijo);
			GD.Print($"  Hijos de {padre.Nombre} después: {padre.Hijos.Count}");
			GD.Print($"  Generación de {hijo.Nombre}: {hijo.Generacion}");
			
			// Probar RemoverHijo
			GD.Print($"\nRemover hijo:");
			GD.Print($"  Hijos antes de remover: {padre.Hijos.Count}");
			padre.RemoverHijo(hijo);
			GD.Print($"  Hijos después de remover: {padre.Hijos.Count}");
			
			// Volver a agregar
			padre.AgregarHijo(hijo);
			GD.Print($"  Hijos después de volver a agregar: {padre.Hijos.Count}");
			
			// Probar ObtenerInformacionCompleta
			GD.Print($"\nInformación completa de {hijo.Nombre}:");
			GD.Print(hijo.ObtenerInformacionCompleta());
		}
	}
}
