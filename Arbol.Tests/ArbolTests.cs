using Xunit;
using Arbol_Core.Models;
using Arbol_Core.DataStructures;
using System;

namespace Arbol.Tests
{
    public class ArbolTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_InicializaArbolVacio()
        {
            // Arrange & Act
            var arbol = new Arbol_Core.DataStructures.Arbol();

            // Assert
            Assert.Equal(0, arbol.CantidadMiembros);
            Assert.Empty(arbol.ObtenerTodasLasPersonas());
        }

        #endregion

        #region AgregarPersona Tests

        [Fact]
        public void AgregarPersona_AgregaPersonaCorrectamente()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var persona = CrearPersonaPrueba("Juan", "603444678");

            // Act
            var resultado = arbol.AgregarPersona(persona);

            // Assert
            Assert.True(resultado);
            Assert.Equal(1, arbol.CantidadMiembros);
        }

        [Fact]
        public void AgregarPersona_AsignaGeneracionCorrectamente()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var padre = CrearPersonaPrueba("Padre", "603444678", Persona.Genero.Masculino);
            padre.TipoPersona = "familiar";
            
            var hijo = CrearPersonaPrueba("Hijo", "203444678");
            hijo.TipoPersona = "familiar";
            hijo.Padre = padre;

            // Act
            arbol.AgregarPersona(padre);
            arbol.AgregarPersona(hijo);

            // Assert
            Assert.Equal(0, padre.Generacion);
            Assert.Equal(1, hijo.Generacion);
        }

        #endregion

        #region BuscarPorCedula Tests

        [Fact]
        public void BuscarPorCedula_EncuentraPersonaExistente()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var persona = CrearPersonaPrueba("Juan", "603444678");
            arbol.AgregarPersona(persona);

            // Act
            var resultado = arbol.BuscarPorCedula("603444678");

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Juan", resultado.Nombre);
        }

        [Fact]
        public void BuscarPorCedula_RetornaNullParaCedulaInexistente()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            arbol.AgregarPersona(CrearPersonaPrueba("Juan", "603444678"));

            // Act
            var resultado = arbol.BuscarPorCedula("113445544");

            // Assert
            Assert.Null(resultado);
        }

        #endregion

        #region ObtenerTodasLasPersonas Tests

        [Fact]
        public void ObtenerTodasLasPersonas_RetornaTodasLasPersonasAgregadas()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var persona1 = CrearPersonaPrueba("Juan", "603444678");
            var persona2 = CrearPersonaPrueba("María", "203444678");
            
            arbol.AgregarPersona(persona1);
            arbol.AgregarPersona(persona2);

            // Act
            var resultado = arbol.ObtenerTodasLasPersonas();

            // Assert
            Assert.Equal(2, resultado.Count);
            Assert.Contains(persona1, resultado);
            Assert.Contains(persona2, resultado);
        }

        #endregion

        #region Métodos Helper

        /// <summary>
        /// Crea una persona de prueba con valores por defecto
        /// </summary>
        private Persona CrearPersonaPrueba(
            string nombre = "Juan",
            string cedula = "123456789",
            Persona.Genero genero = Persona.Genero.Masculino)
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
                tipoPersona: "familiar"
            );

            persona.GeneroPersona = genero;

            return persona;
        }

        #endregion
    }
}