extends TextureButton

@export var normal_texture: Texture
@export var hover_texture: Texture
@export var pressed_texture: Texture

func _ready():
	texture_normal = normal_texture
	texture_hover = hover_texture
	texture_pressed = pressed_texture


func _on_pressed() -> void:
	get_tree().change_scene_to_file("res://Scenes/CharacterSelect.tscn")
	pass # Replace with function body.
