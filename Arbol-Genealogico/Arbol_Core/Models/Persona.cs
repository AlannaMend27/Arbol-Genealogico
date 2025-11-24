using System;
using System.Collections.Generic;

namespace Arbol_Core.Models
{
	/// <summary>
	/// Clase que representa a una persona en el árbol genealógico
	/// </summary>
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


		/// <summary>
		/// Constructor que inicializa las propiedades de la persona
		/// </summary>
		/// <param name="nombre">El nombre de la persona</param>
		/// <param name="apellido">El apellido de la persona</param>
		/// <param name="cedula">La cédula de la persona</param>
		/// <param name="fechaNacimiento">La fecha de nacimiento de la persona</param>
		/// <param name="edad">La edad de la persona</param>
		/// <param name="latitud">La latitud de la residencia de la persona</param>
		/// <param name="longitud">La longitud de la residencia de la persona</param>
		/// <param name="foto">La ruta de la fotografía de la persona</param>
		/// <param name="tipoPersona">El tipo de persona (familiar o conyugue)</param>
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
		
		/// <summary>
		/// Agrega un hijo a la lista de hijos de la persona
		/// </summary>
		/// <param name="hijo">El hijo a agregar</param>
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
		
		/// <summary>
		/// Remueve un hijo de la lista de hijos de la persona
		/// </summary>
		/// <param name="hijo">El hijo a remover</param>
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
		
		/// <summary>
		/// Establece los padres de la persona y actualiza la generación
		/// </summary>
		/// <param name="padre">El padre a establecer</param>
		/// <param name="madre">La madre a establecer</param>
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
		
		/// <summary>
		/// Actualiza la generación de la persona basada en sus padres
		/// </summary>
		private void ActualizarGeneracion()
		{
			int generacionPadre = Padre?.Generacion ?? -1;
			int generacionMadre = Madre?.Generacion ?? -1;
			Generacion = Math.Max(generacionPadre, generacionMadre) + 1;
		}
		
		/// <summary>
		/// Obtiene la lista de hermanos de la persona
		/// </summary>
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
		
		/// <summary>
		/// Calcula la distancia entre la persona y otra persona
		/// </summary>
		/// <param name="otra">La otra persona</param>
		/// <returns>La distancia entre las dos personas</returns>
		public double CalcularDistancia(Persona otra)
		{
			if (otra == null || !TieneCoordenadasValidas() || !otra.TieneCoordenadasValidas())
				return -1;

			double diferenciaLat = (otra.Latitud - this.Latitud) * 111;
			double diferenciaLon = (otra.Longitud - this.Longitud) * 111 * Math.Cos((this.Latitud + otra.Latitud) / 2 * Math.PI / 180);
			
			double distancia = Math.Sqrt(diferenciaLat * diferenciaLat + diferenciaLon * diferenciaLon);
			
			return distancia; 
		}
		
		/// <summary>
		/// Verifica si la persona tiene datos válidos
		/// </summary>
		public bool EsValido()
		{
			return !string.IsNullOrWhiteSpace(Nombre) &&
				   !string.IsNullOrWhiteSpace(Apellido) &&
				   !string.IsNullOrWhiteSpace(Cedula) &&
				   FechaNacimiento != default(DateTime) &&
				   TieneCoordenadasValidas();
		}

		/// <summary>
		/// Verifica si las coordenadas de la persona son válidas
		/// </summary>
		public bool TieneCoordenadasValidas()
		{

		if (Latitud < -90 || Latitud > 90 || Longitud < -180 || Longitud > 180)
			return false;

		return EstaEnTierra();

		}

		/// <summary>
		/// Verifica si las coordenadas de la persona están en tierra
		/// </summary>
		private bool EstaEnTierra()
		{

			// oceanos interiores 

			// Golfo de México
			if (Latitud >= 18 && Latitud <= 31 && Longitud >= -98 && Longitud <= -81)
				return false;

			// Mar Caribe
			if (Latitud >= 10 && Latitud <= 20 && Longitud >= -80 && Longitud <= -60)
				return false;

			// Mar Mediterráneo
			if (Latitud >= 30 && Latitud <= 46 && Longitud >= 0 && Longitud <= 36)
				return false;

			// Mar Rojo
			if (Latitud >= 10 && Latitud <= 30 && Longitud >= 32 && Longitud <= 44)
				return false;

			// Océano Índico central (entre África y Australia)
			if (Latitud >= -10 && Latitud <= 10 && Longitud >= 52 && Longitud <= 113)
				return false;

			//continentes

			// América del Norte
			if (Latitud >= 15 && Latitud <= 72 && Longitud >= -168 && Longitud <= -52)
				return true;

			// América Central
			if (Latitud >= 7 && Latitud <= 18 && Longitud >= -92 && Longitud <= -77)
				return true;

			// América del Sur
			if (Latitud >= -56 && Latitud <= 13 && Longitud >= -81 && Longitud <= -34)
				return true;

			// Europa
			if (Latitud >= 36 && Latitud <= 71 && Longitud >= -10 && Longitud <= 40)
				return true;

			// África
			if (Latitud >= -35 && Latitud <= 37 && Longitud >= -18 && Longitud <= 52)
				return true;

			// Asia
			if (Latitud >= -10 && Latitud <= 77 && Longitud >= 26 && Longitud <= 180)
				return true;

			// Oceanía
			if (Latitud >= -47 && Latitud <= -10 && Longitud >= 113 && Longitud <= 179)
				return true;

			// Nueva Zelanda
			if (Latitud >= -47 && Latitud <= -34 && Longitud >= 166 && Longitud <= 179)
				return true;

			// Está en agua
			return false;
		}
			
		/// <summary>
		/// Obtiene una lista de errores de validación para la persona
		/// </summary>
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
		
		/// <summary>
		/// Devuelve una representación en cadena de la persona
		/// </summary>
		public override string ToString()
		{
			return $"{NombreCompleto} ({Cedula}) - {Edad} años";
		}
		
	}
}
