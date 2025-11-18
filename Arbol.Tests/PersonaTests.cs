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

        #region CalcularDistancia Tests

        [Fact]
        public void CalcularDistancia_CalculaDistanciaCorrectamente()
        {
            // Arrange
            // San José, Costa Rica
            var persona1 = CrearPersonaPrueba();
            persona1.Latitud = 9.93;
            persona1.Longitud = -84.08;
            
            // Ciudad de México
            var persona2 = CrearPersonaPrueba();
            persona2.Latitud = 19.43;
            persona2.Longitud = -99.13;

            // Act
            double distancia = persona1.CalcularDistancia(persona2);

            // Assert
            Assert.True(distancia > 0);
            Assert.InRange(distancia, 1800, 2000); // la distancia deberia ser aproximadamente 1900km
        }

        [Fact]
        public void CalcularDistancia_RetornaMenosUnoParaCoordenadasInvalidas()
        {
            // Arrange
            var persona1 = CrearPersonaPrueba();
            persona1.Latitud = 100; 
            
            var persona2 = CrearPersonaPrueba();

            // Act
            double distancia = persona1.CalcularDistancia(persona2);

            // Assert
            Assert.Equal(-1, distancia);
        }

        #endregion

        #region TieneCoordenadasValidas Tests

        [Fact]
        public void TieneCoordenadasValidas_RetornaTrueParaCoordenadasValidas()
        {
            // Arrange
            var persona = CrearPersonaPrueba();

            // coordenadas de san jose
            persona.Latitud = 9.7489;
            persona.Longitud = -83.7534;

            // Act & Assert
            Assert.True(persona.TieneCoordenadasValidas());
        }

        [Fact]
        public void TieneCoordenadasValidas_RetornaFalseParaCoordenadasEnMar()
        {
            // Arrange
            var persona = CrearPersonaPrueba();

            // coordenada en oceano pacifico
            persona.Latitud = 0.0;
            persona.Longitud = -150.0;

            // Act & Assert
            Assert.False(persona.TieneCoordenadasValidas());
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

        #endregion

        #region Métodos Helper

        // Crea una persona de prueba con valores por defecto
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