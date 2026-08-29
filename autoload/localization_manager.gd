extends Node

const SUPPORTED_LOCALES := ["en", "km", "zh"]

func set_locale(locale: String) -> void:
	if locale in SUPPORTED_LOCALES:
		TranslationServer.set_locale(locale)
