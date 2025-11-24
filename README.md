# Árbol Genealógico

Este proyecto fue desarrollado para la materia CE1103 - Algoritmos y Estructuras de Datos I del Tecnológico de Costa Rica durante el II Semestre del 2025. Corresponde a una aplicación desarrollada en C# con Godot Engine que permite crear y gestionar árboles genealógicos de manera visual e interactiva. La aplicación implementa estructuras de datos avanzadas (árboles n-arios y grafos) para representar relaciones familiares y ubicaciones geográficas.

## Características

- Gestión de Familiares: Ingreso de miembros con información detallada (nombre, apellido, cédula, fecha de nacimiento, fotografía)
- Visualización del Árbol: Representación gráfica del linaje familiar con conexiones entre generaciones
- Mapa Interactivo: Visualización de ubicaciones geográficas de familiares en un mapa mundial
- Cálculo de Distancias: Sistema de grafo para calcular distancias entre familiares según sus ubicaciones
- Estadísticas: 
  - Par de familiares más cercanos
  - Par de familiares más lejanos
  - Distancia promedio entre familiares
- Validación de Datos: Sistema de validación para mantener coherencia en el árbol genealógico

## Tecnologías Utilizadas

- Motor Gráfico: Godot Engine 4.5.1
- Lenguaje: C# (.NET 8.0)

- Framework de Testing: xUnit
- SDK: Godot.NET.Sdk
- Control de Versiones: Git / GitHub

## Requisitos Previos

Para ejecutar este proyecto necesitas tener instalado:

- [Godot Engine 4.5.1](https://godotengine.org/download) o superior con soporte para .NET
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) o superior
- Git (para clonar el repositorio)

## Estructura del Proyecto

```
.
├── arbol/                         # Carpeta raíz del workspace
│   ├── Arbol.Tests/               # Proyecto de pruebas unitarias (SEPARADO)
│   │   ├── Arbol.Tests.csproj    
│   │   └── [archivos de tests]   
│   │
│   ├── Arbol-Genealogico/        # Proyecto principal de Godot
│   │   ├── .godot/               # Archivos generados por Godot 
│   │   │
│   │   ├── Arbol_Core/           # Lógica del juego (Backend)
│   │   │   ├── Models/           # Modelos de datos
│   │   │   │   └── Persona.cs   
│   │   │   └── DataStructures/   # Estructuras de datos
│   │   │       ├── Arbol.cs     
│   │   │       └── Grafo.cs    
│   │   │
│   │   ├── assets/               # Recursos gráficos
│   │   │   └── [imágenes, texturas, etc.]
│   │   │
│   │   ├── fotos_default/        # Fotografías por defecto
│   │   │   └── [imágenes default cuando no se sube foto]
│   │   │
│   │   ├── fotos_personas/       # Fotografías de los familiares
│   │   │
│   │   ├── scenes/              # Escenas de Godot
│   │   │   ├── MainMenu.tscn    # Menú principal
│   │   │   ├── Tree.tscn        # Vista del árbol genealógico
│   │   │   └── Map.tscn         # Vista del mapa interactivo
│   │   │
│   │   ├── scripts/             # Scripts de UI (Frontend)
│   │   │   ├── UI/              # Scripts de interfaz gráfica
│   │   │   │   ├── AgregarPersona.cs
│   │   │   │   ├── MainMenu.cs
│   │   │   │   ├── MapaUI.cs
│   │   │   │   ├── VisualizadorArbol.cs
│   │   │   │   └── VisualizadorArbolUI.cs
│   │   │
│   │   ├── .editorconfig        
│   │   ├── .gitattributes    
│   │   ├── .gitignore        
│   │   ├── Arbol-Genealogico.csproj 
│   │   ├── Arbol-Genealogico.sln     
│   │   ├── project.godot      
│   │   └── README.md           
│   │
│   └── scripts/                  

```

## Autores

- Miguel Valdelomar Martinez 
- Alanna Mendoza Fonseca
- Dilana Gamboa Gonzalez

## Wiki del proyecto

Para más información sobre el uso detallado de la aplicación, consulta la [Wiki del proyecto](../../wiki).
