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
            var persona = CrearPersonaPrueba("Juan", "123");

            // Act
            var resultado = arbol.AgregarPersona(persona);

            // Assert
            Assert.True(resultado);
            Assert.Equal(1, arbol.CantidadMiembros);
        }

        [Fact]
        public void AgregarPersona_NoAgregaPersonaNulaOInvalida()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var personaInvalida = CrearPersonaPrueba("", "123"); // Nombre vacío

            // Act
            var resultadoNula = arbol.AgregarPersona(null);
            var resultadoInvalida = arbol.AgregarPersona(personaInvalida);

            // Assert
            Assert.False(resultadoNula);
            Assert.False(resultadoInvalida);
            Assert.Equal(0, arbol.CantidadMiembros);
        }

        [Fact]
        public void AgregarPersona_AsignaGeneracionCorrectamente()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var padre = CrearPersonaPrueba("Padre", "111", Persona.Genero.Masculino);
            padre.TipoPersona = "familiar";
            
            var hijo = CrearPersonaPrueba("Hijo", "222");
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
            var persona = CrearPersonaPrueba("Juan", "123");
            arbol.AgregarPersona(persona);

            // Act
            var resultado = arbol.BuscarPorCedula("123");

            // Assert
            Assert.NotNull(resultado);
            Assert.Equal("Juan", resultado.Nombre);
        }

        [Fact]
        public void BuscarPorCedula_RetornaNullParaCedulaInexistente()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            arbol.AgregarPersona(CrearPersonaPrueba("Juan", "123"));

            // Act
            var resultado = arbol.BuscarPorCedula("999");

            // Assert
            Assert.Null(resultado);
        }

        [Fact]
        public void BuscarPorCedula_RetornaNullParaCedulaNulaOVacia()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();

            // Act & Assert
            Assert.Null(arbol.BuscarPorCedula(null));
            Assert.Null(arbol.BuscarPorCedula(""));
        }

        #endregion

        #region ObtenerTodasLasPersonas Tests

        [Fact]
        public void ObtenerTodasLasPersonas_RetornaListaVaciaEnArbolVacio()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();

            // Act
            var resultado = arbol.ObtenerTodasLasPersonas();

            // Assert
            Assert.Empty(resultado);
        }

        [Fact]
        public void ObtenerTodasLasPersonas_RetornaTodasLasPersonasAgregadas()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var persona1 = CrearPersonaPrueba("Juan", "111");
            var persona2 = CrearPersonaPrueba("María", "222");
            
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

        #region ObtenerPersonasFundadoras Tests

        [Fact]
        public void ObtenerPersonasFundadoras_RetornaPersonasSinPadres()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var fundador = CrearPersonaPrueba("Fundador", "111");
            fundador.TipoPersona = "familiar";
            
            arbol.AgregarPersona(fundador);

            // Act
            var fundadores = arbol.ObtenerPersonasFundadoras();

            // Assert
            Assert.Single(fundadores);
            Assert.Contains(fundador, fundadores);
        }

        [Fact]
        public void ObtenerPersonasFundadoras_NoRetornaDescendientes()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            var fundador = CrearPersonaPrueba("Fundador", "111", Persona.Genero.Masculino);
            fundador.TipoPersona = "familiar";
            
            var hijo = CrearPersonaPrueba("Hijo", "222");
            hijo.TipoPersona = "familiar";
            hijo.Padre = fundador;

            arbol.AgregarPersona(fundador);
            arbol.AgregarPersona(hijo);

            // Act
            var fundadores = arbol.ObtenerPersonasFundadoras();

            // Assert
            Assert.Single(fundadores);
            Assert.DoesNotContain(hijo, fundadores);
        }

        #endregion

        #region LimpiarArbol Tests

        [Fact]
        public void LimpiarArbol_EliminaTodosLosMiembros()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            arbol.AgregarPersona(CrearPersonaPrueba("Juan", "111"));
            arbol.AgregarPersona(CrearPersonaPrueba("María", "222"));

            // Act
            arbol.LimpiarArbol();

            // Assert
            Assert.Equal(0, arbol.CantidadMiembros);
            Assert.Empty(arbol.ObtenerTodasLasPersonas());
        }

        [Fact]
        public void LimpiarArbol_PermiteOperacionesDespuesDeLimpiar()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();
            arbol.AgregarPersona(CrearPersonaPrueba("Juan", "111"));
            arbol.LimpiarArbol();

            // Act
            var persona = CrearPersonaPrueba("María", "222");
            var resultado = arbol.AgregarPersona(persona);

            // Assert
            Assert.True(resultado);
            Assert.Equal(1, arbol.CantidadMiembros);
            Assert.Null(arbol.BuscarPorCedula("111"));
        }

        #endregion

        #region Tests de Integración

        [Fact]
        public void Integracion_CreaArbolGenealogico3Generaciones()
        {
            // Arrange
            var arbol = new Arbol_Core.DataStructures.Arbol();

            // Generación 0 (Abuelos)
            var abuelo = CrearPersonaPrueba("Abuelo", "100", Persona.Genero.Masculino);
            abuelo.TipoPersona = "familiar";

            // Generación 1 (Padre)
            var padre = CrearPersonaPrueba("Padre", "200", Persona.Genero.Masculino);
            padre.TipoPersona = "familiar";
            padre.Padre = abuelo;

            // Generación 2 (Hijo)
            var hijo = CrearPersonaPrueba("Hijo", "300");
            hijo.TipoPersona = "familiar";
            hijo.Padre = padre;

            // Act
            arbol.AgregarPersona(abuelo);
            arbol.AgregarPersona(padre);
            arbol.AgregarPersona(hijo);

            // Assert
            Assert.Equal(3, arbol.CantidadMiembros);
            Assert.Equal(0, abuelo.Generacion);
            Assert.Equal(1, padre.Generacion);
            Assert.Equal(2, hijo.Generacion);
            Assert.Contains(hijo, padre.Hijos);
        }

        #endregion

        #region Métodos Helper

        /// Crea una persona de prueba con valores por defecto
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