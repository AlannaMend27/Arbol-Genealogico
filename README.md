# Árbol Genealógico 🌳

Aplicación interactiva para la creación y visualización de árboles genealógicos con representación geográfica de miembros familiares.

## 📋 Descripción

Este proyecto es una aplicación desarrollada en C# con Godot Engine que permite crear y gestionar árboles genealógicos de manera visual e interactiva. La aplicación implementa estructuras de datos avanzadas (árboles n-arios y grafos) para representar relaciones familiares y ubicaciones geográficas.

## ✨ Características

- **Gestión de Familiares**: Ingreso de miembros con información detallada (nombre, apellido, cédula, fecha de nacimiento, fotografía)
- **Visualización del Árbol**: Representación gráfica del linaje familiar con conexiones entre generaciones
- **Mapa Interactivo**: Visualización de ubicaciones geográficas de familiares en un mapa mundial
- **Cálculo de Distancias**: Sistema de grafo para calcular distancias entre familiares según sus ubicaciones
- **Estadísticas**: 
  - Par de familiares más cercanos
  - Par de familiares más lejanos
  - Distancia promedio entre familiares
- **Validación de Datos**: Sistema de validación para mantener coherencia en el árbol genealógico

## 🛠️ Tecnologías Utilizadas

- **Motor Gráfico**: Godot Engine 4.5.1
- **Lenguaje**: C# (.NET 8.0)

- **Framework de Testing**: xUnit
- **SDK**: Godot.NET.Sdk
- **Control de Versiones**: Git / GitHub

## 📦 Requisitos Previos

Para ejecutar este proyecto necesitas tener instalado:

- [Godot Engine 4.5.1](https://godotengine.org/download) o superior con soporte para .NET
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) o superior
- Git (para clonar el repositorio)

## 🚀 Instalación

1. **Clonar el repositorio**
   ```bash
   git clone https://github.com/tu-usuario/Arbol-Genealogico.git
   cd Arbol-Genealogico
   ```

2. **Abrir el proyecto en Godot**
   - Abre Godot Engine
   - Haz clic en "Importar"
   - Navega hasta la carpeta `arbol/Arbol-Genealogico/`
   - Selecciona el archivo `project.godot`
   - Haz clic en "Importar y Editar"

3. **Compilar el proyecto**
   - Godot compilará automáticamente el proyecto C# la primera vez que lo abras
   - Si encuentras errores, asegúrate de tener instalado .NET SDK 8.0
   - El proyecto se compilará generando el ensamblado necesario

## 🎮 Cómo Usar

1. **Menú Principal**: Al iniciar la aplicación verás dos opciones principales:
   - "Ingresar miembros": Para agregar familiares al árbol
   - "Ver mapa": Para visualizar las ubicaciones geográficas
<img width="759" height="494" alt="image" src="https://github.com/user-attachments/assets/24fe969b-6f3a-4a84-949e-7f9b4a30e331" />

2. **Agregar Familiares**:
   - Completa el formulario con los datos del familiar
   - Ingresa las coordenadas de su lugar de residencia
   - Sube una fotografía
   - El árbol se actualizará automáticamente
<img width="759" height="494" alt="image" src="https://github.com/user-attachments/assets/7630e860-9374-4ad7-a2b8-bd5fb22d2268" />


3. **Explorar el Mapa**:
   - Haz clic en las fotografías para ver información detallada
   - Las líneas muestran las distancias entre familiares
   - Consulta las estadísticas en la sección correspondiente
<img width="759" height="494" alt="image" src="https://github.com/user-attachments/assets/9e94143c-9905-4264-a7a6-ffe3b5d0e526" />



## 📁 Estructura del Proyecto

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

### 🎯 Decisiones de Diseño de la Estructura

#### Separación Backend/Frontend

La arquitectura separa claramente las responsabilidades:

- **`Arbol_Core/`**: Contiene toda la lógica
  - Estructuras de datos puras (Árbol, Grafo)
  - Modelos de dominio (Persona)
  - Sin dependencias de Godot
  - Fácilmente testeable

- **`scripts/UI/`**: Scripts que manejan la interfaz gráfica
  - Heredan de nodos de Godot (Control, Node2D, etc.)
  - Interactúan con las escenas `.tscn`
  - Llaman a la lógica en `Arbol_Core`

Esta separación permite:
- Testear la lógica sin necesidad de Godot
- Reutilizar el código de `Arbol_Core` en otros contextos
- Mantener el código limpio y organizado

## 🧪 Pruebas Unitarias

El proyecto incluye un conjunto completo de pruebas unitarias usando xUnit en un proyecto separado.

### Ejecutar las pruebas:

```bash
# Navegar a la carpeta de tests
cd arbol/Arbol.Tests

# Ejecutar todas las pruebas
dotnet test

```


Debido a las limitaciones de Godot con proyectos C#, mantener las pruebas unitarias en un proyecto independiente evita conflictos de compilación y permite ejecutar los tests sin problemas.

## 👥 Autores

- **Miguel Valdelomar Martinez** 
- **Alanna Mendoza Fonseca**
- **Dilana Gamboa Gonzalez**

## 📚 Proyecto Académico

Este proyecto fue desarrollado para la materia **CE1103 - Algoritmos y Estructuras de Datos I** del Tecnológico de Costa Rica, II Semestre 2025.

### Objetivos del Proyecto
- Implementar grafos mediante una aplicación de árbol genealógico
- Modelar el linaje familiar como árbol y grafo coherentes
- Gestionar datos esenciales de cada familiar
- Visualizar y analizar la red familiar en un mapa mundial por medio de una interfaz grafica

### 📄 Licencia (MIT)

Se concede permiso, de forma gratuita, a cualquier persona que obtenga una copia de este software y de la documentación asociada, para usar, copiar, modificar y distribuir el software sin restricciones, sujeto a que se mantenga este aviso de licencia en todas las copias.

## 📂 Wiki del proyecto

Para más información sobre el uso detallado de la aplicación, consulta la [Wiki del proyecto](../../wiki).
