using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArbolGenealogico.scripts.UI
{
    public partial class MainMenu : Node2D
        {
            private Button btnIngresarMiembros;
            private Button btnVerMapa;

            public override void _Ready()
            {
                // Obtener referencias a los botones por su ruta en el árbol de nodos
                btnIngresarMiembros = GetNode<Button>("MenuBackground/Button");
                btnVerMapa = GetNode<Button>("MenuBackground/Button2");

                // Conectar las señales de los botones
                btnIngresarMiembros.Pressed += OnIngresarMiembrosPressed;
                btnVerMapa.Pressed += OnVerMapaPressed;
            }

            private void OnIngresarMiembrosPressed()
            {
                // Cambiar a la escena del árbol
                GetTree().ChangeSceneToFile("res://scenes/Tree.tscn");
            }

            private void OnVerMapaPressed()
            {
                // Cambiar a la escena del mapa
                GetTree().ChangeSceneToFile("res://scenes/Map.tscn");
            }
        }
}
