using Xunit;
using Arbol_Core.Models;
using Arbol_Core.DataStructures;
using System;

namespace Arbol.Tests
{
    public class GrafoTests
    {
        #region Constructor Tests

        [Fact]
        public void Constructor_InicializaGrafoVacio()
        {
            // Arrange & Act
            var grafo = new Grafo();

            // Assert
            Assert.Equal(0, grafo.CantidadNodos);
            Assert.Empty(grafo.ObtenerTodasLasPersonas());
        }

        #endregion

        #region AgregarNodo Tests

        [Fact]
        public void AgregarNodo_AgregaNodoCorrectamente()
        {
            // Arrange
            var grafo = new Grafo();
            var persona = CrearPersonaPrueba("Juan", "703450678", 9.93, -84.08);

            // Act
            grafo.AgregarNodo(persona);

            // Assert
            Assert.Equal(1, grafo.CantidadNodos);
            Assert.True(grafo.ExistePersona("703450678"));
        }

        [Fact]
        public void AgregarNodo_NoAgregaNodoNuloODuplicado()
        {
            // Arrange
            var grafo = new Grafo();
            var persona = CrearPersonaPrueba("Juan", "703450654");

            // Act
            grafo.AgregarNodo(null);
            grafo.AgregarNodo(persona);
            grafo.AgregarNodo(persona); // Intento duplicado

            // Assert
            Assert.Equal(1, grafo.CantidadNodos);
        }

        #endregion

        #region Construir Aristas Tests

         [Fact]
        public void ConstruirAristas_CreaConexionesEntreTodasLasPersonas()
        {
            // Arrange
            var grafo = new Grafo();
            var persona1 = CrearPersonaPrueba("Juan", "703450678", 9.93, -84.08);
            var persona2 = CrearPersonaPrueba("María", "203457778", 10.02, -84.21);
            var persona3 = CrearPersonaPrueba("Pedro", "603444678", 19.43, -99.13);

            grafo.AgregarNodo(persona1);
            grafo.AgregarNodo(persona2);
            grafo.AgregarNodo(persona3);

            // Act
            grafo.ConstruirAristas();

            // Assert - Verificar que todas las conexiones existen
            Assert.True(grafo.ObtenerDistancia("703450678", "203457778") > 0);
            Assert.True(grafo.ObtenerDistancia("703450678", "603444678") > 0);
            Assert.True(grafo.ObtenerDistancia("203457778", "603444678") > 0);
            
            // Verificar que no hay auto-conexiones
            Assert.Equal(-1, grafo.ObtenerDistancia("703450678", "703450678"));
        }


        #endregion

        #region ObtenerDistancia Tests

        [Fact]
        public void ObtenerDistancia_CalculaDistanciaEntreDosPaises()
        {
            // Arrange
            var grafo = new Grafo();
            // San José, Costa Rica
            var persona1 = CrearPersonaPrueba("Juan", "303450678", 9.93, -84.08);
            // Ciudad de México, México
            var persona2 = CrearPersonaPrueba("María", "703450678", 19.43, -99.13);

            grafo.AgregarNodo(persona1);
            grafo.AgregarNodo(persona2);
            grafo.ConstruirAristas();

            // Act
            double distancia = grafo.ObtenerDistancia("303450678", "703450678");

            // Assert
            Assert.True(distancia > 0);
            Assert.InRange(distancia, 1800, 2000); // Aproximadamente 1900 km
        }

        #endregion

        #region ObtenerParMasCercano y MasLejano Tests

        [Fact]
        public void ObtenerParMasCercano_EncuentraParMasCercano()
        {
            // Arrange
            var grafo = new Grafo();
            var persona1 = CrearPersonaPrueba("Juan", "603444678", 9.93, -84.08);
            var persona2 = CrearPersonaPrueba("María", "253444678", 10.02, -84.21);
            var persona3 = CrearPersonaPrueba("Pedro", "603444678", 40.71, -74.00);
            grafo.AgregarNodo(persona1);
            grafo.AgregarNodo(persona2);
            grafo.AgregarNodo(persona3);
            grafo.ConstruirAristas();

            // Act
            var resultado = grafo.ObtenerParMasCercano();

            // Assert
            Assert.NotNull(resultado.persona1);
            Assert.NotNull(resultado.persona2);
            Assert.True(resultado.distancia > 0);
            Assert.True(resultado.distancia < 100); // Menos de 100 km
        }

        [Fact]
        public void ObtenerParMasLejano_EncuentraParMasLejano()
        {
            // Arrange
            var grafo = new Grafo();
            var persona1 = CrearPersonaPrueba("Juan", "111", 9.93, -84.08); // Costa Rica
            var persona2 = CrearPersonaPrueba("María", "222", 10.02, -84.21); // Costa Rica
            var persona3 = CrearPersonaPrueba("Pedro", "333", -33.45, -70.66); // Chile

            grafo.AgregarNodo(persona1);
            grafo.AgregarNodo(persona2);
            grafo.AgregarNodo(persona3);
            grafo.ConstruirAristas();

            // Act
            var resultado = grafo.ObtenerParMasLejano();

            // Assert
            Assert.NotNull(resultado.persona1);
            Assert.NotNull(resultado.persona2);
            Assert.True(resultado.distancia > 1000); // Más de 1000 km
        }

        #endregion

        #region CalcularDistanciaPromedio Tests

        [Fact]
        public void CalcularDistanciaPromedio_CalculaPromedioCorrectamente()
        {
            // Arrange
            var grafo = new Grafo();
            var persona1 = CrearPersonaPrueba("Juan", "101230456", 9.93, -84.08);
            var persona2 = CrearPersonaPrueba("María", "202340567", 10.02, -84.21);
            var persona3 = CrearPersonaPrueba("Pedro", "303450678", 19.43, -99.13);

            grafo.AgregarNodo(persona1);
            grafo.AgregarNodo(persona2);
            grafo.AgregarNodo(persona3);
            grafo.ConstruirAristas();

            // Act
            double promedio = grafo.CalcularDistanciaPromedio();

            // Assert
            Assert.True(promedio > 0);
        }

        #endregion

        #region Métodos Helper

        // Crea una persona de prueba con valores por defecto
        private Persona CrearPersonaPrueba(
            string nombre = "Juan",
            string cedula = "123456789",
            double latitud = 10.0,
            double longitud = -84.0)
        {
            var persona = new Persona(
                nombre: nombre,
                apellido: "Pérez",
                cedula: cedula,
                fechaNacimiento: new DateTime(1990, 1, 1),
                edad: 34,
                latitud: latitud,
                longitud: longitud,
                foto: "foto.png",
                tipoPersona: "familiar"
            );

            persona.GeneroPersona = Persona.Genero.Masculino;

            return persona;
        }

        #endregion
    }
}