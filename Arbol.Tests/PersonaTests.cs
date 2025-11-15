using Xunit;
using Arbol_Core.Models;
using System;

public class PersonaTests
{
    [Fact]
    public void Constructor_CreaPersonaCorrectamente()
    {
        var persona = new Persona(
            nombre: "Ana",
            apellido: "Lopez",
            cedula: "123",
            fechaNacimiento: new DateTime(2000, 1, 1),
            edad: 25,
            latitud: 10.0,
            longitud: -84.0,
            foto: "foto.png",
            tipoPersona: "Familiar"
        );

        Assert.Equal("Ana Lopez", persona.NombreCompleto);
        Assert.Equal("123", persona.Cedula);
        Assert.True(persona.EstaVivo);
    }
}
