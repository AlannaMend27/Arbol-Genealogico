using Godot;
using System;
using System.Collections.Generic;

namespace ArbolGenealogico
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
		public string Ciudad { get; set; }
		public string Pais { get; set; }
		
		// Relaciones familiares (estructura de árbol)
		public Persona Padre { get; set; }
		public Persona Madre { get; set; }
		public List<Persona> Hijos { get; set; }
		public Persona Conyuge { get; set; }
		
		// Información adicional
		public enum Genero { Masculino, Femenino, Otro, NoEspecificado }
		public Genero GeneroPersona { get; set; }

		// Esta propiedad es util para saber cual es la medida del arbol
		public int Generacion { get; set; }

		// Constructor de la clase pesona
	
		public Persona(string nombre, string apellido, string cedula, DateTime fechaNacimiento,
					  int edad, double latitud, double longitud, string ciudad, string pais,
					  string rutaFoto)
		{
			Nombre = nombre;
			Apellido = apellido;
			Cedula = cedula;
			FechaNacimiento = fechaNacimiento;
			Edad = edad;
			Latitud = latitud;
			Longitud = longitud;
			Ciudad = ciudad;
			Pais = pais;
			RutaFotografia = rutaFoto;

			// Inicalizar variables importantes
			Hijos = new List<Persona>();
			EstaVivo = true;
			GeneroPersona = Genero.NoEspecificado;
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
		
		public List<Persona> ObtenerAbuelos()
		{
			var abuelos = new List<Persona>();
			
			if (Padre != null)
			{
				if (Padre.Padre != null) abuelos.Add(Padre.Padre);
				if (Padre.Madre != null) abuelos.Add(Padre.Madre);
			}
			
			if (Madre != null)
			{
				if (Madre.Padre != null) abuelos.Add(Madre.Padre);
				if (Madre.Madre != null) abuelos.Add(Madre.Madre);
			}
			
			return abuelos;
		}
		
		public List<Persona> ObtenerNietos()
		{
			var nietos = new List<Persona>();
			
			foreach (var hijo in Hijos)
			{
				nietos.AddRange(hijo.Hijos);
			}
			
			return nietos;
		}
		
		// Métodos de ubicación y distancia
		public double CalcularDistancia(Persona otraPersona)
		{
			if (otraPersona == null)
				return 0;
				
			return CalcularDistanciaEnMapa(
				this.Latitud, this.Longitud,
				otraPersona.Latitud, otraPersona.Longitud
			);
		}

		private double CalcularDistanciaEnMapa(double lat1, double lon1, double lat2, double lon2)
		{
			// Fórmula de Pitágoras para mapa plano
			// Aproximación: 1 grado ≈ 111 km
			double diferenciaLat = (lat2 - lat1) * 111;
			double diferenciaLon = (lon2 - lon1) * 111;
			
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
			return $"{NombreCompleto} ({Cedula}) - {Edad} años - {Ciudad}, {Pais}";
		}
		
		public string ObtenerInformacionCompleta()
		{
			var info = $"Nombre: {NombreCompleto}\n";
			info += $"Cédula: {Cedula}\n";
			info += $"Fecha de Nacimiento: {FechaNacimiento.ToShortDateString()}\n";
			info += $"Edad: {Edad} años\n";
			info += $"Estado: {(EstaVivo ? "Vivo" : "Fallecido")}\n";
			
			if (!EstaVivo && FechaFallecimiento.HasValue)
				info += $"Fecha de Fallecimiento: {FechaFallecimiento.Value.ToShortDateString()}\n";
				
			info += $"Residencia: {Ciudad}, {Pais}\n";
			info += $"Coordenadas: ({Latitud}, {Longitud})\n";
			info += $"Generación: {Generacion}\n";
			info += $"Hijos: {Hijos.Count}\n";
			
			if (Padre != null)
				info += $"Padre: {Padre.NombreCompleto}\n";
			if (Madre != null)
				info += $"Madre: {Madre.NombreCompleto}\n";
				
			return info;
		}
	}
}
