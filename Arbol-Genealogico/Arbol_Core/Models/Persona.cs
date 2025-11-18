using System;
using System.Collections.Generic;

namespace Arbol_Core.Models
{
	public partial class Persona
	{
		// Propiedades básicas
		public string Nombre { get; set; }
		public string Apellido { get; set; }
		public string Cedula { get; set; }
		public string NombreCompleto => $"{Nombre} {Apellido}";
		public string RutaFotografia { get; set; }
		public DateTime FechaNacimiento { get; set; }
		public int Edad { get; private set; }
		public bool EstaVivo { get; set; }
		public DateTime? FechaFallecimiento { get; set; }
		
		// Coordenadas de residencia
		public double Latitud { get; set; }
		public double Longitud { get; set; }
		
		// Relaciones familiares (estructura de árbol)
		public Persona Padre { get; set; }
		public Persona Madre { get; set; }
		public List<Persona> Hijos { get; set; }
		public Persona Conyuge { get; set; }
		
		// Información adicional
		public enum Genero { Masculino, Femenino}
		public Genero GeneroPersona { get; set; }

		// util para calcular la altura del arbol
		public int Generacion { get; set; }

		// tipo de persona (familiar o conyugue)
		public string TipoPersona { get; set; }


		// Constructor de la clase pesona
	
		public Persona(string nombre, string apellido, string cedula, DateTime fechaNacimiento,
					  int edad, double latitud, double longitud, string foto,
					  string tipoPersona)
		{
			Nombre = nombre;
			Apellido = apellido;
			Cedula = cedula;
			FechaNacimiento = fechaNacimiento;
			Edad = edad;
			Latitud = latitud;
			Longitud = longitud;
			TipoPersona = tipoPersona;
			RutaFotografia = foto;

			// Inicalizar variables importantes
			Hijos = new List<Persona>();
			EstaVivo = true;
			GeneroPersona = Genero.Femenino;
			Generacion = 0;
		}
		
		// Métodos de relaciones familiares
		public void AgregarHijo(Persona hijo)
		{
			if (hijo == null || Hijos.Contains(hijo))
				return;
				
			Hijos.Add(hijo);
			
			if (GeneroPersona == Genero.Masculino)
				hijo.Padre = this;
			else if (GeneroPersona == Genero.Femenino)
				hijo.Madre = this;
				
			hijo.Generacion = this.Generacion + 1;
		}
		
		public void RemoverHijo(Persona hijo)
		{
			if (hijo == null)
				return;
				
			Hijos.Remove(hijo);
			
			if (hijo.Padre == this)
				hijo.Padre = null;
			if (hijo.Madre == this)
				hijo.Madre = null;
		}
		
		public void EstablecerPadres(Persona padre, Persona madre)
		{
			if (padre != null)
			{
				Padre = padre;
				if (!padre.Hijos.Contains(this))
					padre.Hijos.Add(this);
			}
			
			if (madre != null)
			{
				Madre = madre;
				if (!madre.Hijos.Contains(this))
					madre.Hijos.Add(this);
			}
			
			ActualizarGeneracion();
		}
		
		private void ActualizarGeneracion()
		{
			int generacionPadre = Padre?.Generacion ?? -1;
			int generacionMadre = Madre?.Generacion ?? -1;
			Generacion = Math.Max(generacionPadre, generacionMadre) + 1;
		}
		
		public List<Persona> ObtenerHermanos()
		{
			var hermanos = new List<Persona>();
			var procesados = new HashSet<string>();
			
			if (Padre != null)
			{
				foreach (var hijo in Padre.Hijos)
				{
					if (hijo != this && !procesados.Contains(hijo.Cedula))
					{
						hermanos.Add(hijo);
						procesados.Add(hijo.Cedula);
					}
				}
			}
			
			if (Madre != null)
			{
				foreach (var hijo in Madre.Hijos)
				{
					if (hijo != this && !procesados.Contains(hijo.Cedula))
					{
						hermanos.Add(hijo);
						procesados.Add(hijo.Cedula);
					}
				}
			}
			
			return hermanos;
		}
		
		// Métodos de ubicación y distancia
		public double CalcularDistancia(Persona otra)
		{
			if (otra == null || !TieneCoordenadasValidas() || !otra.TieneCoordenadasValidas())
				return -1;

			double diferenciaLat = (otra.Latitud - this.Latitud) * 111;
			double diferenciaLon = (otra.Longitud - this.Longitud) * 111 * Math.Cos((this.Latitud + otra.Latitud) / 2 * Math.PI / 180);
			
			double distancia = Math.Sqrt(diferenciaLat * diferenciaLat + diferenciaLon * diferenciaLon);
			
			return distancia; 
		}
		
		public bool TieneCoordenadasValidas()
		{
			return Latitud >= -90 && Latitud <= 90 &&
				   Longitud >= -180 && Longitud <= 180;
		}
		
		// Validación de datos
		public bool EsValido()
		{
			return !string.IsNullOrWhiteSpace(Nombre) &&
				   !string.IsNullOrWhiteSpace(Apellido) &&
				   !string.IsNullOrWhiteSpace(Cedula) &&
				   FechaNacimiento != default(DateTime) &&
				   TieneCoordenadasValidas();
		}
		
		public List<string> ObtenerErroresValidacion()
		{
			var errores = new List<string>();
			
			if (string.IsNullOrWhiteSpace(Nombre))
				errores.Add("El nombre es requerido");
				
			if (string.IsNullOrWhiteSpace(Apellido))
				errores.Add("El apellido es requerido");
				
			if (string.IsNullOrWhiteSpace(Cedula))
				errores.Add("La cédula es requerida");
				
			if (FechaNacimiento == default(DateTime))
				errores.Add("La fecha de nacimiento es requerida");
				
			if (FechaNacimiento > DateTime.Today)
				errores.Add("La fecha de nacimiento no puede ser futura");
				
			if (!TieneCoordenadasValidas())
				errores.Add("Las coordenadas de residencia no son válidas");
				
			if (!EstaVivo && !FechaFallecimiento.HasValue)
				errores.Add("Debe especificar la fecha de fallecimiento");
				
			if (FechaFallecimiento.HasValue && FechaFallecimiento < FechaNacimiento)
				errores.Add("La fecha de fallecimiento no puede ser anterior al nacimiento");
				
			return errores;
		}
		
		// Métodos de información
		public override string ToString()
		{
			return $"{NombreCompleto} ({Cedula}) - {Edad} años";
		}
		
	}
}
