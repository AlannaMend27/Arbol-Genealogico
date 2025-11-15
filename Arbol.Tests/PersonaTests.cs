using Xunit;
using Arbol_Core.Models;
using System;
using System.Linq;

namespace Arbol.Tests
{
    public class PersonaTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_CreaPersonaCorrectamente()
        {
            // Arrange & Act
            var persona = new Persona(
                nombre: "Ana",
                apellido: "López",
                cedula: "123456789",
                fechaNacimiento: new DateTime(2000, 1, 1),
                edad: 25,
                latitud: 10.0,
                longitud: -84.0,
                foto: "foto.png",
                tipoPersona: "Familiar"
            );

            // Assert
            Assert.Equal("Ana López", persona.NombreCompleto);
            Assert.Equal("123456789", persona.Cedula);
            Assert.True(persona.EstaVivo);
            Assert.Equal(25, persona.Edad);
            Assert.NotNull(persona.Hijos);
            Assert.Empty(persona.Hijos);
            Assert.Equal(0, persona.Generacion);
        }

        [Fact]
        public void Constructor_InicializaListaHijosVacia()
        {
            // Arrange & Act
            var persona = CrearPersonaPrueba();

            // Assert
            Assert.NotNull(persona.Hijos);
            Assert.Empty(persona.Hijos);
        }

        #endregion

        #region Propiedades Tests

        [Fact]
        public void NombreCompleto_RetornaNombreYApellidoConcatenados()
        {
            // Arrange & Act
            var persona = new Persona(
                nombre: "Juan",
                apellido: "Pérez",
                cedula: "111",
                fechaNacimiento: DateTime.Now.AddYears(-30),
                edad: 30,
                latitud: 0,
                longitud: 0,
                foto: "foto.png",
                tipoPersona: "Familiar"
            );

            // Assert
            Assert.Equal("Juan Pérez", persona.NombreCompleto);
        }

        [Fact]
        public void EstaVivo_InicializaComoTrue()
        {
            // Arrange & Act
            var persona = CrearPersonaPrueba();

            // Assert
            Assert.True(persona.EstaVivo);
        }

        #endregion

        #region AgregarHijo Tests

        [Fact]
        public void AgregarHijo_AgregaHijoCorrectamente_PadreMasculino()
        {
            // Arrange
            var padre = CrearPersonaPrueba("Carlos", "Masculino");
            var hijo = CrearPersonaPrueba("Luis");

            // Act
            padre.AgregarHijo(hijo);

            // Assert
            Assert.Contains(hijo, padre.Hijos);
            Assert.Equal(padre, hijo.Padre);
            Assert.Equal(1, hijo.Generacion);
        }

        [Fact]
        public void AgregarHijo_AgregaHijoCorrectamente_MadreFemenino()
        {
            // Arrange
            var madre = CrearPersonaPrueba("María", "Femenino");
            var hijo = CrearPersonaPrueba("Luis");

            // Act
            madre.AgregarHijo(hijo);

            // Assert
            Assert.Contains(hijo, madre.Hijos);
            Assert.Equal(madre, hijo.Madre);
            Assert.Equal(1, hijo.Generacion);
        }

        [Fact]
        public void AgregarHijo_NoAgregaHijoNulo()
        {
            // Arrange
            var padre = CrearPersonaPrueba();

            // Act
            padre.AgregarHijo(null);

            // Assert
            Assert.Empty(padre.Hijos);
        }

        [Fact]
        public void AgregarHijo_NoAgregaHijoDuplicado()
        {
            // Arrange
            var padre = CrearPersonaPrueba();
            var hijo = CrearPersonaPrueba("Luis");

            // Act
            padre.AgregarHijo(hijo);
            padre.AgregarHijo(hijo);

            // Assert
            Assert.Single(padre.Hijos);
        }

        [Fact]
        public void AgregarHijo_ActualizaGeneracionCorrectamente()
        {
            // Arrange
            var abuelo = CrearPersonaPrueba("Abuelo", "Masculino");
            abuelo.Generacion = 0;
            
            var padre = CrearPersonaPrueba("Padre", "Masculino");
            var hijo = CrearPersonaPrueba("Hijo");

            // Act
            abuelo.AgregarHijo(padre);
            padre.AgregarHijo(hijo);

            // Assert
            Assert.Equal(0, abuelo.Generacion);
            Assert.Equal(1, padre.Generacion);
            Assert.Equal(2, hijo.Generacion);
        }

        #endregion

        #region RemoverHijo Tests

        [Fact]
        public void RemoverHijo_EliminaHijoCorrectamente()
        {
            // Arrange
            var padre = CrearPersonaPrueba("Carlos", "Masculino");
            var hijo = CrearPersonaPrueba("Luis");
            padre.AgregarHijo(hijo);

            // Act
            padre.RemoverHijo(hijo);

            // Assert
            Assert.DoesNotContain(hijo, padre.Hijos);
            Assert.Null(hijo.Padre);
        }

        [Fact]
        public void RemoverHijo_NoHaceNadaConHijoNulo()
        {
            // Arrange
            var padre = CrearPersonaPrueba();
            var hijo = CrearPersonaPrueba("Luis");
            padre.AgregarHijo(hijo);

            // Act
            padre.RemoverHijo(null);

            // Assert
            Assert.Single(padre.Hijos);
        }

        #endregion

        #region EstablecerPadres Tests

        [Fact]
        public void EstablecerPadres_AsignaPadreYMadreCorrectamente()
        {
            // Arrange
            var padre = CrearPersonaPrueba("Carlos", "Masculino");
            var madre = CrearPersonaPrueba("María", "Femenino");
            var hijo = CrearPersonaPrueba("Luis");

            // Act
            hijo.EstablecerPadres(padre, madre);

            // Assert
            Assert.Equal(padre, hijo.Padre);
            Assert.Equal(madre, hijo.Madre);
            Assert.Contains(hijo, padre.Hijos);
            Assert.Contains(hijo, madre.Hijos);
        }

        [Fact]
        public void EstablecerPadres_ActualizaGeneracion()
        {
            // Arrange
            var padre = CrearPersonaPrueba("Carlos", "Masculino");
            padre.Generacion = 1;
            var hijo = CrearPersonaPrueba("Luis");

            // Act
            hijo.EstablecerPadres(padre, null);

            // Assert
            Assert.Equal(2, hijo.Generacion);
        }

        [Fact]
        public void EstablecerPadres_PermitePadreNulo()
        {
            // Arrange
            var madre = CrearPersonaPrueba("María", "Femenino");
            var hijo = CrearPersonaPrueba("Luis");

            // Act
            hijo.EstablecerPadres(null, madre);

            // Assert
            Assert.Null(hijo.Padre);
            Assert.Equal(madre, hijo.Madre);
        }

        #endregion

        #region ObtenerHermanos Tests

        [Fact]
        public void ObtenerHermanos_RetornaHermanosDelPadre()
        {
            // Arrange
            var padre = CrearPersonaPrueba("Carlos", "Masculino");
            var hijo1 = CrearPersonaPrueba("Luis", "Masculino", "111");
            var hijo2 = CrearPersonaPrueba("Ana", "Femenino", "222");
            
            padre.AgregarHijo(hijo1);
            padre.AgregarHijo(hijo2);

            // Act
            var hermanos = hijo1.ObtenerHermanos();

            // Assert
            Assert.Single(hermanos);
            Assert.Contains(hijo2, hermanos);
        }

        [Fact]
        public void ObtenerHermanos_RetornaListaVaciaSinPadres()
        {
            // Arrange
            var persona = CrearPersonaPrueba();

            // Act
            var hermanos = persona.ObtenerHermanos();

            // Assert
            Assert.Empty(hermanos);
        }

        [Fact]
        public void ObtenerHermanos_NoIncluyeHermanosDuplicados()
        {
            // Arrange
            var padre = CrearPersonaPrueba("Carlos", "Masculino");
            var madre = CrearPersonaPrueba("María", "Femenino");
            var hijo1 = CrearPersonaPrueba("Luis", "Masculino", "111");
            var hijo2 = CrearPersonaPrueba("Ana", "Femenino", "222");
            
            padre.AgregarHijo(hijo1);
            padre.AgregarHijo(hijo2);
            madre.AgregarHijo(hijo1);
            madre.AgregarHijo(hijo2);

            // Act
            var hermanos = hijo1.ObtenerHermanos();

            // Assert
            Assert.Single(hermanos);
            Assert.Contains(hijo2, hermanos);
        }

        #endregion

        #region CalcularDistancia Tests

        //faltan las pruebas unitarias para calcular distancias

        #endregion

        #region TieneCoordenadasValidas Tests

        [Fact]
        public void TieneCoordenadasValidas_RetornaTrueParaCoordenadasValidas()
        {
            // Arrange
            var persona = CrearPersonaPrueba();
            persona.Latitud = 10.0;
            persona.Longitud = -84.0;

            // Act & Assert
            Assert.True(persona.TieneCoordenadasValidas());
        }

        [Theory]
        [InlineData(91, 0)]
        [InlineData(-91, 0)]
        [InlineData(0, 181)]
        [InlineData(0, -181)]
        public void TieneCoordenadasValidas_RetornaFalseParaCoordenadasInvalidas(double lat, double lon)
        {
            // Arrange
            var persona = CrearPersonaPrueba();
            persona.Latitud = lat;
            persona.Longitud = lon;

            // Act & Assert
            Assert.False(persona.TieneCoordenadasValidas());
        }

        [Fact]
        public void TieneCoordenadasValidas_AceptaLimitesExactos()
        {
            // Arrange
            var persona1 = CrearPersonaPrueba();
            persona1.Latitud = 90;
            persona1.Longitud = 180;

            var persona2 = CrearPersonaPrueba();
            persona2.Latitud = -90;
            persona2.Longitud = -180;

            // Act & Assert
            Assert.True(persona1.TieneCoordenadasValidas());
            Assert.True(persona2.TieneCoordenadasValidas());
        }

        #endregion

        #region EsValido Tests

        [Fact]
        public void EsValido_RetornaTrueParaPersonaCompleta()
        {
            // Arrange
            var persona = CrearPersonaPrueba();

            // Act & Assert
            Assert.True(persona.EsValido());
        }

        [Theory]
        [InlineData("", "López", "123")]
        [InlineData("Juan", "", "123")]
        [InlineData("Juan", "López", "")]
        public void EsValido_RetornaFalseParaDatosVacios(string nombre, string apellido, string cedula)
        {
            // Arrange
            var persona = CrearPersonaPrueba();
            persona.Nombre = nombre;
            persona.Apellido = apellido;
            persona.Cedula = cedula;

            // Act & Assert
            Assert.False(persona.EsValido());
        }

        [Fact]
        public void EsValido_RetornaFalseParaCoordenadasInvalidas()
        {
            // Arrange
            var persona = CrearPersonaPrueba();
            persona.Latitud = 100;

            // Act & Assert
            Assert.False(persona.EsValido());
        }

        #endregion

        #region ObtenerErroresValidacion Tests

        [Fact]
        public void ObtenerErroresValidacion_RetornaListaVaciaParaPersonaValida()
        {
            // Arrange
            var persona = CrearPersonaPrueba();

            // Act
            var errores = persona.ObtenerErroresValidacion();

            // Assert
            Assert.Empty(errores);
        }

        [Fact]
        public void ObtenerErroresValidacion_DetectaNombreVacio()
        {
            // Arrange
            var persona = CrearPersonaPrueba();
            persona.Nombre = "";

            // Act
            var errores = persona.ObtenerErroresValidacion();

            // Assert
            Assert.Contains(errores, e => e.Contains("nombre"));
        }

        [Fact]
        public void ObtenerErroresValidacion_DetectaFechaNacimientoFutura()
        {
            // Arrange
            var persona = CrearPersonaPrueba();
            persona.FechaNacimiento = DateTime.Today.AddDays(1);

            // Act
            var errores = persona.ObtenerErroresValidacion();

            // Assert
            Assert.Contains(errores, e => e.Contains("futura"));
        }

        [Fact]
        public void ObtenerErroresValidacion_DetectaFechaFallecimientoSinMuerte()
        {
            // Arrange
            var persona = CrearPersonaPrueba();
            persona.EstaVivo = false;
            persona.FechaFallecimiento = null;

            // Act
            var errores = persona.ObtenerErroresValidacion();

            // Assert
            Assert.Contains(errores, e => e.Contains("fallecimiento"));
        }

        [Fact]
        public void ObtenerErroresValidacion_DetectaFechaFallecimientoAnteriorANacimiento()
        {
            // Arrange
            var persona = CrearPersonaPrueba();
            persona.FechaNacimiento = new DateTime(2000, 1, 1);
            persona.FechaFallecimiento = new DateTime(1999, 1, 1);

            // Act
            var errores = persona.ObtenerErroresValidacion();

            // Assert
            Assert.Contains(errores, e => e.Contains("anterior"));
        }

        #endregion

        #region ToString Tests

        [Fact]
        public void ToString_RetornaFormatoEsperado()
        {
            // Arrange
            var persona = new Persona(
                nombre: "Juan",
                apellido: "Pérez",
                cedula: "123456",
                fechaNacimiento: DateTime.Now.AddYears(-30),
                edad: 30,
                latitud: 0,
                longitud: 0,
                foto: "foto.png",
                tipoPersona: "Familiar"
            );

            // Act
            var resultado = persona.ToString();

            // Assert
            Assert.Contains("Juan Pérez", resultado);
            Assert.Contains("123456", resultado);
            Assert.Contains("30 años", resultado);
        }

        #endregion

        #region Métodos Helper

        /// <summary>
        /// Crea una persona de prueba con valores por defecto
        /// </summary>
        private Persona CrearPersonaPrueba(
            string nombre = "Juan", 
            string genero = "Masculino",
            string cedula = "123456789")
        {
            var persona = new Persona(
                nombre: nombre,
                apellido: "Pérez",
                cedula: cedula,
                fechaNacimiento: new DateTime(1990, 1, 1),
                edad: 34,
                latitud: 10.0,
                longitud: -84.0,
                foto: "foto.png",
                tipoPersona: "Familiar"
            );

            if (genero == "Masculino")
                persona.GeneroPersona = Persona.Genero.Masculino;
            else
                persona.GeneroPersona = Persona.Genero.Femenino;

            return persona;
        }

        #endregion
    }
}