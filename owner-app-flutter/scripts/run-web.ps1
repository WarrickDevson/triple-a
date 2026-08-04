# Owner app — Flutter Web on fixed port 8068 (matches API DevCors)
Set-Location $PSScriptRoot\..
flutter run -d chrome --web-port=8068 --web-hostname=localhost -t lib/main_dev.dart
